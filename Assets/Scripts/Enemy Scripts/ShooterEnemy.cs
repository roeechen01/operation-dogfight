using UnityEngine;

public class ShooterEnemy : Enemy
{

    float shootingDistance = 6.5f; //maximum distance at which the enemy can shoot at the player
    float rangeDistance = 1f; //The range from the spot that the enemy will be ok with just staying in position and not moving closer to the spot, unless spot moves further
    public EnemyBullet bulletPrefab; //the prefab used by the enemy for shooting bullets
    private float closeEnoughDistance = 0.2f; //The distance from the spot that the enemy considers to be in the spot, and will stop moving
    bool delayed = false; //Is the enemy currently being delayed (can't move yet)
    bool stayingInSpot = false; //Is the enemy currently staying in place (after it reached it's desired spot)
    int spotIndex = -1; //The spot index in EnemySpots spots
    public static int Queue = 0; //Queue saves the amount of enemies waiting to be spawned, enemies go to queue if they had no space to spawn normally in the game.

    void Start()
    {
        speed = 2f;
        InvokeRepeating(nameof(Shoot), 1.5f, 2.5f); //Try to shoot every few seconds
        spots = FindObjectOfType<Game>().midRange;//Setting the right EnemeySpots spots for this enemy type, the mid range spots.
    }

    public override void Spawn(bool fromQueue = false) //Overriding the base Spawn func, handling case when spawning an enemy from the enemy queue
    {

        if (fromQueue) //If the enemy is to be spawned from the queue
        {

            if (Game.MidRangeCircle.IsThereSpace()) //If there's a free enemy spot
            {
                base.Spawn(); //Spawn an enemy
                Queue--; //Decrease the amount of enemies in queue by 1
            }

        }
        else //If the enemy is to be spawned normally (not from queue)
        {
            if (Game.MidRangeCircle.IsThereSpace()) //If there's a free enemy spot
                base.Spawn(); //Spawn an enemy
            else //If there isn't a free enemy spot
            {

                Queue++; //Increase the amount of enemies queue by 1, so that it will spawn later when another enemy is killed

            }
        }
    }

    void Update()
    {
        Rotate();
        Movement();
    }

