using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Luya")]
public class LuyaAlahas : Alahas
{
    [SerializeField] private float convertChance = 0.3f;

    public override void ApplyAlahas()
    {
        AlahasManager.Instance.goldenTileChance += convertChance;
    }

    public override void RemoveAlahas()
    {
        AlahasManager.Instance.goldenTileChance -= convertChance;
    }
}