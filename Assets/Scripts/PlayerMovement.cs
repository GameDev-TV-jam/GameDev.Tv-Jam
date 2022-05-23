using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerMovement : MonoBehaviour
{
    #region Private Variables

    int jumps;

    float moveInput;
    float glideCurrentTimer;
    float glideDirection;

    Vector2 dashingDir;

    bool facingRight = true;
    bool isGrounded;
    bool isWalled;
    bool isGliding;
    bool glideCheck = true;
    bool isDashing;

    #endregion

    #region Serialize and Public Variables

    [Header("Componenets")]

    [SerializeField]
    Rigidbody2D playerRigidBody;

    [Header("Can Player")]

    [SerializeField]
    bool canWalk;

    [SerializeField]
    bool canJump;

    [SerializeField]
    bool canGlide;

    [SerializeField]
    bool canWallJump;

    [SerializeField]
    bool canDash;

    [Header("Key Codes")]

    [SerializeField]
    KeyCode JumpKey;

    [SerializeField]
    KeyCode GlideKey;

    [SerializeField]
    KeyCode DashKey;

    [Header("Ground Check Components Settings")]

    [SerializeField]
    Transform groundCheck;

    [SerializeField]
    float groundCheckRadius;

    [SerializeField]
    LayerMask WhatIsGround;

    [Header("Wall Jump Components")]

    [SerializeField]
    Transform wallCheck;

    [SerializeField]
    float wallCheckRadius;

    [SerializeField]
    LayerMask WhatIsWall;

    [Header("Player Move Settings")]

    [SerializeField]
    float baseSpeed;

    [SerializeField]
    float moveSpeed;

    [Header("Player Jump Settings")]

    [SerializeField]
    float playerJumpForce;

    [SerializeField]
    int extraJumps;

    [Header("Player Glide Settings")]

    [SerializeField] float GlideForce;
    [SerializeField] float StartGlideTime;

    [Header("Player Dash Settings")]
    [SerializeField]
    float dashPower = 14f;

    [SerializeField]
    float dashTime = 0.5f;

    #endregion

    #region In-Built Functions

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
    }

    void Start()
    {
        moveSpeed = baseSpeed;
    }

    private void Update()
    {
        if (isGrounded == true)
        {
            glideCheck = true;
            jumps = extraJumps;
        }

        if(canJump)
        {
            JumpMechanism();
        }

        if(canGlide)
        {
            GlideMechanics();
        }

        if(canWallJump)
        {
            WallJumpMechanism();
        }

        if(canDash)
        {
            DashMechanism();
        }
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, WhatIsGround);

        isWalled = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, WhatIsWall);

        if(canWalk)
        {
            PlayerMove();
        }
    }

    #endregion

    #region Player Attributes

    #region Movement Mechanics

    void PlayerMove()
    {
        if(!isDashing)
        {
            moveInput = Input.GetAxis("Horizontal");
            playerRigidBody.velocity = new Vector2(moveInput * moveSpeed, playerRigidBody.velocity.y);

            if (facingRight == false && moveInput > 0)
            {
                FlipCharacter();
            }
            else if (facingRight == true && moveInput < 0)
            {
                FlipCharacter();
            }
        }
        
    }

    #endregion

    #region Jump Mechanics

    void JumpMechanism()
    {
        if (Input.GetKeyDown(JumpKey) && jumps > 0)
        {
            playerRigidBody.velocity = playerJumpForce * Vector2.up;
            jumps--;
        }

        else if (Input.GetKeyUp(JumpKey) && jumps == 0 && isGrounded == true)
        {
            playerRigidBody.velocity = playerJumpForce * Vector2.up;
        }
    }

    #endregion

    #region Wall Mechanics

    void WallJumpMechanism()
    {
        if(isWalled)
        {
            jumps = extraJumps;
        }
    }

    #endregion

    #region Flip Mechanics

    void FlipCharacter()
    {
        facingRight = !facingRight;
        Vector3 scalar = transform.localScale;
        scalar.x *= -1;
        transform.localScale = scalar;
    }

    #endregion

    #region Glide Mechanics

    void GlideMechanics()
    {
        float movX = Input.GetAxis("Horizontal");

        if(Input.GetKeyDown(GlideKey) && !isGrounded && movX != 0 && glideCheck)
        {
            isGliding = true;
            glideCurrentTimer = StartGlideTime;
            playerRigidBody.velocity = Vector2.zero;
            glideDirection = (int)movX;
        }

        if(Input.GetKeyUp(GlideKey))
        {
            isGliding = false;
            glideCheck = false;
        }

        if (isGliding)
        {
            playerRigidBody.velocity = transform.right * glideDirection * GlideForce;

            glideCurrentTimer -= Time.deltaTime;

            if (glideCurrentTimer <= 0)
            {
                isGliding = false;
                glideCheck = false;
            }
        }
    }

    #endregion

    #region Dash Mechanism

    void DashMechanism()
    {
        if(CrossPlatformInputManager.GetButtonDown("Fire1"))
        {
            StartCoroutine(Dash());
        }
    }
    
    IEnumerator Dash()
    {
        isDashing = true;
        if(facingRight)
        {
            playerRigidBody.velocity = new Vector2(dashPower, playerRigidBody.velocity.y);
        }
        else
        {
            playerRigidBody.velocity = new Vector2(-dashPower, playerRigidBody.velocity.y);
        }

        yield return new WaitForSeconds(dashTime);
        isDashing = false;
    }

    #endregion

    #endregion
}
