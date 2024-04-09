using System.Collections.Generic;
using UnityEngine;

public class CrossingSprayEnemy : Enemy //Enemy that leaves poison trail, goes from left side of the screen to the right, or the opposite
{
    bool directionRightToLeft = true; //Is the travel directionof the enemy from right side to life side or the opposite
    public Vector3 targetPos, startPos; //Starting position and target to go to position

    public PoisonTrail posionTrailPrefab; //pefab for the poison trail particle system effect
    Vector3 lastTrailPos = Vector3.zero; //Saving here the position of the last trail position
    float distanceToSpawnTrail = 0.2f; //The distance that is big enough from the last trail, to spawn another one
    List<PoisonTrail> trails = new List<PoisonTrail>(); //List of the trails spawned by the enemy

    void Start()
    {
        killMoney = 10;
        speed = 3f;

        if (Random.Range(1, 3) == 2)
            directionRightToLeft = false; //Randomally choosing if this enemy goes from right to left or the opposite

        //Setting start and target positions for each case
        if (directionRightToLeft)
        {
            startPos = new Vector3(4, Random.Range(-4f, 4f), transform.position.z);
            targetPos = new Vector3(-4, Random.Range(-4f, 4f), transform.position.z);
        }
        else
        {
            startPos = new Vector3(-4, Random.Range(-4f, 4f), transform.position.z);
            targetPos = new Vector3(4, Random.Range(-4f, 4f), transform.position.z);
        }

        transform.position = startPos; //Setting enemy position to startPos


    }

    void Update()
    {
        //Spawning trail if distance from last trail spawned is big enough
        if (Vector2.Distance(transform.position, lastTrailPos) > distanceToSpawnTrail)
            SpawnTrail();

        //Rotation and Movement towards targetPos
        RotateTowardsTarget();
        MoveTowardsTarget();

        if (Vector2.Distance(transform.position, targetPos) < 0.1f)
            Invoke(nameof(OutOfBoundsDeath), 4); //Kill enemy 4 seconds after reacing targetPos
    }

    void SpawnTrail() //Function that spawns trail and adding it to the trail instances list.
    {
        if (!alive)
            return;

        trails.Add(Instantiate(posionTrailPrefab, transform.position, Quaternion.identity));
        lastTrailPos = transform.position;
    }

   

    private void OnTriggerEnter2D(Collider2D collision) //Handling collisions with various game objects
    {
        if (collision.gameObject.GetComponent<Player>() && !player.invincible) //If collided with the player
        {
            target.GetComponent<Player>().GotHit(10, true, true); //Activate player GotHit func to handle the hit
            Death(); //Self Dedtroy the plane
        }

        if (collision.gameObject.GetComponent<PlayerBullet>()) //If collided wit a player bullet
        {
            Game.KillAfterSFX(collision.gameObject); //Destroy the bullet
            Death(); //Kill the enemy
        }
    }

    void RotateTowardsTarget() //Rotate the enemy towards the target
    {
        Vector2 direction = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.rotation = angle - 90; //adding fix needed for the sprite in order to be rotated correctly.
    }

    void MoveTowardsTarget() //Move the enemy towards the target
    {
        Vector3 moveDirection = (targetPos - transform.position).normalized;
        moveDirection.z = 0;
        rb.velocity = moveDirection * speed;
    }

    void GetRidOfTrail() //Function that is called when enemy dies, to let the poison instances know they need to dissapwar soon
    {
        for(int i = 0; i < trails.Count; i++)
        {
            if (trails[i])
                trails[i].EnemyDied();
        }
    }

    void OutOfBoundsDeath()
    {
        Death(false);
    }

    protected override void Death(bool realKill = true) //Overriding the default death function the first rid of the poison trail
    {
        GetRidOfTrail();
        base.Death(realKill);
    }
}
