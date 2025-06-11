using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LotUI lotUI;

    private void Update()
    {
        // Don't process input if clicking over UI
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null)
            {
                Lot lot = hit.collider.GetComponent<Lot>();
                if (lot != null)
                {
                    lotUI.Show(lot, hit.collider.transform.position);
                }
            }
        }
    }
}
