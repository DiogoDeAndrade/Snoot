using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : Resource
{
    override protected void UpdateVisual()
    {
        spriteRenderer.color = Color.Lerp(initialSpriteColor.ChangeAlpha(0.0f), initialSpriteColor, resourceCount / maxResourceCount);
    }

    override protected void OnGrab(Player player, float delta)
    {
        player.ChangeWater(delta);
    }

}
