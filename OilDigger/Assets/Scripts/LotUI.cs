using UnityEngine;
using UnityEngine.UI;
public class LotUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button surveyButton;
    [SerializeField] private Button drillButton;
    [SerializeField] private Button skipButton;

    private Lot currentLot;

    public void Show(Lot lot, Vector2 position)
    {
        if (GameManager.Instance.HasInteractedThisTurn) return;

        currentLot = lot;
        panel.transform.position = Camera.main.WorldToScreenPoint(position);

        if(!lot.IsDrilled && !lot.IsSkipped)
            panel.SetActive(true);
        if(lot.IsDrilled || lot.IsSkipped)
            panel.SetActive(false);

        surveyButton.gameObject.SetActive(!lot.IsSurveyed && !lot.IsDrilled && !lot.IsSkipped && GameManager.Instance.Money >= 40000);
        drillButton.gameObject.SetActive(!lot.IsDrilled && !lot.IsSkipped && GameManager.Instance.Money >= 250000);
        skipButton.gameObject.SetActive((lot.IsSurveyed && !lot.IsDrilled) || (!lot.IsSurveyed && !lot.IsDrilled));
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    private void Start()
    {
        panel.SetActive(false);

        surveyButton.onClick.AddListener(() =>
        {
            currentLot?.Survey();
            Hide();
        });

        drillButton.onClick.AddListener(() =>
        {
            if(currentLot.Drill())
                GameManager.Instance.EndTurn();
            Hide();
        });

        skipButton.onClick.AddListener(() =>
        {
            if(currentLot.Skip())
                GameManager.Instance.EndTurn();
            Hide();
        });
    }
}
