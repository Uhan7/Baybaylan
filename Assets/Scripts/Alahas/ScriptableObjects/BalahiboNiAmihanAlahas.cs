using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Balahibo ni Amihan")]
public class BalahiboNiAmihanAlahas : Alahas
{
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
        AlahasSubManager.Instance.toolTipTiles = true;
    }
}