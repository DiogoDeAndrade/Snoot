using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nutrient : Resource
{
    public enum Type { Hydrogen = 0, Oxygen = 1, Nitrogen = 2, Phosphorous = 3, Potassium = 4 };


    [SerializeField] private Type       type;

    public Type nutrientType => type;
    public float radius => 16;

    private void Awake()
    {
        nutrientList.Add(this);
    }

    override protected void UpdateVisual()
    {
        spriteRenderer.color = Color.Lerp(initialSpriteColor.ChangeAlpha(0.0f), initialSpriteColor, resourceCount / maxResourceCount);
    }

    override protected void OnGrab(Player player, float delta)
    {
        player.AddToSequence(type, this);
    }

    struct NutrientDistance
    {
        public Nutrient nutrient;
        public float dist;
    }
    static List<Nutrient> nutrientList = new List<Nutrient>();

    public static List<Nutrient> GetSortedNutrientList(Vector3 pos)
    {
        nutrientList.RemoveAll((x) => x == null);

        if (nutrientList.Count == 0) return null;

        var sortedNutrients = new List<NutrientDistance>();
        foreach (var n in nutrientList) sortedNutrients.Add(new NutrientDistance { nutrient = n, dist = Vector3.Distance(pos, n.transform.position) });
        sortedNutrients.Sort((n1, n2) => (n1.dist == n2.dist) ? (0) : ((n1.dist < n2.dist) ? (-1) : (1)));

        var ret = new List<Nutrient>();
        foreach (var n in sortedNutrients)
        {
            ret.Add(n.nutrient);
        }
        return ret;
    }

}
