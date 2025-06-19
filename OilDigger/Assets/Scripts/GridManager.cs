using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int rows = 6;
    [SerializeField] private int columns = 5;
    [SerializeField] private float tileSpacing = 10f;

    [Header("Lot Prefab")]
    [SerializeField] private GameObject lotPrefab;

    private Lot[,] gridArray;

    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        if (lotPrefab == null)
        {
            Debug.LogError("Lot Prefab is not assigned!");
            return;
        }

        gridArray = new Lot[columns, rows];

        // Offset to center the grid
        float offsetX = (columns - 1) * tileSpacing * 0.5f;
        float offsetZ = (rows - 1) * tileSpacing * 0.5f;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Vector3 worldPos = new Vector3(
                    x * tileSpacing - offsetX,
                    0f,
                    y * tileSpacing - offsetZ
                );

                GameObject lotObj = Instantiate(lotPrefab, worldPos, Quaternion.identity, transform);
                lotObj.name = $"Lot_{x}_{y}";

                Lot lot = lotObj.GetComponent<Lot>();
                if (lot != null)
                {
                    int oilChance = Random.Range(0, 101);
                    // lot.Initialize(oilChance);
                    gridArray[x, y] = lot;
                }
                else
                {
                    Debug.LogWarning($"Lot prefab is missing the Lot script! {lotObj.name}");
                }
            }
        }
    }
}
