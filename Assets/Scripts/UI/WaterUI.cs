using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaterUI : PlayerUI
{
    [SerializeField] Image  fillImage;

    protected override void RunUI()
    {
        base.RunUI();

        fillImage.fillAmount = player.waterPercentage;
    }
}
