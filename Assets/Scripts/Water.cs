using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : Resource
{
    public float genRadius
    {
        get
        {
            var extents = GetComponent<Collider2D>().bounds.extents;

            return Mathf.Max(extents.x, extents.y) * 1.2f;
        }
    }

    override public bool isWater => true;

    override protected void UpdateVisual()
    {
        spriteRenderer.color = Color.Lerp(initialSpriteColor.ChangeAlpha(0.0f), initialSpriteColor, resourceCount / maxResourceCount);
    }

    override protected void OnGrab(Player player, float delta)
    {
        player.ChangeWater(delta);
    }

}
