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
    NagpapalitAnyoPaghihigpit nagpapalitAnyo;
    int currentIndex;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        nagpapalitAnyo = GameManager.Instance.config.GetPaghihigpit(PaghihigpitTypes.NagpapalitAnyo) as NagpapalitAnyoPaghihigpit;
        currentIndex = 0;
    }

    //gets called from the alahas submanager cuz i got lazy 
    public void switchBoss(bool keepIndex)
    {
        if(!nagpapalitAnyo)
        {
            Debug.Log("no nagpapalitAnyo found");
            return;
        }

        if(!keepIndex)
            currentIndex = getNewIndex();
        List<Paghihigpit> newPaghihigpit = new List<Paghihigpit>();
        setNewPaghihigpitList(newPaghihigpit);

        enemyImage.sprite = nagpapalitAnyo.bosses[currentIndex].sprite;
        enemyNameText.text = nagpapalitAnyo.bosses[currentIndex].name;
        paghihigpitDisplay.setNewTiles(newPaghihigpit);
        GameManager.Instance.config.replaceActivePaghihigpits(newPaghihigpit);
    }

    void setNewPaghihigpitList(List<Paghihigpit> list)
    {
        foreach(Paghihigpit paghihigpit in nagpapalitAnyo.bosses[currentIndex].paghihigpit)
            list.Add(paghihigpit);

        if(!list.Contains(nagpapalitAnyo))
            list.Insert(0, nagpapalitAnyo);
    }

    int getNewIndex()
    {
        int newIndex = 0;
        do
        {
            newIndex = UnityEngine.Random.Range(0, nagpapalitAnyo.bosses.Length);
        } while(newIndex == currentIndex);

        return newIndex;
    }
}