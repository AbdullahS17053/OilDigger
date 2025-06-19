using UnityEngine;

public class CameraController : MonoBehaviour
{

    public float panSpeed = 10f;

    private Vector3 lastMousePosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
         if (Input.GetMouseButtonDown(1)) // Right mouse button pressed
        {
            // Debug.Log("CameraController Update called");
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1)) // Right mouse button held
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            // Move along X (horizontal) and Z (vertical in world space)
            Vector3 move = new Vector3(-delta.x, 0, -delta.y) * panSpeed * Time.deltaTime;

            transform.Translate(move, Space.World);

            lastMousePosition = Input.mousePosition;
        }
    }
}
