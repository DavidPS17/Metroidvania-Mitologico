using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public enum Type { Normal, Fire, Water, Electric, Grass }
    public Type type;

    public SpriteRenderer sprite;

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        switch (type)
        {
            case Type.Normal:
                sprite.color = Color.white;
                break;
            case Type.Grass:
                sprite.color = Color.green;
                break;
            case Type.Fire:
                sprite.color = Color.red;
                break;
            case Type.Water:
                sprite.color = Color.blue;
                break;
            case Type.Electric:
                sprite.color = Color.yellow;
                break;
        }

        Destroy(gameObject, 10);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);

        if (collision.gameObject.tag == "Bullet")
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 5.0f, LayerMask.NameToLayer("Enemy"));
            foreach (Collider2D enemy in colliders)
            {
                Enemy nearEnemyScript = enemy.gameObject.GetComponent<Enemy>();
                nearEnemyScript.health -= 10;
            }
        }

        if (collision.gameObject.tag == "Enemy")
        {
            Enemy enemyScript = collision.gameObject.GetComponent<Enemy>();

            if (enemyScript.magicState != Enemy.MagicState.Normal)
            {
                if (type == Type.Normal)
                {
                    enemyScript.health -= 10;
                }
                else
                {
                    if (!enemyScript.isWaterFire && !enemyScript.isWaterElectric && !enemyScript.isWaterGrass && !enemyScript.isFireElectric && !enemyScript.isFireGrass && !enemyScript.isElectricGrass)
                    {
                        enemyScript.MagicAffect2 = true;
                        enemyScript.health -= 20;
                        
                        if ((enemyScript.magicState == Enemy.MagicState.Water && type == Type.Fire) || (enemyScript.magicState == Enemy.MagicState.Fire && type == Type.Water))
                        {
                            // Agua + Fuego = Menos velocidad durante un tiempo
                            enemyScript.waterFire();
                        }
                        if ((enemyScript.magicState == Enemy.MagicState.Water && type == Type.Electric) || (enemyScript.magicState == Enemy.MagicState.Electric && type == Type.Water))
                        {
                            // Agua + Electrico = Daño a un enemigo cercano
                            enemyScript.waterElectric();
                        }
                        if ((enemyScript.magicState == Enemy.MagicState.Water && type == Type.Grass) || (enemyScript.magicState == Enemy.MagicState.Grass && type == Type.Water))
                        {
                            // Agua + Planta = Menos defensa durante un tiempo
                            enemyScript.waterGrass();
                        }
                        if ((enemyScript.magicState == Enemy.MagicState.Fire && type == Type.Electric) || (enemyScript.magicState == Enemy.MagicState.Electric && type == Type.Fire))
                        {
                            // Fuego + Electrico = Explosion alrededor
                            enemyScript.fireElectric();
                        }
                        if ((enemyScript.magicState == Enemy.MagicState.Fire && type == Type.Grass) || (enemyScript.magicState == Enemy.MagicState.Grass && type == Type.Fire))
                        {
                            // Fuego + Planta = Daño durante un tiempo
                            enemyScript.fireGrass();
                        }
                        if ((enemyScript.magicState == Enemy.MagicState.Electric && type == Type.Grass) || (enemyScript.magicState == Enemy.MagicState.Grass && type == Type.Electric))
                        {
                            // Electrico + Planta = Nada (Gravedad invertida?? xD)
                            enemyScript.electricGrass();
                        }

                        enemyScript.magicState = Enemy.MagicState.Normal;
                    }
                    else
                    {
                        enemyScript.health -= 10;
                    }
                }
            }
            else
            {
                enemyScript.MagicAffect1 = true;
                enemyScript.health -= 10;

                switch (type)
                {
                    case Type.Grass:
                        enemyScript.magicState = Enemy.MagicState.Grass;
                        break;
                    case Type.Fire:
                        enemyScript.magicState = Enemy.MagicState.Fire;
                        break;
                    case Type.Water:
                        enemyScript.magicState = Enemy.MagicState.Water;
                        break;
                    case Type.Electric:
                        enemyScript.magicState = Enemy.MagicState.Electric;
                        break;
                }
            }
        }
    }
}
