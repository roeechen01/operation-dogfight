using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public bool isAndroid = false;
    float speed = 4f;
    SpriteRenderer sr;
    public Transform boost; //The blue boost sprite that shows when player is moving (hides when player stops)
    EnemiesSpawner spawner;

    AudioSource audioSource;
    public AudioClip crashSFX, gotHitSFX, reloadSFX;

    public GameObject winText;
    public Transform adTransform; //The transform holding the the ad pop up window
    public static int LastScore = 0; //Saving the previous game score
    public static int HighScore = 0; //Saving the current highscore
    public Image bulletSpriteIndicator;

    public Sprite shinySprite; //Sprite for the player to use when sht cooldown finihed and player is ready to shoot
    public Sprite normalSprite; //The default sprite for the player
    public float height, hp; //distance travelled and hp

    public static int Money = 0, MoneyFromGame = 0;
    
    bool movingToReadyPoint = false; //It's true in round beginnings, so the player knows it needs to go to the start round pos

    public bool invincible; //Can the player currently take demage

    public GameObject joystickDecoy;


    public Bullet bulletPrefab;
    public TextMeshProUGUI heightText, hpText, moneyText;
    public Transform normalReadyPoint, ambushReadyPoint; //The points the player go to at the start of round, one for ambush rounds, one for normal rounds
    Transform currentWaveReadyPoint; //The right point for this round
    float shotCooldown = 0.85f; //Time to wait after shooting before player can shoot again
    Joystick joystick;
    public Transform fireButton;

    Rigidbody2D rb;


    float poisonDemageRate = 4f; //Poison demage value


    int poisonCollisionCounter = 0; //This variable keeps track on collisons with poison instances, if more than 1 collision, we will reduce poison demage from player HP in Update()
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!movingToReadyPoint && collision.gameObject.GetComponent<PoisonTrail>()) //If touching poison
        {
            poisonCollisionCounter++; //Add 1 to the poison collisions counter
        }
    }

    void OnTriggerExit2D(Collider2D collision) //Funciton thas is called when exiting a collision
    {
        if (collision.gameObject.GetComponent<PoisonTrail>()) //If not touching poison anymore
        {
            poisonCollisionCounter--; //Decrease 1 to the poison collisions counter
        }
    }

    public static void UpdateHighScore()
    {
            PlayerPrefs.SetInt("HighScore", Player.HighScore);
    }

    public static void UpdateMoney()
    {
        PlayerPrefs.SetInt("Money", Money);
    }


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        MoneyFromGame = 0;
        hp = 100;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        joystick = FindObjectOfType<Joystick>();
        spawner = FindObjectOfType<EnemiesSpawner>();
        if (!isAndroid)
        {
            Destroy(joystick.gameObject);
            Destroy(fireButton.gameObject);
        }

        adTransform.gameObject.SetActive(false); //Set ad transform to false, to hide the ad stuff

        //Setting the ready points Z pos as the player Z pos, required for it to work
        normalReadyPoint.position = new Vector3(normalReadyPoint.position.x, normalReadyPoint.position.y, transform.position.z);
        ambushReadyPoint.position = new Vector3(ambushReadyPoint.position.x, ambushReadyPoint.position.y, transform.position.z);

        InvokeRepeating(nameof(IncreaseHeight), 0.1f, 0.1f);//Increase the height distance travelled of the level every 0.1s


        InvokeRepeating(nameof(CheckMovement), 0.1f, 0.1f); //Invoke check every 0.1f to set the blue boost visibility based on if player is moving

        Time.timeScale = 1; //Ensuring game time scale is normal at the begninning of the game

        

    }

    void Update()
    {
        //If poison collisions counter is more than 1 (touching poison) and invincibility is false
        if (poisonCollisionCounter > 0 && !invincible)
        {
            hp -= Time.deltaTime * poisonDemageRate; //Decrease health by the poisonDemageRate every moment with time.deltaTime
            if (hp <= 0) //Kill if HP < 0
                Death();
        }
       

        if (movingToReadyPoint) //If player needs to move to ready point for the round start, move to the point
            MoveToReadyPoint();
        else //If not, move normally
        {
            if (!isAndroid)
            {
                Movement();
                Rotation();
               
            }
            else
            {
                MovementWithJoystick();
                JoystickRotation();
            }

        }
        if(!isAndroid)
            Shoot();

        heightText.text = height + "m";
        hpText.text = hp >= 0 ? Mathf.Ceil(hp).ToString() : "0";
        moneyText.text = MoneyFromGame.ToString();
    }

    Vector2 lastCheckPos; //player position last check (updated every time)
    float enough = 0.05f; //Distance that player needs to move in a time period for showing the blue boost
    void CheckMovement() //Check is moved enough to show the blue boost sprite
    {
        
        if(movingToReadyPoint || Vector2.Distance(transform.position, lastCheckPos) > enough) //If moved enough from lastCheckPos
        {
            boost.gameObject.SetActive(true); //Show the blue boost
        }
        else
        {
            Invoke(nameof(BoostOff), 0.125f); //If not, wait a second then check if still didn't move, if so - hide the blue boost sprite
        }

        lastCheckPos = transform.position; //Update the check positions every check
    }

    void BoostOff() //  Function to check if to hide the blue boost, and hides it if needed
    {
        if (Vector2.Distance(transform.position, lastCheckPos) < enough) //If didn't move enough to show the blue boost still
            boost.gameObject.SetActive(false); //Hide the blue boost
    }

    void IncreaseHeight()
    {
        if(spawner.heightMeterMoving)   
            height++; //Increase player height (score)
        else
        {
            height = NearestMultipleOfTen(height);
        }
    }

    float NearestMultipleOfTen(float number)
    {
        // Calculate the remainder when dividing by 10
        float remainder = number % 10f;

        // Round up only if remainder is strictly greater than 5
        if (remainder > 5f)
        {
            return number + (10f - remainder);
        }
        else
        {
            return number - remainder;
        }
    }

    public void SwitchMode() //Called from EnemySpawner class, the function makes the player ready to move the the right ready point for the round
    {
        currentWaveReadyPoint = Game.Ambush ? ambushReadyPoint : normalReadyPoint; //Setting currentWaveReadyPoint to the right point based on if it's ambush or not
        movingToReadyPoint = true; //Changing to true, so later in the code we will move towards the point
        
    }

    public void AddHP(float hpAddition) //Function to add hp from medkits
    {
        if (hp + hpAddition > 100)
            hp = 100;
        hp += hpAddition;
    } 

    public void GotHit(float demage, bool invincibiltyAttack = false, bool crash = false, float invincibilityDuration = 1) //This function handled the player getting hit, parameters are demage, and wether the player gets invincibility
    {
        
        if (invincible) //If player is currently invincible, don't do anything and exit the function
            return;

        if (crash)
            PlaySound(crashSFX);

        if (!crash && !invincibiltyAttack)
            audioSource.PlayOneShot(gotHitSFX);

        if (invincibiltyAttack) //If the attack is an attack that gives the player short invincibilty
            StartInvincible(invincibilityDuration); //Give the player short invincibility

        hp -= demage;
        

        if(hp <= 0) //If player is dead
        {
            Death();
        }

       
    }

   


    bool adWatched = false; //Keeps track if player wached an ad this game
    Vector3 rotationBeforeAd;
    void Death()
    {
        rb.velocity = Vector2.zero;
        FloatingJoystick.ResetJoystickPosition();
        
        hp = 0;
        if (!adWatched) //If didnt watch add
        {
            joystick.gameObject.SetActive(false);
            adTransform.gameObject.SetActive(true); //Activate the ad pop up transform
            Time.timeScale = 0; //Stop game time
        }
           
        else //If already watched ad
        {
            GameOver(); //End the game with GameOver() func
        }
        
    }

    
    public void WatchedAd() //Function to handle player watching ad
    {
        Time.timeScale = 1; //Reverting to normal time scale so the game will run normally
        hp = 100; //Setting hp to 100
        
        EndInvincible();
        StartInvincible(2); //Hitting player with 0 demage, to give invincibility (the parameter "true" means the player will get invincibility)
        adWatched = true; //Set adWatched bool to true so we know to not let the player watch another ad after dying a second time
        adTransform.gameObject.SetActive(false); //Hide the ad pop up transform

    }

    public void DidntWatchAd() //Function that handles a case when the player decided to not watch an ad
    {
        GameOver(); //Ending the game
       
    }

    public void GameOver()
    {
        Time.timeScale = 1; //Setting to 1 to insure the game's time scale is normal
        LastScore = (int)height; //Set the last score to this game score
        if (LastScore > HighScore)
        {
            HighScore = LastScore;
            UpdateHighScore();
        }
            

        SceneManager.LoadScene("Lose Screen"); //Load the lose screen
    }

    void StartInvincible(float duration = 1)//Starts invincibility for the player
    {
        invincible = true;
        sr.color = new Color(1, 1, 1, 0.5f); //Make player a little transparent
        Invoke(nameof(EndInvincible), duration); //Calls EndInvincible() to finish invincibilty after time duration (can be set in duration parameter)
    }

    void EndInvincible()//Ends invincibility for the player
    {
        if(IsInvoking(nameof(EndInvincible)))
            CancelInvoke(nameof(EndInvincible));
        invincible = false;
        sr.color = new Color(1, 1, 1, 1f);//Revert player to be fully visible
    }

   

    bool speedNotSet = true; //Is the speed the player needs to move in order to reach the ready point in 3 seconds not set yet
    float speedToPoint; //The speed the player needs to move in order to reach the ready point in 3 seconds
    void MoveToReadyPoint()
    {
        Vector3 directionToPoint = currentWaveReadyPoint.position - transform.position; //Direction from player to to ready point
        float distanceToPoint = directionToPoint.magnitude; //Distacne from player to to ready point

        if (distanceToPoint > 0.05f) //If the player didn't reach the ready point
        {
            if (speedNotSet) //If the required speed has not been set yet
            {
                speedToPoint = distanceToPoint / 3f; //Set the right speed
                speedNotSet = false; //Set that to false, so we know we already set the right speed
            }

            Vector3 direction = (currentWaveReadyPoint.position - transform.position).normalized; //Normalized direction from player to to ready point

            rb.velocity = direction * speedToPoint;//Move the plane to the ready point

            //Rotation
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; //Rotation angle for the player, based on it's movement direction

            angle -= 90; //Rotation fix for the plane sprite

            transform.rotation = Quaternion.Euler(0f, 0f, angle); //Update the rotation
        }
        else //If the player reached the point
        { 
                
            rb.velocity = new Vector2(0, 0); //Make the plane stop moving
            if(Game.Ambush)
                  transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * Mathf.Atan2(0, 1)); //Rotate towards the sky (for normal attack)
            else transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * Mathf.Atan2(0, -1)); //Rotate towards the ground (for ambush)

            Invoke("EndMovingToPoint", 2); //Set the player movement free after 2 seconds

        }
    }

    void EndMovingToPoint() //Handle when player reached the ready point, so that the player will move freely again in the wave
    {
        movingToReadyPoint = false; //Reverting to true, the player will be able to move freely
        speedNotSet = true; //Reverting to true, so that it will be ready for next wave start
    }

    void ShootPC() //Shoots a bullet
    {
        if (Input.GetMouseButtonDown(0) && !IsInvoking(nameof(ShootCooldown)))
        {
            Shoot();
        }

    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void ShootFromButton()
    {
        if (!IsInvoking(nameof(ShootCooldown)))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        Invoke(nameof(ShootCooldown), shotCooldown);
        bulletSpriteIndicator.enabled = false;
        Vector3 bulletPos = new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.5f);
        Instantiate(bulletPrefab, bulletPos, Quaternion.identity);
    }

    void ShootCooldown() //Function meant to be invoked and checked with IsInvoking() - to know if shot cooldown is over or not
    {
        audioSource.PlayOneShot(reloadSFX, 0.55f);
        ShinySprite(); //Make the player shine after loaded shot
        bulletSpriteIndicator.enabled = true;
    }

    void ShinySprite() //Function to call when player is ready to shoot again
    {
        sr.sprite = shinySprite; //Changing the player sprite to shine after cooldwn ended
        Invoke(nameof(NormalSprite), 0.03f); //Revert to normal sprite after a short while
    }

    void NormalSprite() //Function to call when player is in shiny sprite, to switch to normal again
    {
        sr.sprite = normalSprite;
    }




    void Rotation()
    {
        if (movingToReadyPoint || Time.timeScale == 0) //If player is moving to ready point, exit the function. Rotation will be handled in MoveToReadyPoint()
            return; //Exit function

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); //Save mouse position
        mousePosition.z = transform.position.z; //Setting Z pos to same as player so we don't get errors

        Vector3 direction = (mousePosition - transform.position).normalized; //Normalized direction from the player to the mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; //Angle to rotate based on the direction

        angle -= 90; //Rotation fix for the plane sprite

        transform.rotation = Quaternion.Euler(0f, 0f, angle); //Update the rotation

    }

    void JoystickRotation()
    {
        if (movingToReadyPoint || Time.timeScale == 0 || !joystick.isDragging && joystick.Direction == Vector2.zero) //If player is moving to ready point, exit the function. Rotation will be handled in MoveToReadyPoint()
            return; //Exit function

        Vector2 playerVelocity = rb.velocity.normalized; //Normalized velocity of the player
        float angle = Mathf.Atan2(playerVelocity.y, playerVelocity.x) * Mathf.Rad2Deg; //Angle to rotate based on the player's velocity

        angle -= 90; //Rotation fix for the plane sprite

        transform.rotation = Quaternion.Euler(0f, 0f, angle); //Update the rotation
    }


    void Movement() 
    {
        if (movingToReadyPoint) //If player is moving to ready point, exit the function. Movement will be handled in MoveToReadyPoint()
            return; //Exit function

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);//Save mouse position
        mousePosition.z = transform.position.z; //Setting Z pos to same as player so we don't get errors

        Vector3 directionToMouse = mousePosition - transform.position; //direction from the plane to the mouse
        float distanceToMouse = directionToMouse.magnitude; //Saving the distance from player to mouse

        if (distanceToMouse > 0.6f) //If the player plane is a little far from the mouse
        {
            Vector3 direction = (mousePosition - transform.position).normalized;  //Normalized direction from the player to the mouse
            direction.z = 0;

            // Move the plane towards the mouse
            rb.velocity = direction * speed;
        }
        else //If the player is close to the mouse
        {
            rb.velocity = new Vector2(0, 0); //Stop player movement
        }

        Boundries();//Check the boundries
    }

    void MovementWithJoystick()
    {
        if (movingToReadyPoint) //If player is moving to ready point, exit the function. Movement will be handled in MoveToReadyPoint()
            return; //Exit function

        
        Vector2 direction = Vector2.right * joystick.Direction + Vector2.up * joystick.Direction.y;

        // Normalize the direction vector if its magnitude is greater than 1
        if (direction.magnitude > 1f)
            direction.Normalize();

        rb.velocity = direction * speed;

        // Ensure the player doesn't move beyond the screen boundaries
        Boundries();
    }

    void Boundries()//Function that limits the plane movement based on screen boundries
    {
        float fixSize = 0.7f; // Fix needed based on the plane size

        // Using Mathf.Clamp() to make sure X and Y posisions are between the valid boundries of the screen
        float clampedX = Mathf.Clamp(transform.position.x, -Game.ScreenHalfWidth + fixSize, Game.ScreenHalfWidth - fixSize);
        float clampedY = Mathf.Clamp(transform.position.y, Camera.main.transform.position.y - Game.ScreenHalfHeight + fixSize, Camera.main.transform.position.y + Game.ScreenHalfHeight - fixSize);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z); //Updaing player position to an appropiate one
    }
}

