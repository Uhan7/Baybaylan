using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Bilaong Yantok")]
class BilaongRatanAlahas : Alahas
{
    int extraTurns = 1;
    int extraTiles = 0;

    public override void onStart()
    {
        AlahasSubManager.Instance.addExtraTurnAndTile = true;
        AlahasSubManager.Instance.extraTurn = extraTurns;
        AlahasSubManager.Instance.extraTile = extraTiles;
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
        
    }
}
