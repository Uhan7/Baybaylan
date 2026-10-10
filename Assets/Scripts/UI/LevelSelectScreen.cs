using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

class LevelSelectScreen : MonoBehaviour
{
    [SerializeField] SaveManager.SaveDataNames levelId;
    [SerializeField] string sceneNameToEnter;
    [SerializeField] GameObject lockVisual;
    bool isLocked = false;

    void Start()
    {
        Button button = transform.GetComponentInChildren<Button>();
        button.onClick.AddListener(() => buttonFunc(sceneNameToEnter));

        int currentLevelProgress = SaveManager.Instance.getSaveData<int>(levelId);
        if(currentLevelProgress == 0)
        {
            isLocked = true;
        }
        lockVisual.SetActive(isLocked);
    }

    void buttonFunc(string sceneName)
    {
        if(isLocked)
            return;

        SceneManager.LoadScene(sceneName);
    }
}