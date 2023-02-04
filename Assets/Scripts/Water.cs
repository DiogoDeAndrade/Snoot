using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : Resource
{
    public float radius => GetComponent<CircleCollider2D>().radius * transform.localScale.x;

    override protected void UpdateVisual()
    {
        spriteRenderer.color = Color.Lerp(initialSpriteColor.ChangeAlpha(0.0f), initialSpriteColor, resourceCount / maxResourceCount);
    }

    override protected void OnGrab(Player player, float delta)
    {
        player.ChangeWater(delta);
    }

}
