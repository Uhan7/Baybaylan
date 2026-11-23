using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Daliri ni Tarabusaw")]
class DaliriNiTarabusawAlahas : Alahas
{
    const int MinTilesToRecommend = 4;
    const int MaxTilesToRecommend = 6;

    public override void onStart()
    {
        AlahasSubManager.Instance.reccWordButtonActive = false;
    }

    public override bool triggerCondition()
    {
        // Daliri is activated directly by clicking its equipped HUD slot.
        return false;
    }

    public override void onTriggerEffect()
    {
        TryActivate();
    }

    public bool TryActivate()
    {
        if (AlahasSubManager.Instance == null ||
            AlahasManager.Instance == null ||
            !AlahasManager.Instance.CanActivate(this))
            return false;

        if (!AlahasSubManager.Instance.TryFormRecommendedWord(
                MinTilesToRecommend,
                MaxTilesToRecommend))
            return false;

        if (!AlahasManager.Instance.TryConsumeActivation(this))
            return false;

        AlahasSubManager.Instance.reccButtonPressed = false;
        AlahasSubManager.Instance.reccWordButtonActive = false;
        return true;
    }

    public override void onSubmit()
    {
        AlahasSubManager.Instance.reccWordButtonActive = false;
    }

    public override void onTurnEnd()
    {

    }

    public override void onUpdate()
    {

    }
}
