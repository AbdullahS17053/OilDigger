using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LotUI lotUI;

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject(0)) return; // Ignore UI touches

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            TrySelectObject(Input.mousePosition);
#else
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
            TrySelectObject(Input.touches[0].position);
#endif
    }

    void TrySelectObject(Vector2 screenPosition)
    {
        Debug.Log("here");
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("here1");

            if (hit.collider.CompareTag("lot"))
            {
                Debug.Log("here2");

                Debug.Log("Lot selected: " + hit.collider.name);
                Lot lot = hit.collider.GetComponent<Lot>();
                Vector3 offsetPoint = hit.point + new Vector3(0, 0.5f, 0); // raise the UI a bit
                Vector2 screenPos = mainCamera.WorldToScreenPoint(offsetPoint);
                if (lot != null)
                {
                    if (GameManager.Instance.isInteractionGoing)
                    {
                        if (lot.IsTurnGoing)
                        {
                            lotUI.Show(lot, screenPos);
                        }
                    }
                    else
                        lotUI.Show(lot, screenPos);
                }
            }
        }
    }
}