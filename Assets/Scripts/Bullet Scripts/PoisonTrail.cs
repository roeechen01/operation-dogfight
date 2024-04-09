using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonTrail : MonoBehaviour
{
    float timeUntilVanish = 1.5f; //Time until poison instance disappears (1.5f)
    
    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 2); //Setting to the right Z value
    }

    void Update()
    {
        timeUntilVanish -= Time.deltaTime; //Reucing the time until vanish timer with Time.deltaTime
        if (timeUntilVanish <= 0)
            Destroy(gameObject); //Destroy gameobject if timer hits 0
    }

    public void EnemyDied()
    {
        timeUntilVanish /= 1.5f; //If the enemy that created the poison died, shorten the poison lifetime.
    }

}
