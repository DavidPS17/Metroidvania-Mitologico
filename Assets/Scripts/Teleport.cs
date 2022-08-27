using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform posOtherTeleport;
    public Teleport scriptOtherTeleport;

    public float MaxCD;
    public float CD;

    private void Update()
    {
        CD += Time.deltaTime;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3 || collision.gameObject.tag == "Enemy")
        {
            if (CD > MaxCD && scriptOtherTeleport.CD > scriptOtherTeleport.MaxCD)
            {
                collision.transform.position = posOtherTeleport.position;
                CD = 0.0f;
                scriptOtherTeleport.CD = 0.0f;
            }
        }
    }
}
