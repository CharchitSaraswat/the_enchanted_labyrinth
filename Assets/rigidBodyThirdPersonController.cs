using UnityEngine;
using UnityEngine.UI;

public class SwordsmanController : MonoBehaviour
{
    private Animator animator;

    private CharacterController character_controller;
    public float walkSpeed = 3.0f;
    public float runSpeed = 5.0f;

    float gravity = -9.81f;
    private Vector3 movement_direction;
    private bool isRunning = false;

    public float velocity;

    private Level level;

    private bool isWalking = false;
    private bool stab = false;
    private bool slash = false;
    private bool jab = false;

    public GameObject scroll_bar;
    private bool is_dead = false;

    public float player_health = 100.0f;

    RaycastHit hit;

    private Virus og_dragon;
    private float maxHeight;
    private GameObject level_obj;

    private float healthDeductionInterval = 0.25f;

    private float timer = 0.0f;
    private bool isDying = false;

    void Start()
    {
        level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();
        animator = GetComponent<Animator>();
        velocity = 0.0f;
        character_controller = GetComponent<CharacterController>();
        movement_direction = new Vector3(0.0f, 0.0f, 0.0f);
    }

    private void PerformAttack()
    {
        float attackRadius = 1.2f; // Example radius
        float attackAngle = 60.0f; // Example angle for the field of view

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRadius);
        foreach (var hitCollider in hitColliders)
        {
            Debug.Log("Hit name: " + hitCollider.name);
            if (hitCollider.name == "COVID")
            {
                // Calculate direction from player to the potential target
                Vector3 targetDirection = hitCollider.transform.position - transform.position;
                // Calculate the angle between the forward direction of the player and the direction to the target
                float angle = Vector3.Angle(targetDirection, transform.forward);

                // Check if the target is within the player's field of view
                if (angle <= attackAngle)
                {
                    Virus dragon = hitCollider.GetComponent<Virus>();
                    if (dragon != null)
                    {
                        Debug.Log("Hit dragon");
                        // Perform attack
                        dragon.virus_health -= 5.0f; // Reduce health
                        Debug.Log("Hit health: " + dragon.virus_health);
                    }
                }
            }
        }
    }


    void Update()
    {
        // Debug.Log("Player health: " + player_health);
        timer += Time.deltaTime;
        scroll_bar.GetComponent<Scrollbar>().size = player_health / 100.0f;
        if (0 < player_health && player_health < 50.0f)
        {
            ColorBlock cb = scroll_bar.GetComponent<Scrollbar>().colors;
            cb.disabledColor = new Color(1.0f, 0.0f, 0.0f);
            scroll_bar.GetComponent<Scrollbar>().colors = cb;
        }
        else
        {
            ColorBlock cb = scroll_bar.GetComponent<Scrollbar>().colors;
            cb.disabledColor = new Color(0.0f, 1.0f, 0.25f);
            scroll_bar.GetComponent<Scrollbar>().colors = cb;
        }
        if (player_health <= 0.001f && !isDying)
        {
            isDying = true;
            animator.SetTrigger("dead");
            return;
        }
        if (!isDying)
        {
            if (!character_controller.isGrounded) {
                movement_direction.y += gravity * Time.deltaTime;
            } else {
                movement_direction.y = 0.0f;
            }
            Vector3 moveVector = movement_direction * velocity * Time.deltaTime;

            if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftShift)) {
                velocity = Mathf.Lerp(velocity, runSpeed / 2.0f, Time.deltaTime);
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", true);
                movement_direction = transform.TransformDirection(Vector3.forward);
                character_controller.Move(moveVector);
            }
            // else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.LeftShift)) {
            //     velocity = Mathf.Lerp(velocity, runSpeed / 2.0f, Time.deltaTime);
            //     animator.SetBool("isWalking", false);
            //     animator.SetBool("isRunning", true);
            //     movement_direction = transform.TransformDirection(Vector3.back);
            //     character_controller.Move(moveVector);
            // }
            else if (Input.GetKey(KeyCode.UpArrow)){
                velocity = Mathf.Lerp(velocity, walkSpeed / 2.0f, Time.deltaTime);
                animator.SetBool("isWalking", true);
                animator.SetBool("isRunning", false);
                movement_direction = transform.TransformDirection(Vector3.forward);
                character_controller.Move(moveVector);
            }
            else if (Input.GetKey(KeyCode.DownArrow)){
                velocity = Mathf.Lerp(velocity, walkSpeed / 2.0f, Time.deltaTime);
                animator.SetBool("isWalking", true);
                animator.SetBool("isRunning", false);
                movement_direction = transform.TransformDirection(Vector3.back);
                character_controller.Move(moveVector);
            }
            else {
                velocity = Mathf.Lerp(velocity, 0.0f, Time.deltaTime);
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
                isWalking = false;
                isRunning = false;
                if (Input.GetKey(KeyCode.S)) {
                    // Debug.Log("Stab");
                    animator.SetTrigger("stab");
                    stab = true;
                    slash = false;
                    jab = false;
                    if (timer >= healthDeductionInterval)
                    {
                        timer = 0.0f;
                        PerformAttack();
                    }
                }
                else if (Input.GetKey(KeyCode.A)) {
                    // Debug.Log("Slash");
                    animator.SetTrigger("slash");
                    slash = true;
                    stab = false;
                    jab = false;
                    if (timer >= healthDeductionInterval)
                    {
                        timer = 0.0f;
                        PerformAttack();
                    }
                }
                else if (Input.GetKey(KeyCode.C)) {
                    // Debug.Log("Jab");
                    animator.SetTrigger("jab");
                    slash = false;
                    stab = false;
                    jab = true;
                    if (timer >= healthDeductionInterval)
                    {
                        timer = 0.0f;
                        PerformAttack();
                    }
                }
                else {
                    stab = false;
                    slash = false;
                    jab = false;
                }
            }

            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            {
                float rotationSpeed = 100.0f;
                transform.Rotate(0, (Input.GetKey(KeyCode.RightArrow) ? 1 : -1) * rotationSpeed * Time.deltaTime, 0);
            }
        }
    }
}
