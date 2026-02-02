using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera active;
    //Variables
    public float sensitivity;
    [SerializeField]
    public Transform player;
    [SerializeField]
    private float minRotX,maxRotX;
    [SerializeField]
    private float bobSpeed, bobSmoothSpeed;
    [SerializeField]
    private float bobAmountY,bobAmountX;
    [SerializeField]
    private Vector2 renderTextureResolution;
    //Run time
    [NonSerialized]
    public float rotationX;
    private float bobX, bobY;
    private Camera camera;

    private bool inThirdPerson;

    //New Input Package
    private InputAction lookAction;
    private void Start()
    {
        active = this;
        
        lookAction = InputSystem.actions.FindAction("Look");
        Cursor.lockState = CursorLockMode.Locked;

        camera = GetComponent<Camera>();
    }
    private void Update()
    {
        if (InputManager.altAction.WasPerformedThisFrame())
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;

        //First Person
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            var lookInput = lookAction.ReadValue<Vector2>();

            rotationX -= lookInput.y * sensitivity;
            rotationX = Mathf.Clamp(rotationX, minRotX, maxRotX);
            transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

            player.rotation *= Quaternion.Euler(0, lookInput.x * sensitivity, 0);
        }

        CameraBob();
    }

    private void CameraBob()
    {
        float mult = Mathf.Clamp01(PlayerMovement.active.rb.linearVelocity.magnitude  * 0.3f);
        
        bobX = Mathf.Lerp(bobX, Mathf.Cos(Time.time * bobSpeed * 0.5f) * bobAmountX * mult, Time.deltaTime * bobSmoothSpeed);
        bobY = Mathf.Lerp(bobY, Mathf.Sin(Time.time * bobSpeed ) * bobAmountY * mult, Time.deltaTime * bobSmoothSpeed);

        transform.localPosition = new Vector3(bobX, bobY, 0);
    }

    public Vector3 GetRaycastPos()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 100f))
        {
            return hit.point;
        }
        else
        {
            return transform.position + transform.forward * 100f;
        }
    }

    public void WorldPosToUI(Vector3 worldPos, out Vector3 screenPos, out bool onScreen)
    {
        Vector3 renderTexturePos = camera.WorldToScreenPoint(worldPos);
        if (renderTexturePos.z < 0f)
        {
            onScreen = false;
        }
        else
        {
            onScreen = true;
        }
        screenPos = new Vector3(renderTexturePos.x * (Screen.width / renderTextureResolution.x),renderTexturePos.y * (Screen.height / renderTextureResolution.y),renderTexturePos.z);
    }
    
}
