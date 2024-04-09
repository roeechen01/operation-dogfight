using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float timer = 0; //Timer to keep track of time since spawn (player shouldn't take demage if he touched explosion after it existed for a while)
    void Start()
    {
        Invoke(nameof(SelfDestroy), 0.75f); //Destroy the explosion shortly after it appeared
    }

    void SelfDestroy()
    {
        Game.KillAfterSFX(gameObject);
    }

    void Update()
    {
        timer += Time.deltaTime; //Setting the timer to show how much time passed since spawn
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        Player player = collision.gameObject.GetComponent<Player>();
        if (player && timer < 0.15f) //If explosion touched the player and explosion exists for max 0.15s
        {
            player.GotHit(20, true); //Activate the player.GotHit() to handle the hit
            timer = 0.15f; //Setting to 0.15 to make sure this explosion won't hit player again
        }
       
    }
}
