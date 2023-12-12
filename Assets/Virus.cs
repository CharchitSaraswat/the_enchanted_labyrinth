using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// functionality of virus
// animates color and size of the virus
// and attacks the player if the player is near the virus (check the code)
public class Virus : MonoBehaviour
{
    private GameObject fps_player_obj;
    private Level level;
    private float radius_of_search_for_player;
    private float virus_speed;

    private float storey_height;
    private Animator animator;
    private float maxHeight;

	void Start ()
    {
        GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();
        if (level == null)
        {
            Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
            return;
        }
        fps_player_obj = level.fps_player_obj;
        Bounds bounds = level.GetComponent<Collider>().bounds;
        radius_of_search_for_player = (bounds.size.x + bounds.size.z) / 5.0f;
        virus_speed = level.virus_speed;
        storey_height = level.storey_height;
        maxHeight = storey_height / 4.0f;

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on the virus!");
        }
    }

    public void UpdatePlayerReference(GameObject newPlayerObj)
    {
        fps_player_obj = newPlayerObj;
    }

    // *** YOU NEED TO COMPLETE THIS PART OF THE FUNCTION TO ANIMATE THE VIRUS ***
    // so that it moves towards the player when the player is within radius_of_search_for_player
    // a simple strategy is to update the position of the virus
    // so that it moves towards the direction d=v/||v||, where v=(fps_player_obj.transform.position - transform.position)
    // with rate of change (virus_speed * Time.deltaTime)
    // make also sure that the virus y-coordinate position does not go above the wall height
    // void Update()
    // {
    //     if (level.player_health < 0.001f || level.player_entered_house || fps_player_obj == null)
    //         return;
    //     Color redness = new Color
    //     {
    //         r = Mathf.Max(1.0f, 0.25f + Mathf.Abs(Mathf.Sin(2.0f * Time.time)))
    //     };
    //     // if ( transform.childCount > 0)
    //         // transform.GetChild(0).GetComponent<MeshRenderer>().material.color = redness;
    //     // else
    //         // transform.GetComponent<MeshRenderer>().material.color = redness;
    //     // transform.localScale = new Vector3(
    //     //                        0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time)), 
    //     //                        0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time)), 
    //     //                        0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time))
    //     //                        );
    //     transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
    //     /*** implement the rest ! */
    //     if (radius_of_search_for_player > Vector3.Distance(transform.position, fps_player_obj.transform.position) )
    //     {
    //         // Calculate direction towards the player
    //         Vector3 moveDir = (fps_player_obj.transform.position - transform.position);
    //         moveDir.y = 0;
    //         moveDir = moveDir.normalized;
    //         Vector3 nextPos = transform.position + moveDir * virus_speed * Time.deltaTime;

    //         nextPos.y = Mathf.Min(nextPos.y, maxHeight);

    //         // Apply the new position
    //         transform.position = nextPos;
    //     }
    //     transform.position = new Vector3(transform.position.x, Mathf.Min(transform.position.y, maxHeight), transform.position.z);

    // }


    void Update()
{
    if (level.player_health < 0.001f || level.player_entered_house || fps_player_obj == null)
        return;

    // Existing code for color and scale animations...
    // Color redness = new Color
    //     {
    //         r = Mathf.Max(1.0f, 0.25f + Mathf.Abs(Mathf.Sin(2.0f * Time.time)))
    //     };
        
        transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
       

    if (radius_of_search_for_player > Vector3.Distance(transform.position, fps_player_obj.transform.position))
    {
        // Calculate direction towards the player
        Vector3 moveDir = (fps_player_obj.transform.position - transform.position);
        moveDir.y = 0; // Keeping y-axis fixed to avoid tilting
        moveDir = moveDir.normalized;

        // Rotate towards the player
        Quaternion targetRotation = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, virus_speed * Time.deltaTime);

        // Check if the NPC is facing the player before moving
        if (Quaternion.Angle(transform.rotation, targetRotation) < 1.0f)
        {   animator.SetBool("isFlyingForward", true);
            animator.SetBool("isFireballShoot", true);
            Vector3 nextPos = transform.position + moveDir * virus_speed * Time.deltaTime;
            nextPos.y = Mathf.Min(nextPos.y, maxHeight);

            // Apply the new position
            transform.position = nextPos;
        }
        else
            {
                // NPC is not yet facing the player, stop the fly forward animation
                animator.SetBool("isFlyingForward", false);
                animator.SetBool("isFireballShoot", false);
            }
    }
     else
        {
            // NPC is outside the search radius, stop the fly forward animation
            animator.SetBool("isFlyingForward", false);
            animator.SetBool("isFireballShoot", false);
        }

    transform.position = new Vector3(transform.position.x, Mathf.Min(transform.position.y, maxHeight), transform.position.z);
}


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "PLAYER")
        {
            if (!level.virus_landed_on_player_recently)
                level.timestamp_virus_landed = Time.time;
            level.num_virus_hit_concurrently++;
            level.virus_landed_on_player_recently = true;
            Destroy(gameObject);
        }
    }
    
}