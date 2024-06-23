using System.Collections;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D colli)
    {
        if (colli.CompareTag("Ground"))
        {
            transform.parent.GetComponent<PlayerController>().onGround = true;
        }
    }

    private void OnTriggerExit2D(Collider2D colli)
    {
        if (colli.CompareTag("Ground"))
        {
            transform.parent.GetComponent<PlayerController>().onGround = false;
        }
    }
}
