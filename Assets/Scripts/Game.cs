using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Game : MonoBehaviour
{
    public static float ScreenHalfWidth;//The screen half width - needed for calculations
    public static float ScreenHalfHeight;//The screen half height - needed for calculations
    public static float ScreenHeight; //The screen height - needed for calculations
    public static float UpSpeed = 2.5f; //The background speed
    public static bool Ambush = false; //Is the game in ambush mode (enemies come from top), or normal mode (from bottom)
    Player player;
    Joystick joystick;
    public EnemySpots midRange, arches; //Strategically placed spots for the enemies to go to and attack the player
    public TextMeshProUGUI distanceText, bestText, moneyMenuText, roundMoneyLoseText, totalMoneyLoseText;
    public Transform doubleMoneyAd, bonusAd;

    public static EnemySpots MidRangeCircle, Arches; //Same spots, though static so can be accessed from Game class itself
    



    void Awake() //Awake is called before Start(), so we set values here to ensure they will have value when we use them
    {
        ScreenHalfWidth = Camera.main.orthographicSize * Camera.main.aspect; //Saving the value of half the screen width
        ScreenHalfHeight = Camera.main.orthographicSize;//Saving the value of half the screen height
        ScreenHeight = ScreenHalfHeight * 2;//Saving the value of the screen height
        Arches = arches;
        MidRangeCircle = midRange;
        player = FindObjectOfType<Player>();
        joystick = FindObjectOfType<Joystick>();
        
        
    }

    static bool LoadedSavedInfo = false;

    private void Start()
    {

        if (SceneManager.GetActiveScene().name == "Lose Screen") //If distance text is set (only in LoseScreen scene)
        {
            distanceText.text = "Score: " + Player.LastScore + "m"; //Set the text to the score from last game
            bestText.text = "Best: " + Player.HighScore + "m"; //Set the text to the best score yet
            Player.Money += Player.MoneyFromGame;
            roundMoneyLoseText.text = Player.MoneyFromGame.ToString();
            totalMoneyLoseText.text = Player.Money.ToString();
            Player.UpdateMoney();
        }
         else if(SceneManager.GetActiveScene().name == "Menu")
        {
            if (!LoadedSavedInfo)
            {
                LoadedSavedInfo = true;
                // Load highscore and money from PlayerPrefs
                Player.HighScore = PlayerPrefs.GetInt("HighScore", 0);
                Player.Money = PlayerPrefs.GetInt("Money", 0);
            }
            
            moneyMenuText.text = Player.Money.ToString();
        }
        
    }

    public void Ad() //Handle player wathcing ad
    {
        Time.timeScale = 1;
        joystick.gameObject.SetActive(true);
        player.WatchedAd();
    }

    public void DoubleEarnings()
    {
        Player.Money += Player.MoneyFromGame;
        Player.MoneyFromGame *= 2;
        roundMoneyLoseText.text = Player.MoneyFromGame.ToString();
        totalMoneyLoseText.text = Player.Money.ToString();
        Destroy(doubleMoneyAd.gameObject);
        Player.UpdateMoney();

    }

    public void NoAd() //Handle player not wathcing ad
    {
        Time.timeScale = 1;
        joystick.gameObject.SetActive(true);
        player.DidntWatchAd();
    }

    public void LoadScene(string sceneName) //Loading a scene by name
    {
        SceneManager.LoadScene(sceneName);
    }

    void RestartLevel() //Restarting the level
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShootButton()
    {
        player.ShootFromButton();
    }

    public void AdBonus(int bonus)
    {
        Player.Money += bonus;
        Player.UpdateMoney();
        moneyMenuText.text = Player.Money.ToString();
        Destroy(bonusAd.gameObject);
    }



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (Input.GetKeyDown(KeyCode.Tab))
            RestartLevel();

    }

    public static bool DistanceCloseEnough(Vector2 pos1, Vector2 pos2, float closeDistance) //Function that returns true if the 2 positions are close enogh based on closeDistance float parameter, else returns false
    {
        return Vector2.Distance(pos1 , pos2) < closeDistance;
    }

    public static bool DistanceCloseEnough(float num1, float num2, float closeDistance)//Function that returns true if the 2 numbers are close enogh based on closeDistance float parameter, else returns false
    {
        return Mathf.Abs(num1 - num2) < closeDistance;
    }

    public static void KillAfterSFX(GameObject obj)
    {
        Destroy(obj, 1.5f);
        obj.GetComponent<SpriteRenderer>().enabled = false;
        obj.GetComponent<Collider2D>().enabled = false;
        if(obj.GetComponent<Enemy>())
            obj.GetComponent<Enemy>().alive = false;
    }



}
