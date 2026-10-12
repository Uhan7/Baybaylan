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

    // Main Functions ----------------------------------------------------------
    private void Start()
    {
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
        if (nextLevelId != SaveManager.SaveDataNames.NoneOrNull &&
            SaveManager.Instance.getSaveData<int>(nextLevelId) == 0)
        {
            SaveManager.Instance.saveData(nextLevelId, 1, true);
        }

        StartSceneSwap(sceneName);
    }

    private void StartSceneSwap(string sceneName)
    {
        if (transitionOnSwap != null)
        {
            transitionOnSwap.SetActive(true);
            transitionOnSwap.GetComponent<ImageFader>().SetAlpha(0);
            transitionOnSwap.GetComponent<ImageFader>().FadeTo(1, transitionTime);
        }

        Debug.Log($"Active Self: {gameObject.activeSelf}, Active In Hierarchy: {gameObject.activeInHierarchy}");

        StartCoroutine(Swap(sceneName));
    }

    public void titleStartButton()
    {
        if (transitionOnSwap != null)
        {
            transitionOnSwap.SetActive(true);
            transitionOnSwap.GetComponent<ImageFader>().SetAlpha(0);
            transitionOnSwap.GetComponent<ImageFader>().FadeTo(1, transitionTime);
        }

        Debug.Log($"Active Self: {gameObject.activeSelf}, Active In Hierarchy: {gameObject.activeInHierarchy}");

        StartCoroutine(Swap("Area 0 - Intro"));
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

    private IEnumerator Swap(string sceneName)
    {
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(sceneName);
    }

    public void Reload()
    {
        SwapWrapper(SceneManager.GetActiveScene().name);
    }

    public void QuitWrapper()
    {
        if (transitionOnSwap != null)
        {
            transitionOnSwap.SetActive(true);
            transitionOnSwap.GetComponent<ImageFader>().SetAlpha(0);
            transitionOnSwap.GetComponent<ImageFader>().FadeTo(1, transitionTime);
        }

        StartCoroutine(Quit());
    }

    private IEnumerator Quit()
    {
        yield return new WaitForSeconds(transitionTime);
        Application.Quit();
        Debug.LogError("You quit the game! Paalam!");
    }
}
