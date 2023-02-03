using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    [SerializeField] protected float maxResourceCount = 20;
    [SerializeField] protected float initialResourceCount = 20;
    [SerializeField] protected float resourcePerSecond = 100;

    protected float             resourceCount;
    protected SpriteRenderer    spriteRenderer;
    protected Color             initialSpriteColor;


    virtual protected void Start()
    {
        resourceCount = initialResourceCount;
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialSpriteColor = spriteRenderer.color;

        UpdateVisual();
    }

    virtual protected void UpdateVisual()
    {

    }

    virtual protected void OnGrab(Player player, float delta)
    {

    }

    virtual public void Grab(Player player)
    {
        float newResourceCount = Mathf.Max(resourceCount - Time.deltaTime * resourcePerSecond, 0);
        OnGrab(player, resourceCount - newResourceCount);

        resourceCount = newResourceCount;
        if (resourceCount == 0.0f)
        {
            Exhaust();
        }

        UpdateVisual();        
    }

    virtual protected void Exhaust()
    {
        Destroy(gameObject);
    }
}
