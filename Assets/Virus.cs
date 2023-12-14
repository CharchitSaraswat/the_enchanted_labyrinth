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
    private float radius_of_search_for_player = 15.0f;
    private float virus_speed;
    private SwordsmanController littleSoldier;
    private float storey_height;
    private Animator animator;
    public float maxHeight;
    public float virus_health = 100.0f;
    public float attack_radius = 1.0f;
    public float damagePerAttack = 1.0f;

    private float healthDeductionInterval = 0.5f;

    private float timer = 0.0f;

    private bool isDying = false;


	void Start ()
    {
        GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();

        GameObject littleSoldier_obj = GameObject.FindGameObjectWithTag("Player");
        littleSoldier = littleSoldier_obj.GetComponent<SwordsmanController>();

        if (level == null)
        {
            Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
            return;
        }
        fps_player_obj = level.fps_player_obj;
        Bounds bounds = level.GetComponent<Collider>().bounds;
    //    radius_of_search_for_player = (bounds.size.x + bounds.size.z) / 5.0f;
        virus_speed = level.virus_speed;
        storey_height = level.storey_height;

        maxHeight = storey_height / 15.0f;

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

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    void Update()
    {
        if (littleSoldier.player_health < 0.001f || level.player_entered_house || fps_player_obj == null)
            return;
        // Debug.Log("Virus health: " + virus_health);
        if (virus_health < 0.001f && !isDying)
        {
            isDying = true;
            animator.SetTrigger("dead");
            StartCoroutine(DestroyAfterDelay(2.0f)); // 1 second delay
            return;
        }
    
        if (!isDying)
        {
            timer += Time.deltaTime;

            transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        

            float distanceToPlayer = Vector3.Distance(transform.position, fps_player_obj.transform.position);

            if (distanceToPlayer <= attack_radius ) {
                animator.SetBool("isFlyingForward", false);
                
                if (timer >= healthDeductionInterval)
                {
                    animator.SetTrigger("flyShot");
                    littleSoldier.player_health -= damagePerAttack;
                    timer = 0.0f;
                }
            }
            else if (radius_of_search_for_player > distanceToPlayer)
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
                    Vector3 nextPos = transform.position + moveDir * virus_speed * Time.deltaTime;
                    nextPos.y = Mathf.Min(nextPos.y, maxHeight);

                    // Apply the new position
                    transform.position = nextPos;
                }
                else
                {
                    // NPC is not yet facing the player, stop the fly forward animation
                    animator.SetBool("isFlyingForward", false);
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
    }


    // private void OnCollisionEnter(Collision collision)
    // {
    //     if (collision.gameObject.name == "PLAYER")
    //     {
    //         if (!level.virus_landed_on_player_recently)
    //             level.timestamp_virus_landed = Time.time;
    //         level.num_virus_hit_concurrently++;
    //         level.virus_landed_on_player_recently = true;
    //         Destroy(gameObject);
    //     }
    // }
    
}