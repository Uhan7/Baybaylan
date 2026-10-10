using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; 

//basically only moves the screens 
class LevelSelectManager : MonoBehaviour
{
    [SerializeField] GameObject levelScreenHolder;
    [SerializeField] float moveDuration;
    List<RectTransform> screenCenters = new List<RectTransform>();
    int index = 0;
    int prevIndex = 0;
    bool isMoving = false;

    void Start()
    {
        screenCenters.Clear();
        StartCoroutine(start());
    }

    //by gpt
    IEnumerator start()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            levelScreenHolder.GetComponent<RectTransform>()
        );

        foreach (Transform child in levelScreenHolder.transform)
        {
            GameObject center = child.GetChild(0).gameObject;
            screenCenters.Add(center.GetComponent<RectTransform>());
            //Debug.Log($"{center.name}: {center.GetComponent<RectTransform>().position}");
        }

        goToLastUsedIndex();
    }

    void goToLastUsedIndex()
    {
        int savedIndex = SaveManager.Instance.getSaveData<int>(SaveManager.SaveDataNames.LevelSelectionIndex);
        index = savedIndex;

        if(index < 0)
            index = 0;
        if(index >= screenCenters.Count)
            index = screenCenters.Count - 1;

        prevIndex = index;

        StartCoroutine(moveSelection(0.0f));
    }

    public void SelectionButton(int next)
    {
        if(isMoving)
            return;

        index += next;

        if(index < 0)
            index = 0;
        if(index >= screenCenters.Count)
            index = screenCenters.Count - 1;

        if(index != prevIndex)
            StartCoroutine(moveSelection(moveDuration));
        prevIndex = index;

        SaveManager.Instance.saveData(SaveManager.SaveDataNames.LevelSelectionIndex, index, false);
    }

    public void enterScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    //temp func to move the sceens
    // by gpt
    IEnumerator moveSelection(float duration)
    {
        isMoving = true;

        RectTransform holder = levelScreenHolder.GetComponent<RectTransform>();

        // Get the selected center's screen position
        Vector2 selectedScreenPos =
            RectTransformUtility.WorldToScreenPoint(
                null, screenCenters[index].position);

        // The screen center (or your desired selection center)
        Vector2 targetScreenPos = new Vector2(
            Screen.width / 2f,
            Screen.height / 2f
        );

        // Calculate how far the holder needs to move
        Vector2 offset = targetScreenPos - selectedScreenPos;

        Vector3 startPosition = holder.position;
        Vector3 targetPosition = startPosition +
            new Vector3(offset.x, offset.y, 0f);

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;

            holder.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                Mathf.Clamp01(timer / duration)
            );

            yield return null;
        }

        holder.position = targetPosition;
        isMoving = false;
    }
}