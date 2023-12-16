using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Dragon : MonoBehaviour
{
    private GameObject player_obj;
    private Level level;
    private float radius_of_search_for_player = 20.0f;
    private float dragon_speed;
    private SwordsmanController littleSoldier;
    private float storey_height;
    private Animator animator;
    public float maxHeight;
    public float dragon_health = 100.0f;
    public float attack_radius = 1.0f;
    public float damagePerAttack = 1.0f;

    private float healthDeductionInterval = 0.5f;

    private float timer = 0.0f;

    private bool isDying = false;
    private Rigidbody rb;

    private AudioSource source;

    public AudioClip dino_attack;


	void Start ()
    {
        source = gameObject.GetComponent<AudioSource>();
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
        }
        dino_attack = Resources.Load<AudioClip>("DinoAttack");

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found on the dragon!");
        }
        GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();

        GameObject littleSoldier_obj = GameObject.FindGameObjectWithTag("Player");
        littleSoldier = littleSoldier_obj.GetComponent<SwordsmanController>();

        if (level == null)
        {
            Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
            return;
        }
        player_obj = level.player_obj;
        Bounds bounds = level.GetComponent<Collider>().bounds;
    //    radius_of_search_for_player = (bounds.size.x + bounds.size.z) / 5.0f;
        dragon_speed = level.dragon_speed;
        storey_height = level.storey_height;

        maxHeight = storey_height / 15.0f;

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on the dragon!");
        }
    }

    public void PlaySoundWithLimit(AudioClip clip, float duration)
    {
        source.PlayOneShot(clip);
        StartCoroutine(StopAudioAfterDuration(duration));
    }

    private IEnumerator StopAudioAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        source.Stop();
    }

    public void UpdatePlayerReference(GameObject newPlayerObj)
    {
        player_obj = newPlayerObj;
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    private void MoveTowardsPlayer(Vector3 moveDir)
    {
        Vector3 nextPos = rb.position + moveDir * dragon_speed * Time.deltaTime;
        nextPos.y = Mathf.Min(nextPos.y, maxHeight);
        rb.MovePosition(nextPos);
    }

    private void RotateTowardsPlayer(Quaternion targetRotation)
    {
        Quaternion newRotation = Quaternion.Slerp(rb.rotation, targetRotation, dragon_speed * Time.deltaTime);
        rb.MoveRotation(newRotation);
    }

    void FixedUpdate()
    {
        if (littleSoldier.player_health < 0.001f || level.player_entered_house || player_obj == null)
            return;
        if (dragon_health < 0.001f && !isDying)
        {
            isDying = true;
            animator.SetTrigger("dead");
            level.DefeatDragon();
            StartCoroutine(DestroyAfterDelay(2.0f)); // 1 second delay
            return;
        }

        if(!isDying) 
        {
            timer += Time.deltaTime;
        
            transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        

            float distanceToPlayer = Vector3.Distance(transform.position, player_obj.transform.position);

            if (distanceToPlayer <= attack_radius ) {
                animator.SetBool("isFlyingForward", false);
                if (timer >= healthDeductionInterval)
                {
                    animator.SetTrigger("flyShot");
                    source.PlayOneShot(dino_attack);
                    timer = 0.0f;
                    littleSoldier.player_health -= damagePerAttack;
                }
            }
            else if (radius_of_search_for_player > distanceToPlayer)
            {
                Vector3 moveDir = (player_obj.transform.position - transform.position);
                moveDir.y = 0; // Keeping y-axis fixed to avoid tilting
                moveDir = moveDir.normalized;

                Quaternion targetRotation = Quaternion.LookRotation(moveDir);

                float angleToPlayer = Quaternion.Angle(rb.rotation, targetRotation);

                if (angleToPlayer < 1.0f)
                {
                    animator.SetBool("isFlyingForward", true);
                    MoveTowardsPlayer(moveDir);
                }
                else {
                    animator.SetBool("isFlyingForward", false);
                    RotateTowardsPlayer(targetRotation);
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
}