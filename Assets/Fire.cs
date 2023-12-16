using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// functionality of a water/soap (whatever) puddle
// checks if the player is on water/soap puddle
public class Fire : MonoBehaviour
{
    private Level level;

    private AudioSource source;
    public AudioClip fire_sound;

    void Start()
    {
        source = gameObject.GetComponent<AudioSource>();
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
        }
        fire_sound = Resources.Load<AudioClip>("FireEnter");
        GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();
        if (level == null)
        {
            Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "PLAYER")
        {
            SwordsmanController swordsmanController = other.GetComponent<SwordsmanController>();
            if (swordsmanController != null)
            {
                source.PlayOneShot(fire_sound);
                swordsmanController.player_health -= Random.Range(5, 20);
                if (swordsmanController.player_health <= 0)
                {
                    swordsmanController.player_health = 0;
                }
            }
            level.player_is_on_fire = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "PLAYER")
        {
            level.player_is_on_fire = false;
        }
    }
}
