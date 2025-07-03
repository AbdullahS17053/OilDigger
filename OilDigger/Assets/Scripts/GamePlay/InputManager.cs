using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] private Camera mainCamera;
    public Lot selectedLot = null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


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
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {

            if (hit.collider.CompareTag("lot"))
            {
                // Debug.Log("Lot selected: " + hit.collider.name);
                Lot lot = hit.collider.GetComponent<Lot>();
                Vector3 offsetPoint = hit.point + new Vector3(0, 0.5f, 0); // raise the UI a bit
                Vector2 screenPos = mainCamera.WorldToScreenPoint(offsetPoint);
                if (lot != null)
                {
                    if (GameManager.Instance != null)
                    {
                        if (GameManager.Instance.isInteractionGoing)
                        {
                            if (lot.IsTurnGoing)
                            {
                                OpsHandler.Instance.Show(lot);
                                if (selectedLot != null && selectedLot != lot)
                                {
                                    selectedLot.SetSelected(false);
                                }

                                selectedLot = lot;
                                selectedLot.SetSelected(true);

                            }
                        }

                        else
                        {
                            OpsHandler.Instance.Show(lot);
                            if (selectedLot != null && selectedLot != lot)
                            {
                                selectedLot.SetSelected(false);
                            }

                            selectedLot = lot;
                            selectedLot.SetSelected(true);

                        }
                    }

                }
            }
        }
    }
}