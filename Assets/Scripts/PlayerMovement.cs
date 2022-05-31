using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerMovement : MonoBehaviour
{
    #region Variables/Declarations

    int jumps;

    float moveInput;

    bool facingRight = true;
    //bool isGrounded;
    bool isDashing;

    [Header("Componenets")]

    [SerializeField]
    Rigidbody2D playerRigidBody;

    [Header("Can Player")]

    [SerializeField]
    bool canWalk;

    [SerializeField]
    bool canJump;

    [SerializeField]
    bool canDash;

    [Header("Key Codes")]

    [SerializeField]
    KeyCode JumpKey;

    public Animator animator;

    [Header("Ground Check Components Settings")]

    [SerializeField]
    Transform groundCheck;

    [SerializeField]
    float groundCheckRadius;

    [SerializeField]
    LayerMask WhatIsGround;

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

    [Header("Player Dash Settings")]
    [SerializeField]
    float dashPower = 14f;

    [SerializeField]
    float dashTime = 0.5f;

    [Header("Health and Special and Death")]

    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthFill;

    //[SerializeField] float deathFall = -40f; // how far the player falls below 0 on y-axis before dying
    [SerializeField] GameObject fireBall;
    [SerializeField] Vector2 fireBallVelocity = new Vector2(1.5f, 0f);
    [SerializeField] Text currentAbilityText;
    [SerializeField] Text CollectiblesCollectedText;
    [SerializeField] BoxCollider2D playerBoxCollider;
    [SerializeField] Barrier barrier;

    [SerializeField] Text unlockDoorText;

    [SerializeField] int keysToUnlockDoor = 8;

    [SerializeField] ParticleSystem dashParticles;

    [SerializeField] TrailRenderer dashTrail;

    [SerializeField] GameObject Key;

    public PlayerMovement player;

    [SerializeField] AudioSource fireBallSound;
    [SerializeField] AudioSource hurtSound;
    [SerializeField] AudioSource jumpSound;
    [SerializeField] AudioSource dashSound;
    [SerializeField] AudioSource keySound;

    public float maxHealth = 3;
    public float health = 3;
    bool isAlive = true;
    bool isTakingDamage = false;
    bool isKnockedBack = false;

    bool FireSpecial = false;
    bool DashSpecial = false;
    public bool isFireballInScene = false;

    int SpecialNum;
    int CollectiblesCount = 0;

    float lastXPosition;
    float lastYPosition;

    Vector2 fireBallSpawnPoint;
    Vector2 Knockback;

    CinemachineVirtualCamera vcam;
    CinemachineBasicMultiChannelPerlin noise;


    #endregion

    #region Awake/Start/Update

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    private void Awake()
    {
        health = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;
    }

    void Start()
    {
        vcam = GameObject.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>();
        noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        playerRigidBody.constraints = RigidbodyConstraints2D.None;
        playerRigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;

        Vector3 scalar = transform.localScale;
        scalar.x = 1;
        transform.localScale = scalar;

        moveSpeed = baseSpeed;

        vcam.Follow = this.transform;
        vcam.LookAt = this.transform;

        if (PlayerPrefs.HasKey("PositionX") || PlayerPrefs.HasKey("Collected"))
        {
            //SceneManager.LoadScene(PlayerPrefs.GetInt("CurrentLevel"));
            gameObject.transform.position = new Vector2(PlayerPrefs.GetFloat("PositionX"), PlayerPrefs.GetFloat("PositionY"));
            CollectiblesCount = PlayerPrefs.GetInt("Collected");
            CollectiblesCollectedText.text = CollectiblesCount.ToString();
            
        }

        lastXPosition = gameObject.transform.position.x;
        lastYPosition = gameObject.transform.position.y;

        PlayerPrefs.SetFloat("PositionX", lastXPosition);
        PlayerPrefs.SetFloat("PositionY", lastYPosition);
        PlayerPrefs.SetInt("CurrentLevel", SceneManager.GetActiveScene().buildIndex);

        SpecialNum = 0;

        if (PlayerPrefs.HasKey("SpecialNum"))
        {
            if (PlayerPrefs.GetInt("SpecialNum") != 2)
            {
                SpecialNum = PlayerPrefs.GetInt("SpecialNum") + 1;
            }
        }

        if(SpecialNum == 0)
        {
            extraJumps = 1;
            FireSpecial = false;
            DashSpecial = false;

            currentAbilityText.text = "Double Jump";
        }
        if (SpecialNum == 1)
        {
            extraJumps = 0;
            FireSpecial = true;
            DashSpecial = false;

            currentAbilityText.text = "Fireball";
        }
        if (SpecialNum == 2)
        {
            extraJumps = 0;
            FireSpecial = false;
            DashSpecial = true;

            currentAbilityText.text = "Dash";
        }

    }

    private void Update()
    {
        if (!isAlive)
        {
            return;
        }

        if(!playerBoxCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            animator.SetBool("isJumping", true);
        }

        if (playerBoxCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            jumps = extraJumps;
            animator.SetBool("isJumping", false);
        }

        if(canJump)
        {
            JumpMechanism();
        }

        if(canDash)
        {
            DashMechanism();
        }
        
        ShootFireball();
        Pause();

        if (health <= 0)
        {
            StartCoroutine(Die());
        }

        if (CrossPlatformInputManager.GetButtonDown("Fire3"))
        {
            Suicide();
        }


    }

    private void FixedUpdate()
    {
        //isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, WhatIsGround);

        if(canWalk)
        {
            PlayerMove();
        }
    }

    #endregion

    #region Game Management

    private void Pause()
    {
        if (CrossPlatformInputManager.GetButtonDown("Cancel"))
        {
            transform.GetChild(2).gameObject.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void UnPause()
    {
        Time.timeScale = 1;
        transform.GetChild(2).gameObject.SetActive(false);
    }

    public void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    public void Save()
    {
        lastXPosition = gameObject.transform.position.x;
        lastYPosition = gameObject.transform.position.y;

        PlayerPrefs.SetFloat("PositionX", lastXPosition);
        PlayerPrefs.SetFloat("PositionY", lastYPosition);
        PlayerPrefs.SetInt("CurrentLevel", SceneManager.GetActiveScene().buildIndex);
        PlayerPrefs.SetInt("Collected", CollectiblesCount);
    }

    IEnumerator UnlockDoorMessage()
    {
        transform.GetChild(1).transform.GetChild(0).gameObject.SetActive(true);
        unlockDoorText.text = "You need " + (keysToUnlockDoor - CollectiblesCount) + " more key(s) to unlock the door!";

        yield return new WaitForSeconds(1f);
        unlockDoorText.text = "";
        transform.GetChild(1).transform.GetChild(0).gameObject.SetActive(false);
    }

    #endregion

    #region Damage/Death/Collectibles/Level Exit

    IEnumerator Die()
    {
        if (isAlive)
        {
            isAlive = false;
            animator.SetTrigger("isDead");
            this.playerRigidBody.constraints = RigidbodyConstraints2D.FreezeAll;

            yield return new WaitForSeconds(1f);

            lastXPosition = gameObject.transform.position.x;
            lastYPosition = gameObject.transform.position.y;

            PlayerPrefs.SetFloat("PositionX", lastXPosition);
            PlayerPrefs.SetFloat("PositionY", lastYPosition);
            PlayerPrefs.SetInt("CurrentLevel", SceneManager.GetActiveScene().buildIndex);
            PlayerPrefs.SetInt("SpecialNum", SpecialNum);

            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Instantiate(player, this.transform.position, player.transform.rotation);
            Destroy(this.gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && isTakingDamage == false)
        {
            if (other.gameObject.transform.position.x > this.transform.position.x)
            {
                Knockback = new Vector2(-5f, 0f);
            }

            if (other.gameObject.transform.position.x < this.transform.position.x)
            {
                Knockback = new Vector2(5f, 0f);
            }

            StartCoroutine(TakeDamage());
        }

        if (other.CompareTag("Collectibles"))
        {
            keySound.Play();
            CollectiblesCount += 1;
            CollectiblesCollectedText.text = CollectiblesCount.ToString();
            PlayerPrefs.SetInt("Collected", CollectiblesCount);
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Exit"))
        {
            if (CollectiblesCount >= keysToUnlockDoor)
            {
                PlayerPrefs.DeleteAll();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                StartCoroutine(UnlockDoorMessage());
            }
        }

        if (other.CompareTag("Barrier") && isDashing)
        {
            noise.m_AmplitudeGain = 5;
            noise.m_FrequencyGain = 5;
            barrier = other.gameObject.GetComponent<Barrier>();
            StartCoroutine(barrier.DestroyBarrier());
        }
    }

    IEnumerator TakeDamage()
    {
        isTakingDamage = true;
        isKnockedBack = true;
        playerRigidBody.velocity = Knockback;
        health -= 1;
        hurtSound.Play();

        if (health != 0)
        {
            animator.SetTrigger("isHurting");
        }

        healthBar.value = health;
        yield return new WaitForSeconds(.3f);
        isKnockedBack = false;
        isTakingDamage = false;
    }

    public void Suicide()
    {
        health = 0;
    }

    #endregion

    #region Movement Mechanics

    void PlayerMove()
    {
        if(!isDashing && !isKnockedBack && isAlive)
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

    #region Fireball Mechanics

    private void ShootFireball()
    {
        if (CrossPlatformInputManager.GetButtonDown("Fire1") && FireSpecial == true && isFireballInScene == false)
        {
            isFireballInScene = true;
            StartCoroutine(ShootAnim());
            fireBallSpawnPoint = new Vector2(gameObject.transform.GetChild(0).transform.position.x, gameObject.transform.GetChild(0).transform.position.y);
            GameObject fireBallInstance = Instantiate(fireBall, fireBallSpawnPoint, Quaternion.identity);
            fireBallSound.Play();

            if (facingRight)
            {
                fireBallInstance.GetComponent<Rigidbody2D>().velocity = fireBallVelocity;
            }

            if (!facingRight)
            {
                Vector3 scalar = fireBallInstance.transform.localScale;
                scalar.x *= -1;
                fireBallInstance.transform.localScale = scalar;
                fireBallInstance.GetComponent<Rigidbody2D>().velocity = fireBallVelocity * -1;
            }
        }
    }

    IEnumerator ShootAnim()
    {
        animator.SetBool("isShooting", true);
        yield return new WaitForSeconds(1f);
        animator.SetBool("isShooting", false);
        isFireballInScene = false;
    }

    #endregion

    #region Jump Mechanics

    void JumpMechanism()
    {
        if (Input.GetKeyDown(JumpKey) && jumps > 0)
        {
            playerRigidBody.velocity = playerJumpForce * Vector2.up;
            jumps--;
            jumpSound.Play();

        }

        else if (Input.GetKeyDown(JumpKey) && jumps == 0 && playerBoxCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            playerRigidBody.velocity = playerJumpForce * Vector2.up;
            jumpSound.Play();
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

    #region Dash Mechanics

    void DashMechanism()
    {
        if(CrossPlatformInputManager.GetButtonDown("Fire2") && DashSpecial == true)
        {
            StartCoroutine(Dash());
        }
    }
    
    IEnumerator Dash()
    {
        dashSound.Play();
        isDashing = true;
        if(facingRight)
        {
            playerRigidBody.velocity = new Vector2(dashPower, playerRigidBody.velocity.y);
        }
        else
        {
            playerRigidBody.velocity = new Vector2(-dashPower, playerRigidBody.velocity.y);
        }

        //dashParticles.Play();
        dashTrail.emitting = true;

        yield return new WaitForSeconds(dashTime);
        isDashing = false;
        noise.m_AmplitudeGain = 0;
        noise.m_FrequencyGain = 0;
        dashTrail.emitting = false;
    }

    #endregion

}
