using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MapArea : MonoBehaviour
{
    [SerializeField] private bool       autoGenerate;
    [SerializeField] private Light2D    lightPrefab;
    [SerializeField] private Vector2Int minMaxLights = new Vector2Int(1, 3);

    void Start()
    {
        if (autoGenerate)
        {
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
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