    void Shoot()
    {
        if (alive && Vector2.Distance(transform.position, target.position) < shootingDistance) // Checks if the player is within the shooting distance
        {
            Instantiate(bulletPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.5f), Quaternion.identity); //Spawning a shot
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) //Handling collisions
    {
        if (collision.gameObject.GetComponent<PlayerBullet>()) //If enemy collides with a player bullet
        {
            Game.KillAfterSFX(collision.gameObject); //Destroy the bullet
            Death(); //Kill the enemy
        }

        if (collision.gameObject.GetComponent<Player>() && !player.invincible && !IsInvoking(nameof(AccidentCoolDown))) //If enemy collides with the player and is not in accident cooldown
        {
            target.GetComponent<Player>().GotHit(10, true, true); //Hit the enemy (the "true" makes the player have short invincibilty)
            if (hadAccident) //If it already had an accident with the player
                Death(); //Kill enemy
            else //else, If it's the first accident with the player
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
        if (spotIndex == -1) //If spot index is not set yet
        {
            spotIndex = spots.FindNewSpot(spotIndex); //Get a spot using EnemySpot spot
            return; //Exit the function
        }

        if (Game.Ambush && target.position.y > transform.position.y) //If ambush mode and enemy is below the player, don't move
        {
            rb.velocity = Vector2.zero; //Stopping movement
            return; //Exiting function
        }
        else if (!Game.Ambush && target.position.y < transform.position.y) //If normal mode and enemy is above the player, don't move
        {
            rb.velocity = Vector2.zero; //Stopping movement
            return; //Exiting function
        }

        //If enemy is currently not stopping in the spot but is actually in the spot (close enough)
        if (!stayingInSpot && Game.DistanceCloseEnough(transform.position, spots.RightSpots[spotIndex].position, closeEnoughDistance))
        {
            stayingInSpot = true; //Setting stayingInSpot to true, so the enemy will stop moving on the spot
        }

        //If enemy is currently stopping on the spot, but the player moved (The spot moves with the player), and the distance between the spot and the enemy now is bigger than "rangeDistance"
        if (stayingInSpot && !Game.DistanceCloseEnough(transform.position, spots.RightSpots[spotIndex].position, rangeDistance))
        {
            stayingInSpot = false; //Set stayingInSpot to false, so we know the enemy is no longer staying in the spot
            delayed = true; //setting delayed to true, in order to delay the enemy movement a bit before it can move again after the spot
            Invoke(nameof(EndSecDelay), 0.8f); //Calling EndSecDelay after a short while to end the delay, letting enemy move agin
        }

        if (delayed || stayingInSpot) //If enemy is either delayed or staying in spot
        {
            rb.velocity = Vector2.zero; //Stop movement in that case
            return; //And exit function
        }
        else //Else, if enemy is not delayed and is not staying in the spot
        {
            //If there isn't another free spot in the screen boundries or if the enemy and the spot are in scren boundries
            if (!spots.IsThereAnotherFreeSpotInBoundries(spotIndex) || (spots.IsPositionOnScreen(spots.RightSpots[spotIndex].position, 0.2f) && spots.IsPositionOnScreen(transform.position, 0.2f)))
            {
                //If enemy is close enough to the spot
                if (Vector2.Distance(transform.position, spots.RightSpots[spotIndex].position) < closeEnoughDistance)
                {
                    rb.velocity = Vector2.zero; //Stop movement
                }

                //If enemy is too far from the spot
                if (Vector2.Distance(transform.position, spots.RightSpots[spotIndex].position) > closeEnoughDistance)
                {
                   
                    Vector3 moveDirection = (spots.RightSpots[spotIndex].position - transform.position).normalized;//Getting the direction to the spot
                    moveDirection.z = 0;

                    //If ambush mode, and enemy moves up or it's normal mode and the enemy moves down
                    if ((Game.Ambush && moveDirection.y > 0) || (!Game.Ambush && moveDirection.y < 0))
                        moveDirection.y = 0; //Setting the movement direction Y to 0, to make sure they don't go up or down when they shouldn't like in theses cases

                    rb.velocity = moveDirection * speed; //Moving the player in the desired velocity
                }
            }
            else //Else (which means either there is a free spot in screen boundries, or that the enemy or spot is not in screen boundries)
            {
                int previous = spotIndex; //saving the spot index in previous to use it later
                spotIndex = spots.FindNewSpot(spotIndex, false); // Find a new spot (parameter false means we are not checking an inner arch)

                if (previous == spotIndex) //If the previous spot index and the new one is the same
                {
                    //If enemy is too far from the spot
                    if (Vector2.Distance(transform.position, spots.RightSpots[spotIndex].position) > closeEnoughDistance)
                    {

                        Vector3 moveDirection = (spots.RightSpots[spotIndex].position - transform.position).normalized;//Getting the direction to the spot
                        moveDirection.z = 0;

                        //If ambush mode, and enemy moves up or it's normal mode and the enemy moves down
                        if ((Game.Ambush && moveDirection.y > 0) || (!Game.Ambush && moveDirection.y < 0))
                            moveDirection.y = 0; //Setting the movement direction Y to 0, to make sure they don't go up or down when they shouldn't like in theses cases

                        rb.velocity = moveDirection * speed; //Moving the player in the desired velocity
                    }
                }
            }
        }

        Boundries(); //Calling  Boundries() to handle a case when the enemy is not in the screen boundries
    }



    void EndSecDelay() //This function is invoked after starting enemy delay movement, to end the delay and allow the enemy to move again
    {
        delayed = false; //Setting delayed to false, enabling enemy to move after the spot again
    }

    override protected void Death(bool realKill = true)
    {
        spots.SetFree(spotIndex); //Frees the spot occupied by the enemy after dying

        if (Queue > 0) //If there is an enemy on queue
            Spawn(true); //Spawn enemy from queue

        base.Death(realKill); //Perform base death function from the base Enemy class
    }

    void Boundries() // This function restricts the enemy position within the game boundaries
    {

        if (transform.position.x > 3f) //Limit max X
            transform.position = new Vector3(3f, transform.position.y, transform.position.z);

        if (transform.position.x < -3f) //Limit min X
            transform.position = new Vector3(-3f, transform.position.y, transform.position.z);

        if (!Game.Ambush && transform.position.y > 3f) //If normal mode, limit max Y
            transform.position = new Vector3(transform.position.x, 3f, transform.position.z);

        if (Game.Ambush && transform.position.y < -3f) //If ambush mode, limit min Y
            transform.position = new Vector3(transform.position.x, -3f, transform.position.z);
    }
}
