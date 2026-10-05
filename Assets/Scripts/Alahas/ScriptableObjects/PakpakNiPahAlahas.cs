using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Pakpak ni Pah")]
class PakpakNiPahAlahas : Alahas
{
    bool countdownStarted = false;
    bool tilesAreReady = false;
    float durationTillDelete = 15f;
    float tileDeleteInterval = 0.1f;
    [SerializeField] private AnimationCurve disappearanceCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0f),
        new Keyframe(0.7f, 0.08f, 0.15f, 0.15f),
        new Keyframe(0.9f, 0.35f, 3f, 6f),
        new Keyframe(1f, 1f, 8f, 0f));
    int tilesToAdd = 3;
    int tilesToDelete = 5;

    public override void onStart()
    {
        countdownStarted = false;
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
        TileSet.Instance.CancelPakpakTileCountdown();
        countdownStarted = false;
        tilesAreReady = false;
    }

    public override void onUpdate()
    {
        AlahasSubManager.Instance.add3ExtraTiles = true;
        AlahasSubManager.Instance.extraTilesToAdd = tilesToAdd;
        if(AlahasSubManager.Instance.dialogueEnded && tilesAreReady && !countdownStarted)
        {
            countdownStarted = true;
            TileSet.Instance.StartPakpakTileCountdown(
                tilesToDelete,
                durationTillDelete,
                disappearanceCurve,
                tileDeleteInterval);
        }
    }

    public void OnTilesRefreshed()
    {
        countdownStarted = false;
        tilesAreReady = true;
    }
}
