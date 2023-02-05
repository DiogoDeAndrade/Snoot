using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NutrientUI : PlayerUI
{
    [SerializeField] Image  fillImage;
    [SerializeField] Image  branchImage;
    [SerializeField] Image  lightningImage;

    protected override void RunUI()
    {
        base.RunUI();

        fillImage.fillAmount = player.nutritionPercentage;

        if (branchImage)
        {
            branchImage.enabled = player.canBranch;
            branchImage.color = (new Color(0.8f, 0.8f, 0.8f, 1.0f) + 0.2f * Color.white * Mathf.Cos(Time.time * 10.0f)).Clamp().ChangeAlpha(1.0f);
        }
        if (lightningImage)
        {
            lightningImage.enabled = player.canLightning;
            lightningImage.color = (new Color(0.8f, 0.8f, 0.8f, 1.0f) + 0.2f * Color.white * Mathf.Cos(Time.time * 10.1f + 1.4f)).Clamp().ChangeAlpha(1.0f);
        }
    }
}
