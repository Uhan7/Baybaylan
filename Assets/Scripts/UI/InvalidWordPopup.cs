using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

class InvalidWordPopup : MonoBehaviour
{
    public enum InvalidWordType
    {
        NotInWordlist,
        AlreadyUsed
    }

    [SerializeField] GameObject parentObj;
    TextMeshProUGUI invalidWordText;
    Image bgImg; 

    public void ShowInvalidWordPopup(InvalidWordType type)
    {
        switch (type)
        {
            case InvalidWordType.NotInWordlist:
                invalidWordText.text = "Salita is not in the wordlist!";
                break;
            case InvalidWordType.AlreadyUsed:
                invalidWordText.text = "Salita has already been used!";
                break;
        }

        parentObj.SetActive(true);
        StartCoroutine(fadeOut(1.5f));
    }

    IEnumerator fadeOut(float fadeOutTime)
    {
        float startOpacity = 1f;
        float elapsed = 0f;

        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;

            bgImg.color = new Color(bgImg.color.r, bgImg.color.g, bgImg.color.b, Mathf.Lerp(startOpacity, 0f, elapsed / fadeOutTime));
            invalidWordText.color = new Color(invalidWordText.color.r, invalidWordText.color.g, invalidWordText.color.b, Mathf.Lerp(startOpacity, 0f, elapsed / fadeOutTime));

            yield return null;
        }

        parentObj.SetActive(false);
        bgImg.color = new Color(bgImg.color.r, bgImg.color.g, bgImg.color.b, startOpacity); 
        invalidWordText.color = new Color(invalidWordText.color.r, invalidWordText.color.g, invalidWordText.color.b, startOpacity);
    }

    //wacky work around to get the components while disabled, gets called in salita slots in its start
    public void getComponents()
    {
        invalidWordText = GetComponentInChildren<TextMeshProUGUI>(true);
        bgImg = GetComponentInChildren<Image>(true);

        parentObj.SetActive(false);
    }
}