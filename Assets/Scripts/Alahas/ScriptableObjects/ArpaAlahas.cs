using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Arpa")]
public class ArpaAlahas : Alahas
{
    [SerializeField] float vowelScoreMultiplier = 3f;
    [SerializeField] float vowelSpawnMultiplier = 3f;

    public override bool triggerCondition()
    {
        return false;
    }

    public override void onTriggerEffect()
    {
        
    }

    public override void onSubmit()
    {
        //maybe
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