using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : Bullet
{
    Transform target; //The player transform

    // Start is called before the first frame update
    void Start()
    {
        speed = 7;
        target = FindObjectOfType<Player>().transform;

        RotateTowardsTarget();
        MoveTowardsTarget();

        Destroy(gameObject, 7);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player) //If bullet touched the player
        {
            Game.KillAfterSFX(gameObject); //Destory bullet
            player.GotHit(4); //Activating the player Got Hit function
        }
    }


    void RotateTowardsTarget() //Rotate the bullet towards the player
    {
        Vector2 direction = (target.position - transform.position).normalized; //Saving the direction vector from the bullet to the player.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.rotation = angle + 90; //adding fix needed for the sprite in order to be rotated correctly.
    }

    void MoveTowardsTarget()//Moves the bullet towards the player
    {

        Vector3 moveDirection = (target.position - transform.position).normalized;//Saving the direction vector from the bullet to the player.
        moveDirection.z = 0;
        rb.velocity = moveDirection * speed;
    }
}
