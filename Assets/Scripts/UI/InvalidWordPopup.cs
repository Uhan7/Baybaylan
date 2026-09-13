using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

class InvalidWordPopup : MonoBehaviour
{
    [SerializeField] GameObject parentObj;
    [SerializeField] GameObject popupPrefab;
    [SerializeField] float fadeOutTime = 1.5f;
    [SerializeField] float upDistance = 100f;

    public void ShowInvalidWordPopup(InvalidWordTypes.InvalidWordType type, string word)
    {
        GameObject popupInstance = Instantiate(popupPrefab, parentObj.transform);
        popupInstance.transform.localPosition = Vector3.zero; 
        TextMeshProUGUI invalidWordText = popupInstance.GetComponentInChildren<TextMeshProUGUI>();
        Image bgImg = popupInstance.GetComponentInChildren<Image>();

        invalidWordText.text = InvalidWordTypes.GetInvalidWordMessage(type, word);

        StartCoroutine(fadeOut(fadeOutTime, upDistance, popupInstance, invalidWordText, bgImg));
    }

    IEnumerator fadeOut(float fadeOutTime, float upDistance, GameObject popupInstance, TextMeshProUGUI invalidWordText, Image bgImg)
    {
        float startOpacity = 1f;
        float elapsed = 0f;

        float upPerDelta = upDistance / fadeOutTime;

        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;

            popupInstance.transform.Translate(Vector3.up * upPerDelta * Time.deltaTime);
            bgImg.color = new Color(bgImg.color.r, bgImg.color.g, bgImg.color.b, Mathf.Lerp(startOpacity, 0f, elapsed / fadeOutTime));
            invalidWordText.color = new Color(invalidWordText.color.r, invalidWordText.color.g, invalidWordText.color.b, Mathf.Lerp(startOpacity, 0f, elapsed / fadeOutTime));

            yield return null;
        }

        Destroy(popupInstance);
    }
}