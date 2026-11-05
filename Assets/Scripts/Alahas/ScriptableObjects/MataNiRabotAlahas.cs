using UnityEngine;
using System.Globalization;

[CreateAssetMenu(menuName = "Alahas/Mata ni Rabot")]
class MataNiRabot : Alahas
{
    float minMulti = -0.25f;
    float maxMulti = 0.25f;
    float currentMulti = 0;
    float multiPerTurn = 0;
    bool inited = false;

    void initValues()
    {
        int turnCount = AksyonCounter.Instance.maxAvailableAksyon; 
        turnCount--; //last turn doesnt count apparently 
        multiPerTurn = (maxMulti - minMulti) / turnCount;
        currentMulti = minMulti;
        inited = true;

        //Debug.Log("multiPerTurn: " + multiPerTurn);
    }

    public override void onStart()
    {
        inited = false;
        currentMulti = minMulti;
        multiPerTurn = 0f;
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
        
    }

    public override void onTurnEnd()
    {
        currentMulti += multiPerTurn;
        if(currentMulti > maxMulti)
            currentMulti = maxMulti;
    }

    public override void onUpdate()
    {
        if(!inited && AksyonCounter.Instance.maxAvailableAksyon > 0)
            initValues();

        if(inited)
            AlahasSubManager.Instance.scoreMultiplier += currentMulti;
    }

    public override bool TryGetExtraBadgeText(out string text)
    {
        float displayedMultiplier = 1f + (inited ? currentMulti : minMulti);
        text = "x" + displayedMultiplier.ToString("0.##", CultureInfo.InvariantCulture);
        return true;
    }
}
