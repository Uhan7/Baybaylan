using UnityEngine;
using System.Collections.Generic;

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
            // The description promises one 20% increase per qualifying word,
            // not 20% per letter (which made a seven-letter word add 140%).
            totalScoreMultiplier += scoreMultiplier;
            Debug.Log("Kuwintas ng Pitong Tuka: " + totalLetterCount + " letters submitted. Total score multiplier: " + totalScoreMultiplier);
        }
        else
        {
            totalScoreMultiplier = 0f;
            Debug.Log("Kuwintas ng Pitong Tuka: Not enough letters submitted. Total score multiplier reset to 0.");
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
}
