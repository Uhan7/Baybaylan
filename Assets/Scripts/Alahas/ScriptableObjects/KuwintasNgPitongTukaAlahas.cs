using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

[CreateAssetMenu(menuName = "Alahas/Kuwintas ng Pitong Tuka")]
class KuwintasNgPitongTukaAlahas : Alahas
{
    int letterCountRequirement = 7;
    float scoreMultiplier = 0.2f;
    float totalScoreMultiplier = 0f;

    public override void onStart()
    {
        totalScoreMultiplier = 0;
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
        SalitaSlots salitaSlots = Object.FindAnyObjectByType<SalitaSlots>();
        List<Tile> activeTiles = salitaSlots.activeTiles;

        string text = "";
        foreach(Tile tile in activeTiles)
        {
            text += tile.latinText;
        }
        int totalLetterCount = text.Length;
        if(totalLetterCount >= letterCountRequirement)
        {
            // Each qualifying word adds 0.2 to the total multiplier:
            // x1.2, x1.4, x1.6, and so on.
            totalScoreMultiplier += scoreMultiplier;
            Debug.Log(
                "Kuwintas ng Pitong Tuka: " + totalLetterCount +
                " letters submitted. Total score multiplier: " + FormatCurrentMultiplier());
        }
        else
        {
            totalScoreMultiplier = 0f;
            Debug.Log(
                "Kuwintas ng Pitong Tuka: Not enough letters submitted. " +
                "Total score multiplier reset to x1.");
        }
    }

    public override void onTurnEnd()
    {
        
    }

    public override void onUpdate()
    {
        // AlahasSubManager already supplies the base 1x multiplier.
        AlahasSubManager.Instance.scoreMultiplier += totalScoreMultiplier;
    }

    public override bool TryGetExtraBadgeText(out string text)
    {
        text = FormatCurrentMultiplier();
        return true;
    }

    private string FormatCurrentMultiplier()
    {
        float currentMultiplier = 1f + totalScoreMultiplier;
        return "x" + currentMultiplier.ToString("0.#", CultureInfo.InvariantCulture);
    }
}
