using UnityEngine; 

class AlahasInfoPopup : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void closeButton()
    {
        gameObject.SetActive(false);
    }

    public void openPopup()
    {
        gameObject.SetActive(true);
    }
}