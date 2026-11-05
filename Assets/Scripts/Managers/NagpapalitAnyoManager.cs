using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

class NagpapalitAnyoManager : MonoBehaviour
{
    static public NagpapalitAnyoManager Instance;
    [SerializeField] Image enemyImage;
    [SerializeField] TextMeshProUGUI enemyNameText;
    [SerializeField] PaghihigpitDisplay paghihigpitDisplay;
    Paghihigpit nagpapalitAnyo;
    LevelConfig config;
    int currentIndex;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (GameManager.Instance == null || GameManager.Instance.config == null) return;

        config = GameManager.Instance.config;
        nagpapalitAnyo = config.GetPaghihigpit(PaghihigpitTypes.NagpapalitAnyo);
        currentIndex = 0;
    }

    // Called by the Alahas sub-manager when the active form should refresh.
    public void switchBoss(bool keepIndex)
    {
        if (!nagpapalitAnyo)
        {
            Debug.LogWarning("No Nagpapalit-anyo Paghihigpit is active in this level.");
            return;
        }

        if (config.nagpapalitAnyoForms == null || config.nagpapalitAnyoForms.Count == 0)
        {
            Debug.LogWarning("Nagpapalit-anyo is active, but this Level Config has no forms configured.");
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, config.nagpapalitAnyoForms.Count - 1);
        if (!keepIndex)
            currentIndex = getNewIndex();

        NagpapalitAnyoForm currentForm = config.nagpapalitAnyoForms[currentIndex];
        List<Paghihigpit> newPaghihigpit = new List<Paghihigpit>();
        setNewPaghihigpitList(currentForm, newPaghihigpit);

        if (enemyImage != null) enemyImage.sprite = currentForm.bossSprite;
        if (enemyNameText != null) enemyNameText.text = currentForm.bossName;
        if (paghihigpitDisplay != null) paghihigpitDisplay.setNewTiles(newPaghihigpit);
        config.replaceActivePaghihigpits(newPaghihigpit);
    }

    void setNewPaghihigpitList(NagpapalitAnyoForm form, List<Paghihigpit> list)
    {
        if (form != null && form.paghihigpit != null)
        {
            foreach (Paghihigpit paghihigpit in form.paghihigpit)
            {
                if (paghihigpit != null && !list.Contains(paghihigpit))
                    list.Add(paghihigpit);
            }
        }

        if (!list.Contains(nagpapalitAnyo))
            list.Insert(0, nagpapalitAnyo);
    }

    int getNewIndex()
    {
        int formCount = config.nagpapalitAnyoForms.Count;
        if (formCount <= 1) return 0;

        int newIndex = 0;
        do
        {
            newIndex = UnityEngine.Random.Range(0, formCount);
        } while (newIndex == currentIndex);

        return newIndex;
    }
}
