using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Draggable))]
public class Tile : MonoBehaviour
{

    // Setups... put this in an enums manager
    public enum Diacritic
    {
        None,
        Top,
        Bottom,
        Krus
    }

    // Variables ---------------------------------------------------------------
    [Header("Components")]
    [HideInInspector] private Draggable draggableScript;
    [HideInInspector] private Image imageComponent;
    [HideInInspector] private CanvasGroup canvasGroup;
    [HideInInspector] private RectTransform rectTransform;

    [Header("Diacritics")]
    [SerializeField] private GameObject topKudlit;
    [SerializeField] private GameObject bottomKudlit;
    [SerializeField] private GameObject krus;

    [Header("Current State")]
    [SerializeField] private Diacritic currentCharmod = Diacritic.None;

    [Header("Animator")]
    [SerializeField] private Animator m_animator = null;

    [Header("Audio")]
    [HideInInspector] public AudioSource sfxSource; // To be set by spawners (TileSet.cs)
    [SerializeField] private AudioClip diacriticSFX;

    [Header("Tile Info")]
    [SerializeField] public bool isVowel; // Read in TileSet.cs
    [HideIf("isVowel"), SerializeField] public string rootConsonant; // Read in TileSet.cs
    [ShowIf("isVowel"), SerializeField] public string vowel; // Read in TileSet.cs
    [ReadOnly, SerializeField] public string latinText; // Used in TileSlot.cs

    [Header("Default Visuals")]
    [SerializeField] private Sprite[] availableTileSprites;
    [SerializeField] private Color availableTileColor;
    [SerializeField] private Sprite[] activeTileSprites;
    [SerializeField] private Color activeTileColor;
    [SerializeField] private GameObject[] strokes;

    [Header("Modified Tile Visuals")]
    [SerializeField] private Color availableGoldenStrokeColor;
    [SerializeField] private Color activeGoldenStrokeColor;
    [SerializeField] private GameObject vowelBoostedSymbol;

    [Header("Score Info")]
    [SerializeField] private int baseScore = 10;
    [HideIf("isVowel"), ReadOnly, SerializeField] private int diacriticScore = 0;
    [ReadOnly, SerializeField] public float scoreMultiplier = 1;
    [HideInInspector] public int UnmultipliedScore => baseScore + diacriticScore;
    [HideInInspector] public int Score => Mathf.RoundToInt((baseScore + diacriticScore) * scoreMultiplier); // Used in SalitaSlots.cs

    [Header("Other Tile Info")]
    [SerializeField] private int chance = 5;

    [Header("Flags")]
    [HideInInspector] private bool wasBeingDragged;
    [ReadOnly, SerializeField] private bool isActiveTile;
    [SerializeField] public bool isGold;
    [SerializeField] public bool isVowelBoosted;
    [SerializeField] public bool isPearl;
    [SerializeField] public bool isShy;
    [SerializeField] public bool isBlossom;
    [SerializeField] public bool isToolTipped;
    [SerializeField] public bool isTemp; // from the PakpakNiPahAlahas

    ToolTipAble tooltipScript;
    private readonly List<Graphic> pakpakFadeGraphics = new List<Graphic>();
    private readonly List<Color> pakpakFadeStartColors = new List<Color>();
    private bool isPakpakFading;
    private float pakpakFadeProgress;
    private Coroutine appearanceTransitionRoutine;
    private Coroutine scorePulseRoutine;
    private Graphic[] scorePulseGraphics;
    private Color[] scorePulseBaseColors;
    private Vector3 scorePulseBaseScale;
    private float scorePulseIntensity;
    private float scorePulseScaleMultiplier = 1f;
    private float scorePulseBrightness;
    private bool scorePulseActive;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        draggableScript = GetComponent<Draggable>();
        imageComponent = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        tooltipScript = GetComponentInChildren<ToolTipAble>();
        if(null == m_animator) m_animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // Tiles always start as inactive/available
        ChangeSprite(false);

        currentCharmod = Diacritic.None;
        ToggleCharmodObject();
        if (isVowel)
        {
            latinText = vowel;
            diacriticScore = 0;
        }
        else latinText = rootConsonant + "a";

        //ResetTileModifications();

