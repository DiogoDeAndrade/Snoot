using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nutrient : Resource
{
    public enum Type { Hydrogen, Oxygen, Nitrogen, Phosphorous, Potassium };

    [SerializeField] private Type type;

    public float radius => GetComponent<CircleCollider2D>().radius * transform.localScale.x;

    override protected void UpdateVisual()
    {
        spriteRenderer.color = Color.Lerp(initialSpriteColor.ChangeAlpha(0.0f), initialSpriteColor, resourceCount / maxResourceCount);
    }

    override protected void OnGrab(Player player, float delta)
    {
        
    }

}