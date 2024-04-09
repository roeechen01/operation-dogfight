using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medkit : MonoBehaviour
{
    Rigidbody2D rb;
    Player player;
    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent < Rigidbody2D>();
        player = FindObjectOfType<Player>();
        Destroy(gameObject, 10); //Destroy self after 10 seconds
        rb.velocity = Vector2.down * Game.UpSpeed; //Set velocity to go down exactly with the background speed
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>()){ //If touching player
            audioSource.Play();
            Game.KillAfterSFX(gameObject); //Destroy self
            player.AddHP(Random.Range(15, 31)); //Increase player HP
            }   
    }
}
