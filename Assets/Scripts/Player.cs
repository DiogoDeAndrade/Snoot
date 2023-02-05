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
    [SerializeField] private    AudioClip       pickupNutrientClip;
    [SerializeField] private    AudioClip       badNutrientClip;
    [SerializeField] private    AudioClip       sequenceCompleteClip;
    [SerializeField] private    AudioClip       deathSound;
    [SerializeField] private    AudioClip       branchSoundClip;
    [SerializeField] private    AudioClip       zapSoundClip;
    [SerializeField] private    AudioSource     digSoundInstance;

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
    private Coroutine           flashCR;
    private bool                wasPaused = false;
    private Vector2             prevVelocity;
    private float               digSoundBaseVolume;

    struct PrevBranch
    {
        public GameObject player;
        public int        pathIndex;
    }

    private List<GameObject>   prevBranches = new List<GameObject>();

    public class SequenceElem
    {
        public Nutrient.Type    type;
        public bool             caught;
        public GameObject       icon;
    };


    private List<SequenceElem>  nutrientSequence;

    public float waterPercentage => water / maxWater;
    public float nutritionPercentage => nutrition / maxNutrition;
    public bool canBranch => nutrition >= nutritionLossPerBranch;
    public bool canLightning => nutrition >= nutritionLossPerLighting;

    void Start()
    {
        digSoundBaseVolume = digSoundInstance.volume;

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
        if (GameManager.instance.isPaused)
        {
            if (!wasPaused)
            {
                prevVelocity = rb.velocity;
                rb.velocity = Vector2.zero;
            }
            wasPaused = GameManager.instance.isPaused;

            var poemDisplay = FindObjectOfType<PoemDisplay>();
            if (poemDisplay)
            {
                if (poemDisplay.poemIsDisplayed)
                {
                    if ((Input.GetButtonDown("Jump")) ||
                        (Input.GetButtonDown("Fire1")))
                    {
                        poemDisplay.HidePoem();
                    }
                }
            }

            digSoundInstance.volume = 0.0f;

            return;
        }
        else
        {
            if (wasPaused)
            {
                rb.velocity = prevVelocity;
            }
            wasPaused = GameManager.instance.isPaused;
        }

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

                    SoundManager.PlaySound(branchSoundClip, 1.0f, 1.0f);
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

                    SoundManager.PlaySound(zapSoundClip, 1.0f, 1.0f);

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

            digSoundInstance.pitch = 1.0f;

            if (nutrition > nutritionLossPerEnemyHit) 
            { 
                glowActive = true;
                digSoundInstance.volume = digSoundBaseVolume;
                digSoundInstance.pitch = 1.25f;
            }

            if (inWater) 
            {
                digSoundInstance.volume = digSoundBaseVolume * 0.5f;

                waterActive = true; 
            }
            else 
            { 
                dirtActive = true; 
            }

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
            if (nutrientSequence != null)
            {
                var nutrients = Nutrient.GetSortedNutrientList(transform.position);
                var alreadyProcessed = new List<bool>();
                for (int i = 0; i < nutrientSequence.Count; i++) alreadyProcessed.Add(false);

                foreach (var n in nutrients)
                {
                    var idx = (int)n.nutrientType;

                    for (int i = 0; i < nutrientSequence.Count; i++)
                    {
                        if (nutrientSequence[i].type != n.nutrientType) continue;
                        if (nutrientSequence[i].caught) continue;
                        if (alreadyProcessed[i]) continue;

                        if (nutrientSequence[i].icon)
                        {
                            HUDIconManager.SetPos(nutrientSequence[i].icon, n.transform);
                        }
                        alreadyProcessed[i] = true;
                        break;
                    }

                    bool sequenceProcessed = true;
                    foreach (var b in alreadyProcessed)
                    {
                        sequenceProcessed &= b;
                    }
                    if (sequenceProcessed) break;
                }
            }
        }
    }

    void CreateSequence()
    {
        var nutrients = Nutrient.GetSortedNutrientList(transform.position);

        int r = Random.Range(1, Mathf.Min(3, nutrients.Count));

        nutrientSequence = new List<SequenceElem>();
        for (int i = 0; i < r; i++)
        {
            nutrientSequence.Add(new SequenceElem 
            { 
                type = nutrients[i].nutrientType, 
                caught = false,
                icon = HUDIconManager.AddIcon(gameData.GetNutrientSprite(nutrients[i].nutrientType), new Color(1.0f, 1.0f, 1.0f, 0.25f), nutrients[i].transform, 4, false, false)
            });
        }
    }

    void ClearSequence()
    {
        if (nutrientSequence == null) return;

        foreach (var n in nutrientSequence)
        {
            if (n.icon) HUDIconManager.RemoveIcon(n.icon);
        }
        nutrientSequence = null;
    }

    public List<SequenceElem> GetNutrientSequence() => nutrientSequence;

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
        CameraShake2d.Shake(10.0f, 0.25f);
        SoundManager.PlaySound(deathSound, 1.0f, 1.0f);

        ClearSequence();

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
            else
            {
                var gameOver = FindObjectOfType<GameOver>();
                gameOver.Show();
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
        if (nutrientSequence == null) return;

        bool inSequence = false;
        for (int i = 0; i < nutrientSequence.Count; i++)
        {
            var n = nutrientSequence[i];
            if ((n.type == type) && (!n.caught))
            {
                n.caught = true;
                inSequence = true;
                HUDIconManager.RemoveIcon(n.icon);
                n.icon = null;
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

                ClearSequence();
                lastSequenceComplete = Time.time;

                SoundManager.PlaySound(sequenceCompleteClip, 1.0f, 1.0f);
            }
            else
            {
                SoundManager.PlaySound(pickupNutrientClip, 1.0f, 1.0f);
            }
        }
        else 
        {
            // Loose nutrition
            ChangeNutrition(-nutritionLossPerBadSequence);

            SoundManager.PlaySound(badNutrientClip, 1.0f, 1.0f);

            ClearSequence();
            lastSequenceComplete = Time.time;

            CameraShake2d.Shake(10.0f, 0.25f);
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
                CameraShake2d.Shake(5.0f, 0.1f);
            }
            else
            {
                // Just die
                Die();
            }
        }
        Crystal crystal = collision.GetComponent<Crystal>();
        if (crystal != null)
        {
            var poemDisplay = FindObjectOfType<PoemDisplay>();
            if (poemDisplay)
            {
                poemDisplay.ShowPoem(crystal.text, crystal.image);
                crystal.Die();
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
