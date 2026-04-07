using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    void Update()
    {
        transform.position = Camera.main.ScreenPointToRay(Input.mousePosition).GetPoint(10);
    }
}