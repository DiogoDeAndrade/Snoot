using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MapArea : MonoBehaviour
{
    [SerializeField] private bool       autoGenerate;
    [SerializeField] private Vector2Int minMaxLights = new Vector2Int(1, 3);
    [SerializeField] private Light2D    lightPrefab;
    [SerializeField] private Vector2Int minMaxObstacles = new Vector2Int(2, 4);
    [SerializeField] private Obstacle[] obstaclePrefabs;

    void Start()
    {
        if (autoGenerate)
        {
            var gridPos = MapAreaManager.ToGrid(transform.position);
            float difficulty = 1.0f + Vector2Int.Distance(Vector2Int.zero, gridPos) / 5.0f;

            if (lightPrefab)
            {
                int r = Random.Range(minMaxLights.x, minMaxLights.y + 1);

                for (int i = 0; i < r; i++)
                {
                    var newLight = Instantiate(lightPrefab, transform);
                    newLight.transform.localPosition = new Vector3(Random.Range(-MapAreaManager.areaHalfSize, MapAreaManager.areaHalfSize),
                                                                   Random.Range(-MapAreaManager.areaHalfSize, MapAreaManager.areaHalfSize), 
                                                                   0.0f);
                }
            }

            if ((obstaclePrefabs != null) && (obstaclePrefabs.Length > 0)) 
            {
                int r = Mathf.FloorToInt(Random.Range(minMaxObstacles.x * difficulty, minMaxObstacles.y * difficulty));

                for (int i = 0; i < r; i++)
                {
                    int o = Random.Range(0, obstaclePrefabs.Length);

                    var newLight = Instantiate(obstaclePrefabs[o], transform);
                    newLight.transform.localPosition = new Vector3(Random.Range(-MapAreaManager.areaHalfSize, MapAreaManager.areaHalfSize),
                                                                   Random.Range(-MapAreaManager.areaHalfSize, MapAreaManager.areaHalfSize),
                                                                   0.0f);
                    newLight.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(0, 360));
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
