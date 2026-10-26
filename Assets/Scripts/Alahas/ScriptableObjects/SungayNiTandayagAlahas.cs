using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Sungay ni Tandayag")]
class SungayNiTandayagAlahas : Alahas
{
    float timer = 0;
    float scoreMulti = 0.5f;

    public override void onStart()
    {
        timer = 0;
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
        if(timer <= 10)
            AlahasSubManager.Instance.scoreMultiplier += scoreMulti;
    }
}