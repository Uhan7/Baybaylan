using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
public class LevelSelectManager : MonoBehaviour
{
    [SerializeField] private GameObject levelScreenHolder;
    [SerializeField, Min(0f)] private float moveDuration = 0.3f;
    [SerializeField] private RectTransform overlayContent;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    private readonly List<RectTransform> pages = new List<RectTransform>();
    private RectTransform holder;
    private RectTransform viewport;
    private Vector2 viewportSize;
    private int index;
    private int childCount;
    private Coroutine movement;

    private void OnEnable()
    {
        if (levelScreenHolder == null) return;
        holder = levelScreenHolder.GetComponent<RectTransform>();
        viewport = holder.parent as RectTransform;
        childCount = holder.childCount;
        pages.Clear();
        foreach (Transform child in holder)
            if (child is RectTransform page && child.GetComponent<LevelSelectScreen>() != null)
                pages.Add(page);
        index = Mathf.Clamp(index, 0, Mathf.Max(0, pages.Count - 1));
        Canvas.willRenderCanvases -= RefreshLayout;
        Canvas.willRenderCanvases += RefreshLayout;
        LayoutPages();
    }

    private void Start()
    {
        if (Application.isPlaying && SaveManager.Instance != null)
            index = SaveManager.Instance.getSaveData<int>(SaveManager.SaveDataNames.LevelSelectionIndex);
        index = Mathf.Clamp(index, 0, Mathf.Max(0, pages.Count - 1));
        LayoutPages();
    }

    private void LateUpdate()
    {
        RefreshLayout();
    }

    private void RefreshLayout()
    {
        if (holder != null && holder.childCount != childCount) OnEnable();
        else if (viewport != null && viewport.rect.size != viewportSize) LayoutPages();

        if (holder == null || viewport == null || movement != null || pages.Count == 0) return;

        // Canvas scaling and editor resizing can finish after LateUpdate.
        // Use the actual page center so a stale scroll offset cannot leave a
        // strip of the neighbouring page visible at the edge of the screen.
        Vector3 viewportCenter = viewport.TransformPoint(viewport.rect.center);
        RectTransform selectedPage = pages[index];
        Vector3 pageCenter = selectedPage.TransformPoint(selectedPage.rect.center);
        Vector3 correction = viewportCenter - pageCenter;
        if (correction.sqrMagnitude > 0.000001f)
            holder.position += correction;
    }

    private void OnDisable()
    {
        Canvas.willRenderCanvases -= RefreshLayout;
        if (movement != null) StopCoroutine(movement);
        movement = null;
    }

    private void LayoutPages()
    {
        if (viewport == null || holder == null) return;
        if (movement != null) StopCoroutine(movement);
        movement = null;
        // The scrolling holder always occupies the full canvas. Only its
        // anchored position changes when navigating between pages.
        holder.anchorMin = Vector2.zero;
        holder.anchorMax = Vector2.one;
        holder.pivot = Vector2.one * 0.5f;
        holder.sizeDelta = Vector2.zero;
        holder.localScale = Vector3.one;
        viewportSize = viewport.rect.size;
        for (int i = 0; i < pages.Count; i++)
        {
            RectTransform page = pages[i];
            page.anchorMin = Vector2.zero;
            page.anchorMax = Vector2.one;
            page.pivot = Vector2.one * 0.5f;
            page.sizeDelta = Vector2.zero;
            page.localScale = Vector3.one;
            page.anchoredPosition = new Vector2(i * viewportSize.x, 0f);
            page.GetComponent<LevelSelectScreen>().RefreshPresentation();
        }
        if (overlayContent != null)
            overlayContent.localScale = Vector3.one * Mathf.Min(viewportSize.x / 1920f, viewportSize.y / 1080f);
        holder.anchoredPosition = new Vector2(-index * viewportSize.x, 0f);
        UpdateButtons();
    }

    public void SelectionButton(int next)
    {
        if (pages.Count == 0 || movement != null) return;
        int nextIndex = Mathf.Clamp(index + next, 0, pages.Count - 1);
        if (nextIndex == index) return;
        index = nextIndex;
        UpdateButtons();
        if (Application.isPlaying)
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.saveData(SaveManager.SaveDataNames.LevelSelectionIndex, index, false);
            if (moveDuration > 0f) movement = StartCoroutine(MoveSelection());
            else holder.anchoredPosition = new Vector2(-index * viewportSize.x, 0f);
        }
        else holder.anchoredPosition = new Vector2(-index * viewportSize.x, 0f);
    }

    private void UpdateButtons()
    {
        if (previousButton != null) previousButton.interactable = index > 0;
        if (nextButton != null) nextButton.interactable = index < pages.Count - 1;
    }

    private IEnumerator MoveSelection()
    {
        Vector2 start = holder.anchoredPosition;
        Vector2 destination = new Vector2(-index * viewportSize.x, 0f);
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            holder.anchoredPosition = Vector2.Lerp(start, destination, Mathf.Clamp01(elapsed / moveDuration));
            yield return null;
        }
        holder.anchoredPosition = destination;
        movement = null;
    }

    public void enterScene(string sceneName) { SceneManager.LoadScene(sceneName); }
}
