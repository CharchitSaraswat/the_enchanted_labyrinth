﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// functionality of drug
// animates color, and triggers a variable if the player reaches the drug 
public class Gem : MonoBehaviour
{
    private GameObject player_obj;
    private Level level;

    void Start()
    {
        GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();
        if (level == null)
        {
            Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
            return;
        }
        player_obj = level.player_obj;
    }

    void Update()
    {
        Color greenness = new Color
        {
            g = Mathf.Max(1.0f, 0.1f + Mathf.Abs(Mathf.Sin(Time.time)))
        };
        GetComponent<MeshRenderer>().material.color = greenness;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "PLAYER")
        {
            SwordsmanController swordsmanController = collision.gameObject.GetComponent<SwordsmanController>();
            if (swordsmanController != null)
            {
                swordsmanController.player_health += Random.Range(5, 20);
                if (swordsmanController.player_health >= 100.0f)
                {
                    swordsmanController.player_health = 100.0f;
                }
            }
            level.CollectGem();
            Debug.Log("Gem collided with " + collision.gameObject.name);
            level.gem_landed_on_player_recently = true;
            Destroy(gameObject);
        }
    }
}