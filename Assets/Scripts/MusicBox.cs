using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicBox : MonoBehaviour
{
    static bool first = true;
    void Start()
    {
        if (first)
        {
            DontDestroyOnLoad(gameObject);
            first = false;
        }
        else
        {
            Destroy(gameObject); //Destroy duplicate music player
        }
    }
}
