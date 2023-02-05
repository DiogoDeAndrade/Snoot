using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Crystal : MonoBehaviour
{
    [SerializeField]
    private Color   iconColor = Color.white;
    [SerializeField]
    private Sprite  iconSprite;
    [SerializeField] 
    private AudioClip pickupCrystal;

    public Sprite image;
    [ResizableTextArea]
    public string text;

    void Start()
    {
        HUDIconManager.AddIcon(iconSprite, iconColor, transform, 1.0f, false, false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Die()
    {
        SoundManager.PlaySound(pickupCrystal, 1.0f, 1.0f);
        Destroy(gameObject);
    }
}
