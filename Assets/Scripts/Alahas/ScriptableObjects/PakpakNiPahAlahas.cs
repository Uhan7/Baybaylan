using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Pakpak ni Pah")]
class PakpakNiPahAlahas : Alahas
{
    float timer = 0f;
    bool alreadyDeleted = false;
    bool tilesAreReady = false;
    float durationTillDelete = 15f;
    float tileDeleteInterval = 0.1f;
    int tilesToAdd = 3;
    int tilesToDelete = 5;

    public override void onStart()
    {
        timer = 0f;
        alreadyDeleted = false;
        tilesAreReady = false;

        AlahasSubManager.Instance.add3ExtraTiles = true;
        AlahasSubManager.Instance.extraTilesToAdd = tilesToAdd;
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
        tilesAreReady = false;
        timer = 0f;
    }

    public override void onUpdate()
    {
        AlahasSubManager.Instance.add3ExtraTiles = true;
        AlahasSubManager.Instance.extraTilesToAdd = tilesToAdd;
        if(AlahasSubManager.Instance.dialogueEnded && tilesAreReady)
            timer += Time.deltaTime;

        if(timer >= durationTillDelete && !alreadyDeleted)
        {
            alreadyDeleted = true;
            TileSet.Instance.StartPakpakTileDeletion(tilesToDelete, tileDeleteInterval);
        }
    }

    public void OnTilesRefreshed()
    {
        timer = 0f;
        alreadyDeleted = false;
        tilesAreReady = true;
    }
}
