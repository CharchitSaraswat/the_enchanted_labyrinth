using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// functionality of the house at the exit of the maze
// it simply has a trigger when player arrived in the house
public class EndZone : MonoBehaviour
{
    private Level level;

    // Use this for initialization
    void Start()
    {
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
            level.player_entered_house = true;
        }
    }
}