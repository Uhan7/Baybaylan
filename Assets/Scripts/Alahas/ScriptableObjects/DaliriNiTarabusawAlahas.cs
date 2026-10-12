using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Daliri ni Tarabusaw")]
class DaliriNiTarabusawAlahas : Alahas
{
    const int MinTilesToRecommend = 3;
    const int MaxTilesToRecommend = 5;
    bool hasReccomended = false;

    public override void onStart()
    {
        hasReccomended = false;
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
        if (hasReccomended || AlahasSubManager.Instance == null ||
            AlahasManager.Instance == null ||
            !AlahasManager.Instance.CanActivate(this))
            return false;

        if (!AlahasSubManager.Instance.TryFormRecommendedWord(
                MinTilesToRecommend,
                MaxTilesToRecommend))
            return false;

        if (!AlahasManager.Instance.TryConsumeActivation(this))
            return false;

        hasReccomended = true;
        AlahasSubManager.Instance.reccButtonPressed = false;
        AlahasSubManager.Instance.reccWordButtonActive = false;
        return true;
    }

    public override void onSubmit()
    {
        if (hasReccomended)
            AlahasSubManager.Instance.reccWordButtonActive = false;
    }

    public override void onTurnEnd()
    {

    }

    public override void onUpdate()
    {

    }
}
