using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

class EasyTileMoveManager : MonoBehaviour
{
    [SerializeField] GameObject tileSet;
    [SerializeField] GameObject salitaSlots;
    PlayerInputHandler inputHandler;
    bool rightinputLock = false;
    bool leftinputLock = false;

    void Start()
    {
        // tileSet = TileSet.Instance.gameObject;
        // salitaSlots = SalitaSlots.Instance.gameObject;
        inputHandler = PlayerInputHandler.Instance;
    }

    void FixedUpdate()
    {
        GameObject uiObj = GetUIUnderMouse();
        if(uiObj == null) return;

        inputCheck(inputHandler.rightClick, ref rightinputLock, uiObj);
        inputCheck(inputHandler.leftClick && inputHandler.shift, ref leftinputLock, uiObj);
    }

    void inputCheck(bool input, ref bool inputLock, GameObject uiObj)
    {
        if(!input) inputLock = false;
        if(inputLock) return;

        if(input)
        {
            swapPos(uiObj);
            inputLock = true;
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

        //Debug.Log("Swapped " + obj.name + " to " + obj.transform.parent.name);
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