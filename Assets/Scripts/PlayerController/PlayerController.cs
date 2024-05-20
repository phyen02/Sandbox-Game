using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float moveSpeed;
    public float jumpForce;
    public bool onGround;
    public float horizontal;
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerStay2D(Collider2D colli)
    {
        if (colli.CompareTag("Ground"))
        {
            onGround = true;
        }
    }

    private void OnTriggerExit2D(Collider2D colli)
    {
        if (colli.CompareTag("Ground"))
        {
            onGround = false;
        }
    }
    private void FixedUpdate()
    {
        horizontal = Input.GetAxis("Horizontal");
        float jump = Input.GetAxis("Jump");
        float vertical = Input.GetAxis("Vertical");

        if (vertical >= 0.1f || jump >= 0.1f)
        {
            
        }

        rigid.velocity = new Vector2(horizontal*moveSpeed, rigid.velocity.y);
    }
}
