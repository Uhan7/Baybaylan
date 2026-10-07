using UnityEngine;
using UnityEngine.UI;

public class MahikaBar : MonoBehaviour
{
    [SerializeField] private Image fill;

    public void SetFill(float _percentage)
    {
        fill.fillAmount = _percentage;
    }
}
