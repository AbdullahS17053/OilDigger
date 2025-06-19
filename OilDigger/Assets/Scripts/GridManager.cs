using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
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

                // int angle = Random.value < 0.5f ? 90 : 270;
                GameObject lotObj = Instantiate(lotPrefab, worldPos, Quaternion.Euler(0, 90, 0), transform);
                lotObj.name = $"Lot_{x}_{y}";

                Lot lot = lotObj.GetComponent<Lot>();
                if (lot != null)
                {
                    int oilChance = Random.Range(0, 101);
                    // lot.Initialize(oilChance);
                    gridArray[x, y] = lot;
                }

                // Try spawning a prop
                TrySpawnProp(lotObj);
                RegisterTankPositions(lotObj);
            }
        }

    }
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

    private void RegisterTankPositions(GameObject lotObj)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found.");
            return;
        }

        if (GameManager.Instance.tankTransforms == null)
        {
            GameManager.Instance.tankTransforms = new List<Transform>();
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
                GameManager.Instance.tankTransforms.Add(tankPos);
            }
            else
            {
                Debug.LogWarning($"Tank_{i} not found in {tankParent.name}");
            }
        }
    }

}
