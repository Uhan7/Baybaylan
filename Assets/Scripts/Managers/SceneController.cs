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
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwapWrapper("Alahas 2");
    }

    // Helper Functions --------------------------------------------------------
    public void SwapWrapper(string sceneName) // Called by buttons n stuff
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