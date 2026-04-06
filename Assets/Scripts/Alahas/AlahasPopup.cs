using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AlahasPopup : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Components")]
    [SerializeField] private Alahas alahas;
    [SerializeField] private Button buttonComponent;

    [Header("UI Stuff References")]
    [SerializeField] private Image alahasImage;
    [SerializeField] private Sprite placeholderAlahasImage;
    [SerializeField] private TextMeshProUGUI alahasName;
    [SerializeField] private TextMeshProUGUI alahasDescription;
    [SerializeField] private TextMeshProUGUI alahasExtraText;

    // Main Functions ----------------------------------------------------------
    private void Start()
    {
        if (alahas == null)
        {
            Debug.LogError("Enabled an Alahas Popup with no Alahas Reference!");
            return;
        }

        ClearAlahasDetails();
        SetNewAlahas();

        AlahasPopupManager.Instance.OnSelectedPopup += HandlePopupSelected;
    }

    private void OnDisable()
    {
        AlahasPopupManager.Instance.OnSelectedPopup -= HandlePopupSelected;
    }

    // Helper Functions --------------------------------------------------------
    private void ClearAlahasDetails()
    {
        alahasImage.sprite = placeholderAlahasImage;
        alahasName.text = "Default Alahas";
        alahasDescription.text = "Default Description.";
        alahasExtraText.text = "Default Extra Text.";

        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(OnClickGeneral);

        buttonComponent.interactable = true;
    }

    private void OnClickGeneral()
    {
        buttonComponent.enabled = false;
        AlahasPopupManager.Instance.OnSelectedPopup?.Invoke();
    }

    private void SetNewAlahas()
    {
        alahasImage.sprite = alahas.alahasSprite;
        alahasName.text = alahas.alahasName;
        alahasDescription.text = alahas.description;
        alahasExtraText.text = alahas.extraText;

        buttonComponent.onClick.AddListener(alahas.ApplyAlahas);
    }

    private void HandlePopupSelected()
    {
        if (!buttonComponent.enabled) return;

        buttonComponent.interactable = false;
    }
}
