using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Components")]
    Rigidbody2D rigid;

    public enum Type { Minion, Archer, Brute, Boss }
    public Type type;

    public enum MagicState { Normal, Fire, Water, Electric, Grass }
    public MagicState magicState = MagicState.Normal;

    [Header("Magic variables")]
    public float timeMagicAffect1 = 0.0f;
    public float timeMagicAffect2 = 0.0f;
    public float timeMagicAffectMax = 3.0f;
    public bool MagicAffect1 = false;
    public bool MagicAffect2 = false;

    [Header("Stats")]
    public float health;
    public float speed;
    public float damage;
    public float defense;

    [Header("State")]
    public bool isWaterFire = false;
    public bool isWaterElectric = false;
    public bool isWaterGrass = false;
    public bool isFireElectric = false;
    public bool isFireGrass = false;
    public bool isElectricGrass = false;


    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (MagicAffect1)
        {
            timeMagicAffect1 += Time.deltaTime;

            if (timeMagicAffect1 >= timeMagicAffectMax)
            {
                magicState = MagicState.Normal;
                MagicAffect1 = false;
                timeMagicAffect1 = 0.0f;
            }
        }

        if (MagicAffect2)
        {
            MagicAffect1 = false;
            timeMagicAffect1 = 0.0f;

            timeMagicAffect2 += Time.deltaTime;

            if (timeMagicAffect2 >= timeMagicAffectMax)
            {
                magicState = MagicState.Normal;
                MagicAffect2 = false;
                timeMagicAffect2 = 0.0f;
            }
        }

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void waterFire()
    {
        // Agua + Fuego = Menos velocidad durante un tiempo
        isWaterFire = true;

        speed *= 0.5f;

        Invoke("waterFireOut", 2.0f);
    }
    public void waterFireOut()
    {
        speed *= 2.0f;

        isWaterFire = false;
    }
    public void waterElectric()
    {
        // Agua + Electrico = Daño a un enemigo cercano
        isWaterElectric = true;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 5.0f, LayerMask.NameToLayer("Enemy"));
        Enemy nearEnemyScript = colliders[0].gameObject.GetComponent<Enemy>();
        nearEnemyScript.health -= 10;

        isWaterElectric = false;
    }
    public void waterGrass()
    {
        // Agua + Planta = Menos defensa durante un tiempo
        isWaterGrass = true;

        defense *= 0.5f;

        Invoke("waterGrassOut", 2.0f);
    }
    public void waterGrassOut()
    {
        defense *= 2.0f;

        isWaterGrass = false;
    }
    public void fireElectric()
    {
        // Fuego + Electrico = Explosion alrededor
        isFireElectric = true;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 5.0f, LayerMask.NameToLayer("Enemy"));
        foreach (Collider2D enemy in colliders)
        {
            Enemy nearEnemyScript = enemy.gameObject.GetComponent<Enemy>();
            nearEnemyScript.health -= 10;
        }

        isFireElectric = false;
    }
    public void fireGrass()
    {
        // Fuego + Planta = Daño durante un tiempo
        isFireGrass = true;

        health -= 20;

        isFireGrass = false;
    }
    public void electricGrass()
    {
        // Electrico + Planta = Nada (Gravedad invertida?? xD)
        isElectricGrass = true;

        rigid.gravityScale = -0.5f;

        Invoke("electricGrassOut", 2.0f);
    }
    public void electricGrassOut()
    {
        rigid.gravityScale = 3;

        isElectricGrass = false;
    }
}
