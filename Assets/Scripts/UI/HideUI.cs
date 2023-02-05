using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideUI : PlayerUI
{
    CanvasGroup canvasGroup;

    // Start is called before the first frame update
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0.0f;
    }

    override protected void Update()
    {
        FindPlayer();
        if ((player == null) || (!player.playerControl))
        {
            canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha - Time.deltaTime * 2.0f);
        }
        else 
        {
            canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha + Time.deltaTime * 2.0f);
        }
    }
}
