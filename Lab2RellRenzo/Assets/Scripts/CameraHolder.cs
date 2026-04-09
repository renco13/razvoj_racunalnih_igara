using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    public Transform cameraPosition;

    void Update()
    {
        transform.position = cameraPosition.position;
    }
}
