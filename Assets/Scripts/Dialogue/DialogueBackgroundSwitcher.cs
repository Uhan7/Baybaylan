using UnityEngine;
using UnityEngine.UI;

public class DialogueBackgroundSwitcher : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite mountainBackground;
    [SerializeField] private Sprite castleBackground;

    public void ShowMountainBackground()
    {
        SetBackground(mountainBackground);
    }

    public void ShowCastleBackground()
    {
        SetBackground(castleBackground);
    }

    private void SetBackground(Sprite background)
    {
        if (backgroundImage == null || background == null) return;
        backgroundImage.sprite = background;
    }
}
