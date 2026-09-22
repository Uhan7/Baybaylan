using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Dahon Ng Kawayan")]
public class DahonNgKawayanAlahas : Alahas
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
 
    }

    public override void onTurnEnd()
    {
        
    }

    public override void onUpdate()
    {
        AlahasSubManager.Instance.canCreateTile = true;
    }
}