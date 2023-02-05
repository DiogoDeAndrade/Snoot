using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

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

    [SerializeField] 
    private NutrientDataElem[]  nutrients;                     
    public  GameObject          playerPrefab;
    [Scene] 
    public string[]             levels;
    [Scene]
    public string               gameEndScene;

    public Sprite GetNutrientSprite(Nutrient.Type type)
    {
        foreach (var nde in nutrients)
        {
            if (nde.type == type) return nde.sprite;
        }

        return null;
    }
}
