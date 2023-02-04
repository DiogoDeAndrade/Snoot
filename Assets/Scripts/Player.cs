using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Player : MonoBehaviour
{
    [SerializeField] private    GameData        gameData;
    [SerializeField] private    float           startupRunHead = 0.0f;
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
    [SerializeField] private    float           nutritionLossPerBadSequence = 5.0f;
    [SerializeField] private    float           nutritionLossPerEnemyHit = 5.0f;
    [SerializeField] private    float           nutritionLossPerBranch = 12.0f;
    [SerializeField] private    float           nutritionLossPerLighting = 8.0f;
    [SerializeField] private    Material        flashMaterial;
    [SerializeField] private    Color           flashColor;

    private List<Vector3>   path = new List<Vector3>();
    private Rigidbody2D     rb;
    private Mesh            bodyMesh;
    private List<Vector3>   bodyVertices = new List<Vector3>();
    private List<Vector2>   bodyUV = new List<Vector2>();
    private List<int>       bodyTriangles = new List<int>();
    private Mesh            headMesh;
    private List<Vector3>   headVertices;
    private List<Vector2>   headUV;
    private List<int>       headTriangles;

    private float           lastPointInsertedTime;
    private Vector3         lastPointInsertedDirection;
    private int             lastPointInsertedIndex;
    private float           distance;

    private float               water;
    private float               nutrition;
    private float               lastSequenceComplete;
    private bool                inWater;
    public  bool                playerControl = true;
    private float               autoRun = 0.0f;
    private GameObject[]        mapNutrient;
    private Coroutine           flashCR;

    struct PrevBranch
    {
        public GameObject player;
        public int        pathIndex;
    }

    private List<GameObject>   prevBranches = new List<GameObject>();

    private List<Nutrient.SequenceElem> nutrientSequence;

   
    public float waterPercentage => water / maxWater;
    public float nutritionPercentage => nutrition / maxNutrition;

    void Start()
    {
        bodyMesh = new Mesh();
        bodyMesh.name = "RootBody";
        bodyMeshFilter.mesh = bodyMesh;

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
        if (startupRunHead > 0)
        {
            if (playerControl)
            {
                // Restore player control after a bit
                Invoke("ActivePlayerControl", startupRunHead);
            }
            
            autoRun = startupRunHead;
            playerControl = false;
        }

        water = initialWater;
        nutrition = initialNutrition;

        mapNutrient = new GameObject[5];
    }

    void ActivePlayerControl()
    {
        playerControl = true;
    }

    private void FixedUpdate()
    {
        distance += rb.velocity.magnitude * Time.fixedDeltaTime;

        inWater = false;
    }

    void Update()
    {
        if (playerControl)
        {
            float rotation = Input.GetAxis("Horizontal");

            transform.rotation = transform.rotation * Quaternion.Euler(0, 0, -rotation * baseRotateSpeed * Time.deltaTime);

            rb.velocity = transform.up * (baseMoveSpeed + varianceMoveSpeed * (((water / maxWater) - 0.5f) * 2.0f));

            if (Input.GetButtonDown("Jump"))
            {
                if (nutrition > nutritionLossPerBranch)
                {
                    ChangeNutrition(-nutritionLossPerBranch);
                    Split();
                }
            }

            if (Input.GetButtonDown("Fire1"))
            {
                if ((flashCR == null) && (nutrition > nutritionLossPerLighting))
                {
                    var insects = FindObjectsOfType<Insect>(true);
                    foreach (var insect in insects)
                    {
                        if (insect.isAttacking)
                        {
                            insect.Die();
                        }
                    }
                    ChangeNutrition(-nutritionLossPerLighting);

                    flashCR = StartCoroutine(LightningFlashCR());
                }
            }

            UpdateRoot();
            DetectSelfIntersection();
        }
        else
        {
            if (autoRun > 0)
            {
                autoRun -= Time.deltaTime;

                rb.velocity = transform.up * (baseMoveSpeed + varianceMoveSpeed * (((water / maxWater) - 0.5f) * 2.0f));

                UpdateRoot();
            }
            else
            {
                rb.velocity = Vector2.zero;
            }

        }

        if (playerControl)
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


            if ((nutrientSequence == null) && ((Time.time - lastSequenceComplete) > 1.0f))
            {
                CreateSequence();
            }

            bool waterActive = false;
            bool dirtActive = false;
            bool glowActive = false;

            if (nutrition > nutritionLossPerEnemyHit) { glowActive = true; }

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

            // Update map
            var nutrients = Nutrient.GetSortedNutrientList(transform.position);

            var nFound = new bool[5] { false, false, false, false, false };

            foreach (var n in nutrients )
            {
                var idx = (int)n.nutrientType;
                if (!nFound[idx])
                { 
                    nFound[idx] = true;
                    if (mapNutrient[idx])
                    {
                        HUDIconManager.SetPos(mapNutrient[idx], n.transform);
                    }
                    else
                    {
                        mapNutrient[idx] = HUDIconManager.AddIcon(gameData.GetNutrientSprite(n.nutrientType), new Color(1.0f, 1.0f, 1.0f, 0.25f), n.transform, 4, false, false);
                    }
                }
                bool allFound = true;
                for (int i = 0; i < nFound.Length; i++)
                {
                    if (!nFound[i])
                    {
                        allFound = false;
                        break;
                    }
                }
                if (allFound) break;
            }
            for (int i = 0; i < nFound.Length; i++)
            {
                if (!nFound[i])
                {
                    if (mapNutrient[i] != null) HUDIconManager.RemoveIcon(mapNutrient[i]);
                    mapNutrient[i] = null;
                }
            }
        }
    }

    void CreateSequence()
    {
        var nutrients = Nutrient.GetSortedNutrientList(transform.position);

        int r = Random.Range(1, Mathf.Min(3, nutrients.Count));

        nutrientSequence = new List<Nutrient.SequenceElem>();
        for (int i = 0; i < r; i++)
        {
            nutrientSequence.Add(new Nutrient.SequenceElem { type = nutrients[i].nutrientType, caught = false });
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
        for (int i = 0; i < mapNutrient.Length; i++)
        {
            if (mapNutrient[i])
            {
                HUDIconManager.RemoveIcon(mapNutrient[i]);
                mapNutrient[i] = null;
            }
        }

        if (!playerControl)
        {
            prevBranches.RemoveAll((e) => e == transform.parent.gameObject);
        }
        else
        {
            playerControl = false;

            // Pass player control to previous branch
            if (prevBranches.Count > 0)
            {
                GameObject pb = prevBranches[prevBranches.Count - 1];
                prevBranches.RemoveAt(prevBranches.Count - 1);
                var branchPlayer = pb.GetComponentInChildren<Player>();
                branchPlayer.Invoke("ActivePlayerControl", 1.0f);
                var camera = FindObjectOfType<Camera>();
                if (camera != null)
                {
                    var follow = camera.GetComponent<CameraFollow>();
                    if (follow)
                    {
                        follow.targetObject = branchPlayer.transform;
                    }
                }
                transform.parent.position = new Vector3(0, 0, 0.1f);
            }
        }
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
            ChangeNutrition(-nutritionLossPerBadSequence);

            nutrientSequence = null;
            lastSequenceComplete = Time.time;
        }
    }

    void Split()
    {
        float rot = -1.0f;
        if (Random.Range(0, 100) < 50) rot = 1.0f;

        // Create new player at this position, pointint 30 degrees clockwise
        var newPlayer = Instantiate(gameData.playerPrefab, Vector3.zero, Quaternion.identity);
        var newPlayerComponent = newPlayer.GetComponentInChildren<Player>();
        newPlayerComponent.transform.position = transform.position;
        newPlayerComponent.transform.rotation = transform.rotation * Quaternion.Euler(0, 0, rot * Random.Range(40, 80));
        newPlayerComponent.playerControl = false;
        // Copy path
        newPlayerComponent.path = new List<Vector3>(path);
        newPlayerComponent.bodyVertices = new List<Vector3>(bodyVertices);
        newPlayerComponent.bodyTriangles = new List<int>(bodyTriangles);
        newPlayerComponent.bodyUV = new List<Vector2>(bodyUV);
        newPlayerComponent.lastPointInsertedDirection = lastPointInsertedDirection;
        newPlayerComponent.lastPointInsertedTime = lastPointInsertedTime;
        newPlayerComponent.lastPointInsertedIndex = lastPointInsertedIndex;
        newPlayerComponent.prevBranches = prevBranches;

        newPlayerComponent.water = water;
        newPlayerComponent.nutrition = nutrition;

        newPlayerComponent.prevBranches.Add(newPlayer);
    }

    public float GetClosestPoint(Vector3 pos, out Vector3 closestPoint)
    {
        float minDist = float.MaxValue;
        closestPoint = new Vector3(float.MaxValue, float.MaxValue, 0);

        for (int i = 1; i < path.Count; i++)
        {
            Vector3 cPoint = Line.GetClosestPoint(path[i - 1], path[i], pos);

            float dist = (cPoint - pos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closestPoint = cPoint;
            }
        }

        return Mathf.Sqrt(minDist);
    }

    IEnumerator LightningFlashCR()
    {
        MeshRenderer renderer = bodyMeshFilter.GetComponent<MeshRenderer>();
        Material     originalMaterial = renderer.material;

        renderer.material = flashMaterial;

        float duration = 1.0f;
        float timer = 0.0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;

            flashMaterial.SetColor("_EmissionColor", flashColor * 4 * (1.0f - (timer / duration)));

            yield return null;
        }

        renderer.material = originalMaterial;
        flashCR = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Obstacle obstacle = collision.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            // Die!
            Die();
        }
        Insect insect = collision.GetComponent<Insect>();
        if (insect != null)
        {
            if (nutrition> nutritionLossPerEnemyHit)
            {
                // Kill enemy
                ChangeNutrition(-nutritionLossPerEnemyHit);
                insect.Die();
            }
            else
            {
                // Just die
                Die();
            }
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
