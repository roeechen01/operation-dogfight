using UnityEngine;

public class HomingMissileEnemy : Enemy
{
    public EnemyHomingMissile homingMissilePrefab; //The missile prefab the enemy shoots
    float prefferredDistance = 6f; //The distance the enemy wants to be near the player, if it's bigger- the enemy will move closer (if possible)
    bool onInnerArch = false; //Is the plane on the inner arch
    bool reachedSpot = false; //Did the plane reach his original enemy spot, in the outside arch
    bool exitedSpot = false; //Did the enemy start leaving it's original spot on the outer arch, and on the way to an inner arch spot.
    int oldSpotIndex = -1; //The previous enemy spot index the plane was on
    int spotIndex = -1; //The current enemy spot index the plane is on
    private float closeEnoughDistance = 0.05f; // The distance from the enemy spot, that is close enough that the enemy will stop moving
    float delayBetweenMissiles = 4f;
    public static int Queue = 0; //Queue saves the amount of enemies waiting to be spawned, enemies go to queue if they had no space to spawn normally in the game.

    void Start()
    {
        killMoney = 15;
        spots = FindObjectOfType<Game>().arches; //Setting the right enemy spots to the enemy, missile enemy uses the constant positions arches spots
        speed = 1.5f;
        Invoke(nameof(Shoot), 3); //Make the enemy shoot once every few seconds
    }

    void Update()
    {
        Rotate();
        Movement();
    }

    void DelayMovementAfterReachingSpot() //We'll invoke the func when reaching the outer arch spot and use IsInvoking(), to know if enough time passed since reaching the spot, to enable moving to an inner arch spot closer to the player (If player is far)
    {

    }


    public override void Spawn(bool fromQueue = false) //Overriding the base Spawn func, handling case when spawning an enemy from the enemy queue
    {

        if (fromQueue) //If the enemy is to be spawned from the queue
        {

            if (Game.Arches.IsThereSpace()) //If there's a free enemy spot
            {
                base.Spawn(); //Spawn an enemy
                Queue--; //Decrease the amount of enemies in queue by 1
            }

        }
        else //If the enemy is to be spawned normally (not from queue)
        {
            if (Game.Arches.IsThereSpace()) //If there's a free enemy spot
                base.Spawn(); //Spawn an enemy
            else //If there isn't a free enemy spot
            {

                Queue++; //Increase the amount of enemies queue by 1, so that it will spawn later when another enemy is killed

            }
        }
    }

