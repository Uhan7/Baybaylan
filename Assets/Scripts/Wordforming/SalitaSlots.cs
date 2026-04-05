using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using NaughtyAttributes;

[RequireComponent(typeof(DropZone))]
public class SalitaSlots : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Configurations")]
    [HideInInspector] private LevelConfig config;

    [Header("Reference")]
    [SerializeField] private Button submitButton;

    [Header("Tiles")]
    [SerializeField] private TileSet tileSet;
    [SerializeField] private List<Tile> activeTiles = new List<Tile>();

    [Header("Word Properties")]
    [ReadOnly, SerializeField] private string baybayinSalita;
    [ReadOnly, SerializeField] private string latinSalita;

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
    private void Awake()
    {
        config = GameManager.Instance.config;
    }

    private void Update() // Temporarily
    {
        if (replacingTiles || scoringSalita) return;

        UpdateActiveTiles();
        GetSalitaFromTiles();
        UpdateSalitaText();

        submitButton.interactable = (latinSalita != "");
    }

    // Button Functions
    public void EvaluateSalita()
    {
        UpdateActiveTiles();
        GetSalitaFromTiles();
        UpdateSalitaText();

        if (!IsSalitaValid())
        {
            StartCoroutine(TempRedText());
            sfxSource.PlayOneShot(wrongSFX);
        }
        else
        {
            StartCoroutine(ScoreSalita());
        }
    }

    // Helper Functions --------------------------------------------------------
    private bool IsSalitaValid()
    {
        // Note that words may be "spelled" same but still be incorrect if it won't appear in the baybayin wordlist
        // Because Ako can be spelled A-K-O (instead of A-Ko) which is 3 characters,,,, thas wrong since Baybayin emphasizes "syllables"

        // This means eventually... we'll probably use comparisons based on the Baybayin-ized wordlist instead

        if (GameManager.Instance.wordsUsed.Contains(latinSalita) && config.bawalUmulit) return false;

        if (GameManager.Instance.validWords.Contains(latinSalita)) return true;
        else return false;
    }

    private void UpdateActiveTiles()
    {
        activeTiles.Clear();

        foreach (Transform child in transform)
        {
            Tile tile = child.GetComponent<Tile>();
            if (tile != null) activeTiles.Add(tile);
        }
    }

    private void GetSalitaFromTiles()
    {
        latinSalita = "";
        baybayinSalita = ""; // Eventually get the baybayin as well

        foreach (Tile activeTile in activeTiles) latinSalita += activeTile.latinText;
    }

    private IEnumerator ScoreSalita()
    {
        scoringSalita = true;
        salitaScore = 0;
        float activeTileCount = 0;
        preMultipliedScoreText.text = "";
        multiplierText.text = "";

        scoreCalculationsContainer.SetActive(true);

        foreach (Tile activeTile in activeTiles)
        {
            // Some cool effects here
            if (activeTile == null) continue;

            salitaScore += (int) (activeTile.Score * scoreScaleValue);
            activeTile.GetComponent<Animator>().Play("tile_hold");
            activeTile.sfxSource.PlayOneShot(tileTickSFX);

            activeTileCount += 1 * scoreScaleValue;

            preMultipliedScoreText.text = salitaScore.ToString();
            multiplierText.text = activeTileCount.ToString();

            salitaText.color = Color.green;

            yield return new WaitForSeconds(0.25f);
        }

        yield return new WaitForSeconds(0.25f);

        salitaScore = (int)(salitaScore * activeTileCount);
        GameManager.Instance.ChangeMahika(salitaScore);

        sfxSource.PlayOneShot(correctSFX);
        BackgroundsManager.Instance.AdjustCorruptedBG();
        GameManager.Instance.wordsUsed.Add(latinSalita);
        AksyonCounter.Instance.ConcludeAksyon();

        yield return new WaitForSeconds(0.25f);
        scoreCalculationsContainer.SetActive(false);

        scoringSalita = false;

        if (AksyonCounter.Instance.HasRemainingAksyon() && GameManager.Instance.mahikaPercent < 1) StartCoroutine(ReplaceActiveTiles());
        else GameManager.Instance.EndRound();
    }

    private void UpdateSalitaText()
    {
        salitaText.text = latinSalita;
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
        yield return new WaitForSeconds(1f);

        foreach (Tile activeTile in activeTiles)
        {
            if (activeTile == null) continue;
            Destroy(activeTile.gameObject);
            yield return new WaitForSeconds(0.2f);
        }

        StartCoroutine(tileSet.SpawnTiles(activeTiles.Count));

        salitaText.color = Color.white; // Eventually make this play animation
        replacingTiles = false;
    }
}
