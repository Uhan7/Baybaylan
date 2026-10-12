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
        if (AlahasManager.Instance == null || AlahasManager.Instance.heldAlahas == null)
            return;

        foreach (Alahas alahas in AlahasManager.Instance.heldAlahas)
        {
            if (!(alahas is DaliriNiTarabusawAlahas daliri)) continue;
            displayWord = daliri.TryActivate();
            return;
        }
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
