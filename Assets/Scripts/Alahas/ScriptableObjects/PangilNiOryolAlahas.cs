using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Pangil ni Oryol")]
class PangilNiOryolAlahas : Alahas
{
    public void refreshTiles()
    {
        if(AlahasManager.Instance.TryConsumeActivation(this))
        {
            int tileCount = TileSet.Instance.transform.childCount;
            foreach(Transform tile in TileSet.Instance.transform)
                Destroy(tile.gameObject);
            AlahasSubManager.Instance.spawnTiles(tileCount);
        }
    }

    public override void onStart()
    {

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
        
    }

    public override void onUpdate()
    {

    }
}