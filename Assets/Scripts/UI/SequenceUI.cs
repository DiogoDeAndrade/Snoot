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

    override protected void Start()
    {
        base.Start();

        foreach (var item in nutrients)
        {
            item.enabled = false;
        }
        text.enabled = false;
    }

    override protected void RunUI()
    {
        var seq = player.GetNutrientSequence();
        if ((seq != null) && (seq.Count > 0))
        {
            text.enabled = true;
            for (int i = 0; i < seq.Count; i++)
            {
                nutrients[i].enabled = true;
                nutrients[i].sprite = gameData.GetNutrientSprite(seq[i].type);
                nutrients[i].color = (seq[i].caught) ? (Color.gray) : (Color.white);
            }
            for (int i = seq.Count; i < nutrients.Length;  i++)
            {
                nutrients[i].enabled = false;
            }
        }
        else
        {
            text.enabled = false;
            foreach (var item in nutrients)
            {
                item.enabled = false;
            }
        }
    }
}
