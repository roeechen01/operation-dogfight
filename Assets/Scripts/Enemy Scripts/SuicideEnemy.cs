using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuicideEnemy : Enemy //A suicide plane enemy that follows the player until it's close enough, then continue to fly straight hoping to hit the player
{

    bool closeEnough = false; //Did the plane get close enough to the player
    float closeDistance = 1.2f; //The distance the enemy wants to be from the player before it stops following him

    void Start()
    {
        speed = 3f;
        

    }

    void Update()
    {
        if(!closeEnough) //If the plane still didn't get close enough to the player, move and rotate towrds the player
        {
            RotateTowardsTarget();
            MoveTowardsTarget();
        }
            

        if(Vector2.Distance(transform.position, target.position) < closeDistance) //If enemy is close enough to the player, set closeEnough to true (enemy will continue in it's current direcion and stop following the player). 
        {
            closeEnough = true;
            Invoke(nameof(OutOfBoundsDeath), 7); //In case the enemy exited game boundries, destory after a few seconds

        }
    }

    void OutOfBoundsDeath()
    {
        Death(false);
    }

    

    private void OnTriggerEnter2D(Collider2D collision) //Handling collisions with various game objects
    {
        if (collision.gameObject.GetComponent<Player>() && !player.invincible) //If collided with the player
        {
            target.GetComponent<Player>().GotHit(15, true, true); //Activate player GotHit func to handle the hit
            Death(); //Kill The enemy
        }

        if (collision.gameObject.GetComponent<PlayerBullet>()) //If collided wit a player bullet
        {
            Game.KillAfterSFX(collision.gameObject); //Destroy the bullet
            Death(); //Kill the enemy
        }
    }

    void RotateTowardsTarget() //Rotate the enemy towards the player
    {
        Vector2 direction = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.rotation = angle - 90; //adding fix needed for the sprite in order to be rotated correctly.
    }

    void MoveTowardsTarget() //Move the enemy towards the player
    {
        Vector3 moveDirection = (target.position - transform.position).normalized;
        moveDirection.z = 0;
        rb.velocity = moveDirection * speed;
    }
}
