using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameData gameData;
    public bool     isPaused = true;
    
    public static GameManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Update()
    {
        if (!isPaused)
        {
            var crystals = FindObjectsOfType<Crystal>();
            if ((crystals == null) || (crystals.Length == 0))
            {
                isPaused = true;

                // Next level is...
                FullscreenFader.FadeOut(0.5f, Color.black, () =>
                {
                    // Find which level is this one
                    Scene scene = SceneManager.GetActiveScene();
                    var sceneName = scene.name;

                    for (int i = 0; i < gameData.levels.Length; i++)
                    {
                        if (gameData.levels[i] == sceneName)
                        {
                            if ((i + 1) < gameData.levels.Length)
                            {
                                SceneManager.LoadScene(gameData.levels[i + 1]);
                            }
                            else
                            {
                                SceneManager.LoadScene(gameData.gameEndScene);
                            }
                        }
                    }
                });
            }
        }
    }
}
