using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private    float           baseMoveSpeed = 200.0f;
    [SerializeField] private    float           varianceMoveSpeed = 100.0f;
    [SerializeField] private    float           baseRotateSpeed = 180.0f;
    [SerializeField] private    float           baseThickness = 5.0f;
    [SerializeField] private    float           varianceThickness = 5.0f;
    [SerializeField] private    MeshFilter      bodyMeshFilter;
    [SerializeField] private    MeshFilter      headMeshFilter;
    [SerializeField] private    float           collisionRadius = 20.0f;
    [SerializeField] private    Color           deathColor = Color.red;
    [SerializeField] private    float           initialWater = 10;
    [SerializeField] private    float           maxWater = 20;
    [SerializeField] private    float           consumeWaterPerSecond = 0.5f;
    [SerializeField] private    ParticleSystem  dirtPS;
    [SerializeField] private    ParticleSystem  waterPS;
    [SerializeField] private    ParticleSystem  glowPS;
    [SerializeField] private    float           initialNutrition = 10;
    [SerializeField] private    float           maxNutrition = 20;
    [SerializeField] private    float           consumeNutritionPerSecond = 0.25f;
    [SerializeField] private    float           nutritionPerSequenceElem = 5.0f;
    [SerializeField] private    float           nutritionLoss = 5.0f;
    [SerializeField] private    SpriteRenderer  headGlow;

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

    private float           water;
    private float           nutrition;
    private float           lastSequenceComplete;
    private bool            inWater;

    private List<Nutrient.SequenceElem> nutrientSequence;

    struct NutrientDistance
    {
        public Nutrient nutrient;
        public float    dist;
    }
    
    public float waterPercentage => water / maxWater;
    public float nutritionPercentage => nutrition / maxNutrition;

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

        water = initialWater;
        nutrition = initialNutrition;
    }

    private void FixedUpdate()
    {
        distance += rb.velocity.magnitude * Time.fixedDeltaTime;

        inWater = false;
    }

    void Update()
    {
        if (baseMoveSpeed > 0)
        {
            float rotation = Input.GetAxis("Horizontal");

            transform.rotation = transform.rotation * Quaternion.Euler(0, 0, -rotation * baseRotateSpeed * Time.deltaTime);

            rb.velocity = transform.up * (baseMoveSpeed + varianceMoveSpeed * (((water / maxWater) - 0.5f) * 2.0f));

            UpdateRoot();
            DetectSelfIntersection();
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
        if (baseMoveSpeed > 0)
        { 
            ChangeWater(-consumeWaterPerSecond * Time.deltaTime);
            if (water <= 0.0f)
            {
                Die();
            }
            ChangeNutrition(-consumeNutritionPerSecond * Time.deltaTime);
            if (nutrition <= 0.0f)
            {
                Die();
            }
        }

        if ((nutrientSequence == null) && ((Time. time - lastSequenceComplete) > 1.0f))
        {
            CreateSequence();
        }

        if (headGlow)
        {
            float alpha = Mathf.Lerp(0.0f, 0.5f, 2.0f * ((nutrition/ maxNutrition) - 0.5f));

            alpha = Mathf.Clamp01(alpha + 0.2f * Mathf.Cos(Time.time * 15.0f));

            if (water <= 0) alpha = 0.0f;

            headGlow.color = headGlow.color.ChangeAlpha(alpha);
        }

        bool waterActive = false;
        bool dirtActive = false;
        bool glowActive = false;

        if ((nutrition / maxNutrition) > 0.5f) { glowActive = true; }

        if (inWater) { waterActive = true; }
        else { dirtActive = true; }

        if (waterPS)
        {
            var emission = waterPS.emission;
            emission.enabled = waterActive;
        }
        if (dirtPS)
        {
            var emission = dirtPS.emission;
            emission.enabled = dirtActive;
        }
        if (glowPS)
        {
            var emission = glowPS.emission;
            emission.enabled = glowActive;
        }
    }

    void CreateSequence()
    {
        var nutrients = FindObjectsOfType<Nutrient>();
        if (nutrients.Length == 0) return;

        // Sort his list by distance
        var sortedNutrients = new List<NutrientDistance>();
        foreach (var n in nutrients) sortedNutrients.Add(new NutrientDistance { nutrient = n, dist = Vector3.Distance(transform.position, n.transform.position) });
        sortedNutrients.Sort((n1, n2) => (n1.dist == n2.dist) ? (0) : ((n1.dist < n2.dist) ? (-1) : (1)));

        int r = Random.Range(1, Mathf.Min(3, sortedNutrients.Count));

        nutrientSequence = new List<Nutrient.SequenceElem>();
        for (int i = 0; i < r; i++)
        {
            nutrientSequence.Add(new Nutrient.SequenceElem { type = sortedNutrients[i].nutrient.nutrientType, caught = false });
        }
    }

    public List<Nutrient.SequenceElem> GetNutrientSequence() => nutrientSequence;

    void DetectSelfIntersection()
    {
        float tolerance = (collisionRadius * 2.0f); tolerance *= tolerance;

        int removeSegments = Mathf.FloorToInt(5 + 10 * (1.0f - (water / maxWater)));

        for (int i = 1; i < path.Count - removeSegments; i++)
        {
            Vector3 cPoint = Line.GetClosestPoint(path[i - 1], path[i], transform.position);

            float dp = Mathf.Abs(Vector3.Dot((path[i] - path[i - 1]).normalized, transform.up));
            if (dp < 0.9f)
            {
                float dist = (cPoint - transform.position).sqrMagnitude;
                if (dist < tolerance)
                {
                    Die();
                }
            }
        }
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

    IEnumerator ShrinkRootCR()
    {
        float totalTime = 0.5f;
        float time = 0.0f;

        Material material1 = new Material(bodyMeshFilter.GetComponent<MeshRenderer>().material);
        bodyMeshFilter.GetComponent<MeshRenderer>().material = material1;
        Material material2 = new Material(headMeshFilter.GetComponent<MeshRenderer>().material);
        headMeshFilter.GetComponent<MeshRenderer>().material = material1;

        while (time < totalTime)
        {
            time += Time.deltaTime;

            float maxDelta = 0.75f * ((baseThickness - varianceThickness) / totalTime) * Time.deltaTime;

            ShrinkMesh(bodyMesh, bodyVertices, bodyUV, bodyTriangles, bodyVertices.Count, maxDelta);
            ShrinkMesh(headMesh, headVertices, headUV, headTriangles, headVertices.Count - 1, maxDelta);

            material1.SetColor("_Color", Color.Lerp(Color.white, deathColor, (time / totalTime)));
            material2.SetColor("_Color", Color.Lerp(Color.white, deathColor, (time / totalTime)));

            yield return null;
        }

    }

    private void ShrinkMesh(Mesh mesh, List<Vector3> vertices, List<Vector2> UV, List<int> triangles, int vertexCount, float maxDelta)
    {
        for (int i = 0; i < vertexCount; i += 2)
        {
            Vector3 v1 = vertices[i];
            Vector3 v2 = vertices[i + 1];
            Vector3 c = (v1 + v2) * 0.5f;
            vertices[i] = Vector3.MoveTowards(v1, c, maxDelta);
            vertices[i + 1] = Vector3.MoveTowards(v2, c, maxDelta);
        }

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, UV);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        mesh.UploadMeshData(false);
    }

    private void Die()
    {
        baseMoveSpeed = 0.0f;
        StartCoroutine(ShrinkRootCR());
    }

    public void ChangeWater(float delta)
    {
        water = Mathf.Clamp(water + delta, 0.0f, maxWater);
    }

    public void ChangeNutrition(float delta)
    {
        nutrition = Mathf.Clamp(nutrition + delta, 0.0f, maxNutrition);
    }

    public void AddToSequence(Nutrient.Type type)
    {
        bool inSequence = false;
        foreach (var n in nutrientSequence)
        {
            if ((n.type == type) && (!n.caught))
            {
                n.caught = true;
                inSequence = true;
                break;
            }
        }
        if (inSequence)
        {
            bool sequenceComplete = true;
            foreach (var s in nutrientSequence)
            {
                if (!s.caught)
                {
                    sequenceComplete = false;
                    break;
                }
            }
            if (sequenceComplete)
            {
                // Get nutrition
                ChangeNutrition(nutrientSequence.Count * nutritionPerSequenceElem);

                nutrientSequence = null;
                lastSequenceComplete = Time.time;
            }
        }
        else
        {
            // Loose nutrition
            ChangeNutrition(-nutritionLoss);

            nutrientSequence = null;
            lastSequenceComplete = Time.time;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Obstacle obstacle = collision.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            // Die!
            Die();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Resource res = collision.GetComponent<Resource>();
        if (res != null)
        {
            res.Grab(this);

            if (res.isWater) inWater = true;
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

        /*float minDist = float.MaxValue;
        Vector3 pos = transform.position;
        for (int i = 1; i < path.Count - 5; i++)
        {
            Vector3 cPoint = Line.GetClosestPoint(path[i - 1], path[i], transform.position);
            float dist = (cPoint - transform.position).sqrMagnitude;
            if (minDist > dist)
            {
                minDist = dist;
                pos = cPoint;
            }
        }


        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, pos);//*/
    }
}
