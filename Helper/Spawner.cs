using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridData
{
    public Vector2Int gridOffset;
    public int[,] invalidCount;
    public HashSet<Vector2Int> validPositions;
}

public class Spawner : MonoBehaviour
{
    public static Spawner Instance { get; private set; }

    private Dictionary<string, GridData> dataByGrid;

    [SerializeField] private int minDistance = 5;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            dataByGrid = new Dictionary<string, GridData>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnOnePrefab(string gridName, GameObject prefab)
    {
        Vector2Int spawnPosition = GetRandomSpawnPosition(gridName);
        if (spawnPosition != Vector2Int.zero)
        {
            Instantiate(prefab, new Vector3(spawnPosition.x, spawnPosition.y, 0), Quaternion.identity);
        }
    }

    public void RegisterSpawnPositions(string gridName, HashSet<Vector2Int> positions, int[,] invalidCount, Vector2Int gridOffset)
    {
        if (!dataByGrid.ContainsKey(gridName))
        {
            GridData data = new GridData()
            {
                validPositions = positions,
                invalidCount = invalidCount,
                gridOffset = gridOffset
            };
            dataByGrid.Add(gridName, data);
        }
    }

    Vector2Int GetRandomSpawnPosition(string gridName)
    {
        if (!dataByGrid.ContainsKey(gridName) || dataByGrid[gridName].validPositions.Count == 0)
        {
            return Vector2Int.zero;
        }

        GridData gridData = dataByGrid[gridName];

        Vector2Int[] positionsArray = new Vector2Int[gridData.validPositions.Count];
        gridData.validPositions.CopyTo(positionsArray, 0);
        Vector2Int randomPosition = positionsArray[Random.Range(0, positionsArray.Length)];

        InvalidateSurroundingPositions(randomPosition, gridName);
        return randomPosition;
    }

    void InvalidateSurroundingPositions(Vector2Int position, string gridName)
    {
        if (!dataByGrid.ContainsKey(gridName)) return;

        GridData data = dataByGrid[gridName];
        int arrayX = position.x - data.gridOffset.x;
        int arrayY = position.y - data.gridOffset.y;

        int minX = Mathf.Max(0, arrayX - minDistance);
        int maxX = Mathf.Min(data.invalidCount.GetLength(0) - 1, arrayX + minDistance);
        int minY = Mathf.Max(0, arrayY - minDistance);
        int maxY = Mathf.Min(data.invalidCount.GetLength(1) - 1, arrayY + minDistance);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (data.invalidCount[x, y] >= 0)
                {
                    if (data.invalidCount[x, y] == 0)
                    {
                        Vector2Int pos = new Vector2Int(x + data.gridOffset.x, y + data.gridOffset.y);
                        data.validPositions.Remove(pos);
                    }
                    data.invalidCount[x, y]++;
                }
            }
        }
    }
}
