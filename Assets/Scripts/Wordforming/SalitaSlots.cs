using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
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

    [Header("UI Stuff")]
    [SerializeField] private TextMeshProUGUI salitaText;
    [SerializeField] private GameObject scoreCalculationsContainer;
    [SerializeField] private TextMeshProUGUI preMultipliedScoreText;
    [SerializeField] private TextMeshProUGUI multiplierText;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip wrongSFX;
    [SerializeField] private AudioClip tileTickSFX;
    [SerializeField] private AudioClip correctSFX;

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
        // Note that words may be "spelled" same but still be incorrect if it won't appear in the baybayin wordlist
        // Because Ako can be spelled A-K-O (instead of A-Ko) which is 3 characters,,,, thas wrong since Baybayin emphasizes "syllables"

        // This means eventually... we'll probably use comparisons based on the Baybayin-ized wordlist instead

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
        float activeTileCount = 0;
        preMultipliedScoreText.text = "";
        multiplierText.text = "";

        scoreCalculationsContainer.SetActive(true);

        foreach (Tile activeTile in activeTiles)
        {
            // Some cool effects here
            if (activeTile == null) continue;

            // i just slapped on the alahas' score multiplier on here
            salitaScore += (int) (activeTile.Score * AlahasSubManager.Instance.scoreMultiplier); // removed * scoreScaleValue here... pls find way to make it cleaner
            activeTile.GetComponent<Animator>().Play("tile_hold");
            activeTile.sfxSource.PlayOneShot(tileTickSFX);

            activeTileCount += 1 * scoreScaleValue; // messy bruh... we have a separate value for scale and not base on just count

            preMultipliedScoreText.text = salitaScore.ToString();
            multiplierText.text = activeTileCount.ToString();

            salitaText.color = Color.green;

            yield return new WaitForSeconds(0.25f);
        }

        yield return new WaitForSeconds(0.25f);

        salitaScore = (int)(salitaScore * activeTileCount);
        MahikaManager.Instance.UpdateMahika(salitaScore);

        sfxSource.PlayOneShot(correctSFX);
        if (BackgroundsManager.Instance != null) BackgroundsManager.Instance.AdjustCorruptedBG();
        GameManager.Instance.wordsUsed.Add(latinSalita);

        yield return new WaitForSeconds(0.25f);
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
        yield return new WaitForSeconds(1f);

        int submittedNormalTileCount = 0;

        foreach (Tile activeTile in activeTiles)
        {
            if (activeTile == null) continue;
            if (!activeTile.isTemp) submittedNormalTileCount++;
            Destroy(activeTile.gameObject);
            yield return new WaitForSeconds(0.2f);
        }

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
