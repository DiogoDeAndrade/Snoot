using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAreaManager : MonoBehaviour
{
    [SerializeField] private Camera     gameCamera;
    [SerializeField] private MapArea    areaPrefab;
    
    public Vector2Int   xBounds = new Vector2Int(-10, 10);
    public Vector2Int   yBounds = new Vector2Int(-10, 0);

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
        
        for (int dy = Mathf.Max(yBounds.x - 1, gridMin.y); dy <= Mathf.Min(yBounds.y, gridMax.y); dy++)
        {
            for (int dx = Mathf.Max(xBounds.x - 1, gridMin.x); dx <= Mathf.Min(xBounds.y + 1, gridMax.x); dx++)
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

    private void OnDrawGizmosSelected()
    {
        float x1 =  areaSize * xBounds.x - areaHalfSize;
        float x2 =  areaSize * xBounds.y + areaHalfSize;
        float y1 =  areaSize * yBounds.x - areaHalfSize;
        float y2 =  areaSize * yBounds.y + areaHalfSize;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(new Vector3(x1, y1, 0), new Vector3(x2, y1, 0));
        Gizmos.DrawLine(new Vector3(x2, y1, 0), new Vector3(x2, y2, 0));
        Gizmos.DrawLine(new Vector3(x2, y2, 0), new Vector3(x1, y2, 0));
        Gizmos.DrawLine(new Vector3(x1, y2, 0), new Vector3(x1, y1, 0));
    }

    static public Vector2Int ToGrid(Vector2 p)
    {
        return new Vector2Int(Mathf.FloorToInt((p.x + areaHalfSize) / areaSize), Mathf.FloorToInt((p.y + areaHalfSize) / areaSize));
    }
}
