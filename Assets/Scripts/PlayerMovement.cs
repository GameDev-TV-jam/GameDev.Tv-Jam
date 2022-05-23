using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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

    public Animator animator;

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

    [Header("Health and Special and Death")]

    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthFill;

    [SerializeField] float deathFall = -40f; // how far the player falls below 0 on y-axis before dying
    [SerializeField] GameObject fireBall;
    [SerializeField] Vector2 fireBallVelocity = new Vector2(1.5f, 0f);

    CapsuleCollider2D myBodyCollider; //handles collision for the main part of the player character
    BoxCollider2D myFeet; //handles collision (and therefore jumping and enemy kills) for the player character's feet.

    public float maxHealth = 3;
    public float health = 3;
    bool isAlive = true;
    bool isTakingDamage = false;
    bool isKnockedBack = false;

    Vector2 fireBallSpawnPoint;
    Vector2 Knockback;


    #endregion

    #region In-Built Functions

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
    }

    private void Awake()
    {
        health = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;
    }

    void Start()
    {
        moveSpeed = baseSpeed;
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeet = GetComponent<BoxCollider2D>();
        

    }

    private void Update()
    {
        if (!isAlive)
        {
            return;
        }

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

        Die();
        ShootFireball();
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

    private void ShootFireball()
    {
        if (CrossPlatformInputManager.GetButtonDown("Fire1"))
        {
            fireBallSpawnPoint = new Vector2(gameObject.transform.GetChild(0).transform.position.x, gameObject.transform.GetChild(0).transform.position.y);
            GameObject fireBallInstance = Instantiate(fireBall, fireBallSpawnPoint, Quaternion.identity);
            fireBallInstance.GetComponent<Rigidbody2D>().velocity = fireBallVelocity;
        }
    }

    private void Die()
    {

        if (playerRigidBody.transform.position.y < deathFall || health <= 0)
        {
            isAlive = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            //myAnimator.SetTrigger("Dying");
            //ResetHealth();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && isTakingDamage == false)
        {
            if(other.gameObject.transform.position.x > this.transform.position.x)
            {
                Knockback = new Vector2(-5f, 3f);
            }

            if (other.gameObject.transform.position.x < this.transform.position.x)
            {
                Knockback = new Vector2(5f, 3f);
            }

            StartCoroutine(TakeDamage());
        }
    }

    IEnumerator TakeDamage()
    {
        isTakingDamage = true;
        isKnockedBack = true;
        playerRigidBody.velocity = Knockback;
        health -= 1;
        healthBar.value = health;
        yield return new WaitForSeconds(.3f);
        isKnockedBack = false;
        isTakingDamage = false;
    }

    void PlayerMove()
    {
        if(!isDashing && !isKnockedBack)
        {
            moveInput = Input.GetAxis("Horizontal");
            animator.SetFloat("Speed", Mathf.Abs(moveInput));
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
