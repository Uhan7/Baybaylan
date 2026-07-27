using UnityEngine; 
using UnityEngine.EventSystems;
using TMPro;

//slap this on an obj to let the tooltip display its info when hovered over
class ToolTipAble : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject tooltipObj;
    [SerializeField] string tipText;
    [SerializeField] float ToolTipDelay = 1f;
    [SerializeField] bool followMouse = false;
    static Vector2 ToolTipPositionOffset = new Vector2(300, 100);
    GameObject tooltipObjInstance;
    TMP_Text tooltipText;
    float timer = 0f;
    bool isHovered = false;
    bool onetime = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        endHover();
    }

    void Update()
    {
        if (isHovered)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0f;
        }

        if(timer >= ToolTipDelay)
        {
            oneTime();

            tooltipText.text = tipText;

            if(followMouse)
                tooltipObjInstance.transform.position = Input.mousePosition + (Vector3)ToolTipPositionOffset;
            else
                tooltipObjInstance.transform.position = transform.position + (Vector3)ToolTipPositionOffset;
        }
    }

    void startHover()
    {
        tooltipObjInstance = Instantiate(tooltipObj, transform.position, Quaternion.identity, GameObject.Find("Canvas").transform);
        tooltipObjInstance.SetActive(true);
        tooltipText = tooltipObjInstance.GetComponentInChildren<TMP_Text>();
    }

    void endHover()
    {
        Destroy(tooltipObjInstance);
        onetime = false;
    }

    void oneTime()
    {
        if(onetime)
            return;
        onetime = true;

        startHover();
    }
}