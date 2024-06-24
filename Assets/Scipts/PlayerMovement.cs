using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb;
    Animator animator;
    [Header("GroundCheck")]
    [SerializeField] Collider2D standingCollider;
    [SerializeField] Transform groundCheckCollider;

    [Header("Crouch")]
    [SerializeField] Transform overheadCheckCollider;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] TrailRenderer tr;
    private Transform platformParent;

    const float groundCheckRadius = 0.2f;
    const float overheadCheckRadius = 0.2f;
    public float speed;
    [SerializeField] float jumpPower = 100;
    [SerializeField] int totalJumps;
    int availableJumps;
    float horizontalValue;
    float verticalValue;
    float runSpeedModifier = 2f;
    float crouchSpeedModifier = 0.5f;

    bool isGrounded;
    bool isRunning;
    bool facingRight = true;
    bool crouchPressed;
    bool multipleJump;
    bool coyoteJump;
    bool isDead = false;
    bool isAttacking = false;

    [Header("Sounds")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip jumpSound1;
    /* public InteractionSystem interactionSystem; */

    void Start()
    {
        availableJumps = totalJumps;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        /*interactionSystem = FindObjectOfType<InteractionSystem>();*/
        platformParent = null;
        GroundCheck(); // Kiểm tra trạng thái đất ngay từ đầu
    }

    void Update()
    {
        if (CanMove() == false)
            return;

        // Store the horizontal and vertical values
        horizontalValue = Input.GetAxisRaw("Horizontal");
        verticalValue = Input.GetAxisRaw("Vertical");

        // Setting to Key "Run" for player
        if (Input.GetKeyDown(KeyCode.LeftShift))
            isRunning = true;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            isRunning = false;

        // Setting to Key "Jump" for player
        if (Input.GetButtonDown("Jump"))
            Jump();

        // Setting to Key "Crouch" for player
        if (Input.GetKeyDown(KeyCode.LeftControl))
            crouchPressed = true;
        else if (Input.GetKeyUp(KeyCode.LeftControl))
            crouchPressed = false;

        // Setting to Key "Attack" for player
        if (Input.GetButtonDown("Fire1"))
            Attack();

        // Set to the yVelocity in the animator
        animator.SetFloat("yVelocity", rb.velocity.y);
    }

    void FixedUpdate()
    {
        GroundCheck();
        Move(horizontalValue, verticalValue, crouchPressed);
    }

    #region CheckGround
    void GroundCheck()
{
    bool wasGrounded = isGrounded;
    isGrounded = false;

    Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckCollider.position, groundCheckRadius, groundLayer);
    if (colliders.Length > 0)
    {
        isGrounded = true;
        if (!wasGrounded)
        {
            availableJumps = totalJumps;
            multipleJump = false;
            animator.SetBool("IsJumping", false); // Reset IsJumping when grounded
        }

        foreach (var c in colliders)
        {
            if (c.tag == "MovingPlatform")
                transform.parent = c.transform;
        }
    }
    else
    {
        transform.parent = null;

        if (wasGrounded)
            StartCoroutine(CoyoteJumpDelay());
    }

    animator.SetBool("IsJumping", !isGrounded); // Set IsJumping based on current state
}
    #endregion

    #region Jump
    IEnumerator CoyoteJumpDelay()
    {
        coyoteJump = true;
        yield return new WaitForSeconds(0.2f);
        coyoteJump = false;
    }

    void Jump()
    {
        if (!crouchPressed) // Only jump if the player is not crouching
        {
            if (!isDead)
            {
                if (isGrounded)
                {
                    multipleJump = true;
                    availableJumps--;
                    rb.velocity = Vector2.up * jumpPower;
                    animator.SetBool("IsJumping", true);
                    // Play jump sound here if needed
                }
                else
                {
                    if (coyoteJump)
                    {
                        Debug.Log("Coyote Jump");
                    }

                    if (multipleJump && availableJumps > 0)
                    {
                        availableJumps--;
                        rb.velocity = Vector2.up * jumpPower;
                        animator.SetBool("IsJumping", false); // Reset IsJumping immediately
                    }
                }
            }
        }
    }
    #endregion

    void Move(float horizontalDir, float verticalDir, bool crouchFlag)
    {
        if (isAttacking && horizontalDir != 0)
        {
            animator.SetBool("IsAttacking", false);
            isAttacking = false;
        }

        // Update isRunning based on whether horizontalDir is not zero
        isRunning = horizontalDir != 0;

        animator.SetBool("IsRunning", isRunning);



        #region Crouch
        if (isGrounded)
        {
            if (!crouchFlag)
            {
                if (Physics2D.OverlapCircle(overheadCheckCollider.position, overheadCheckRadius, groundLayer))
                    crouchFlag = true;
            }

            animator.SetBool("IsCrouching", crouchFlag);
            standingCollider.enabled = !crouchFlag;
        }
        #endregion

        #region Move & Run
        float xVal = horizontalDir * speed * 100 * Time.fixedDeltaTime;

        if (isRunning)
            xVal *= runSpeedModifier;

        if (crouchFlag)
            xVal *= crouchSpeedModifier;

        Vector2 targetVelocity = new Vector2(xVal, rb.velocity.y);
        rb.velocity = targetVelocity;

        if (facingRight && horizontalDir < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            facingRight = false;
        }
        else if (!facingRight && horizontalDir > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            facingRight = true;
        }

        animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
        #endregion
    }

    void Attack()
    {
        if (!isAttacking && !isDead)
        {
            isAttacking = true;
            animator.SetBool("IsAttacking", true);
            StartCoroutine(ResetAttack());
        }
    }

    private IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(0.5f); // Adjust this to the duration of your attack animation
        isAttacking = false;
        animator.SetBool("IsAttacking", false);
    }

    public void TakeDamage()
    {
        if (isDead) return;

        animator.SetTrigger("TakeDamageTrigger");
        // Add logic to reduce health here if needed

        // Optionally, handle knockback or other effects here
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetBool("IsDead", true);
        rb.velocity = Vector2.zero; // Stop the player
        rb.isKinematic = true; // Prevent the player from being affected by physics
    }

    public void ResetPlayer()
    {
        isDead = false;
        animator.SetBool("IsDead", false);
        rb.isKinematic = false;
    }

    public bool CanMove()
    {
        bool can = true;

        /* if (interactionSystem.isExamining)
            can = false;
        if (FindObjectOfType<InventorySystem>().isOpen)
            can = false;*/
        if (isDead)
            can = false;

        return can;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(groundCheckCollider.position, groundCheckRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(overheadCheckCollider.position, overheadCheckRadius);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        /*if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage();
        }*/
    }

    #region Moving Platform
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("MovingPlatform"))
        {
            platformParent = other.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("MovingPlatform"))
        {
            platformParent = null;
        }
    }
    #endregion
}
