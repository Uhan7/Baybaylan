using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Pakpak ni Pah")]
class PakpakNiPahAlahas : Alahas
{
    float timer = 0f;
    bool alreadyDeleted = false;
    float durationTillDelete = 15f;
    int tilesToAdd = 3;
    int tilesToDelete = 5;

    public override void onStart()
    {
        timer = 0f;
        alreadyDeleted = false;
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
        if(alreadyDeleted)
            AlahasSubManager.Instance.spawnTiles(tilesToDelete);

        TileSet.Instance.PakpakNiPahDeleteTemps();

        alreadyDeleted = false;
        timer = 0f;
    }

    public override void onUpdate()
    {
        AlahasSubManager.Instance.add3ExtraTiles = true;
        AlahasSubManager.Instance.extraTilesToAdd = tilesToAdd;
        AlahasSubManager.Instance.tilesToDelete = tilesToDelete;

        AlahasSubManager.Instance.delete5Tiles = false; //this loop *should* reset the bool in 1 frame 

        //Debug.Log("timer: " + timer);

        timer += Time.deltaTime;
        if(timer >= durationTillDelete && !alreadyDeleted)
        {
            alreadyDeleted = true;
            AlahasSubManager.Instance.delete5Tiles = true;

            //Debug.Log("delete func");
        }
    }
}