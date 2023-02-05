using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using NaughtyAttributes;

public class MapArea : MonoBehaviour
{
    [SerializeField] private bool       autoGenerate;
    [SerializeField] private Vector2Int minMaxLights = new Vector2Int(1, 3);
    [SerializeField] private Light2D    lightPrefab;
    [SerializeField] private Vector2Int minMaxObstacles = new Vector2Int(2, 4);
    [SerializeField] private Obstacle[] obstaclePrefabs;
    [SerializeField] private Vector2Int minMaxWater = new Vector2Int(2, 4);
    [SerializeField] private Water[]    waterPrefabs;
    [SerializeField] private Vector2Int minMaxNutrients = new Vector2Int(12, 16);
    [SerializeField] private Nutrient[] nutrientPrefabs;
    [SerializeField] private Vector2Int minMaxInsects = new Vector2Int(12, 16);
    [SerializeField] private Insect[]   insectPrefabs;

    struct Circle
    {
        public Vector3  pos;
        public float    radius;
    }

    MapAreaManager mapManager;

    void Start()
    {
        mapManager = FindObjectOfType<MapAreaManager>();
        if (autoGenerate)
        {
            Generate();
        }
    }

    [Button("Generate")]
    void Generate()
    {
        mapManager = FindObjectOfType<MapAreaManager>();

        var children = new List<Transform>(transform.GetComponentsInChildren<Transform>());
        children.RemoveAll((t) => t.parent != transform);
        foreach (var t in children)
        {
            DestroyImmediate(t.gameObject);
        }

        List<Circle> circles = new List<Circle>();

        // Isolate all crystal
        var crystals = FindObjectsOfType<Crystal>();
        foreach (var c in crystals)
        {
            circles.Add(new Circle() { pos = c.transform.position, radius = 200 });
        }

        var gridPos = MapAreaManager.ToGrid(transform.position);
        float difficulty = 1.0f + Vector2Int.Distance(Vector2Int.zero, gridPos) / 5.0f;
        float invDifficulty = 1.0f - Mathf.Clamp(Vector2Int.Distance(Vector2Int.zero, gridPos) / 10.0f, 0.0f, 0.8f);

        var ahs = MapAreaManager.areaHalfSize * 0.8f;

        if (lightPrefab)
        {
            int r = Random.Range(minMaxLights.x, minMaxLights.y + 1);

            for (int i = 0; i < r; i++)
            {
                var newLight = Instantiate(lightPrefab, transform);
                newLight.transform.localPosition = new Vector3(Random.Range(-ahs, ahs),
                                                                Random.Range(-ahs, ahs), 
                                                                0.0f);
            }
        }

        if ((obstaclePrefabs != null) && (obstaclePrefabs.Length > 0)) 
        {
            int r = Mathf.FloorToInt(Random.Range(minMaxObstacles.x * difficulty, minMaxObstacles.y * difficulty));

            for (int i = 0; i < r; i++)
            {
                int o = Random.Range(0, obstaclePrefabs.Length);

                var newObstacle = Instantiate(obstaclePrefabs[o], transform);
                newObstacle.transform.localPosition = new Vector3(Random.Range(-ahs, ahs),
                                                                Random.Range(-ahs, ahs),
                                                                0.0f);
                newObstacle.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(0, 360));

                circles.Add(new Circle { pos = newObstacle.transform.position, radius = newObstacle.genRadius });
            }
        }

