using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    protected Player player;

    virtual protected void Start()
    {
        FindPlayer();
    }

    void Update()
    {
        if (player == null) FindPlayer();
        if (!player.isActiveAndEnabled) FindPlayer();
        if (!player.playerControl) FindPlayer();
        
        if (player != null)
        {
            RunUI();
        }
    }

    virtual protected void RunUI()
    {

    }

    void FindPlayer()
    {
        var players = FindObjectsOfType<Player>();
        foreach (var p in players)
        {
            if ((p.isActiveAndEnabled) && (p.playerControl))
            {
                player = p;
                break;
            }
        }
    }
}
