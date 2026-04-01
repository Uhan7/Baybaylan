using UnityEngine;

[CreateAssetMenu]
public class LuyaAlahas : Alahas
{
    [SerializeField] private float convertChance = 0.5f;

    public override void ApplyAlahas(AlahasManager manager)
    {
        // call manager to convert gold tiles
    }
}