        //AppldChance();
        //if (isVowel) ApplyVowelBoost();
    }

    private void Update()
    {
        ChangeSpriteOnDrag();
        updateLatinTooltipText();
        applyModVisuals();
        ApplyPakpakFadeVisuals();
    }

    private void LateUpdate()
    {
        if (!scorePulseActive) return;

        rectTransform.localScale = scorePulseBaseScale * Mathf.Lerp(
            1f,
            scorePulseScaleMultiplier,
            scorePulseIntensity);

        Color highlight = new Color(1f, 0.92f, 0.55f, 1f);
        for (int i = 0; i < scorePulseGraphics.Length; i++)
        {
            Graphic graphic = scorePulseGraphics[i];
            if (graphic == null) continue;

            Color baseColor = scorePulseBaseColors[i];
            Color brightColor = new Color(
                highlight.r,
                highlight.g,
                highlight.b,
                baseColor.a);
            graphic.color = Color.Lerp(
                baseColor,
                brightColor,
                scorePulseBrightness * scorePulseIntensity);
        }
    }

    private void OnDisable()
    {
        if (appearanceTransitionRoutine != null) StopCoroutine(appearanceTransitionRoutine);
        if (scorePulseRoutine != null) StopCoroutine(scorePulseRoutine);

        appearanceTransitionRoutine = null;
        scorePulseRoutine = null;
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        ResetScorePulse();
    }

    // Helper Functions --------------------------------------------------------
    public void ToggleNextModification() // Called by Button | PLEASE CHANGE NAME TO BE "DIACRITIC"
    {
        if (isVowel) return; // Skip if vowel
        if (Draggable.InteractionLocked) return;
        if (draggableScript.isBeingDragged) return;

        if (currentCharmod == Diacritic.Krus) currentCharmod = Diacritic.None;
        else currentCharmod++;

        sfxSource.PlayOneShot(diacriticSFX);
        ToggleCharmodObject();
    }

    private void ToggleCharmodObject()
    {
        if (isVowel) Debug.LogWarning("ToggleCharmodObject called on a vowel.");

        ClearAllCharmods();

        switch (currentCharmod)
        {
            case Diacritic.None:
                diacriticScore = 0;
                latinText = rootConsonant + "a";
                break;

            case Diacritic.Top:
                diacriticScore = 5;
                topKudlit.SetActive(true);
                latinText = rootConsonant + "i";
                break;

            case Diacritic.Bottom:
                diacriticScore = 10;
                bottomKudlit.SetActive(true);
                latinText = rootConsonant + "u";
                break;

            case Diacritic.Krus:
                diacriticScore = 8;
                krus.SetActive(true);
                latinText = rootConsonant;
                break;

            default:
                Debug.LogError("ActivateModification p_currentModification incorrect enum.");
                break;
        }
    }

    private void ClearAllCharmods()
    {
        topKudlit.SetActive(false);
        bottomKudlit.SetActive(false);
        krus.SetActive(false);
    }

    public int GetChance()
    {
        return chance;
    }

    public void ChangeSprite(bool active) // Can be called by the DropZone obj
    {
        isActiveTile = active;

        if (active)
        {
            imageComponent.sprite = activeTileSprites[Random.Range(0, activeTileSprites.Length)];
        }
        else
        {
            imageComponent.sprite = availableTileSprites[Random.Range(0, availableTileSprites.Length)];
        }

        ApplyStrokeColor();
    }

    public void TransitionToScoredAppearance(float duration)
    {
        if (appearanceTransitionRoutine != null)
            StopCoroutine(appearanceTransitionRoutine);

        if (duration <= 0f)
        {
            ChangeSprite(true);
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            appearanceTransitionRoutine = null;
            return;
        }

        appearanceTransitionRoutine = StartCoroutine(
            FadeToScoredAppearance(duration));
    }

    public void PlayScorePulse(float duration, float scaleMultiplier, float brightness)
    {
        if (scorePulseRoutine != null)
        {
            StopCoroutine(scorePulseRoutine);
            ResetScorePulse();
        }

        if (duration <= 0f) return;

        scorePulseGraphics = GetComponentsInChildren<Graphic>(true);
        scorePulseBaseColors = new Color[scorePulseGraphics.Length];
        for (int i = 0; i < scorePulseGraphics.Length; i++)
            scorePulseBaseColors[i] = scorePulseGraphics[i].color;

        scorePulseBaseScale = rectTransform.localScale;
        scorePulseScaleMultiplier = Mathf.Max(1f, scaleMultiplier);
        scorePulseBrightness = Mathf.Clamp01(brightness);
        scorePulseActive = true;
        scorePulseRoutine = StartCoroutine(AnimateScorePulse(duration));
    }

    private IEnumerator FadeToScoredAppearance(float duration)
    {
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        float halfDuration = duration * 0.5f;

        yield return FadeCanvasGroup(startAlpha, 0f, halfDuration);
        ChangeSprite(true);
        yield return FadeCanvasGroup(0f, startAlpha, halfDuration);

        appearanceTransitionRoutine = null;
    }

    private IEnumerator FadeCanvasGroup(float from, float to, float duration)
    {
        if (canvasGroup == null || duration <= 0f)
        {
            if (canvasGroup != null) canvasGroup.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(from, to, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private IEnumerator AnimateScorePulse(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            scorePulseIntensity = Mathf.Sin(progress * Mathf.PI);
            elapsed += Time.deltaTime;
            yield return null;
        }

        ResetScorePulse();
        scorePulseRoutine = null;
    }

    private void ResetScorePulse()
    {
        if (!scorePulseActive) return;

        if (rectTransform != null) rectTransform.localScale = scorePulseBaseScale;
        if (scorePulseGraphics != null && scorePulseBaseColors != null)
        {
            for (int i = 0; i < scorePulseGraphics.Length; i++)
            {
                if (scorePulseGraphics[i] != null)
                    scorePulseGraphics[i].color = scorePulseBaseColors[i];
            }
        }

        scorePulseIntensity = 0f;
        scorePulseActive = false;
    }

    private void ChangeSpriteOnDrag()
    {
        if (draggableScript.isBeingDragged && !wasBeingDragged) ChangeSprite(false);

        wasBeingDragged = draggableScript.isBeingDragged;
    }

    // Tile Modifications ------------------------------------------------------

    void applyModVisuals()
    {
        ApplyStrokeColor();

        if(isVowelBoosted)
            vowelBoostedSymbol.SetActive(true);
        else
            vowelBoostedSymbol.SetActive(false);
    }

    public void BeginPakpakFade()
    {
        pakpakFadeGraphics.Clear();
        pakpakFadeStartColors.Clear();

        foreach (Graphic graphic in GetComponentsInChildren<Graphic>(true))
        {
            pakpakFadeGraphics.Add(graphic);
            pakpakFadeStartColors.Add(graphic.color);
        }

        pakpakFadeProgress = 0f;
        isPakpakFading = true;
    }

    public void SetPakpakFadeProgress(float progress)
    {
        pakpakFadeProgress = Mathf.Clamp01(progress);
        ApplyPakpakFadeVisuals();
    }

    public void CancelPakpakFade()
    {
        for (int i = 0; i < pakpakFadeGraphics.Count; i++)
        {
            if (pakpakFadeGraphics[i] != null)
                pakpakFadeGraphics[i].color = pakpakFadeStartColors[i];
        }

        pakpakFadeGraphics.Clear();
        pakpakFadeStartColors.Clear();
        pakpakFadeProgress = 0f;
        isPakpakFading = false;
    }

    private void ApplyPakpakFadeVisuals()
    {
        if (!isPakpakFading) return;

        Color fadeTarget = new Color(0f, 0f, 0f, 0f);
        for (int i = 0; i < pakpakFadeGraphics.Count; i++)
        {
            if (pakpakFadeGraphics[i] != null)
                pakpakFadeGraphics[i].color = Color.Lerp(
                    pakpakFadeStartColors[i],
                    fadeTarget,
                    pakpakFadeProgress);
        }
    }

    private void ApplyStrokeColor()
    {
        Color strokeColor;

        if (isGold)
            strokeColor = isActiveTile ? activeGoldenStrokeColor : availableGoldenStrokeColor;
        else
            strokeColor = isActiveTile ? activeTileColor : availableTileColor;

        foreach (GameObject stroke in strokes)
            stroke.GetComponent<Image>().color = strokeColor;
    }

    void updateLatinTooltipText()
    {
        if(isToolTipped)
        {
            tooltipScript.tipText = latinText;
        }
        else
            tooltipScript.enabled = false;
    }

    // private void ApplyGoldChance()
    // {
    //     float goldChance = AlahasSubManager.Instance.goldSpawnChance;

    //     if (Random.value <= goldChance)
    //     {
    //         isGold = true;
    //         scoreMultiplier *= AlahasManager.Instance.goldenTileMultiplier;

    //         foreach (GameObject stroke in strokes) stroke.GetComponent<Image>().color = availableGoldenStrokeColor;
    //     }
    // }

    // private void ApplyVowelBoost()
    // {
    //     if (!AlahasManager.Instance.boostVowels)
    //     {
    //         vowelBoostedSymbol.SetActive(false);
    //         return;
    //     }

    //     scoreMultiplier *= AlahasManager.Instance.vowelScoreMultiplier;
    //     chance *= (int) AlahasManager.Instance.vowelChanceMultiplier; // I have to remove it here...?
    //     vowelBoostedSymbol.SetActive(true);
    // }

    private void ResetTileModifications()
    {
        scoreMultiplier = 1;
        vowelBoostedSymbol.SetActive(false);
    }

    public Tile.Diacritic GetCurrentCharMod()
    {
        if (isVowel) return Tile.Diacritic.None;
        else return currentCharmod;
    }

    public void PlayBlowAwayAnimation()
    {
        if (null == m_animator) return;
        m_animator.SetBool("Blow", true);
    }
}
