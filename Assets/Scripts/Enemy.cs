using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1f;
    Rigidbody2D myRigidBody;
    PolygonCollider2D damageCollider;
    CapsuleCollider2D bodyCollider;
    bool isAlive = true;

    float xStartPos;
    float currentPos;

    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        damageCollider = GetComponent<PolygonCollider2D>();
        bodyCollider = GetComponent<CapsuleCollider2D>();

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

        if(currentPos >= xStartPos + 5 || currentPos <= xStartPos - 5)
        {
            Turn();
        }

        Move();
        Die();
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


    private void Die()
    {
        BoxCollider2D playerFeet = GameObject.Find("Player").GetComponent<BoxCollider2D>();
        if (playerFeet.IsTouching(damageCollider))
        {
            isAlive = false;
            Destroy(gameObject);
        }
    }
}

