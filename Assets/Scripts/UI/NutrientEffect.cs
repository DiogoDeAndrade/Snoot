using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NutrientEffect : MonoBehaviour
{
    public bool     isFall;
    public float    duration;
    public Vector3  destination;

    Vector3 velocity;
    float   timer;

    void Start()
    {
        velocity = Vector2.down * 5.0f;
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (isFall)
        {
            velocity.y += -50.0f * Time.deltaTime;

            transform.localPosition += velocity * Time.deltaTime;
        }

        if (timer > duration)
        {
            Destroy(gameObject);
        }
    }
}
