using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb;
    Animator animator;
    PlayerAttack playerAttack;

    [Header("GroundCheck")]
    [SerializeField] CapsuleCollider2D standingCollider;
    [SerializeField] Transform groundCheckCollider;

    [Header("Roll")]
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

    bool isGrounded;
    bool isRunning;
    bool facingRight = true;
    bool isRolling;
    bool isSliding;
    bool canRoll = true;
    bool multipleJump;
    bool coyoteJump;
    public bool isDead = false;

    [Header("Sounds")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip jumpSound1;

    void Start()
    {
        availableJumps = totalJumps;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerAttack = GetComponent<PlayerAttack>();
        platformParent = null;
        GroundCheck();
    }

    void Update()
    {
        if (CanMove() == false)
            return;

        horizontalValue = Input.GetAxisRaw("Horizontal");
        verticalValue = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (isGrounded && !isRolling && IsOnSlope())
            {
                Slide();
            }
            else
            {
                isRunning = true;
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isRunning = false;
            isSliding = false;
            animator.SetBool("IsSliding", false);
        }

        if (Input.GetButtonDown("Jump"))
            Jump();

        if (Input.GetKeyDown(KeyCode.LeftControl) && canRoll && isGrounded && !isRolling)
        {
            StartCoroutine(Roll());
        }

        animator.SetFloat("yVelocity", rb.velocity.y);
    }

    void FixedUpdate()
    {
        GroundCheck();
        Move(horizontalValue, verticalValue);
    }

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
                animator.SetBool("IsJumping", false);
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

        animator.SetBool("IsJumping", !isGrounded);
    }

    IEnumerator CoyoteJumpDelay()
    {
        coyoteJump = true;
        yield return new WaitForSeconds(0.2f);
        coyoteJump = false;
    }

    void Jump()
    {
        if (!isRolling)
        {
            if (!isDead)
            {
                if (isGrounded)
                {
                    multipleJump = true;
                    availableJumps--;
                    rb.velocity = Vector2.up * jumpPower;
                    animator.SetBool("IsJumping", true);
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
                        animator.SetBool("IsJumping", false);
                    }
                }
            }
        }
    }

    void Move(float horizontalDir, float verticalDir)
    {
        isRunning = horizontalDir != 0;

        animator.SetBool("IsRunning", isRunning);

        float xVal = horizontalDir * speed * 100 * Time.fixedDeltaTime;

        if (isRunning)
            xVal *= runSpeedModifier;

        if (isRolling)
            xVal *= 2;

        RaycastHit2D hit = Physics2D.Raycast(groundCheckCollider.position, Vector2.down, groundCheckRadius + 0.1f, groundLayer);
        if (hit.collider != null)
        {
            Vector2 normal = hit.normal;
            float slopeAngle = Vector2.Angle(normal, Vector2.up);
            if (slopeAngle > 45f)
            {
                xVal = 0;
            }
        }

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
    }

    IEnumerator Roll()
    {
        canRoll = false;
        isRolling = true;
        animator.SetBool("IsRolling", true);

        yield return new WaitForSeconds(0.5f);

        animator.SetBool("IsRolling", false);
        isRolling = false;

        yield return new WaitForSeconds(2f);
        canRoll = true;
    }

    void Slide()
    {
        isSliding = true;
        animator.SetBool("IsSliding", true);
        rb.velocity = new Vector2(rb.velocity.x, -speed * 2);
    }

    bool IsOnSlope()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheckCollider.position, Vector2.down, groundCheckRadius + 0.1f, groundLayer);
        if (hit.collider != null)
        {
            Vector2 normal = hit.normal;
            float slopeAngle = Vector2.Angle(normal, Vector2.up);
            return slopeAngle > 0 && slopeAngle < 90;
        }
        return false;
    }

    public void TakeDamage()
    {
        if (isDead) return;

        animator.SetTrigger("TakeDamageTrigger");
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetBool("IsDead", true);
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
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

        if (isDead)
            can = false;

        if (isRolling)
            can = false;

        return can;
    }

    public bool CanAttack()
    {
        return !isDead && !isRolling && isGrounded;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheckCollider.position, groundCheckRadius);
    }
}
