using UnityEngine;

public class CameraPan : MonoBehaviour
{
    public float panSpeed = 10f;
    public float zoomSpeed = 10f;
    public float minOrthoSize = 5f;
    public float maxOrthoSize = 50f;

    private Vector3 lastMousePosition;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        if (cam == null || !cam.orthographic)
        {
            Debug.LogWarning("CameraPan script requires an orthographic Camera component.");
        }
    }

    void Update()
    {
        // Panning
        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-delta.x, 0, -delta.y) * panSpeed * Time.deltaTime;
            transform.Translate(move, Space.World);
            lastMousePosition = Input.mousePosition;
        }

        // Zooming (adjust orthographic size)
        if (cam != null && cam.orthographic)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                cam.orthographicSize -= scroll * zoomSpeed;
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minOrthoSize, maxOrthoSize);
            }
        }
    }
}