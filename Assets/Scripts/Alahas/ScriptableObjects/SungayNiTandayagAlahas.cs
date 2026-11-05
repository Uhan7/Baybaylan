using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Sungay ni Tandayag")]
class SungayNiTandayagAlahas : Alahas
{
    float timer = 0;
    float maxTime = 10;
    bool stopTimer = false;
    float scoreMulti = 0.5f;

    public override void onStart()
    {
        timer = maxTime * 2;
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
        stopTimer = true;
    }

    public override void onTurnEnd()
    {
        stopTimer = false;
        timer = 0;
    }

    public override void onUpdate()
    {
        if(!stopTimer)
            timer += Time.deltaTime;

        if(timer <= maxTime)
            AlahasSubManager.Instance.scoreMultiplier += scoreMulti;
    }
}