using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PoemDisplay : MonoBehaviour
{
    [SerializeField] private Image              image;
    [SerializeField] private TextMeshProUGUI    text;

    CanvasGroup canvasGroup;
    Coroutine   fadeCR;

    public bool poemIsDisplayed => canvasGroup.alpha > 0.0f;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        
    }

    public void ShowPoem(string text, Sprite image)
    {
        this.image.sprite = image;
        this.text.text = text;

        if (fadeCR != null) StopCoroutine(fadeCR);
        fadeCR = StartCoroutine(SetPoemCR());

        GameManager.instance.isPaused = true;
    }

    public void HidePoem()
    {
        if (fadeCR != null) StopCoroutine(fadeCR);
        fadeCR = StartCoroutine(HidePoemCR());
    }

    IEnumerator SetPoemCR()
    {
        while (canvasGroup.alpha < 1.0f)
        {
            canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha + Time.deltaTime);

            yield return null;
        }

        fadeCR = null;
    }

    IEnumerator HidePoemCR()
    {
        while (canvasGroup.alpha > 0.0f)
        {
            canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha - Time.deltaTime);

            yield return null;
        }

        fadeCR = null;
        GameManager.instance.isPaused = false;
    }
}