    void Shoot() //Spawning a missile
    {
        if (!alive)
            return;
        if(Random.Range(0f, 1f) > 0.4f)
            Instantiate(homingMissilePrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.5f), Quaternion.identity);
        Invoke(nameof(Shoot), delayBetweenMissiles + Random.Range(-0.5f, 1f));
    }

    bool IsCloseEnoughToPlayer() //Is the player close enough to the player based on prefferredDistance value 
    {
        return Vector2.Distance(transform.position, target.position) < prefferredDistance;

    }

    private void OnTriggerEnter2D(Collider2D collision) //Handling collisions with various game objects
    {
        if (collision.gameObject.GetComponent<PlayerBullet>()) //If enemy collides with a player bullet
        {
            Game.KillAfterSFX(collision.gameObject); //Destroy the bullet
            Death(); //Kill the ememy
        }

        if (collision.gameObject.GetComponent<Player>() && !player.invincible && !IsInvoking(nameof(AccidentCoolDown))) //If enemy collides with the player and is not in accident cooldown
        {
            target.GetComponent<Player>().GotHit(10, true, true); //Hit the enemy (the "true" makes the player have short invincibilty)
            if (hadAccident) //If it already had an accident with the player
                Death(); //Kill the enemy
            else //If it's the first accident with the player
            {
                hadAccident = true; //Setting to true so that in the next accident, enemy will die
                Invoke(nameof(AccidentCoolDown), 0.5f); //Activating the accident cooldown
                GetComponent<SpriteRenderer>().color = new Color(1, 0.5f, 0.5f); //Change color to mostly red
            }

        }
    }

    void Rotate()
    {

        if (transform.position.y > target.position.y) //If enemy is higher than the player
        {
            transform.rotation = Quaternion.identity;// rotate towards the sky (it's default rotation)
        }

        else //If enemy is lower, rotate towards the player
        {
            Vector2 direction = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rb.rotation = angle - 90; //adding fix needed for the sprite in order to be rotated correctly.
        }


    }

    void Movement()
    {
        if (spotIndex == -1) //If the spot index is not set
        {
            spotIndex = spots.FindNewSpot(spotIndex); //Call the EnemySpots spots to find a new point
        }

        else //If the point has been set already
        {
            if (Vector2.Distance(transform.position, spots.RightSpots[spotIndex].position) < closeEnoughDistance)// If enemy is close enough to the spot, don't move
            {
                rb.velocity = Vector2.zero; //Stopping movement
                if (!reachedSpot) //If reachedSpot is still false after reachig the spot
                    Invoke(nameof(DelayMovementAfterReachingSpot), 1.5f); //Invoke the delay movement, so that it will stay at the spot a little bit, before it can go to another spot on the inner arch (if player is far)
                reachedSpot = true; //set to true, so we'll know we reached the spot

            }


            else// If far from the spot, move towards it
            {
                // Move towards the spot
                Vector3 moveDirection = (spots.RightSpots[spotIndex].position - transform.position).normalized; //Getting the normalzied direction vector to the spot
                moveDirection.z = 0; //Setting Z to 0 to prevent unexpected bugs
                rb.velocity = moveDirection * speed; //Move in the direction and speed

            }
        }


        //If the player reached the outer arch spot, not on an inner arch spot, there's a free spot in the inner arch, is far from the player and is not on movement cooldown
        if (reachedSpot && !onInnerArch && spots.IsThereSpace(true) && !IsCloseEnoughToPlayer() && !IsInvoking(nameof(DelayMovementAfterReachingSpot)))
        {
            onInnerArch = true; //Setting to true so we know the enemy's spot is a spot in the inner arch
            oldSpotIndex = spotIndex; //Setting it's previous spot index to the previous point on the outer arch
            spotIndex = spots.FindNewSpot(spotIndex, true); //Using EnemySpots spots to set a new spot on the inner arch
            exitedSpot = true; //Setting to true so we know the enemy started leaving it's original spot in the outer arch
        }
        if (onInnerArch) //If the enemy's spot is on the inner arch
        {
            //If the enemy exited the spot and is far enough vertically from it's previous spot
            if (exitedSpot && Mathf.Abs(spots.RightSpots[oldSpotIndex].transform.position.y - transform.position.y) > 0.6f)
            {
                spots.SetFree(oldSpotIndex); //Set the old spot free
                oldSpotIndex = -2; //Setting to -2 cause enemy will not use oldSpotIndex again
                exitedSpot = false; //Setting to false even though technicaclly is still true, because we don't need to reach this code again - the if statement will return false from now on

            }

            //If the enemy is still on the way to the inner arch spot, but still not that close (based on the distance on X and Y axis seperatly)
            if (Vector2.Distance(transform.position, spots.RightSpots[spotIndex].position) > 0.04f && (!Game.DistanceCloseEnough(spots.RightSpots[spotIndex].transform.position.y, transform.position.y, 0.65f) || !Game.DistanceCloseEnough(spots.RightSpots[spotIndex].transform.position.x, transform.position.x, 0.15f)))
            {
                Vector3 moveDirection = (spots.RightSpots[spotIndex].position - transform.position).normalized; //Get the normalized direction to the spot
                moveDirection.z = 0;
                rb.velocity = moveDirection * speed; return; //Move the enemy towards the spot
            }

            //Else, if the enemy still hasn't reached the spot and is far from the player
            else if (Vector2.Distance(transform.position, spots.RightSpots[spotIndex].position) > 0.04f && !IsCloseEnoughToPlayer())
            {
                Vector3 moveDirection = (spots.RightSpots[spotIndex].position - transform.position).normalized;//Getting direction to the spot
                moveDirection.z = 0;
                rb.velocity = moveDirection * speed; return; //Moving the enemy
            }

            else //Else, stop moving
            {
                rb.velocity = Vector2.zero;
                return;
            }

        }

    }

    override protected void Death(bool realKill = true) //Overriding the base Death() function of enemies
    {
        spots.SetFree(spotIndex); //Set free the spot the enemy was on, so that future enemies will be able to use the spot
        if (oldSpotIndex > -1) //oldSpotIndex is bigger than -1 in case it started moving to a inner arch spot, but was still close to it's previous spot, so that no enemy can spawn in either of the spots (the previous point is still taken)
            spots.SetFree(oldSpotIndex); //So in a case like this, we will free also it's previous point

        if (Queue > 0) //If there's an enemy on queue
            Spawn(true); //Spawn an ememy from the queue - the "true" in Spawn(true); is for notifying the spawn func that the enemy should be spawned from the queue

        base.Death(realKill); //Using the base death function for the rest of the job
    }
}