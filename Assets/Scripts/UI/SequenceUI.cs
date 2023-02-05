using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SequenceUI : PlayerUI
{
    [SerializeField] TextMeshProUGUI    text;
    [SerializeField] Image[]            nutrients;
    [SerializeField] GameData           gameData;
    [SerializeField] NutrientEffect     nutrientEffectPrefab;

    public List<Player.SequenceElem>   lastSequence;

    override protected void Start()
    {
        base.Start();

        foreach (var item in nutrients)
        {
            item.gameObject.SetActive(false);
        }
        text.enabled = false;
    }

    override protected void RunUI()
    {
        var seq = player.GetNutrientSequence();
        /*if ((lastSequence != seq) && (lastSequence != null))
        {
            // For all images, create a prefab that falls
            for (int i = 0; i < nutrients.Length; i++)
            {
                if (nutrients[i] == null) continue;
                if (!nutrients[i].isActiveAndEnabled) continue;

                var effect = Instantiate(nutrientEffectPrefab, transform);
                effect.GetComponent<Image>().sprite = nutrients[i].sprite;
                effect.transform.position = nutrients[i].transform.position;
                effect.isFall = true;
                effect.duration = 2.0f;
            }

            lastSequence = seq;
        }*/

        if ((seq != null) && (seq.Count > 0))
        {
            lastSequence = seq;

            text.enabled = true;
            for (int i = 0; i < seq.Count; i++)
            {
                nutrients[i].gameObject.SetActive(true);
                nutrients[i].sprite = gameData.GetNutrientSprite(seq[i].type);
                nutrients[i].color = (seq[i].caught) ? (Color.gray) : (Color.white);
            }
            for (int i = seq.Count; i < nutrients.Length;  i++)
            {
                nutrients[i].gameObject.SetActive(false);
            }
        }
        else
        {
            text.enabled = false;
            foreach (var item in nutrients)
            {
                item.gameObject.SetActive(false);
            }
        }
    }
}
