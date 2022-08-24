using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemigoPersigue : MonoBehaviour
{
    public Transform player;
    public float rango;
    public float maxRango;
    public bool persiguiendo;
    
    public float velocidadMovimiento;

    SpriteRenderer sprite;
   
    Rigidbody2D rb2d;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
       
        
    }

    // Update is called once per frame
    void Update()
    {
        //Distancia hasta el jugador
        float distJugador = Vector2.Distance(transform.position, player.position);
        Debug.Log("Distancia del jugador:" + distJugador);

        if(distJugador < rango)
        {
            persiguiendo = true;
            PerseguirJugador();
        }
        if(distJugador < maxRango && persiguiendo==true)
        {
            PerseguirJugador();
        }
        else
        {
            persiguiendo = false;
            NoPerseguirJugador();
        }

    }
    void PerseguirJugador()
    {
        sprite.color = Color.blue;
        if (transform.position.x < player.position.x)
        {
            rb2d.velocity = new Vector2(velocidadMovimiento, 0f);
            //Flip();
        }
        else if(transform.position.x > player.position.x )
        {
            rb2d.velocity = new Vector2(-velocidadMovimiento, 0f);
            //Flip();
        }
    }

    private void Flip()
    {
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    void NoPerseguirJugador()
    {
        sprite.color = Color.red;
        rb2d.velocity = new Vector2(0f, 0f);
    }
}
