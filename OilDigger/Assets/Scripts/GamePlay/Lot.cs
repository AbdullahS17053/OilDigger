using UnityEngine;

public class Lot : MonoBehaviour
{
    #region Variables
    public int oilChance;
    private int dailyProduction = 0;
    private bool isSurveyed = false;
    private bool isDrilled = false;
    private bool isTurnGoing = false;
    [SerializeField] private GameObject image;
    #endregion
    #region Properties
    public bool IsSurveyed => isSurveyed;
    public bool IsDrilled => isDrilled;
    public bool IsProducing() => isDrilled && dailyProduction > 0;
    public int GetDailyProduction() => dailyProduction;
    public bool IsTurnGoing => isTurnGoing;



    public void Awake()
    {

    }

    void Start()
    {
        oilChance = Random.Range(0, 101);
        float baseProduction = oilChance / 100f * 10f;
        int randomOffset = Random.Range(-3, 4);
        dailyProduction = Mathf.Clamp(Mathf.RoundToInt(baseProduction + randomOffset), 0, 10);
    }

    #endregion
    #region Methods

    public bool Survey(bool isMultiSurvey = false)
    {
        // Skip the turn check for additional surveys in multi-survey operations
        if (isSurveyed || isDrilled || (!isMultiSurvey && GameManager.Instance.HasInteractedThisTurn)) return false;

        if (!GameManager.Instance.TrySpend(40000))
        {
            AudioManager.Instance.Play("Error");
            return false;
        }

        isSurveyed = true;
        AudioManager.Instance.Play("Survey");
        
        // Only set these for the primary survey, not for additional ones
        if (!isMultiSurvey)
        {
            GameManager.Instance.isTurnGoing = true;
            isTurnGoing = true;
        }

        TankManager.Instance.AddNotification(GameManager.Instance.CurrentTurn, $"Survey Result : {oilChance} % chance of oil.");
  
        // GameManager.Instance.RegisterInteraction();

        return true;
    }

    public bool Drill(bool isMultiDrill = false)
    {
        // Skip the turn check for additional drills in multi-drill operations
        if (isDrilled || (!isMultiDrill && GameManager.Instance.HasInteractedThisTurn)) return false;

        if (!GameManager.Instance.TrySpend(250000))
        {
            AudioManager.Instance.Play("Error");
            return false;
        }

        isDrilled = true;
        isTurnGoing = false;

        // Only register the interaction for the primary drill, not for additional ones
        if (!isMultiDrill)
        {
            GameManager.Instance.RegisterInteraction();
        }
        
        if (IsProducing())
        {
            TankManager.Instance.RegisterProducingLot(this);
        }

        // Enable the "Drill" child object
        Transform drillChild = transform.Find("Drill");
        if (drillChild != null)
        {
            drillChild.gameObject.SetActive(true);
            AudioManager.Instance.Play("Drill");
        }

        return true;
    }

    public void SetSelected(bool isSelected)
    {
        if (image != null)
            image.SetActive(isSelected);
    }

    #endregion
}
