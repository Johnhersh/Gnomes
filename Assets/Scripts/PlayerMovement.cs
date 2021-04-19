using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    public Rigidbody2D rigidBody;

    public Animator animator;

    Vector2 _currentMovement;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Input handling

        _currentMovement.x = Input.GetAxisRaw("Horizontal");
        _currentMovement.y = Input.GetAxisRaw("Vertical");

        UpdateAnimationAndMove();
    }

    void UpdateAnimationAndMove()
    {
        if (_currentMovement != Vector2.zero)
        {
            animator.SetFloat("Horizontal", _currentMovement.x);
            animator.SetFloat("Vertical", _currentMovement.y);
            animator.SetFloat("Speed", _currentMovement.sqrMagnitude);
            animator.SetBool("bIsMoving", true);
        }
        else
        {
            animator.SetBool("bIsMoving", false);
        }
    }

    void FixedUpdate()
    {
        // Movement handling
        _currentMovement.Normalize();

        rigidBody.MovePosition(rigidBody.position + _currentMovement * moveSpeed * Time.fixedDeltaTime);
    }
}
