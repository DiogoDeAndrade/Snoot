using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

public class GotoSceneWithKey : MonoBehaviour
{
    [SerializeField, Scene]
    private string sceneName;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetButtonDown("Fire1")) || (Input.GetButtonDown("Jump")))
        {
            FullscreenFader.FadeOut(1.0f, Color.black, () =>
            {
                SceneManager.LoadScene(sceneName);
            });
        }
    }
}