        if (gridPos.y == mapManager.yBounds.y)
        {
            // Upper boundary
            float dx = -1024.0f;
            while (dx < 1024.0f)
            {
                int o = Random.Range(0, obstaclePrefabs.Length);

                var newObstacle = Instantiate(obstaclePrefabs[o], transform);
                newObstacle.transform.localPosition = new Vector3(dx + Random.Range(-10.0f, 10.0f),
                                                                  980 - Random.Range(-10.0f, 10.0f),
                                                                  0.0f);
                newObstacle.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(0, 360));

                circles.Add(new Circle { pos = newObstacle.transform.position, radius = newObstacle.genRadius });

                dx = newObstacle.transform.localPosition.x + newObstacle.genRadius * 1.25f;
            }
        }

        if (gridPos.y == mapManager.yBounds.x)
        {
            // Lower boundary
            float dx = -1024.0f;
            while (dx < 1024.0f)
            {
                int o = Random.Range(0, obstaclePrefabs.Length);

                var newObstacle = Instantiate(obstaclePrefabs[o], transform);
                newObstacle.transform.localPosition = new Vector3(dx + Random.Range(-10.0f, 10.0f),
                                                                  - 980 + Random.Range(-10.0f, 10.0f),
                                                                  0.0f);
                newObstacle.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(0, 360));

                circles.Add(new Circle { pos = newObstacle.transform.position, radius = newObstacle.genRadius });

                dx = newObstacle.transform.localPosition.x + newObstacle.genRadius * 1.25f;
            }
        }

        if (gridPos.x <= mapManager.xBounds.x)
        {
            // Left boundary
            float dy = -1024.0f;
            while (dy < 1024.0f)
            {
                int o = Random.Range(0, obstaclePrefabs.Length);

                var newObstacle = Instantiate(obstaclePrefabs[o], transform);
                newObstacle.transform.localPosition = new Vector3(-980 - Random.Range(-10.0f, 10.0f),
                                                                  dy + Random.Range(-10.0f, 10.0f),
                                                                  0.0f);
                newObstacle.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(0, 360));

                circles.Add(new Circle { pos = newObstacle.transform.position, radius = newObstacle.genRadius });

                dy = newObstacle.transform.localPosition.y + newObstacle.genRadius * 1.25f;
            }
        }

        if (gridPos.x >= mapManager.xBounds.y)
        {
            // Right boundary
            float dy = -1024.0f;
            while (dy < 1024.0f)
            {
                int o = Random.Range(0, obstaclePrefabs.Length);

                var newObstacle = Instantiate(obstaclePrefabs[o], transform);
                newObstacle.transform.localPosition = new Vector3(980 - Random.Range(-10.0f, 10.0f),
                                                                  dy + Random.Range(-10.0f, 10.0f),
                                                                  0.0f);
                newObstacle.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(0, 360));

                circles.Add(new Circle { pos = newObstacle.transform.position, radius = newObstacle.genRadius });

                dy = newObstacle.transform.localPosition.y + newObstacle.genRadius * 1.25f;
            }
        }

        if ((waterPrefabs != null) && (waterPrefabs.Length > 0))
        {
            int r = Mathf.FloorToInt(Random.Range(minMaxWater.x * invDifficulty, minMaxWater.y * invDifficulty));

            for (int i = 0; i < r; i++)
            {
                int w = Random.Range(0, waterPrefabs.Length);

                var newWater = Instantiate(waterPrefabs[w], transform);

                bool    collision = true;
                int     nTries = 0;
                Vector3 tryPos;
                float   radius = newWater.genRadius;
                while ((collision) && (nTries++ < 20))
                {
                    collision = false;

                    tryPos = new Vector3(Random.Range(-ahs, ahs),
                                            Random.Range(-ahs, ahs),
                                            0.0f);

                    foreach (var c in circles)
                    {
                        if (Vector3.Distance(c.pos, tryPos) < (radius + c.radius))
                        {
                            collision = true;
                            break;
                        }
                    }

                    newWater.transform.localPosition = tryPos;
                    newWater.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(0, 360));
                }

                if (collision)
                {
                    Destroy(newWater);
                }
                else
                {
                    circles.Add(new Circle { pos = newWater.transform.position, radius = radius });
                }
            }
        }
        if ((nutrientPrefabs != null) && (nutrientPrefabs.Length > 0))
        {
            int r = Mathf.FloorToInt(Random.Range(minMaxNutrients.x * invDifficulty, minMaxNutrients.y * invDifficulty));

            for (int i = 0; i < r; i++)
            {
                int w = Random.Range(0, nutrientPrefabs.Length);

                var newNutrient = Instantiate(nutrientPrefabs[w], transform);

                bool collision = true;
                int nTries = 0;
                Vector3 tryPos;
                float radius = newNutrient.radius;
                while ((collision) && (nTries++ < 20))
                {
                    collision = false;

                    tryPos = new Vector3(Random.Range(-ahs, ahs),
                                            Random.Range(-ahs, ahs),
                                            0.0f);

                    foreach (var c in circles)
                    {
                        if (Vector3.Distance(c.pos, tryPos) < (radius + c.radius))
                        {
                            collision = true;
                            break;
                        }
                    }

                    newNutrient.transform.localPosition = tryPos;
                }

                if (collision)
                {
                    Destroy(newNutrient);
                }
                else
                {
                    circles.Add(new Circle { pos = newNutrient.transform.position, radius = radius });
                }
            }
        }
        if ((insectPrefabs != null) && (insectPrefabs.Length > 0))
        {
            int m1 = Mathf.CeilToInt(minMaxInsects.x * difficulty);
            int m2 = Mathf.CeilToInt(minMaxInsects.y * difficulty);
            int r = Random.Range(m1, Mathf.Max(m2, minMaxInsects.y) + 1);

            for (int i = 0; i < r; i++)
            {
                int w = Random.Range(0, insectPrefabs.Length);

                var newInsect = Instantiate(insectPrefabs[w], transform);

                bool collision = true;
                int nTries = 0;
                Vector3 tryPos;
                float radius = newInsect.genRadius;
                while ((collision) && (nTries++ < 20))
                {
                    collision = false;

                    tryPos = new Vector3(Random.Range(-ahs, ahs),
                                            Random.Range(-ahs, ahs),
                                            0.0f);

                    foreach (var c in circles)
                    {
                        if (Vector3.Distance(c.pos, tryPos) < (radius + c.radius))
                        {
                            collision = true;
                            break;
                        }
                    }

                    newInsect.transform.localPosition = tryPos;
                }

                if (collision)
                {
                    Destroy(newInsect);
                }
                else
                {
                    circles.Add(new Circle { pos = newInsect.transform.position, radius = radius });
                }
            }
        }
    }
}
