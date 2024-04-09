using System.Collections;
using UnityEngine;
using TMPro;

public class EnemiesSpawner : MonoBehaviour
{
    public Enemy suicidePrefab, shooterPrefab, missilePrefab, circleSprayPrefab, crossingSprayPrefab, homingMissilePrefab; // Declare enemy prefabs
    public Medkit medkit; // Declare the medkit prefab
    Player player; // Reference to the player

    public static int EnemiesLeft = 0; // Declare static variable to track remaining enemies in the current wave (unused in the game)
    int waveIndex = 0; // Variable to store the current wave index
    public bool waveSpawnEnd = true; // Boolean to track if all enemies in the wave have spawned
    public bool heightMeterMoving = true;
    int medsToSpawnInWave = 2;

    public TextMeshProUGUI bonusText;

    float waveDuration = 25; // Duration of each wave

    void Start()
    {
        bonusText.text = "";
        player = FindObjectOfType<Player>();
        EnemiesLeft = 0;
        NewWave(); // Start the first wave
        InvokeRepeating(nameof(EndWaveCheck), 1, 1); // Check for the end of each wave every second
        //StartCoroutine(SpawnEnemyForWave(circleSprayPrefab, 5, 1));
    }

    void EndWaveCheck()
    {
        if (waveSpawnEnd) // Check if all enemies have spawned and no enemies are remaining
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); // Find all game objects with the "Enemy" tag
            GameObject[] missiles = GameObject.FindGameObjectsWithTag("Enemy Missile"); // Find all game objects with the "Enemy" tag
            if (enemies.Length == 0 && missiles.Length == 0) // Check if there are no enemies in the scene
            {
                Invoke(nameof(EndWaveConfirm), 1); // Confirm the end of the wave with another delayed check - EndWaveConfirm()
            }
        }
    }

    void EndWaveConfirm()
    {
        if (waveSpawnEnd) // Check if all enemies have spawned and no enemies are remaining
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); // Find all game objects with the "Enemy" tag
            GameObject[] missiles = GameObject.FindGameObjectsWithTag("Enemy Missile"); // Find all game objects with the "Enemy" tag
            // Check if there are no enemies in the scene and no enemies are queued for spawn
            if (enemies.Length == 0 && missiles.Length == 0 && ShooterEnemy.Queue == 0 && MissileEnemy.Queue == 0) 
            {
                NewWave(); // Start a new wave
            }
        }
    }

    void CleanText()
    {
        bonusText.text = "";
    }
    
    void NewWave()
    {
        medsToSpawnInWave = 2;
        heightMeterMoving = true;
        waveSpawnEnd = false; // Reset the flag indicating the end of the wave spawn
        EnemiesLeft = 0; // Reset the number of remaining enemies
        if (waveIndex == 0) // Determine the mode for the next wave (Ambush or Normal)
            SwitchMode(false); // Set normal mode for the first wave
        else
        {
            SwitchMode(); // Switch the current mode for subsequent waves
            int bonus = 50 * waveIndex;
            Player.MoneyFromGame += bonus;
            bonusText.text = "+" + bonus;
            Invoke(nameof(CleanText), 2);

        }
            
        Invoke(nameof(SetSpawnsStart), 5); // Schedule the start of enemy spawns after a delay
    }

    void SetSpawnsEnd() // Set the flag indicating the end of the wave spawn
    {
        waveSpawnEnd = true;
        heightMeterMoving = false;
    }

    void SetSpawnsStart() // Reset the flag indicating the end of the wave spawn
    {
        waveSpawnEnd = false;
        Invoke(nameof(SetSpawnsEnd), waveDuration); // Schedule the end of enemy spawns after the wave duration
        waveIndex++; // Increment the wave index
        SpawnWaves(); // Spawn enemies for the current wave
        //SpawnEnemy(circleSprayPrefab);
    }

    void SpawnEnemy(Enemy prefab)
    {
        StartCoroutine(SpawnEnemyForWave(prefab, 7, 1, 0));
    }

    void SpawnWaves()
    {
        if (waveIndex == 1)
        {
            waveDuration = 25;
            StartCoroutine(SpawnEnemyForWave(shooterPrefab, 4f, 2, 1));
            StartCoroutine(SpawnEnemyForWave(suicidePrefab, 5f, 1, 6));
        }
        else if (waveIndex == 2)
        {
            waveDuration = 30;
            StartCoroutine(SpawnEnemyForWave(shooterPrefab, 4f, 3, 2));
            StartCoroutine(SpawnEnemyForWave(missilePrefab, 6f, 2, 4));
            StartCoroutine(SpawnEnemyForWave(suicidePrefab, 5f, 1, 8));
        }
        else if (waveIndex == 3)
        {   
            waveDuration = 35;
            StartCoroutine(SpawnEnemyForWave(shooterPrefab, 4.5f, 2, 2));
            StartCoroutine(SpawnEnemyForWave(missilePrefab, 6f, 2, 6));
            StartCoroutine(SpawnEnemyForWave(suicidePrefab, 7f, 1, 7));
            StartCoroutine(SpawnEnemyForWave(crossingSprayPrefab, 7f, 1, 8));
        }
        else if (waveIndex == 4)
        {
            waveDuration = 40;
            StartCoroutine(SpawnEnemyForWave(shooterPrefab, 5f, 3, 2));
            StartCoroutine(SpawnEnemyForWave(missilePrefab, 7f, 2, 5));
            StartCoroutine(SpawnEnemyForWave(suicidePrefab, 6f, 1, 4));
            StartCoroutine(SpawnEnemyForWave(crossingSprayPrefab, 7f, 2, 4));
            StartCoroutine(SpawnEnemyForWave(homingMissilePrefab, 9f, 1, 14));
        }
        else if (waveIndex == 5)
        {
            waveDuration = 45;
            StartCoroutine(SpawnEnemyForWave(suicidePrefab, 6.5f, 1, 3));
            StartCoroutine(SpawnEnemyForWave(shooterPrefab, 5f, 2, 2));
            StartCoroutine(SpawnEnemyForWave(missilePrefab, 12f, 1, 5));
            StartCoroutine(SpawnEnemyForWave(crossingSprayPrefab, 9f, 2, 8));
            StartCoroutine(SpawnEnemyForWave(homingMissilePrefab, 7f, 2, 5));
            StartCoroutine(SpawnEnemyForWave(circleSprayPrefab, 10f, 1, 14));
        }
        else if (waveIndex >= 6)
        {
            StartCoroutine(SpawnEnemyForWave(suicidePrefab, 6f, 1, 3));
            StartCoroutine(SpawnEnemyForWave(shooterPrefab, 5f, 3, 3));
            StartCoroutine(SpawnEnemyForWave(missilePrefab, 12f, 1, 5));
            StartCoroutine(SpawnEnemyForWave(crossingSprayPrefab, 8f, 2, 8));
            StartCoroutine(SpawnEnemyForWave(homingMissilePrefab, 8f, 2, 7));
            StartCoroutine(SpawnEnemyForWave(circleSprayPrefab, 9f, 1, 10));
        }
        
    }

    void MedCoolDown() { }


    IEnumerator SpawnEnemyForWave(Enemy enemyPrefb, float repeatRate, int limit = 0, float delayFirstSpawn = 0, bool random = false) // Coroutine to spawn enemies for a wave
    {
        yield return new WaitForSeconds(delayFirstSpawn); // Wait for a delay before spawning the first enemy
        float timePassed = delayFirstSpawn; // Initialize the time passed since the first spawn

        int enemiesToSpawn = Mathf.FloorToInt((waveDuration - timePassed) / repeatRate); // Calculate the maximum number of enemies to spawn

        enemyPrefb.Spawn(); // Spawn the first enemy

        for (int i = 0; i < enemiesToSpawn - 1; i++) // Loop to spawn additional enemies
        {

            if (!IsInvoking(nameof(MedCoolDown)) && medsToSpawnInWave > 0 &&  player.hp < 100 && Random.Range(1, 15) == 1 && !IsInvoking(nameof(SpawnMedkit))) // Check conditions for spawning a medkit
            {
                Invoke(nameof(MedCoolDown), 2);
                medsToSpawnInWave--;
                Invoke(nameof(SpawnMedkit), Random.Range(1f, 3f)); // Schedule the spawn of a medkit
            }

            

            float rnd = repeatRate; // Initialize the spawn rate for the current enemy
            if (random) // Check if random spawn rate is enabled
                rnd = repeatRate + Random.Range(-1f, 1f); // Adjust the spawn rate randomly

            yield return new WaitForSeconds(rnd); // Wait for the next spawn interval
            timePassed += repeatRate; // Update the time passed

            //if (Random.Range(1, 11) == 10)
            //    continue;

            enemyPrefb.Spawn(); // Spawn the enemy

            if (Random.Range(1, 3) == 2 && limit > 1) // Check conditions for spawning additional enemies
            {
                enemyPrefb.Spawn(); // Spawn an additional enemy
                if (Random.Range(0f, 1f) < 0.41665f && limit > 2) // Check conditions for spawning a third enemy
                {
                    enemyPrefb.Spawn(); // Spawn the third enemy
                }
            }
        }
    }

    public void SpawnMedkit() // Method to spawn a medkit
    {
        Vector3 randomPos = new Vector3(Random.Range(-2f, 2f), 6.5f, medkit.transform.position.z); // Randomize the position for spawning the medkit
        Instantiate(medkit, randomPos, Quaternion.identity); // Instantiate the medkit prefab
    }

    void SwitchMode() // Method to switch between Ambush and Normal modes
    {
        Game.Ambush = !Game.Ambush; // Toggle the Ambush mode
        player.SwitchMode(); // Call the player's SwitchMode method
    }

    void SwitchMode(bool ambush) // Method to switch the mode with a specified Ambush status
    {
        Game.Ambush = ambush; // Set the Ambush mode based on the parameter
        player.SwitchMode(); // Call the player's SwitchMode method
    }
}
