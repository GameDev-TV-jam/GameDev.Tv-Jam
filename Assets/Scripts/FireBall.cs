using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    [SerializeField] float fireBallTimetoDestroy = 3f;
    PlayerMovement player;

    void Start()
    {
        player = FindObjectOfType<PlayerMovement>();
        StartCoroutine(DestroyFireball());
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.GetComponent<CircleCollider2D>().IsTouchingLayers(LayerMask.GetMask("Wall")))
        {
            //player.isFireballInScene = false;
            Destroy(gameObject);
        }
    }

    IEnumerator DestroyFireball()
    {
        yield return new WaitForSeconds(fireBallTimetoDestroy);
        //player.isFireballInScene = false;
        Destroy(gameObject);
    }
}
