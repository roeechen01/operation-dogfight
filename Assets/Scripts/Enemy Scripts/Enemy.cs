using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected float speed;

    protected Rigidbody2D rb;
    protected Transform target; //The player transform
    protected Player player; // Player gameobject
    protected int killMoney = 5;

    public bool alive = true;

    protected AudioSource audioSource;

    public AudioClip deathSFX;

    

    public EnemySpots spots; //EnemySpots contains spots (transforms), which are targets for the enemies to go to, and stay there and shoot the player, it makes the enemies go and stay in different position on the map, on a different spot

    protected bool hadAccident = false; //Saving if the enemy already collide with the player once or not (we'll use it to destory the enemy on second collision)

    protected void Awake()//Awake() is called before Start(), we verify everything is set before using it in other classes
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        player = FindObjectOfType<Player>();
        target = player.transform;
    }

    virtual protected void Death(bool realKill = true) //Basic Death function for enemies, it's a virtual method that can be overriden by the specific enemies types later as needed
    {
        if (realKill)
        {
            PlaySound(deathSFX);
            Player.MoneyFromGame += killMoney;
        }
           

        if (gameObject) //If enemy still exists and alive
        {
            
            EnemiesSpawner.EnemiesLeft--; //Decreasing 1 from the total enemies left in the wave
            Game.KillAfterSFX(gameObject); //Destroy the enemy gameobject
        }
        
       
    }

    

    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }




    public virtual void Spawn(bool fromQueue = false) //The basic function that spawns enemies, fromQueue is a parameter that can be used by the different enemy types later
    {
        float fix = 0.5f; //Fix needed based on enemy plane size
        Vector3 spawnPos = new Vector3(Random.Range(-2.3f, 2.3f), Camera.main.transform.position.y - Game.ScreenHalfHeight - fix, transform.position.z); //Setting the spawn position
        if (Game.Ambush) //If ambush, change the Y position of the spawn to be above the top of the screen
            spawnPos.y = Camera.main.transform.position.y + Game.ScreenHalfHeight + fix;


        Instantiate(this, spawnPos, Quaternion.identity); //Spawning the enemy
        EnemiesSpawner.EnemiesLeft++; //Increasing the enemies amount in wave by 1
        
    } 

    protected void AccidentCoolDown() { } //We invoke AccidentCoolDown() after enemie's first collision with the player,
                                          // if it collides again- we'll check with IsInvoking(AccidentCoolDown), to know if it's still on cooldown,
                                          // if it's on cooldown,we'll ignore the collision. Otherwise - most enemies will die after the second collision


}
