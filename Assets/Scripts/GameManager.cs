using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("General")]
    public static GameManager Instance;
    public int maxFPS;

    void Start()
    {
        Instance = this;
        Application.targetFrameRate = maxFPS;
    }

    void Update()
    {
        
    }
}
