using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Kuwintas na Luya")]
public class KuwintasNaLuyaAlahas : Alahas
{
    public float convertToGoldChance = 0.3f;
    private const float GoldScoreMultiplier = 2f;

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
        //maybe
    }

    public override void onTurnEnd()
    {
        
    }

    public override void onUpdate()
    {
        AlahasSubManager.Instance.spawnGolds = true;
        AlahasSubManager.Instance.goldSpawnChance = Mathf.Clamp01(convertToGoldChance);
        AlahasSubManager.Instance.goldScoreMulti = GoldScoreMultiplier;
    }
}
