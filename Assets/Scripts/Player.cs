using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{

    [SerializeField] float runSpeed = 5.2f; //default run speed
    [SerializeField] float jumpSpeed = 8.5f; // default jump speed/height
    [SerializeField] float deathFall = -40f; // how far the player falls below 0 on y-axis before dying
    
    Rigidbody2D myRigidBody; //the player character's physical frame
    Animator myAnimator; //the animation component
    CapsuleCollider2D myBodyCollider; //handles collision for the main part of the player character
    BoxCollider2D myFeet; //handles collision (and therefore jumping and enemy kills) for the player character's feet.
    Vector2 playerVelocity;

    public float health = 3;
    bool isAlive = true;


    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        //myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeet = GetComponent<BoxCollider2D>();
    }


    void Update()
    {
        if (!isAlive)
        {
            return;
        }

        Run();
        Jump();
        Die();
    }

    private void Run()
    {
        float left_right_movement = CrossPlatformInputManager.GetAxis("Horizontal");
        playerVelocity = new Vector2(left_right_movement * runSpeed, myRigidBody.velocity.y); //creates a new x vector coordinate equal to player input times the runspeed variable
        myRigidBody.velocity = playerVelocity; //sets the player velocity equal to the new vector.
    }

    private void Jump()
    {
        if (myFeet.IsTouchingLayers(LayerMask.GetMask("Ground")) && CrossPlatformInputManager.GetButtonDown("Jump")) //by default gets player's "spacebar" input.
        {
            Vector2 jumpVelocity = new Vector2(0f, jumpSpeed); //creates a new y vector coordinate equal to the Jumpspeed variable
            myRigidBody.velocity += jumpVelocity; //sets the player character velocity equal to the new vector.
            //myAnimator.SetTrigger("isJumping");
        }
    }

    private void Die()
    {

        if (myRigidBody.transform.position.y < deathFall || health <= 0)
        {
            isAlive = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            //myAnimator.SetTrigger("Dying");
            //ResetHealth();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            health -= 1;
        }
    }


}
