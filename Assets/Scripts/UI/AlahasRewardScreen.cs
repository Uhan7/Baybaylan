using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlahasRewardScreen : MonoBehaviour
{
    [SerializeField] private Image alahasImage;
    [SerializeField] private TMP_Text alahasNameText;
    [SerializeField] private TMP_Text alahasDescriptionText;
    [SerializeField] private TMP_Text alahasFlavorText;

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        LevelConfig levelConfig = GameManager.Instance != null
            ? GameManager.Instance.config
            : null;
        Populate(levelConfig != null ? levelConfig.alahasAquiredAfterWin : null);
    }

    public void Populate(Alahas alahas)
    {
        if (alahas == null)
        {
            alahasImage.sprite = null;
            alahasImage.enabled = false;
            alahasNameText.text = string.Empty;
            alahasDescriptionText.text = string.Empty;
            alahasFlavorText.text = string.Empty;
            return;
        }

        alahasImage.sprite = alahas.alahasSprite;
        alahasImage.enabled = alahas.alahasSprite != null;
        alahasNameText.text = alahas.alahasName;
        alahasDescriptionText.text = alahas.description;
        alahasFlavorText.text = alahas.extraText;
    }
}
