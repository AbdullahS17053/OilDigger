using UnityEngine;

public class Lot : MonoBehaviour
{
    private int oilChance;
    private int dailyProduction = 0;
    private bool isSurveyed = false;
    private bool isDrilled = false;
    private bool isSkipped = false;
    private bool isTurnGoing = false;

    public bool IsSurveyed => isSurveyed;
    public bool IsDrilled => isDrilled;
    public bool IsSkipped => isSkipped;
    public bool IsProducing() => isDrilled && dailyProduction > 0;
    public int GetDailyProduction() => dailyProduction;
    public bool IsTurnGoing => isTurnGoing;

    public void Awake()
    {
        oilChance = Random.Range(0, 101);
        float baseProduction = oilChance / 100f * 10f;
        int randomOffset = Random.Range(-3, 4);
        dailyProduction = Mathf.Clamp(Mathf.RoundToInt(baseProduction + randomOffset), 0, 10);
    }

    public bool Survey()
    {
        if (isSurveyed || isDrilled || GameManager.Instance.HasInteractedThisTurn) return false;

        if (!GameManager.Instance.TrySpend(40000)) return false;

        isSurveyed = true;
        isTurnGoing = true;
        GameManager.Instance.isInteractionGoing = true;
        // GetComponent<SpriteRenderer>().color = Color.green;

        Debug.Log($"{name} surveyed. Oil chance: {oilChance}%");
        return true;
    }

    public bool Drill()
    {
        if (isDrilled || GameManager.Instance.HasInteractedThisTurn) return false;

        if (!GameManager.Instance.TrySpend(250000)) return false;

        isDrilled = true;
        isTurnGoing = false;
        GameManager.Instance.isInteractionGoing = false;

        // GetComponent<SpriteRenderer>().color = Color.blue;

        GameManager.Instance.RegisterInteraction();
        if (IsProducing())
        {
            GameManager.Instance.RegisterProducingLot(this);
        }

        // Enable the "Drill" child object
        Transform drillChild = transform.Find("Drill");
        if (drillChild != null)
        {
            drillChild.gameObject.SetActive(true);
        }

        Debug.Log($"{name} drilled. Producing {dailyProduction} barrels/day.");
        return true;
    }

    public bool Skip()
    {
        if (GameManager.Instance.HasInteractedThisTurn) return false;

        isSkipped = true;
        isTurnGoing = false;
        GameManager.Instance.isInteractionGoing = false;

        // Enable the "Skip" child object
        Transform skipChild = transform.Find("Skip");
        if (skipChild != null)
        {
            skipChild.gameObject.SetActive(true);
        }
        // GetComponent<SpriteRenderer>().color = Color.red;
        GameManager.Instance.RegisterInteraction();
        Debug.Log($"{name} skipped.");
        return true;
    }
}
