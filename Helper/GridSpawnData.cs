using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridSpawnData : MonoBehaviour
{
    [SerializeField] private Tilemap spawnTilemap;
    [SerializeField] private Tilemap cliffTilemap;
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private int prefabNum = 30;

    private int[,] invalidCount;
    private HashSet<Vector2Int> validPositions;
    private Vector2Int gridOffset;

    // Start is called before the first frame update
    void Start()
    {
        validPositions = new HashSet<Vector2Int>();
        invalidCount = new int[spawnTilemap.size.x, spawnTilemap.size.y];

        ScanForValidPositions();
        if (validPositions.Count > 0)
        {
            Spawner.Instance.RegisterSpawnPositions(gameObject.name, validPositions, invalidCount, gridOffset);
        }
        else
        {
            Debug.LogWarning($"No spawn positions found for grid: {gameObject.name}");
        }

        for (int i = 0; i < prefabNum; i++)
        {
            Spawner.Instance.SpawnOnePrefab(gameObject.name, prefabToSpawn);
        }
    }

    void ScanForValidPositions()
    {
        BoundsInt bounds = spawnTilemap.cellBounds;
        gridOffset = new Vector2Int(bounds.xMin, bounds.yMin);
        
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                
                if (spawnTilemap.HasTile(cellPosition))
                {
                    if (!cliffTilemap.HasTile(cellPosition))
                    {
                        invalidCount[x - bounds.xMin, y - bounds.yMin] = 0;
                        validPositions.Add(new Vector2Int(x, y));
                    }
                    else
                    {
                        invalidCount[x - bounds.xMin, y - bounds.yMin] = -1;
                    }
                }
            }
        }
    }
}
