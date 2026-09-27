using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Balahibo")]
public class BalahiboAlahas : Alahas
{
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
        AlahasSubManager.Instance.toolTipTiles = true;
    }
}