using UnityEngine;

public class ScrollableBackground : MonoBehaviour //This class handles the background, making it scroll in an infinite loop
{
    public GameObject backgroundPrefab;  // Assign the image prefab in the Unity Editor
    SpriteRenderer backgroundSR;

    Transform[] backgroundInstances;
    float backgroundHeight; //height of the background image

    void Start()
    {
        Destroy(GetComponent<SpriteRenderer>()); //Hiding the placeholder background in Unity Editor
        
        backgroundSR = backgroundPrefab.GetComponent<SpriteRenderer>();
        backgroundHeight = backgroundSR.bounds.size.y;

        CreateBackgroundInstances();

        InvokeRepeating("ScrollBackground", 1f, 0.1f); //Calls every 0.1s to the func handles the background images positions change if needed
    }

    void CreateBackgroundInstances() //Saving and positioning 2 background images instances, we'll use them later.
    {
        backgroundInstances = new Transform[2]; //Saving an array of background prefabs, will be used for the scroll background effect

        for (int i = 0; i < backgroundInstances.Length; i++)
        {
            
            GameObject newBackground = Instantiate(backgroundPrefab, transform); // Instantiate a copy of the background prefab
            backgroundInstances[i] = newBackground.transform; //Placing the copy in the array

            
            Vector3 offset = Vector3.up * i * backgroundHeight; // Offset the position of the new copy vertically
            backgroundInstances[i].position += offset; //Applying the height based on the offset

            backgroundInstances[i].GetComponent<Rigidbody2D>().velocity = Vector2.down * Game.UpSpeed; //Setting the background instance belocity to go down
        }
    }

    void ScrollBackground() //Handles when background is out of the screen to make it on top of the other background image, ensuring an infinite looped background
    {
        for (int i = 0; i < backgroundInstances.Length; i++)
        {
            // Check if a background instance is below the bottom of the screen
            if (backgroundInstances[i].position.y + backgroundHeight / 2 < Camera.main.transform.position.y - Game.ScreenHalfHeight)
            {
                // Move the background instance to the top
                Vector3 offset = Vector3.up * backgroundInstances.Length * backgroundHeight;
                backgroundInstances[i].position += offset;
            }
        }
    }
}