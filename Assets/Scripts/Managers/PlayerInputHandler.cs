using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance {get; private set; }
    void Awake()
    {
        controls = new InputSystem_Actions();

        if(Instance != null && Instance != this) 
        {
            Destroy(this.gameObject);
        } 
        else 
        {
            Instance = this;
            transform.SetParent(null, true);
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private InputSystem_Actions controls;
    public Vector2 mousePos {get; private set; }
    public bool leftClick {get; private set; }
    public bool rightClick {get; private set; }
    public bool shift {get; private set; }

    void OnEnable()
    {
        controls.Enable();

        controls.Player.MousePosition.performed += ctx => mousePos = ctx.ReadValue<Vector2>();
        controls.Player.MousePosition.canceled += ctx => mousePos = Vector2.zero;

        controls.Player.LeftClick.performed += ctx => leftClick = true;
        controls.Player.LeftClick.canceled += ctx => leftClick = false;

        controls.Player.RightClick.performed += ctx => rightClick = true;
        controls.Player.RightClick.canceled += ctx => rightClick = false;

        controls.Player.Shift.performed += ctx => shift = true;
        controls.Player.Shift.canceled += ctx => shift = false;
    }

    void OnDisable()
    {
        controls.Disable();
    }

    public void controlsOn()
    {
        OnEnable();
    }

    public void controlsOff()
    {
        OnDisable();
    }

    public void mouseOn()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void mouseOff()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}