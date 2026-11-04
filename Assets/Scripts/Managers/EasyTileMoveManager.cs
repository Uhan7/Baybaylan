using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

class EasyTileMoveManager : MonoBehaviour
{
    [SerializeField] GameObject tileSet;
    [SerializeField] GameObject salitaSlots;

    void Start()
    {
        // tileSet = TileSet.Instance.gameObject;
        // salitaSlots = SalitaSlots.Instance.gameObject;
    }

    void FixedUpdate()
    {
        GameObject uiObj = GetUIUnderMouse();
        if(uiObj == null) return;

        if (PlayerInputHandler.Instance.rightClick)
        {
            swapPos(uiObj);
        }
        else if (PlayerInputHandler.Instance.leftClick && PlayerInputHandler.Instance.shift)
        {
            swapPos(uiObj);
        }
    }

    void swapPos(GameObject obj)
    {
        if(obj.transform.parent.GetComponent<TileSet>())
        {
            obj.transform.SetParent(salitaSlots.transform, true);
        }
        else if(obj.transform.parent.GetComponent<SalitaSlots>())
        {
            obj.transform.SetParent(tileSet.transform, true);
        }

        Debug.Log("Swapped " + obj.name + " to " + obj.transform.parent.name);
    }

    //by gpt
    GameObject GetUIUnderMouse()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
            return results[0].gameObject;

        return null;
    }
}