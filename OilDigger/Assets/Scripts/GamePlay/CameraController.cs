using UnityEngine;

public class CameraPan : MonoBehaviour
{
    public float panSpeed = 10f;
    public float zoomSpeed = 10f;
    public float minOrthoSize = 5f;
    public float maxOrthoSize = 50f;

    private Vector3 lastMousePosition;
    private Camera cam;
    
    // Mobile touch variables
    private Vector2 lastTouchPosition;
    private float lastTouchDistance;
    private bool isTouching = false;

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
        // Check if we're on mobile
        if (Input.touchCount > 0)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }
    }

    void HandleMouseInput()
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

    void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            // Single touch - panning
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPosition = touch.position;
                isTouching = true;
            }
            else if (touch.phase == TouchPhase.Moved && isTouching)
            {
                Vector2 delta = touch.position - lastTouchPosition;
                Vector3 move = new Vector3(-delta.x, 0, -delta.y) * panSpeed * Time.deltaTime;
                transform.Translate(move, Space.World);
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isTouching = false;
            }
        }
        else if (Input.touchCount == 2)
        {
            // Two touches - pinch to zoom
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            
            float currentDistance = Vector2.Distance(touch1.position, touch2.position);
            
            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                lastTouchDistance = currentDistance;
            }
            else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                float deltaDistance = currentDistance - lastTouchDistance;
                
                if (cam != null && cam.orthographic)
                {
                    cam.orthographicSize -= deltaDistance * zoomSpeed * Time.deltaTime;
                    cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minOrthoSize, maxOrthoSize);
                }
                
                lastTouchDistance = currentDistance;
            }
            
            isTouching = false; // Disable single touch panning during pinch
        }
    }
}