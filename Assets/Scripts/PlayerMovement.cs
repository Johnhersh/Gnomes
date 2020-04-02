using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    public Rigidbody2D rigidBody;

    public Animator animator;
    
    Vector2 currentMovement;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Input handling

        currentMovement.x = Input.GetAxisRaw("Horizontal");
        currentMovement.y = Input.GetAxisRaw("Vertical");

        UpdateAnimationAndMove();
    }

    void UpdateAnimationAndMove()
    {
        if (currentMovement != Vector2.zero)
        {
            animator.SetFloat("Horizontal", currentMovement.x);
            animator.SetFloat("Vertical", currentMovement.y);
            animator.SetFloat("Speed", currentMovement.sqrMagnitude);
            animator.SetBool("bIsMoving", true);
        } else {
            animator.SetBool("bIsMoving", false);
        }
    }

    void FixedUpdate()
    {
        // Movement handling
        currentMovement.Normalize();

        rigidBody.MovePosition(rigidBody.position + currentMovement * moveSpeed * Time.fixedDeltaTime);
    }
}
