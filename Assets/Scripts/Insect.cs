using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Insect : MonoBehaviour
{
    [SerializeField] private float          wanderRadius = 400.0f;
    [SerializeField] private float          detectRadius = 1000.0f;
    [SerializeField] private float          wanderSpeed = 50.0f;
    [SerializeField] private float          attackSpeed = 100.0f;
    [SerializeField] private float          rotationSpeed = 90.0f;
    [SerializeField] private float          attackPower = 1.0f;
    [SerializeField] private Sprite         alertImage;
    [SerializeField] private Color          alertColor;
    [SerializeField] private ParticleSystem deathPS;
    [SerializeField] private AudioClip      deathSound;

    private Vector3         spawnPos;
        private Vector3         targetPos;
        private Player          playerAttacked;
        private GameObject      alertIcon;
        private SpriteRenderer  spriteRenderer;
    new private Collider2D      collider;

    public bool isAttacking => alertIcon != null;

    public float genRadius
    {
        get
        {
            var extents = GetComponent<Collider2D>().bounds.extents;

            return Mathf.Max(extents.x, extents.y) * 1.2f;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        spawnPos = transform.position;
        targetPos = transform.position;

        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!spriteRenderer.enabled) return;
        if (GameManager.instance.isPaused) return;

        float speed = wanderSpeed;
        float angularTolerance = 0.95f;

        if (playerAttacked)
        {
            if (playerAttacked.playerControl)
            {
                float dist = Vector3.Distance(transform.position, targetPos);
                if (dist < 1.0f)
                {
                    // Attack root
                    CameraShake2d.Shake(2.5f, 0.05f);

                    playerAttacked.ChangeNutrition(-attackPower * Time.deltaTime);
                    if (alertIcon == null)
                    {
                        alertIcon = HUDIconManager.AddIcon(alertImage, alertColor, transform, 1.0f, true);
                    }
                }
                else if (dist > 50.0f)
                {
                    if (Random.Range(0.0f, 1.0f) < 0.05f)
                    {
                        playerAttacked.GetClosestPoint(transform.position, out targetPos);
                        targetPos = targetPos + (transform.position - targetPos).normalized * 60;
                    }
                }

                speed = attackSpeed;
                angularTolerance = 0.5f;
            }
            else
            {
                targetPos = spawnPos + Random.insideUnitCircle.xy0() * Random.Range(0.0f, wanderRadius);
                playerAttacked = null;
                HUDIconManager.RemoveIcon(alertIcon);
                alertIcon = null;
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, targetPos) < 1.0f)
            {
                targetPos = spawnPos + Random.insideUnitCircle.xy0() * Random.Range(0.0f, wanderRadius);
            }

            if (Random.Range(0.0f, 1.0f) < 0.1f)
            {
                // Try to find the root
                Player player = FindPlayer();
                if (player != null)
                {
                    // Check for distance and closest point
                    Vector3 closestPoint;
                    float distance = player.GetClosestPoint(transform.position, out closestPoint);
                    if (distance < detectRadius)
                    {
                        playerAttacked = player;
                        targetPos = closestPoint + (transform.position - closestPoint).normalized * 60;
                    }
                }
            }
        }

        Vector3 toTarget = targetPos - transform.position;
        if (toTarget.sqrMagnitude > 0.0f)
        {
            toTarget.Normalize();

            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(Vector3.forward, toTarget), rotationSpeed * Time.deltaTime);

            float dp = Vector3.Dot(toTarget, transform.up);
            if (dp > angularTolerance)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        }
    }

    Player FindPlayer()
    {
        var players = FindObjectsOfType<Player>();
        foreach (var p in players)
        {
            if ((p.isActiveAndEnabled) && (p.playerControl))
            {
                return p;
            }
        }
        return null;
    }

    public void Die()
    {
        spriteRenderer.enabled = false;
        collider.enabled = false;
        deathPS.Play();

        SoundManager.PlaySound(deathSound, 1.0f, Random.Range(0.9f, 1.1f));

        if (alertIcon)
        {
            HUDIconManager.RemoveIcon(alertIcon);
            alertIcon = null;
        }
        Destroy(gameObject, 4.0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        if (playerAttacked)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, targetPos);
        }
    }
}
