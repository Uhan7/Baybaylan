using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Luya")]
public class LuyaAlahas : Alahas
{
    public float convertToGoldChance = 0.3f;
    public static float goldScoreMultiplier = 4f;

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
        AlahasSubManager.Instance.spawnGolds = true;
        AlahasSubManager.Instance.goldSpawnChance = convertToGoldChance;
        AlahasSubManager.Instance.goldScoreMulti = goldScoreMultiplier;
    }
}