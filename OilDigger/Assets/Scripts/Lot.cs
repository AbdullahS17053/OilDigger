using UnityEngine;

public class Lot : MonoBehaviour
{
    private int oilChance;
    private int dailyProduction =0;
    private bool isSurveyed = false;
    private bool isDrilled = false;
    private bool isSkipped = false;

    public bool IsSurveyed => isSurveyed;
    public bool IsDrilled => isDrilled;
    public bool IsSkipped => isSkipped;
    public bool IsProducing() => isDrilled && dailyProduction > 0;
    public int GetDailyProduction() => dailyProduction;


    public void Initialize(int chance)
    {
        oilChance = chance;
        float baseProduction = oilChance / 100f * 10f;
        int randomOffset = Random.Range(-3, 4);
        dailyProduction = Mathf.Clamp(Mathf.RoundToInt(baseProduction + randomOffset), 0, 10);
    }

    public void Survey()
    {
        if (isSurveyed || isDrilled || GameManager.Instance.HasInteractedThisTurn) return;

        if (!GameManager.Instance.TrySpend(40000)) return;

        isSurveyed = true;
        GetComponent<SpriteRenderer>().color = Color.green;

        Debug.Log($"{name} surveyed. Oil chance: {oilChance}%");
    }

    public void Drill()
    {
        if (isDrilled || GameManager.Instance.HasInteractedThisTurn) return;

        if (!GameManager.Instance.TrySpend(250000)) return;

        isDrilled = true;
        GetComponent<SpriteRenderer>().color = Color.blue;

        GameManager.Instance.RegisterInteraction();
        if(IsProducing())
        {
            GameManager.Instance.RegisterProducingLot(this);
        }

        Debug.Log($"{name} drilled. Producing {dailyProduction} barrels/day.");
    }

    public void Skip()
    {
        if (GameManager.Instance.HasInteractedThisTurn) return;

        isSkipped = true;
        GetComponent<SpriteRenderer>().color = Color.red;
        GameManager.Instance.RegisterInteraction();
        Debug.Log($"{name} skipped.");
    }
}
