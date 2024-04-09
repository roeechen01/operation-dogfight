using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : Bullet
{
    

    // Start is called before the first frame update
    void Start()
    {
        speed = 12;
        Vector2 direction = Vector2.down; //The bullet direction
        if (Game.Ambush) //If ambush mode, change the ball direction to up, and change the localScale, so it will face the right direction.
        {
            direction = Vector2.up;
            transform.localScale = new Vector3(1, -1, 1);
        }
        rb.velocity = direction * speed; //Move the bullet based on the direction and the speed
        Destroy(gameObject, 5);
    }
}
