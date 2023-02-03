using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private    float       baseMoveSpeed = 200.0f;
    [SerializeField] private    float       baseRotateSpeed = 180.0f;
    [SerializeField] private    float       baseThickness = 5.0f;
    [SerializeField] private    float       varianceThickness = 5.0f;
    [SerializeField] private    MeshFilter  bodyMeshFilter;
    [SerializeField] private    MeshFilter  headMeshFilter;

    private List<Vector3>   path;
    private Rigidbody2D     rb;
    private Mesh            bodyMesh;
    private List<Vector3>   bodyVertices;
    private List<Vector2>   bodyUV;
    private List<int>       bodyTriangles;
    private Mesh            headMesh;
    private List<Vector3>   headVertices;
    private List<Vector2>   headUV;
    private List<int>       headTriangles;

    private float           lastPointInsertedTime;
    private Vector3         lastPointInsertedDirection;
    private int             lastPointInsertedIndex;
    private int             meshLastPoint;
    private float           distance;

    void Start()
    {
        path = new List<Vector3>();
        
        bodyMesh = new Mesh();
        bodyMesh.name = "RootBody";
        bodyMeshFilter.mesh = bodyMesh;

        bodyVertices = new List<Vector3>();
        bodyTriangles = new List<int>();
        bodyUV = new List<Vector2>();

        headMesh = new Mesh();
        headMesh.name = "RootHead";
        headMeshFilter.mesh = headMesh;

        headVertices = new List<Vector3>()
        {
            Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero
        };
        headUV = new List<Vector2>()
        {
            Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero
        };
        headTriangles = new List<int>()
        {
            0, 1, 2, 0, 2, 3, 3, 2, 4
        };

        rb = GetComponent<Rigidbody2D>();

        // Run a second of head
        UpdateRoot();
        transform.position = transform.up * baseMoveSpeed;
        distance = baseMoveSpeed;
        UpdateRoot();
    }

    private void FixedUpdate()
    {
        distance += rb.velocity.magnitude * Time.fixedDeltaTime;
    }

    void Update()
    {
        float rotation = Input.GetAxis("Horizontal");

        transform.rotation = transform.rotation * Quaternion.Euler(0, 0, -rotation * baseRotateSpeed * Time.deltaTime);

        rb.velocity = transform.up * baseMoveSpeed;

        UpdateRoot();
    }

    void UpdateRoot()
    { 
        Vector3 currentPos = transform.position;

        // Here we should see if this point is necessary
        bool addPoint = false;
        if (path.Count > 0)
        {
            if ((Time.time - lastPointInsertedTime) < 1.0f)
            {
                Vector3 predictedPosition = path[path.Count - 1] + lastPointInsertedDirection * baseMoveSpeed * (Time.time - lastPointInsertedTime);
                if (Vector3.Distance(predictedPosition, transform.position) > 5.0f)
                {
                    addPoint = true;
                }
            }
            else addPoint = true;

        }
        else addPoint = true;

        if (addPoint)
        {
            path.Add(currentPos);

            lastPointInsertedDirection = transform.up;
            lastPointInsertedTime = Time.time;
            lastPointInsertedIndex = path.Count - 1;

            // Add points to the mesh 
            if (path.Count > 1)
            {
                bodyVertices.Add(headVertices[2]);
                bodyVertices.Add(headVertices[3]);
                bodyUV.Add(headUV[2]);
                bodyUV.Add(headUV[3]);

                int index = bodyVertices.Count - 4;

                bodyTriangles.Add(index); bodyTriangles.Add(index + 1); bodyTriangles.Add(index + 3);
                bodyTriangles.Add(index); bodyTriangles.Add(index + 3); bodyTriangles.Add(index + 2);
            }
            else
            {
                Vector3 dir = transform.up;
                Vector3 dirP = new Vector3(dir.y, -dir.x, 0.0f);
                float   radius = baseThickness;

                bodyVertices.Add(currentPos - dirP * radius);
                bodyVertices.Add(currentPos + dirP * radius);
                bodyUV.Add(new Vector2(0.0f, 0.0f));
                bodyUV.Add(new Vector2(1.0f, 0.0f));
            }
        }

        if (bodyVertices.Count >= 2)
        {
            headVertices[0] = bodyVertices[bodyVertices.Count - 1];
            headVertices[1] = bodyVertices[bodyVertices.Count - 2];
            headUV[0] = bodyUV[bodyVertices.Count - 1];
            headUV[1] = bodyUV[bodyVertices.Count - 2];

            Vector3 dir = transform.up;
            Vector3 dirP = new Vector3(dir.y, -dir.x, 0.0f);
            float radius1 = baseThickness + varianceThickness * (Mathf.PerlinNoise(0.0f, Time.time * 2.0f) * 2.0f - 1.0f);
            float radius2 = baseThickness + varianceThickness * (Mathf.PerlinNoise(0.0f, Time.time * 2.0f + 0.25f) * 2.0f - 1.0f);

            const float headSize = 20.0f;

            headVertices[2] = transform.position - dirP * radius1;
            headVertices[3] = transform.position + dirP * radius2;
            headVertices[4] = transform.position + transform.up * headSize;
            headUV[2] = new Vector2(0.0f, distance * 0.01f);
            headUV[3] = new Vector2(1.0f, distance * 0.01f);
            headUV[4] = new Vector3(0.5f, (distance + headSize) * 0.01f);

            headMesh.SetVertices(headVertices);
            headMesh.SetUVs(0, headUV);
            headMesh.SetTriangles(headTriangles, 0);
            headMesh.UploadMeshData(false);
            headMesh.RecalculateBounds();
        }

        if (bodyTriangles.Count > 0)
        {
            bodyMesh.SetVertices(bodyVertices);
            bodyMesh.SetUVs(0, bodyUV);
            bodyMesh.SetTriangles(bodyTriangles, 0);
            bodyMesh.RecalculateBounds();
            bodyMesh.UploadMeshData(false);
        }
    }

    private void OnDrawGizmos()
    {
        if (path == null) return;

        Gizmos.color = Color.yellow;

        if (path.Count > 0)
        {
            for (int i = 1; i < path.Count; i++)
            {
                Gizmos.DrawLine(path[i - 1], path[i]);
            }

            Gizmos.DrawLine(path[path.Count - 1], transform.position);
        }
    }
}
