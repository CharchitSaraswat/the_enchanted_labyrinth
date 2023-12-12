using UnityEngine;

public class SwordsmanController : MonoBehaviour
{
    private Animator animator;

    private CharacterController character_controller;
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;

    float gravity = -9.81f;
    private Vector3 movement_direction;
    private bool isRunning = false;

    public float velocity;

    private bool isWalking = false;
    private bool stab = false;
    private bool slash = false;
    private bool jab = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        velocity = 0.0f;
        character_controller = GetComponent<CharacterController>();
        movement_direction = new Vector3(0.0f, 0.0f, 0.0f);
    }

    void Update()
    {
        if (!character_controller.isGrounded) {
            movement_direction.y += gravity * Time.deltaTime;
        } else {
            movement_direction.y = 0.0f;
        }
        Vector3 moveVector = movement_direction * velocity * Time.deltaTime;

        // print character position
        // Debug.Log("Character position: " + transform.position);

        if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftShift)) {
            velocity = Mathf.Lerp(velocity, runSpeed / 2.0f, Time.deltaTime);
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", true);
            isWalking = false;
            isRunning = true;
            stab = false;
            slash = false;
            jab = false;
            movement_direction = transform.TransformDirection(Vector3.forward);
            character_controller.Move(moveVector);
        }
        else if (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.LeftShift)) {
            velocity = Mathf.Lerp(velocity, runSpeed / 2.0f, Time.deltaTime);
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", true);
            isWalking = false;
            isRunning = true;
            stab = false;
            slash = false;
            jab = false;
            movement_direction = transform.TransformDirection(Vector3.back);
            character_controller.Move(moveVector);
        }
        else if (Input.GetKey(KeyCode.UpArrow)){
            velocity = Mathf.Lerp(velocity, walkSpeed / 2.0f, Time.deltaTime);
            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", false);
            isWalking = true;
            isRunning = false;
            stab = false;
            slash = false;
            jab = false;
            movement_direction = transform.TransformDirection(Vector3.forward);
            character_controller.Move(moveVector);
        }
        else if (Input.GetKey(KeyCode.DownArrow)){
            velocity = Mathf.Lerp(velocity, walkSpeed / 2.0f, Time.deltaTime);
            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", false);
            isWalking = true;
            isRunning = false;
            stab = false;
            slash = false;
            jab = false;
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
                Debug.Log("Stab");
                animator.SetTrigger("stab");
                stab = true;
                slash = false;
                jab = false;
            }
            else if (Input.GetKey(KeyCode.A)) {
                Debug.Log("Slash");
                animator.SetTrigger("slash");
                slash = true;
                stab = false;
                jab = false;
            }
            else if (Input.GetKey(KeyCode.C)) {
                Debug.Log("Jab");
                animator.SetTrigger("jab");
                slash = false;
                stab = false;
                jab = true;
            }
            else {
                Debug.Log("Idle");
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
