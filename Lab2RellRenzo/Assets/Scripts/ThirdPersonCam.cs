using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("TPS Rotation References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObject;
    public float rotationSpeed = 10f;
    public float thirdPersonMouseSensitivity = 180f;
    public float thirdPersonMouseYSensitivity = 180f;
    public float thirdPersonMinPitch = -60f;
    public float thirdPersonMaxPitch = 60f;
    public Transform cameraTransform;
    public Transform fpsCameraAnchor;
    public Transform thirdPersonPitchPivot;
    public Transform thirdPersonPitchTarget;
    
    [Header("Mode Switch")]
    public KeyCode switchViewKey = KeyCode.V;
    public bool startInThirdPerson = false;
    
    [Header("Camera References")]
    public FirstPersonCamMouse firstPersonCamMouse;
    public GameObject thirdPersonAimCamera;

    [Header("Optional - Disable while in TPS")]
    public MonoBehaviour firstPersonFollowScript;
    
    [Header("Weapon Parenting (Optional)")]
    public Transform weaponModel;
    public Transform fpsWeaponAnchor;
    public Transform tpsWeaponAnchor;
    public Vector3 fpsWeaponLocalPosition;
    public Vector3 fpsWeaponLocalEuler;
    public Vector3 tpsWeaponLocalPosition;
    public Vector3 tpsWeaponLocalEuler;

    private bool isThirdPerson;
    private float thirdPersonPitch;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (thirdPersonPitchPivot != null)
        {
            float startPitch = thirdPersonPitchPivot.localEulerAngles.x;
            if (startPitch > 180f) startPitch -= 360f;
            thirdPersonPitch = Mathf.Clamp(startPitch, thirdPersonMinPitch, thirdPersonMaxPitch);
        }
        else if (thirdPersonPitchTarget != null)
        {
            float startPitch = thirdPersonPitchTarget.localEulerAngles.x;
            if (startPitch > 180f) startPitch -= 360f;
            thirdPersonPitch = Mathf.Clamp(startPitch, thirdPersonMinPitch, thirdPersonMaxPitch);
        }
        
        if (thirdPersonPitchTarget == null && thirdPersonAimCamera != null)
        {
            thirdPersonPitchTarget = thirdPersonAimCamera.transform;
        }
        
        ApplyCameraMode(startInThirdPerson);
    }

    private void Update()
    {
        if (Input.GetKeyDown(switchViewKey))
        {
            ApplyCameraMode(!isThirdPerson);
        }

        // TPS camera controls player facing while moving.
        if (!isThirdPerson)
        {
            return;
        }

        Transform activeCam = cameraTransform != null ? cameraTransform : Camera.main != null ? Camera.main.transform : null;
        if (activeCam == null)
        {
            return;
        }
        
        // Mouse yaw drives TPS heading so camera does not feel locked.
        float mouseX = Input.GetAxis("Mouse X") * thirdPersonMouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * thirdPersonMouseYSensitivity * Time.deltaTime;
        
        if (Mathf.Abs(mouseX) > 0.0001f && player != null)
        {
            player.Rotate(Vector3.up * mouseX, Space.World);
        }
        
        if (thirdPersonPitchPivot != null)
        {
            thirdPersonPitch -= mouseY;
            thirdPersonPitch = Mathf.Clamp(thirdPersonPitch, thirdPersonMinPitch, thirdPersonMaxPitch);
            thirdPersonPitchPivot.localRotation = Quaternion.Euler(thirdPersonPitch, 0f, 0f);
        }
        else if (thirdPersonPitchTarget != null)
        {
            thirdPersonPitch -= mouseY;
            thirdPersonPitch = Mathf.Clamp(thirdPersonPitch, thirdPersonMinPitch, thirdPersonMaxPitch);
            Vector3 localEuler = thirdPersonPitchTarget.localEulerAngles;
            localEuler.x = thirdPersonPitch;
            thirdPersonPitchTarget.localEulerAngles = localEuler;
        }

        if (player != null)
        {
            Vector3 playerForward = player.forward;
            playerForward.y = 0f;
            if (playerForward.sqrMagnitude > 0.0001f)
            {
                orientation.forward = playerForward.normalized;
            }
        }

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (inputDir != Vector3.zero)
        {
            playerObject.forward = Vector3.Slerp(playerObject.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }

        if (player != null && playerObject != null)
        {
            Vector3 targetForward = inputDir != Vector3.zero ? inputDir : orientation.forward;
            targetForward.y = 0f;
            if (targetForward.sqrMagnitude > 0.0001f)
            {
                player.forward = Vector3.Slerp(player.forward, targetForward.normalized, Time.deltaTime * rotationSpeed);
                playerObject.forward = Vector3.Slerp(playerObject.forward, targetForward.normalized, Time.deltaTime * rotationSpeed);
            }
        }
    }

    private void ApplyCameraMode(bool enableThirdPerson)
    {
        isThirdPerson = enableThirdPerson;

        if (thirdPersonAimCamera != null)
        {
            thirdPersonAimCamera.SetActive(isThirdPerson);
        }

        if (!isThirdPerson && fpsCameraAnchor != null && cameraTransform != null)
        {
            cameraTransform.position = fpsCameraAnchor.position;
        }

        if (firstPersonCamMouse != null)
        {
            firstPersonCamMouse.SetCameraInputEnabled(!isThirdPerson);
        }

        if (firstPersonFollowScript != null)
        {
            firstPersonFollowScript.enabled = !isThirdPerson;
        }
        
        ApplyWeaponParentForMode();
    }
    
    private void ApplyWeaponParentForMode()
    {
        if (weaponModel == null)
        {
            return;
        }

        Transform targetAnchor = isThirdPerson ? tpsWeaponAnchor : fpsWeaponAnchor;
        if (targetAnchor == null)
        {
            return;
        }

        weaponModel.SetParent(targetAnchor, false);

        if (isThirdPerson)
        {
            weaponModel.localPosition = tpsWeaponLocalPosition;
            weaponModel.localEulerAngles = tpsWeaponLocalEuler;
        }
        else
        {
            weaponModel.localPosition = fpsWeaponLocalPosition;
            weaponModel.localEulerAngles = fpsWeaponLocalEuler;
        }
    }
}
