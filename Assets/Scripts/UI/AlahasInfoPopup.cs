using UnityEngine; 
using TMPro;

class AlahasInfoPopup : ToolTipAble
{
    public Alahas currentAlahas;
    TextMeshProUGUI alahasName;
    TextMeshProUGUI alahasDesc;
    TextMeshProUGUI alahasExtra;

    override protected void startHover()
    {
        tooltipObjInstance = Instantiate(tooltipObj, transform.position, Quaternion.identity, GameObject.FindFirstObjectByType<Canvas>().transform);
        tooltipObjInstance.SetActive(true);
        alahasName = tooltipObjInstance.transform.GetChild(1).transform.GetComponent<TextMeshProUGUI>();
        alahasDesc = tooltipObjInstance.transform.GetChild(2).transform.GetComponent<TextMeshProUGUI>();
        alahasExtra = tooltipObjInstance.transform.GetChild(3).transform.GetComponent<TextMeshProUGUI>();
    }

    override protected void Update()
    {
        if(!currentAlahas)
            return;

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
            base.oneTime();

            alahasName.text = currentAlahas.alahasName;
            alahasDesc.text = currentAlahas.description;
            alahasExtra.text = currentAlahas.extraText;

            if(followMouse)
                tooltipObjInstance.transform.position = Input.mousePosition + (Vector3)ToolTipPositionOffset;
            else
                tooltipObjInstance.transform.position = transform.position + (Vector3)ToolTipPositionOffset;
        }
    }
}