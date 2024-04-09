using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHomingMissile : MonoBehaviour
{
    Transform target;
    public Transform spawner;
    public Explosion explosionPrefab;
    float speed = 3f;
    float homingSpeed = 5f;
    float destroyDelay = 4f;
    float closeDistance = 1.75f;
    //float explodeDistance = 1.5f;
    float homingTime = 3f;
    bool closeEnough = false;
    float timer = 0;
  

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        target = FindObjectOfType<Player>().transform;
        Invoke(nameof(SelfDestroy), destroyDelay);
        Invoke(nameof(StopHoming), homingTime);
    }

    void SelfDestroy()
    {
        Explode();
    }

    void StopHoming()
    {
        closeEnough = true;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (!closeEnough)
        {
            HomingMovement();
            if (Vector2.Distance(transform.position, target.position) < closeDistance)
                closeEnough = true;
        }

        else
        {
            rb.velocity = transform.up * speed;
            //if (Vector2.Distance(transform.position, target.position) < explodeDistance)
            //    Explode();
        }
           


    }

    void HomingMovement()
    {
        if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, homingSpeed * Time.deltaTime);
            rb.velocity = direction * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Player>())
        {
            Explode();
        }
        if (collision.gameObject.GetComponent<PlayerBullet>() && timer > 0.5f)
        {
            Explode();
            Game.KillAfterSFX(collision.gameObject);
        }
        else if (collision.gameObject.GetComponent<Explosion>() && collision.gameObject.GetComponent<Explosion>().timer < 0.15f && timer > 0.5f)
        {
            Explode();
            

        }
    }

    private void Explode()
    {
        Explosion x = Instantiate(explosionPrefab, spawner.position, Quaternion.identity);
        x.transform.localScale *= 1.25f;
        Destroy(gameObject);
    }

    
}
