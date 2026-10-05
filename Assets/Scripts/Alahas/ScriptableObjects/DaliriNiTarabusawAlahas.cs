using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Daliri ni Tarabusaw")]
class DaliriNiTarabusawAlahas : Alahas
{
    int minTilesToRecc = 4;
    int maxTilesToRecc = 6;
    bool hasReccomended = false;

    public override void onStart()
    {
        hasReccomended = false;
        AlahasSubManager.Instance.reccWordButtonActive = true;
    }

    public override bool triggerCondition()
    {
        return AlahasSubManager.Instance.reccButtonPressed && !hasReccomended;
    }

    public override void onTriggerEffect()
    {
        hasReccomended = true;

        AlahasSubManager.Instance.reccWordFunc(minTilesToRecc, maxTilesToRecc);
    }

    public override void onSubmit()
    {
        if(hasReccomended)
            AlahasSubManager.Instance.reccWordButtonActive = false;
    }

    public override void onTurnEnd()
    {

    }

    public override void onUpdate()
    {

    }
}