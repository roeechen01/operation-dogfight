using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour //Basic structure for all the bullets in the game (player bullets and enemy bullets)
{
    protected float speed;
    protected Rigidbody2D rb;

    void Awake() //Awake() is called before Start(), it's important to ensure the RigidBody is set before using it.
    {
        rb = GetComponent<Rigidbody2D>();
    }


}
