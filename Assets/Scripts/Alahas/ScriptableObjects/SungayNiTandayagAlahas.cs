using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Sungay ni Tandayag")]
class SungayNiTandayagAlahas : Alahas
{
    float timer = 0;
    float maxTime = 10;
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
        timer = 0;
    }

    public override void onTurnEnd()
    {
        
    }

    public override void onUpdate()
    {
        timer += Time.deltaTime;
        if(timer <= maxTime)
            AlahasSubManager.Instance.scoreMultiplier += scoreMulti;
    }
}