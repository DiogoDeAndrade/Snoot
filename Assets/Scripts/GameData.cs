using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Snoot/GameData")]
public class GameData : ScriptableObject
{
    [System.Serializable]
    struct NutrientDataElem
    {
        public Nutrient.Type   type;
        public Sprite          sprite;
        public GameObject      prefab;
    }

    [SerializeField] private NutrientDataElem[] nutrients;
                     public  GameObject         playerPrefab;

    public Sprite GetNutrientSprite(Nutrient.Type type)
    {
        foreach (var nde in nutrients)
        {
            if (nde.type == type) return nde.sprite;
        }

        return null;
    }
}
