using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Transition")]
    [SerializeField] private GameObject transitionOnStart;
    [SerializeField] private GameObject transitionOnSwap;
    [SerializeField] private float transitionTime = 1.25f;

    [Header("Alahas Selection")]
    [Tooltip("Disable this in intro/cutscene scenes. When disabled, Alahas-selection requests are ignored and scene swaps continue normally.")]
    [SerializeField] private bool allowAlahasSelection = true;
    [SerializeField] private AlahasSelectionController alahasSelectionPrefab;

    public bool AllowsAlahasSelection => allowAlahasSelection;
    private bool exitingScene;
    private static bool fallbackSceneSwapPending;

    // Main Functions ----------------------------------------------------------
    private void Start()
    {
        // A scene change should never inherit temporary winning-attack slow motion.
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (transitionOnStart != null)
        {
            transitionOnStart.SetActive(true);
            transitionOnStart.GetComponent<ImageFader>().SetAlpha(1);
            transitionOnStart.GetComponent<ImageFader>().FadeTo(0, transitionTime);
        }
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Alpha1)) SwapWrapper("Game Scene");
        // if (Input.GetKeyDown(KeyCode.Alpha2)) SwapWrapper("Alahas 2");
    }

    // Helper Functions --------------------------------------------------------
    public void SwapWrapper(string sceneName) // Called by buttons n stuff
    {
        StartSceneSwap(sceneName);
    }

    public void CompleteLevel(string sceneName)
    {
        SaveManager.SaveDataNames nextLevelId = GameManager.Instance.saveNextLevelID;

        // A final level has nothing left to unlock, so honor its configured exit.
        if (nextLevelId == SaveManager.SaveDataNames.NoneOrNull)
        {
            StartSceneSwap(sceneName);
            return;
        }

        bool isFirstClear = SaveManager.Instance.getSaveData<int>(nextLevelId) == 0;
        if (isFirstClear)
        {
            SaveManager.Instance.saveData(nextLevelId, 1, true);
            StartSceneSwap(sceneName);
            return;
        }

        StartSceneSwap("Level Selection");
    }

    private void StartSceneSwap(string sceneName)
    {
        if (exitingScene) return;
        exitingScene = true;
        PopupUI.HideSettingsForSceneChange();
        Coroutine audioFade = AudioManager.Instance?.FadeOutSceneAudio();
        if (transitionOnSwap != null)
        {
            transitionOnSwap.SetActive(true);
            transitionOnSwap.GetComponent<ImageFader>().SetAlpha(0);
            transitionOnSwap.GetComponent<ImageFader>().FadeTo(1, transitionTime);
        }

        Debug.Log($"Active Self: {gameObject.activeSelf}, Active In Hierarchy: {gameObject.activeInHierarchy}");

        StartCoroutine(Swap(sceneName, audioFade));
    }

    public static void LoadSceneWithAudioFade(string sceneName)
    {
        SceneController controller = FindFirstObjectByType<SceneController>();
        if (controller != null)
        {
            controller.SwapWrapper(sceneName);
            return;
        }

        if (AudioManager.Instance == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        if (fallbackSceneSwapPending) return;
        fallbackSceneSwapPending = true;
        AudioManager.Instance.StartCoroutine(FadeThenLoadScene(sceneName));
    }

    private static IEnumerator FadeThenLoadScene(string sceneName)
    {
        yield return AudioManager.Instance.FadeOutSceneAudio();
        SceneManager.LoadScene(sceneName);
        fallbackSceneSwapPending = false;
    }

    public void titleStartButton()
    {
        StartSceneSwap(SaveManager.Instance.GetContinueScene());
    }

    public void SwapAfterAlahasSelection(string sceneName)
    {
        if (!allowAlahasSelection ||
            !AlahasSelectionController.IsSelectionAllowedInActiveScene())
        {
            Debug.Log(
                $"[Alahas Selection] Ignored SceneController.SwapAfterAlahasSelection " +
                $"in '{SceneManager.GetActiveScene().name}' because selection is disabled for this scene. " +
                $"Continuing normal transition to '{sceneName}'.",
                this);
            SwapWrapper(sceneName);
            return;
        }

        AlahasSelectionController selector = FindFirstObjectByType<AlahasSelectionController>(
            FindObjectsInactive.Include);

        Debug.Log(
            $"[Alahas Selection] OPEN requested by the " +
            $"SceneController.SwapAfterAlahasSelection UnityEvent for '{sceneName}'. " +
            $"Existing selector: {selector != null}; fallback prefab: {alahasSelectionPrefab != null}.",
            this);

        if (selector == null && alahasSelectionPrefab != null)
        {
            Canvas parentCanvas = null;
            Canvas[] canvases = FindObjectsByType<Canvas>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (Canvas candidate in canvases)
            {
                if (!candidate.gameObject.scene.IsValid() || candidate.rootCanvas != candidate)
                    continue;

                parentCanvas = candidate;
                if (candidate.gameObject.activeInHierarchy) break;
            }

            selector = parentCanvas != null
                ? Instantiate(alahasSelectionPrefab, parentCanvas.transform)
                : Instantiate(alahasSelectionPrefab);
        }

        if (selector != null && selector.BeginSelectionBeforeSceneSwap(this, sceneName))
            return;

        Debug.LogWarning("Alahas scene gate could not start; continuing with the scene swap.", this);

        SwapWrapper(sceneName);
    }

    private IEnumerator Swap(string sceneName, Coroutine audioFade)
    {
        yield return new WaitForSecondsRealtime(transitionTime);
        if (audioFade != null) yield return audioFade;
        SceneManager.LoadScene(sceneName);
    }

    public void Reload()
    {
        SwapWrapper(SceneManager.GetActiveScene().name);
    }

    public void QuitWrapper()
    {
        if (exitingScene) return;
        exitingScene = true;
        Coroutine audioFade = AudioManager.Instance?.FadeOutSceneAudio();
        if (transitionOnSwap != null)
        {
            transitionOnSwap.SetActive(true);
            transitionOnSwap.GetComponent<ImageFader>().SetAlpha(0);
            transitionOnSwap.GetComponent<ImageFader>().FadeTo(1, transitionTime);
        }

        StartCoroutine(Quit(audioFade));
    }

    private IEnumerator Quit(Coroutine audioFade)
    {
        yield return new WaitForSecondsRealtime(transitionTime);
        if (audioFade != null) yield return audioFade;
        Application.Quit();
        Debug.LogError("You quit the game! Paalam!");
    }
}
