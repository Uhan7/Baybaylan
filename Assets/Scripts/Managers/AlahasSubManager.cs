using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

//salita slots for wehn submit button is pressed and scoring 
//tile mods are done in tile sets

//detects and changes gamestates for a clean ish implementation of items/alahas
class AlahasSubManager : MonoBehaviour
{
    //states to send to other scripts
    [ReadOnly, SerializeField] public bool boostVowels = false;
    [ReadOnly,SerializeField] public float vowelSpawnChanceIncrease = 0;
    [ReadOnly, SerializeField] public float vowelScoreMulti = 1f;
    [ReadOnly, SerializeField] public bool spawnGolds = false;
    [ReadOnly, SerializeField] public float goldSpawnChance = 0;
    [ReadOnly, SerializeField] public float goldScoreMulti = 1f;
    //-------------------------------------------
    public static AlahasSubManager Instance;
    AlahasManager alahasManagerScript;
    List<Alahas> heldAlahas;

    void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        alahasManagerScript = AlahasManager.Instance;
        heldAlahas = alahasManagerScript.heldAlahas;
    }

    void Update()
    {
        onUpdate();

        //updates all the needed bools 
        
    }

    public void onSubmit()
    {
        foreach(Alahas alahas in heldAlahas)
        {
            alahas.onSubmit();
        }
    }

    public void onTurnEnd()
    {
        foreach(Alahas alahas in heldAlahas)
        {
            alahas.onTurnEnd();
        }
    }

    void onUpdate()
    {
        foreach(Alahas alahas in heldAlahas)
        {
            if(!alahas)
                continue;

            alahas.onUpdate();
            if(alahas.triggerCondition())
                alahas.onTriggerEffect();
        }
    }
}