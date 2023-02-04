using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAreaManager : MonoBehaviour
{
    [SerializeField] private Camera     gameCamera;
    [SerializeField] private MapArea    areaPrefab;

    Dictionary<Vector2Int, MapArea> areas;
    float                           cameraWidth;
    float                           cameraHeight;

    public const float areaSize = 2048.0f;
    public const float areaHalfSize = areaSize * 0.5f;

    void Start()
    {
        areas = new Dictionary<Vector2Int, MapArea>();

        var spawnedAreas = GetComponentsInChildren<MapArea>();
        foreach (var a in spawnedAreas)
        {
            var pos = ToGrid(a.transform.position);
            if (areas.ContainsKey(pos))
            {
                Debug.LogError("Area already exists in map!");
            }
            else
            {
                areas[pos] = a;
                a.name = $"Area ({pos.x}, {pos.y})";
            }
        }

        cameraHeight = gameCamera.orthographicSize * 2.0f;
        cameraWidth = cameraHeight * (1280.0f / 720.0f);
    }

    void Update()
    {
        Vector2 cameraPos = gameCamera.transform.position;
        Vector2 min = cameraPos - new Vector2(cameraWidth, cameraHeight);
        Vector2 max = cameraPos + new Vector2(cameraWidth, cameraHeight);

        Vector2Int gridMin = ToGrid(min);
        Vector2Int gridMax = ToGrid(max);
        
        for (int dy = gridMin.y; dy <= gridMax.y; dy++)
        {
            for (int dx = gridMin.x; dx <= gridMax.x; dx++)
            {
                var areaPos = new Vector2Int(dx, dy);
                if (areas.ContainsKey(areaPos))
                {
                    continue;
                }
                // Spawn a new area at this grid position
                var newArea = Instantiate(areaPrefab, new Vector3(dx * areaSize, dy * areaSize, 0.0f), Quaternion.identity);
                newArea.name = $"Area ({dx}, {dy})";
                newArea.transform.SetParent(transform);

                areas.Add(areaPos, newArea);
            }
        }

        var currentArea = ToGrid(cameraPos);

        foreach (var a in areas)
        {
            if (Vector2Int.Distance(a.Key, currentArea) > 2)
            {
                // Disable area
                a.Value.gameObject.SetActive(false);
            }
            else if (!a.Value.gameObject.activeSelf)
            {
                a.Value.gameObject.SetActive(true);
            }
        }
    }

    static public Vector2Int ToGrid(Vector2 p)
    {
        return new Vector2Int(Mathf.FloorToInt((p.x + areaHalfSize) / areaSize), Mathf.FloorToInt((p.y + areaHalfSize) / areaSize));
    }
}
