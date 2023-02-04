using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Insect : MonoBehaviour
{
    [SerializeField] private float wanderRadius = 400.0f;
    [SerializeField] private float detectRadius = 1000.0f;
    [SerializeField] private float wanderSpeed = 50.0f;
    [SerializeField] private float attackSpeed = 100.0f;
    [SerializeField] private float rotationSpeed = 90.0f;
    [SerializeField] private float attackPower = 1.0f;

    private Vector3 spawnPos;
    private Vector3 targetPos;
    private Player  playerAttacked;

    // Start is called before the first frame update
    void Start()
    {
        spawnPos = transform.position;
        targetPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float speed = wanderSpeed;

        if (playerAttacked)
        {
            if (playerAttacked.playerControl)
            {
                if (Vector3.Distance(transform.position, targetPos) < 1.0f)
                {
                    // Attack root
                    playerAttacked.ChangeNutrition(-attackPower * Time.deltaTime);
                }

                speed = attackSpeed;
            }
            else
            {
                targetPos = spawnPos + Random.insideUnitCircle.xy0() * Random.Range(0.0f, wanderRadius);
                playerAttacked = null;
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
            if (dp > 0.95f)
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
