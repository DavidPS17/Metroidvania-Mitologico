using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movimiento : MonoBehaviour
{
    public int combo;

    public float runSpeed = 2;
    public float jumpSpeed = 3;

    public bool isAttacking;
    public bool isCargandoAttack;

    Rigidbody2D  rb2D;

    public SpriteRenderer spriteRenderer;
    public Animator anim;

    public static movimiento instance;
    
    private void Awake()
    {
        instance = this;
        isCargandoAttack = false;
    }
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey("d") || Input.GetKey("right"))
        {
            rb2D.velocity = new Vector2(runSpeed, rb2D.velocity.y);
            spriteRenderer.flipX = false;
            anim.SetBool("Run", true);
        }
        else if (Input.GetKey("a") || Input.GetKey("left"))
        {
            rb2D.velocity = new Vector2(-runSpeed, rb2D.velocity.y);
            spriteRenderer.flipX = true;
            anim.SetBool("Run", true);
        }
        else
        {
            rb2D.velocity = new Vector2(0, rb2D.velocity.y);
            anim.SetBool("Run", false);
        }
        Combos();
    }
    public void Combos()
    {
        if (Input.GetKey("e") && !isAttacking)
        {
            isAttacking = true;
            anim.SetTrigger("" + combo);
        }
        if (Input.GetKey("q") && !isAttacking)
        {
            anim.SetTrigger("cargado" + 1);
            isAttacking = true;
        }
        
    }
    public void Start_Combo()
    {
        isAttacking = false;
        if(combo < 2)
        {
            combo++;
        }
    }
    public void Finish_Ani()
    {
        isAttacking = false;
        combo = 0;
        isCargandoAttack = false;
    }
    
}
