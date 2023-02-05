using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    CanvasGroup     canvasGroup;

    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0.0f;
    }

    private void Update()
    {
        if (canvasGroup.alpha == 1.0f)
        {
            if ((Input.GetButtonDown("Fire1")) || (Input.GetButtonDown("Jump")))
            {
                FullscreenFader.FadeOut(0.25f);
                StartCoroutine(BackToMainMenuCR());
            }
        }
    }

    IEnumerator BackToMainMenuCR()
    {
        yield return new WaitForSeconds(0.25f);
        SceneManager.LoadScene("Title");
    }

    public void Show()
    {
        StartCoroutine(ShowCR());
    }

    IEnumerator ShowCR()
    {
        while (canvasGroup.alpha < 1.0f)
        {
            canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha + Time.deltaTime * 4.0f);

            yield return null;
        }
    }
}
