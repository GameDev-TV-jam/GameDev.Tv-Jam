using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    [SerializeField] float fireBallTimetoDestroy = 5f;

    void Start()
    {
        StartCoroutine(DestroyFireball());
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.GetComponent<CircleCollider2D>().IsTouchingLayers(LayerMask.GetMask("Wall")))
        {
            Destroy(gameObject);
        }
    }

    IEnumerator DestroyFireball()
    {
        yield return new WaitForSeconds(fireBallTimetoDestroy);
        Destroy(gameObject);
    }
}
