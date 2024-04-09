using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMissile : Bullet
{
    Transform target;
    public Explosion explosionPrefab;
    Vector2 targetPos;
    bool abovePlayer = false;
    float timer = 0;


    // Start is called before the first frame update
    void Start()
    {
        speed = 5;
        target = FindObjectOfType<Player>().transform;
        targetPos = target.position;
        abovePlayer = targetPos.y < transform.position.y;

        RotateTowardsTarget();
        MoveTowardsTarget();

        Destroy(gameObject, 7);
    }

    void Update()
    {
        timer += Time.deltaTime;
        //If missile is close to where the player was when the missile spawned
        if (Game.DistanceCloseEnough(transform.position, targetPos, 0.1f) || (abovePlayer && transform.position.y < targetPos.y) || !abovePlayer && transform.position.y > targetPos.y)
        {
            Explode(); //Activating the explosion
        }
    }

    public void Explode() //This function spawns the explosion prefab and destroyes the missile itself
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Player>())
        {
            Explode();
            
        }
        if (collision.gameObject.GetComponent<PlayerBullet>() && timer > 0.25f)
        {
            Explode();
            Game.KillAfterSFX(collision.gameObject);
        }
        else if (collision.gameObject.GetComponent<Explosion>() && collision.gameObject.GetComponent<Explosion>().timer < 0.15f && timer > 0.25f)
        {
            Explode();


        }
    }


    void RotateTowardsTarget()//Rotate the missile towards the player
    {
        Vector2 direction = (targetPos - (Vector2)transform.position).normalized; //Saving the direction vector from the bullet to the player.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.rotation = angle + 90; //adding fix needed for the sprite in order to be rotated correctly.
    }

    void MoveTowardsTarget()//Move the missile towards the player
    {
        
            Vector3 moveDirection = (targetPos - (Vector2)transform.position).normalized;//Saving the direction vector from the bullet to the player.
        moveDirection.z = 0;
            rb.velocity = moveDirection * speed;
    }
}
