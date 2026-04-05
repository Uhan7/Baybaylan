using UnityEngine;

[CreateAssetMenu(menuName = "Alahas/Arpa")]
public class ArpaAlahas : Alahas
{
    public override void ApplyAlahas()
    {
        AlahasManager.Instance.boostVowels = true;
    }

    public override void RemoveAlahas()
    {
        AlahasManager.Instance.boostVowels = false;
    }
}