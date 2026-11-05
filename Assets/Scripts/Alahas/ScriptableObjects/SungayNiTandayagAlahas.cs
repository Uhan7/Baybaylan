using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Sungay ni Tandayag")]
class SungayNiTandayagAlahas : Alahas
{
    float timer = 0;
    float maxTime = 10;
    bool stopTimer = false;
    bool countdownActive = false;
    float scoreMulti = 0.5f;

    public override void onStart()
    {
        timer = maxTime;
        stopTimer = false;
        countdownActive = false;
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
        if (countdownActive)
            stopTimer = true;
    }

    public override void onTurnEnd()
    {
        countdownActive = false;
        stopTimer = false;
    }

    public override void onUpdate()
    {
        if(countdownActive && !stopTimer)
            timer += Time.deltaTime;

        if(countdownActive && timer <= maxTime)
            AlahasSubManager.Instance.scoreMultiplier += scoreMulti;
    }

    public override void onTilesRefreshed()
    {
        timer = 0f;
        stopTimer = false;
        countdownActive = true;
    }

    public override bool TryGetExtraBadgeText(out string text)
    {
        float remainingTime = countdownActive
            ? Mathf.Max(0f, maxTime - timer)
            : 0f;
        text = Mathf.CeilToInt(remainingTime) + "s";
        return true;
    }
}
