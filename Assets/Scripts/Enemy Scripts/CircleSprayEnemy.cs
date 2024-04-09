using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleSprayEnemy : Enemy
{
    
    bool closeEnough = false; //Did the plane get close enough to the player
    float closeDistance = 0.3f; //The distance the enemy wants to be from the player before it stops following him
    private float angleToPlayer = 0; //Variable used for the circle movement of the enemy
    float circleSpeed; //Speed of the circle movement
    float desiredRadius = 2.5f; //Radius to start the circle movement from
    float radiusDecreaseRate; // Rate at which the circle radius decreases
    float speedIncreaseRate; //Increment speed by this float every moment
    float initialSpeedIncreaseRate; //Increase the  increment value, so enemy will gain speed fast in circle movement
    bool comingFromRight = true; //Is coming from rihg tor left of the player

    public PoisonTrail posionTrailPrefab; //Poison trail particle system instance
    Vector3 lastTrailPos = Vector3.zero; //Last poison spawned positon
    public float distanceToSpawnTrail = 0.2f; //Distance from last poison position, to spawn another one
    List<PoisonTrail> trails = new List<PoisonTrail>(); //List of all the enemies poisons spawned

    void Start()
    {
        killMoney = 15;
        player = target.GetComponent<Player>();
        //Setting values..
        speed = 3f;
        circleSpeed = 1f;
        radiusDecreaseRate = 0.4f;
        speedIncreaseRate = 1f;

        //Choose if coming to left or right of the player randomally
        if (Random.Range(1, 3) == 2)
            comingFromRight = false;

    }

    void SpawnTrail() //Function that spawns trail and adding it to the trail instances list.
    {
        if (!alive)
            return;

        trails.Add(Instantiate(posionTrailPrefab, transform.position, Quaternion.identity));
        lastTrailPos = transform.position; //Saving position as lastTrailPos
    }

    void Update()
    {

        int multiplier = 1; //Multiplier is 1 if coming from thr right, -1 if coming to the left
        if (!comingFromRight)
            multiplier = -1;

        //The point the enemy starts his circle from, calculted by the desired distance and wether right or left to the player using the multiplier
        Vector3 targetPosition = (Vector3)player.transform.position + Vector3.right * desiredRadius * multiplier; 

        // Check if the enemy is close enough to the target position
        if (!closeEnough && Vector2.Distance(transform.position, targetPosition) < closeDistance)
        {
            closeEnough = true; //Set close enough to true
            transform.position = targetPosition; //Change position to the exact start circle movement position
            if (!comingFromRight) //If coming from left
                angleToPlayer = Mathf.PI; // Angle in radians for position directly to the left of the player
            
        }

        if (!closeEnough) // If not close to the start point yet
        {
            //Rotate and move facing the point
            RotateTowardsTarget(); 
            MoveTowardsTarget();
        }
        else
        {
            CirclePlayer(); //Use CirclePlayer() func
            //NewCircleFunc();
        }


        //Spawning trail if distance from last trail spawned is big enough
        if (Vector2.Distance(transform.position, lastTrailPos) > distanceToSpawnTrail)
            SpawnTrail();
      
    }

    void NewCircleFunc()
    {
        // Define the rotation speed
        float rotationSpeed = 100f;

        if ((!Game.Ambush && comingFromRight) || (Game.Ambush && !comingFromRight))
            rotationSpeed *= -1;

        // Define the movement speed towards the player (slower)
        float movementSpeed = 0.25f; // Adjust as needed

       

        // Calculate the direction from the enemy to the player
        Vector3 directionToPlayer = (target.position - transform.position).normalized;

        // Calculate the rotation angle based on the rotation speed and time
        float rotationAngle = rotationSpeed * Time.deltaTime;

       
        

        // Apply the rotation around the player position
        transform.RotateAround(target.position, Vector3.forward, rotationAngle);


        // Move the enemy towards the player

        // Calculate the movement towards the player (slower)
        Vector3 movement = directionToPlayer * movementSpeed * Time.deltaTime;

        // Apply the movement
        transform.Translate(movement, Space.World);


    }





    void CirclePlayer()
    {
        // Update the angle of rotation for the circling movement
        angleToPlayer += Time.deltaTime * circleSpeed;

        // Decrease the circle radius over time (getting closer to the player, eventually hitting him)
        desiredRadius -= Time.deltaTime * radiusDecreaseRate;

        if (desiredRadius < 0.3f) //Limit min radius
            desiredRadius = 0.3f;

        // Increase circle speed over time
        circleSpeed += Time.deltaTime * speedIncreaseRate;

        // Increase the rate of increase for speed increase rate over time
        speedIncreaseRate += Time.deltaTime * initialSpeedIncreaseRate;

        // Ensure the desired radius doesn't go below zero (will pick the highest between 0 and desiredRadius)
        desiredRadius = Mathf.Max(0, desiredRadius);

        int multiplier = 1;
        //Setting negative multiplier for cases when game is normal and coming from right, and game is ambush and coming form the left
        if ((!Game.Ambush && comingFromRight) || Game.Ambush && !comingFromRight)
            multiplier = -1;
       
           
        // Calculate the position relative to the player's position with the desired radius
        Vector3 circularMovement = new Vector2(Mathf.Cos(angleToPlayer), Mathf.Sin(angleToPlayer) * multiplier) * desiredRadius;

       
        // Calculate the angle of rotation based on the circular movement vector
        float angle = Mathf.Atan2(circularMovement.y, circularMovement.x) * Mathf.Rad2Deg;

        //when ambush mode and coming from left or when normal mode and coming from right
        if ((!comingFromRight && Game.Ambush) || comingFromRight && !Game.Ambush)
        {
            // Adjusting the angle to account for sprite orientation needed for this cases
            angle += 180;
        }
        

        // Apply the rotation to the enemy
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        

        // Set the enemy's position relative to the player's position
        Vector3 targetPosition =  target.position + circularMovement;

        // Move the enemy towards the target positionf
        rb.MovePosition(targetPosition);


    }




    private void OnTriggerEnter2D(Collider2D collision) //Handling collisions with various game objects
    {
        if (collision.gameObject.GetComponent<Player>() && !player.invincible && collision is CircleCollider2D) //If collided with the player and collision is circleCollider
        {
            target.GetComponent<Player>().GotHit(15, true, true); //Activate player GotHit func to handle the hit
            Death(); //Self Dedtroy the plane
        }

        if (collision.gameObject.GetComponent<PlayerBullet>()) //If collided wit a player bullet
        {
            Game.KillAfterSFX(collision.gameObject); //Destroy the bullet
            Death(); //Kill the enemy
        }
    }

    void RotateTowardsTarget() //Rotate the enemy towards the player
    {
        int multiplier = 1;
        if (!comingFromRight)
            multiplier = -1;
        //Set left or right point from the player by using a 1 and -1 multiplier for each case in the calculations

        Vector3 targetPosition = target.position + Vector3.right * desiredRadius * multiplier;
        Vector2 direction = (targetPosition - transform.position).normalized;


        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.rotation = angle - 90; //adding fix needed for the sprite in order to be rotated correctly.
    }

    void MoveTowardsTarget() //Move the enemy towards the player
    {
        int multuplier = 1;
        if (!comingFromRight)
            multuplier = -1;
        //Set left or right point from the player by using a 1 and -1 multiplier for each case in the calculations
        Vector3 targetPosition = target.position + Vector3.right * desiredRadius * multuplier;
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        moveDirection.z = 0;
        rb.velocity = moveDirection * speed;
    }


    void GetRidOfTrail() //Function that notifies every poison spawned by this enemy that the enemy had died (So spawn will dissapear fast)
    {
        for (int i = 0; i < trails.Count; i++)
        {
            if (trails[i])
                trails[i].EnemyDied();
        }
    }

    protected override void Death(bool realKill = true) // Overriding Death() to add use of GetRidOfTrail() in order to make the poison disappeaer quickly
    {
        GetRidOfTrail();
        base.Death(realKill);
    }

    public override void Spawn(bool fromQueue = false) //The basic function that spawns enemies, fromQueue is a parameter that can be used by the different enemy types later
    {
        float fix = 0.5f; //Fix needed based on enemy plane size
        Vector3 spawnPos = new Vector3(Random.Range(-2.3f, 2.3f), Camera.main.transform.position.y - Game.ScreenHalfHeight - fix, transform.position.z); //Setting the spawn position
        if (!Game.Ambush) //If normal mode, change the Y position of the spawn to be above the top of the screen
            spawnPos.y = Camera.main.transform.position.y + Game.ScreenHalfHeight + fix;


        Instantiate(this, spawnPos, Quaternion.identity); //Spawning the enemy
        EnemiesSpawner.EnemiesLeft++; //Increasing the enemies amount in wave by 1

    }
}
