using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1f;
    Rigidbody2D myRigidBody;
    bool isAlive = true;
    public Animator animator;

    Rigidbody2D enemyRB;

    float xStartPos;
    float currentPos;

    bool isObstructed;

    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();

        xStartPos = transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive)
        {
            return;
        }

        currentPos = transform.position.x;

        if(currentPos >= xStartPos + 5 || currentPos <= xStartPos - 5 || isObstructed)
        {
            isObstructed = false;
            Turn();
        }

        Move();
        
    }

    void Move()
    {
        myRigidBody.velocity = new Vector2(moveSpeed, 0f);
    }

    void Turn()
    {
        transform.localScale = new Vector2(-(Mathf.Sign(myRigidBody.velocity.x)), 1f);
        moveSpeed *= -1;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fireball"))
        {
            isAlive = false;
            Destroy(other.gameObject);
            StartCoroutine(Die());
        }

        if(gameObject.GetComponent<CapsuleCollider2D>().IsTouchingLayers(LayerMask.GetMask("Wall")) || !other.CompareTag("Player"))
        {
            isObstructed = true;
        }
    }

    IEnumerator Die()
    {
        enemyRB = GetComponent<Rigidbody2D>();
        enemyRB.velocity = new Vector2(0f, 3.2f);

        //enemyRB.constraints = RigidbodyConstraints2D.FreezePositionY;
        animator.SetBool("isDead", true);
        yield return new WaitForSeconds(.5f);
        Destroy(gameObject);
    }
}

