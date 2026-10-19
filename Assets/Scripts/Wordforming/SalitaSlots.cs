using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using NaughtyAttributes;
using Unity.VisualScripting;

[RequireComponent(typeof(DropZone))]
public class SalitaSlots : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Configurations")]
    [HideInInspector] private LevelConfig config;

    [Header("Soft Dependencies")]
    [HideInInspector] InvalidWordPopup invalidWordPopupScript;
    [HideInInspector] AksyonCounter aksyonCounter;
    [SerializeField] private WordSubmissionAnimationPlayer successfulWordAnimationPlayer;
    

    [Header("Reference")]
    [SerializeField] private Button submitButton;

    [Header("Tiles")]
    [SerializeField] private TileSet tileSet;
    [SerializeField] public List<Tile> activeTiles = new List<Tile>();

    [Header("Word Properties")]
    //[ReadOnly, SerializeField] private string baybayinSalita; // maybe will use eventually ..?
    [ReadOnly, SerializeField] private string latinSalita;
    private string revealedLatinSalita;
    private string observedLatinSalita;

    [Header("Score Properties")]
    [ReadOnly, SerializeField] private int salitaScore = 0;
    [SerializeField] private float scoreScaleValue = 0.5f;
    [Tooltip("Time for submitted tiles to fade into their scored appearance.")]
    [SerializeField, Min(0f)] private float scoredTileTransitionDuration = 0.25f;
    [Tooltip("Extra pause after the scored appearance is visible and before counting begins.")]
    [SerializeField, Min(0f)] private float delayBeforeTileScoring = 0.15f;
    [SerializeField, Min(0f)] private float scoreTileDelay = 0.4f;
    [SerializeField, Min(0f)] private float scorePulseDuration = 0.22f;
    [SerializeField, Min(1f)] private float scorePulseScale = 1.12f;
    [SerializeField, Range(0f, 1f)] private float scorePulseBrightness = 0.35f;
    [SerializeField, Min(0f)] private float finalScoreDisplayDelay = 0.75f;
    [Tooltip("Pitch increase per scored tile. Two semitones equal one whole tone.")]
    [SerializeField, Min(0f)] private float tileTickSemitoneStep = 2f;

    [Header("Submitted Tile Exit")]
    [SerializeField, Min(0f)] private float tileExitDelay = 0.15f;
    [SerializeField, Min(0f)] private float tileExitDuration = 0.25f;
    [SerializeField, Min(0f)] private float tileExitSlideDistance = 18f;

    [Header("UI Stuff")]
    [SerializeField] private TextMeshProUGUI salitaText;
    [SerializeField] private GameObject scoreCalculationsContainer;
    [SerializeField] private TextMeshProUGUI preMultipliedScoreText;
    [SerializeField] private TextMeshProUGUI multiplierText;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource tileScoreSfxSource;
    [SerializeField] private AudioClip wrongSFX;
    [SerializeField] private AudioClip tileTickSFX;
    [SerializeField] private AudioClip correctSFX;
    private float tileScoreBasePitch = 1f;
    private readonly List<AudioSource> tileScoreSfxPool = new List<AudioSource>();
    private int nextTileScoreSfxIndex;

    [Header("Flags")]
    [SerializeField] private bool replacingTiles;
    [SerializeField] private bool scoringSalita;

    // Main Functions ----------------------------------------------------------
    private void Start()
    {
        config = GameManager.Instance.config;
        invalidWordPopupScript = FindFirstObjectByType<InvalidWordPopup>();
        aksyonCounter = AksyonCounter.Instance;
        if (successfulWordAnimationPlayer == null)
            successfulWordAnimationPlayer = FindFirstObjectByType<WordSubmissionAnimationPlayer>();

        if (tileScoreSfxSource == null)
        {
            tileScoreSfxSource = gameObject.AddComponent<AudioSource>();
            tileScoreSfxSource.playOnAwake = false;
        }

        tileScoreBasePitch = tileScoreSfxSource.pitch;
        InitializeTileScoreSfxPool();
    }

    private void Update() // Temporarily
    {
        if (replacingTiles || scoringSalita) return;

        UpdateActiveTiles();
        GetSalitaFromTiles();

        if (latinSalita != observedLatinSalita)
        {
            observedLatinSalita = latinSalita;
            revealedLatinSalita = null;
        }

        UpdateSalitaText();

        submitButton.interactable = (latinSalita != "");
    }

    // Button Functions
    public void EvaluateSalita()
    {
        if (scoringSalita || replacingTiles) return;

        UpdateActiveTiles();
        GetSalitaFromTiles();
        observedLatinSalita = latinSalita;
        revealedLatinSalita = latinSalita;
        UpdateSalitaText();

        if (!IsSalitaValid())
        {
            StartCoroutine(TempRedText());
            sfxSource.PlayOneShot(wrongSFX);
        }
        else
        {
            AlahasSubManager.Instance.onSubmit();
            StartCoroutine(ScoreSalita());
        }
    }

    public void ShowRecommendedWord(string word)
    {
        latinSalita = word ?? string.Empty;
        observedLatinSalita = latinSalita;
        revealedLatinSalita = latinSalita;
        UpdateSalitaText();
    }

    // Helper Functions --------------------------------------------------------
    private bool IsSalitaValid()
    {
        // A standalone vowel is only valid at the start of a word or after a
        // tile whose Latin text already ends in a vowel. This distinguishes
        // valid sequences such as MA-A-A-RI from shortcuts such as
        // MA-GA-N-D-A, even though both approaches can produce a valid word.
        if (!BaybayinTileSequenceRules.HasValidStandaloneVowelPlacement(activeTiles))
        {
            invalidWordPopupScript.ShowInvalidWordPopup(
                InvalidWordTypes.InvalidWordType.InvalidVowelPlacement,
                latinSalita);
            return false;
        }

        // The Baybayin NG character must be used instead of separate N and G
        // tiles, regardless of the G tile's diacritic.
        if (!BaybayinTileSequenceRules.UsesDedicatedNgTile(activeTiles))
        {
            invalidWordPopupScript.ShowInvalidWordPopup(
                InvalidWordTypes.InvalidWordType.SplitNgTiles,
                latinSalita);
            return false;
        }

        // PAGHIHIGPIT: Partikular na Salita
        // Check if the candidate salita is the particular word for that aksyon
        if (config.HasPaghihigpit(PaghihigpitTypes.PartikularNaSalita))
        {
            // Get current aksyon
            int currentAksyon = aksyonCounter?.GetCurrentAksyon() ?? 1;

            // Get the particular word for that aksyon
            string particularWord = config.GetPartikularNaSalitaForAksyon(currentAksyon);

            // Check if that particular word is not null or empty
            if (!string.IsNullOrEmpty(particularWord))
            {
                // Check if the submitted word is not the same as the particular word
                if (!string.Equals(particularWord, latinSalita))
                {
                    invalidWordPopupScript.ShowInvalidWordPopup(InvalidWordTypes.InvalidWordType.NotPartikularNaSalita, latinSalita);
                    return false;
                }
            } 
        }

        // PAGHIHIGPIT: Mahabang Salita
        // Check if the candidate salita has 3 or less tiles
        if (config.HasPaghihigpit(PaghihigpitTypes.MahabangSalita))
        {
            if (activeTiles.Count <= 3)
            {
                invalidWordPopupScript.ShowInvalidWordPopup(InvalidWordTypes.InvalidWordType.MahabangSalita, latinSalita);
                return false;
            }
        }

        // PAGHIHIGPIT: Maikkling Salita
        // Check if the candidate salita has 5 or more tiles
        if (config.HasPaghihigpit(PaghihigpitTypes.MaiklingSalita))
        {
            if (activeTiles.Count >= 5)
            {
                invalidWordPopupScript.ShowInvalidWordPopup(InvalidWordTypes.InvalidWordType.MaiklingSalita, latinSalita);
                return false;
            }
        }

        // PAGHIHIGPIT: Marka ng Baybayin
        // Require a diacritic on every consonant tile. Vowel tiles are exempt
        // because Baybayin vowels cannot receive a Kudlit or Krus.
        if (config.HasPaghihigpit(PaghihigpitTypes.MarkaNgBaybayin))
        {
            foreach (Tile tile in activeTiles)
            {
                if (tile.isVowel) continue;
                if (tile.GetCurrentCharMod() != Tile.Diacritic.None) continue;
                invalidWordPopupScript.ShowInvalidWordPopup(InvalidWordTypes.InvalidWordType.AbsentDiacritic, latinSalita);
                return false;
            }
        }

        // PAGHIHIGPIT: Bawal Umulit
        // Check if the candidate salita was already submitted
        if (GameManager.Instance.wordsUsed.Contains(latinSalita) &&
            config.HasPaghihigpit(PaghihigpitTypes.BawalUmulit))
        {
            invalidWordPopupScript.ShowInvalidWordPopup(InvalidWordTypes.InvalidWordType.AlreadyUsed, latinSalita);
            return false;
        }

        // PAGHIHIGPIT: Pag-iibang Titik
        // Check for duplicate Baybayin tiles
        if (config.HasPaghihigpit(PaghihigpitTypes.PagiibangTitik))
        {
            int numActiveTiles = activeTiles.Count;
            for (int i = 0; i < (numActiveTiles-1); i++)
            {
                for (int j = i+1; j < numActiveTiles; j++)
                {
                    string tile1 = activeTiles[i].latinText;
                    string tile2 = activeTiles[j].latinText;
                    char char1 = tile1.ToCharArray()[0];
                    char char2 = tile2.ToCharArray()[0];

                    if (char1 == char2)
                    {
                        invalidWordPopupScript.ShowInvalidWordPopup(InvalidWordTypes.InvalidWordType.NotPagiibangTitik, latinSalita);
                        print("Pagiibang Titik");
                        return false;
                    }
                }
            }
        }

        // Check if the candidate salita is in the wordlist
        if (!GameManager.Instance.validWords.Contains(latinSalita))
        {
            invalidWordPopupScript.ShowInvalidWordPopup(InvalidWordTypes.InvalidWordType.NotInWordlist, latinSalita);
            return false;
        }


        // Candidate salita is valid
        return true;
    }

    private void UpdateActiveTiles()
    {
        activeTiles.Clear();

        foreach (Transform child in transform)
        {
            Tile tile = child.GetComponent<Tile>();
            if (tile == null) continue;
            activeTiles.Add(tile);
        }
    }

    private void GetSalitaFromTiles()
    {
        latinSalita = "";
        //baybayinSalita = ""; // Eventually get the baybayin as well

        foreach (Tile activeTile in activeTiles) latinSalita += activeTile.latinText;
    }

    private IEnumerator ScoreSalita()
    {
        scoringSalita = true;
        Draggable.SetInteractionLocked(true);
        submitButton.interactable = false;
        salitaScore = 0;
        int tileScoreTotal = 0;
        float wordLengthMultiplier = 0;
        float mahikaScoreMultiplier = AlahasSubManager.Instance != null
            ? AlahasSubManager.Instance.scoreMultiplier
            : 1f;
        List<string> tileScoreTerms = new List<string>();
        preMultipliedScoreText.text = "";
        multiplierText.text = "";

        foreach (Tile activeTile in activeTiles)
            if (activeTile != null)
                activeTile.TransitionToScoredAppearance(scoredTileTransitionDuration);

        if (scoredTileTransitionDuration > 0f)
            yield return new WaitForSeconds(scoredTileTransitionDuration);
        if (delayBeforeTileScoring > 0f)
            yield return new WaitForSeconds(delayBeforeTileScoring);

        scoreCalculationsContainer.SetActive(true);

        foreach (Tile activeTile in activeTiles)
        {
            // Some cool effects here
            if (activeTile == null) continue;

            tileScoreTotal += activeTile.Score;
            tileScoreTerms.Add(GetTileScoreTerm(activeTile));
            Draggable activeDraggable = activeTile.GetComponent<Draggable>();
            activeDraggable.PlayScoreParticles();
            activeDraggable.PlayAngleJiggle();
            activeTile.PlayScorePulse(
                scorePulseDuration,
                scorePulseScale,
                scorePulseBrightness);

            AudioSource scoreSfxSource = GetAvailableTileScoreSfxSource();
            if (scoreSfxSource != null)
            {
                SyncTileScoreSfxSettings(scoreSfxSource);
                int pitchStepIndex = tileScoreTerms.Count - 1;
                scoreSfxSource.pitch = Mathf.Clamp(
                    tileScoreBasePitch * Mathf.Pow(
                        2f,
                        tileTickSemitoneStep * pitchStepIndex / 12f),
                    0.01f,
                    3f);
                scoreSfxSource.PlayOneShot(tileTickSFX);
            }

            wordLengthMultiplier += scoreScaleValue;

            preMultipliedScoreText.text = BuildScoreCalculationText(
                tileScoreTerms,
                wordLengthMultiplier,
                mahikaScoreMultiplier);
            multiplierText.text = "";

            salitaText.color = Color.green;

            yield return new WaitForSeconds(scoreTileDelay);
        }

        yield return new WaitForSeconds(0.25f);

        salitaScore = (int)(tileScoreTotal * wordLengthMultiplier * mahikaScoreMultiplier);
        preMultipliedScoreText.text = BuildFinalScoreCalculationText(
            tileScoreTerms,
            wordLengthMultiplier,
            mahikaScoreMultiplier,
            salitaScore);
        MahikaManager.Instance.UpdateMahika(salitaScore);

        sfxSource.PlayOneShot(correctSFX);
        if (BackgroundsManager.Instance != null) BackgroundsManager.Instance.AdjustCorruptedBG();
        GameManager.Instance.wordsUsed.Add(latinSalita);

        yield return new WaitForSeconds(finalScoreDisplayDelay);
        scoreCalculationsContainer.SetActive(false);

        Coroutine winningAttackSlowMotion = null;
        Coroutine winningAttackCharacterSwap = null;
        bool isWinningAttack = MahikaManager.Instance.DidWin() &&
            successfulWordAnimationPlayer != null &&
            successfulWordAnimationPlayer.AnimationAfterSuccessfulWord ==
                WordSubmissionAnimationPlayer.SuccessfulWordAnimation.Attack;

        if (isWinningAttack && GameManager.Instance != null)
        {
            winningAttackSlowMotion = GameManager.Instance.StartWinningAttackSlowMotion();
            winningAttackCharacterSwap = GameManager.Instance.StartWinningAttackCharacterSwap();
        }

        if (successfulWordAnimationPlayer != null)
            yield return successfulWordAnimationPlayer.PlaySelectedAnimation();

        if (winningAttackSlowMotion != null)
            yield return winningAttackSlowMotion;

        if (winningAttackCharacterSwap != null)
            yield return winningAttackCharacterSwap;

        AksyonCounter.Instance.ConcludeAksyon();

        if (AksyonCounter.Instance.HasRemainingAksyon() && MahikaManager.Instance.GetMahikaPercent() < 1) 
        {
            AlahasSubManager.Instance.onTurnEnd();
            yield return ReplaceActiveTiles();

            // Itinakdang Titik can intentionally wait for a dialogue before
            // spawning the next pool. Keep tiles locked until that pool exists.
            while (tileSet.WaitingForDialogueBeforeCurrentAksyonTiles)
                yield return null;

            scoringSalita = false;
            Draggable.SetInteractionLocked(false);
        }
        else
        {
            scoringSalita = false;
            GameManager.Instance.EndRound();
        }
    }

    private static string GetTileScoreTerm(Tile tile)
    {
        string score = tile.UnmultipliedScore.ToString(CultureInfo.InvariantCulture);
        if (Mathf.Approximately(tile.scoreMultiplier, 1f)) return score;

        return $"{score} (x{FormatMultiplier(tile.scoreMultiplier)})";
    }

    private static string BuildMultiplierText(
        float wordLengthMultiplier,
        float mahikaScoreMultiplier)
    {
        string text = FormatMultiplier(wordLengthMultiplier);
        if (!Mathf.Approximately(mahikaScoreMultiplier, 1f))
            text += $" x {FormatMultiplier(mahikaScoreMultiplier)}";

        return text;
    }

    private static string BuildScoreCalculationText(
        List<string> tileScoreTerms,
        float wordLengthMultiplier,
        float mahikaScoreMultiplier)
    {
        return $"({string.Join(" + ", tileScoreTerms)}) x "
            + BuildMultiplierText(wordLengthMultiplier, mahikaScoreMultiplier);
    }

    private static string BuildFinalScoreCalculationText(
        List<string> tileScoreTerms,
        float wordLengthMultiplier,
        float mahikaScoreMultiplier,
        int finalScore)
    {
        return BuildScoreCalculationText(
                tileScoreTerms,
                wordLengthMultiplier,
                mahikaScoreMultiplier)
            + $" = <size=+8>{finalScore.ToString(CultureInfo.InvariantCulture)}</size>";
    }

    private static string FormatMultiplier(float multiplier)
    {
        return multiplier.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private void InitializeTileScoreSfxPool()
    {
        tileScoreSfxPool.Clear();
        tileScoreSfxPool.Add(tileScoreSfxSource);

        int initialPoolSize = 1;
        if (tileTickSFX != null && scoreTileDelay > 0f)
        {
            initialPoolSize = Mathf.CeilToInt(tileTickSFX.length / scoreTileDelay) + 1;
        }

        while (tileScoreSfxPool.Count < initialPoolSize)
            AddTileScoreSfxSourceToPool();

        foreach (AudioSource source in tileScoreSfxPool)
            SyncTileScoreSfxSettings(source);
    }

    private AudioSource GetAvailableTileScoreSfxSource()
    {
        if (tileScoreSfxPool.Count == 0) return null;

        for (int offset = 0; offset < tileScoreSfxPool.Count; offset++)
        {
            int sourceIndex = (nextTileScoreSfxIndex + offset) % tileScoreSfxPool.Count;
            AudioSource source = tileScoreSfxPool[sourceIndex];
            if (source == null || source.isPlaying) continue;

            nextTileScoreSfxIndex = (sourceIndex + 1) % tileScoreSfxPool.Count;
            return source;
        }

        AudioSource addedSource = AddTileScoreSfxSourceToPool();
        nextTileScoreSfxIndex = 0;
        return addedSource;
    }

    private AudioSource AddTileScoreSfxSourceToPool()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.pitch = tileScoreBasePitch;
        tileScoreSfxPool.Add(source);
        SyncTileScoreSfxSettings(source);
        return source;
    }

    private void SyncTileScoreSfxSettings(AudioSource tileScoreSource)
    {
        if (sfxSource == null || tileScoreSource == null) return;

        tileScoreSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
        tileScoreSource.volume = sfxSource.volume;
        tileScoreSource.mute = sfxSource.mute;
        tileScoreSource.ignoreListenerPause = sfxSource.ignoreListenerPause;
        tileScoreSource.ignoreListenerVolume = sfxSource.ignoreListenerVolume;
    }

    private void UpdateSalitaText()
    {
        salitaText.text = latinSalita == revealedLatinSalita ? latinSalita : "";
        // Separate function because it may get complicated with i/e and o/u conversion
    }

    private IEnumerator TempRedText() // Temporary Coroutine
    {
        salitaText.color = Color.red; // Eventually make this play animation
        yield return new WaitForSeconds(1f);
        salitaText.color = Color.white; // Eventually make this play animation
    }

    private IEnumerator ReplaceActiveTiles()
    {
        replacingTiles = true;
        revealedLatinSalita = null;
        observedLatinSalita = null;
        yield return tileSet.RemovePakpakTemporaryTiles();
        if (tileExitDelay > 0f)
            yield return new WaitForSeconds(tileExitDelay);

        int submittedNormalTileCount = 0;

        foreach (Tile activeTile in activeTiles)
        {
            if (activeTile == null) continue;
            if (!activeTile.isTemp) submittedNormalTileCount++;
            activeTile.PlaySubmitExitAnimation(
                tileExitDuration,
                tileExitSlideDistance);
        }

        if (tileExitDuration > 0f)
            yield return new WaitForSeconds(tileExitDuration);

        foreach (Tile activeTile in activeTiles)
        {
            if (activeTile == null) continue;
            Destroy(activeTile.gameObject);
        }

        // Let every submitted tile leave the hierarchy together before the
        // replacement pool begins to populate the layouts.
        yield return null;

        if (config.HasPaghihigpit(PaghihigpitTypes.ItinakdangTitik))
        {
            tileSet.ConsumePakpakNormalTilesRemoved();

            if (tileSet.WaitingForDialogueBeforeCurrentAksyonTiles)
                yield return tileSet.ClearTiles();
            else
            {
                yield return tileSet.ReplaceWithCurrentAksyonTiles();
                yield return tileSet.SpawnPakpakTemporaryTiles();
            }
        }
        else
        {
            int normalTilesToReplenish = submittedNormalTileCount
                + tileSet.ConsumePakpakNormalTilesRemoved();

            yield return tileSet.SpawnTiles(normalTilesToReplenish);
            yield return tileSet.SpawnPakpakTemporaryTiles();
        }

        salitaText.color = Color.white; // Eventually make this play animation
        replacingTiles = false;
    }
}
