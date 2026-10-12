using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Gumamela")]
public class GumamelaAlahas : Alahas
{
    [SerializeField] float vowelScoreMultiplier = 3f;
    [SerializeField] float vowelSpawnMultiplier = 3f;

    public override void onStart()
    {
        
    }

    public override bool triggerCondition()
    {
        return false;
    }

    public override void onTriggerEffect()
    {
        
    }

    public override void onSubmit()
    {
 
    }

    public override void onTurnEnd()
    {
        
    }

    public override void onUpdate()
    {
        AlahasSubManager.Instance.boostVowels = true;
        AlahasSubManager.Instance.vowelSpawnChanceIncrease = vowelSpawnMultiplier;
        AlahasSubManager.Instance.vowelScoreMulti = vowelScoreMultiplier;
    }
}