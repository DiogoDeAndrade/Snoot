using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public void StartGame()
    {
        FullscreenFader.FadeOut(0.5f, Color.black, () =>
        {
            SceneManager.LoadScene("Level01");
        });
    }
    public void Quit()
    {
        FullscreenFader.FadeOut(0.5f, Color.black, () =>
        {
            Application.Quit();
        });
    }
}
