using UnityEngine; 
using UnityEngine.EventSystems;
using TMPro;

//slap this on an obj to let the tooltip display its info when hovered over
class ToolTipAble : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected GameObject tooltipObj;
    [SerializeField] string tipText;
    [SerializeField] protected float ToolTipDelay = 1f;
    [SerializeField] protected bool followMouse = false;
    [SerializeField] protected Vector2 ToolTipPositionOffset = new Vector2(300, 100);
    protected GameObject tooltipObjInstance;
    TMP_Text tooltipText;
    protected float timer = 0f;
    protected bool isHovered = false;
    protected bool onetime = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        endHover();
    }

    protected virtual void Update()
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

    protected virtual void startHover()
    {
        tooltipObjInstance = Instantiate(tooltipObj, transform.position, Quaternion.identity, GameObject.FindFirstObjectByType<Canvas>().transform);
        tooltipObjInstance.SetActive(true);
        tooltipText = tooltipObjInstance.GetComponentInChildren<TMP_Text>();
    }

    void endHover()
    {
        Destroy(tooltipObjInstance);
        onetime = false;
    }

    protected virtual void oneTime()
    {
        if(onetime)
            return;
        onetime = true;

        startHover();
    }
}