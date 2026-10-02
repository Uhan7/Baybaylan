using UnityEngine;
using TMPro;


class ReccomendWordButton : MonoBehaviour
{
    public static ReccomendWordButton Instance;
    TMP_Text wordText;
    bool displayWord = false;
    [HideInInspector] public string reccWord;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        wordText = GetComponentInChildren<TMP_Text>();
    }

    public void OnClick()
    {
        AlahasSubManager.Instance.reccButtonPressed = true;
        displayWord = true;
    }

    void Update()
    {
        if(AlahasSubManager.Instance.reccWordButtonActive)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }

        if(displayWord)
        {
            wordText.text = reccWord;
        }
    }
}