using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    public Animator barrierAnimator;


    public IEnumerator DestroyBarrier()
    {
        barrierAnimator.SetBool("isCracking", true);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
