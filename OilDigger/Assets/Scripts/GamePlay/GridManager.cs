using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] private int rows = 6;
    [SerializeField] private int columns = 5;
    [SerializeField] private float horizontalSpacing = 10f;
    [SerializeField] private float verticalSpacing = 10f;

    [Header("Lot Prefab")]
    [SerializeField] private GameObject lotPrefab;

    [Header("Props to Spawn")]
    [SerializeField] private GameObject[] prefabsToSpawn;

    private Lot[,] gridArray;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        GenerateGrid();
    }

    #region Grid Generation
    private void GenerateGrid()
    {
        if (lotPrefab == null)
        {
            Debug.LogError("Lot Prefab is not assigned!");
            return;
        }

        if (prefabsToSpawn == null || prefabsToSpawn.Length == 0)
        {
            Debug.LogWarning("No props assigned to spawn.");
        }

        gridArray = new Lot[columns, rows];

        // Offset to center the grid
        float offsetX = (columns - 1) * horizontalSpacing * 0.5f;
        float offsetZ = (rows - 1) * verticalSpacing * 0.5f;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Vector3 worldPos = new Vector3(
                    x * horizontalSpacing - offsetX,
                    0f,
                    y * verticalSpacing - offsetZ
                );

                GameObject lotObj = Instantiate(lotPrefab, worldPos, Quaternion.Euler(0, 90, 0), transform);
                lotObj.name = $"Lot_{x}_{y}";

                Lot lot = lotObj.GetComponent<Lot>();
                if (lot != null)
                {
                    int oilChance = Random.Range(0, 101);
                    gridArray[x, y] = lot;
                }

                // Try spawning a prop
                TrySpawnProp(lotObj);
                RegisterTankPositions(lotObj);
            }
        }
    }
    #endregion

    #region Grid Operations
    // Find the grid coordinates of a lot
    public bool TryGetLotCoordinates(Lot lot, out int x, out int y)
    {
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                if (gridArray[i, j] == lot)
                {
                    x = i;
                    y = j;
                    return true;
                }
            }
        }

        x = -1;
        y = -1;
        return false;
    }

    // Get adjacent lots (up, down, left, right)
    public List<Lot> GetAdjacentLots(Lot lot, bool includeDiagonals = false)
    {
        List<Lot> adjacentLots = new List<Lot>();

        if (!TryGetLotCoordinates(lot, out int x, out int y))
            return adjacentLots;

        // Check orthogonal directions (up, down, left, right)
        if (x > 0) adjacentLots.Add(gridArray[x - 1, y]);                // Left
        if (x < columns - 1) adjacentLots.Add(gridArray[x + 1, y]);      // Right
        if (y > 0) adjacentLots.Add(gridArray[x, y - 1]);                // Down
        if (y < rows - 1) adjacentLots.Add(gridArray[x, y + 1]);         // Up

        // Optionally check diagonal directions
        if (includeDiagonals)
        {
            if (x > 0 && y > 0) adjacentLots.Add(gridArray[x - 1, y - 1]);             // Bottom-Left
            if (x < columns - 1 && y > 0) adjacentLots.Add(gridArray[x + 1, y - 1]);    // Bottom-Right
            if (x > 0 && y < rows - 1) adjacentLots.Add(gridArray[x - 1, y + 1]);       // Top-Left
            if (x < columns - 1 && y < rows - 1) adjacentLots.Add(gridArray[x + 1, y + 1]); // Top-Right
        }

        return adjacentLots;
    }

    // Get lots within a certain radius (in grid cells, not distance)
    public List<Lot> GetLotsInRadius(Lot centerLot, int radius)
    {
        List<Lot> lotsInRadius = new List<Lot>();
        
        if (!TryGetLotCoordinates(centerLot, out int centerX, out int centerY))
            return lotsInRadius;
        
        for (int x = Mathf.Max(0, centerX - radius); x <= Mathf.Min(columns - 1, centerX + radius); x++)
        {
            for (int y = Mathf.Max(0, centerY - radius); y <= Mathf.Min(rows - 1, centerY + radius); y++)
            {
                // Skip the center lot
                if (x == centerX && y == centerY)
                    continue;
                
                // Use Manhattan distance for radius check
                int distance = Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY);
                if (distance <= radius)
                {
                    lotsInRadius.Add(gridArray[x, y]);
                }
            }
        }
        
        return lotsInRadius;
    }

    // Get the most promising lots based on oil chance
    public List<Lot> GetMostPromisingLots(List<Lot> lots, int count)
    {
        // Sort by oil chance (highest to lowest)
        lots.Sort((a, b) => b.oilChance.CompareTo(a.oilChance));
        
        // Return the top 'count' lots or all if there are fewer than 'count'
        return lots.GetRange(0, Mathf.Min(count, lots.Count));
    }
    #endregion

    #region Prop Spawning
    private void TrySpawnProp(GameObject lotObj)
    {
        if (prefabsToSpawn.Length == 0) return;

        Transform propsParent = lotObj.transform.Find("Props");
        if (propsParent == null)
        {
            Debug.LogWarning($"Props object not found in {lotObj.name}");
            return;
        }

        // Get available prop positions
        List<Transform> availableSpots = new List<Transform>();
        for (int i = 1; i <= 4; i++)
        {
            Transform t = propsParent.Find($"Prop_{i}");
            if (t != null) availableSpots.Add(t);
        }

        // Decide how many props to spawn on this lot
        int spawnCount = GetSmartSpawnCount();

        // Randomly pick positions to spawn
        for (int i = 0; i < spawnCount && availableSpots.Count > 0; i++)
        {
            int randPosIndex = Random.Range(0, availableSpots.Count);
            Transform chosenPos = availableSpots[randPosIndex];

            GameObject randomPrefab = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Length)];
            Instantiate(randomPrefab, chosenPos.position, chosenPos.rotation, chosenPos);

            availableSpots.RemoveAt(randPosIndex); // prevent duplicate usage
        }
    }

    // Spawns 0–3 props per lot, with weights favoring 1–2
    private int GetSmartSpawnCount()
    {
        float roll = Random.value;
        if (roll < 0.15f) return 0;        // 15% chance: empty
        else if (roll < 0.6f) return 1;    // 45% chance: 1 prop
        else if (roll < 0.9f) return 2;    // 30% chance: 2 props
        else return 3;                    // 10% chance: 3 props
    }

    #endregion

    #region Tank Position Registration
    private void RegisterTankPositions(GameObject lotObj)
    {
        if (TankManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found.");
            return;
        }

        if (TankManager.Instance.tankTransforms == null)
        {
            TankManager.Instance.tankTransforms = new List<Transform>();
        }

        Transform tankParent = lotObj.transform.Find("Tank Positions");
        if (tankParent == null)
        {
            Debug.LogWarning($"Tank Positions not found in {lotObj.name}");
            return;
        }

        for (int i = 1; i <= 4; i++)
        {
            Transform tankPos = tankParent.Find($"Tank_{i}");
            if (tankPos != null)
            {
                TankManager.Instance.tankTransforms.Add(tankPos);
            }
            else
            {
                Debug.LogWarning($"Tank_{i} not found in {tankParent.name}");
            }
        }
    }
    #endregion
}
