using UnityEngine;

public class CameraPan : MonoBehaviour
{
    public float panSpeed = 10f;
    public float zoomSpeed = 20f;
    public float minZoom = 5f;
    public float maxZoom = 50f;

    private Vector3 lastMousePosition;

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
            Vector3 move = new Vector3(delta.x, 0, delta.y) * panSpeed * Time.deltaTime;
            transform.Translate(move, Space.World);
            lastMousePosition = Input.mousePosition;
        }

        // Zooming
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 pos = transform.position;
            pos.y -= scroll * zoomSpeed;
            pos.y = Mathf.Clamp(pos.y, minZoom, maxZoom);
            transform.position = pos;
        }
    }
}