using UnityEngine;

public class FirstPersonCamMouse : MonoBehaviour
{
    // public Transform player;
    // public float mousesens = 100f;
    // public float xRotation = 0f;

    public float sensX;
    public float sensY;

    public Transform orientation;
    public bool cameraInputEnabled = true;

    float xRotation;
    float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!cameraInputEnabled)
        {
            return;
        }

        // float mouseX = Input.GetAxis("Mouse X") * mousesens * Time.deltaTime;
        // float mouseY = Input.GetAxis("Mouse Y") * mousesens * Time.deltaTime;

        // xRotation -= mouseY;
        // xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        // player.Rotate(player.up, mouseX, Space.Self);

        // Spremanje mouse inputa
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -60f, 60f);

        // Rotacija kamere i pracnje smjera
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void SetCameraInputEnabled(bool enabled)
    {
        if (enabled && !cameraInputEnabled)
        {
            SyncRotationFromCurrentView();
        }

        cameraInputEnabled = enabled;
    }

    private void SyncRotationFromCurrentView()
    {
        if (orientation != null)
        {
            yRotation = orientation.eulerAngles.y;
        }

        float currentX = transform.eulerAngles.x;
        if (currentX > 180f)
        {
            currentX -= 360f;
        }

        xRotation = Mathf.Clamp(currentX, -60f, 60f);
    }
}
