using UnityEngine;
using UnityEngine.UI;

public class MahikaBar : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private Image background;

    public void SetBackgroundOpacity(float opacity)
    {
        if (background == null) return;
        Color color = background.color;
        color.a = Mathf.Clamp01(opacity);
        background.color = color;
    }

    public void SetFill(float _percentage)
    {
        fill.fillAmount = _percentage;
    }
}
