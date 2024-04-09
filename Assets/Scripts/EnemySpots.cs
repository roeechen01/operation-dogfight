using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The parent of the circles transforms (or arches transforms) will hold the code,
/// getting the spots transforms that are inside the inner and outer circles (or arches).
/// Then we'll set two lists, one for the higher spots, containing the upper inner circle spots and the upper outer circle.
/// The other list is for the bottom position spots in both circles / both arches.
/// </summary>

public class EnemySpots : MonoBehaviour
{
    public static Player player; // Reference to the player
    public bool isCircle = false; //Are this EnemySpots spots in circles
    public bool isArches = false; // Are this EnemySpots spots in arches

    public List<Transform> spots; // List of all spots in this EnemySpots
    public Transform firstCircle, secondaryCircle; // Transforms of the two circles (or arches)

    List<Transform> firstCircleSpots, secondaryCircleSpots; // Lists for spots in each circle / arch (wll be set in )
    public List<Transform> upSpots, downSpots; // Lists for upper and lower spots
    List<bool> upSpotsTaken, downSpotsTaken; // Lists to track if spots are taken

    public float innerCircleRadius = 2.5f; // Radius of the inner circle
    public float outerCircleRadius = 3.5f; // Radius of the outer circle


    // Property that returns the index of the first spot of the secondary circle / arch  (Used in some functions)
    public int SecondaryCircleStartIndex
    {
        //Getting the right values based on if it's circle or arches spots
        get { return isCircle ? firstCircleSpots.Count / 2 + 1 : firstCircleSpots.Count / 2; } 
    }

    // Property to get right spots based on ambush mode or normal
    public List<Transform> RightSpots
    {
        get { return Game.Ambush ? upSpots : downSpots; }
    }

    // Property to get which spotsTaken is relevant based on ambush mode. (upSpotsTaken or downSpotsTaken)
    public List<bool> RightSpotsTaken
    {
        get { return Game.Ambush ? upSpotsTaken : downSpotsTaken; } //Returning the right spotsTaken list based on ambush or not
    }


    void Start()
    {
        player = FindObjectOfType<Player>(); // Find the player object
        SetRightSpots(); // Set the right spots and values in upSpots and downSpots, And position them if necessary
        SetFree(); // Set all spots as free (no enemy targets them at the start of the game)
    }

    void Update()
    {
        if (isCircle) //If this is a circle EnemySpots
            transform.position = player.transform.position; // Move with player
    }

    // Function to set the correct spots based on flags
    void SetRightSpots()
    {
        // Setting inner circle spots
        firstCircleSpots = new List<Transform>(firstCircle.GetComponentsInChildren<Transform>()); //This will give us a list of all the transforms (spots) inside the first circle
        firstCircleSpots.Remove(firstCircle); //Removing the circle parent of the spots that might be added unecessarily in the previous line

        // Initializing both upper and lower spotstaken lists, the lists that will determine wether each spot is free or taken
        upSpotsTaken = new List<bool>();
        downSpotsTaken = new List<bool>();

        // Initializing both upper and lower spots lists, the lists that will contatin the spots
        upSpots = new List<Transform>();
        downSpots = new List<Transform>();

        for (int i = 0; i < firstCircleSpots.Count; i++) //Loop through all the spots in the first circle
        {
            spots.Add(firstCircleSpots[i]); //Add the spot to the entire spots list
            if (i < firstCircleSpots.Count / 2) //If the index is lower than half the spots, it is an upper spot
            {
                upSpots.Add(firstCircleSpots[i]); //Adding the spot to the upSpots list
                upSpotsTaken.Add(false); //Add an upSpotTaken value of false (the spot we added will be considered free)
            }
            else
            {
                downSpots.Add(firstCircleSpots[i]); //Adding the spot to the downSpots list
                downSpotsTaken.Add(false); //Add an downSpotTaken value of false (the spot we added will be considered free)
            }
        }

        //Doing the same things for the secondary circle / arch
        secondaryCircleSpots = new List<Transform>(secondaryCircle.GetComponentsInChildren<Transform>());
        secondaryCircleSpots.Remove(secondaryCircle);

        for (int i = 0; i < secondaryCircleSpots.Count; i++)
        {
            spots.Add(secondaryCircleSpots[i]);
            if (i < secondaryCircleSpots.Count / 2)
            {
                upSpots.Add(secondaryCircleSpots[i]);
                upSpotsTaken.Add(false);
            }
            else
            {
                downSpots.Add(secondaryCircleSpots[i]);
                downSpotsTaken.Add(false);
            }
        }
        //

        if (!isArches) //If this EnemySpots instance is for spots in circle, not arches
            SetSpotsDistance(); // This funtion changes the spots position to the right distance based on  it's current direction from the circle (needed only in circle EnemySpots)
    }

    // Function to change the spots distance to the desired one (necessary for the circle EnemySpots)
    void SetSpotsDistance()
    {
        foreach (Transform spot in firstCircleSpots) //Looping through every spot in the first circle
        {
            // Get the direction from the spot to the circle
            Vector3 directionToSpot = (spot.position - transform.position).normalized;

            // Calculate the new position of the spot based on direction and desired radius
            Vector3 newPosition = transform.position + directionToSpot * innerCircleRadius;
            spot.position = newPosition; //Setting the new spot position
        }

        //Doing the same things for the second circle, this time using outerCircleRadius insted of inner
        foreach (Transform spot in secondaryCircleSpots)
        {
            Vector3 directionToSpot = (spot.position - transform.position).normalized;
            Vector3 newPosition = transform.position + directionToSpot * outerCircleRadius;
            spot.position = newPosition;
        }
        //
    }

    // Function to check if there is space for a new point
    public bool IsThereSpace(bool insideArch = false) //insideArch should be true if we want to check if there's a free spot on the inner arch.
    {
        if (isArches) //If this EnemySpots has arches spots (not circle)
        {
            if (insideArch) //Do we need to check the inner arch
            {
                for (int i = SecondaryCircleStartIndex; i < RightSpotsTaken.Count; i++) //Setting i to the first index of the inner arch and looping the rest of the inner spots
                {
                    if (!RightSpotsTaken[i])
                        return true; //Returning true if the spot is free
                }
            }
            else //If not, check the outer arch
            {
                for (int i = 0; i < SecondaryCircleStartIndex; i++) //Looping through the arches outer spots (stopping loop when we get to the outer spots)
                {
                    if (!RightSpotsTaken[i])
                        return true; //Returning true if the spot is free
                }
            }
        }
        else //Else (EnemySpots in circle based)
        {
            foreach (bool taken in RightSpotsTaken) //Loop through all the spots, if a spot if free, return true
                if (!taken)
                    return true;
        }
        return false; //If we get here, no spot is free. Returning false.
    }

    // Function to check if a position is inside the screen boundries
    public bool IsPositionOnScreen(Vector2 point, float verticalLimitOffset = 0) //Getting the spot position and vertical offset to the boundries Y limit
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(point); //Getting the screen boundries to Vector3 screenPoint.

        if (Game.Ambush) // If in ambush mode, enemies can go slightly above the boundaries
            return screenPoint.x > 0 && screenPoint.x < 1.025f && screenPoint.y > 0 && screenPoint.y < 1 + verticalLimitOffset;

        else // If in normal enemies attack mode, enemies can go slightly below the boundaries
            return screenPoint.x > 0 && screenPoint.x < 1.025f && screenPoint.y > -verticalLimitOffset && screenPoint.y < 1;
    }

    // Function to check if there is another free spot within screen boundaries
    public bool IsThereAnotherFreeSpotInBoundries(int index)
    {
        int validSpotsFound = 0; //Counter for valid spots found
        for (int i = 0; i < RightSpots.Count; i++) //Looping through every spot (in upSpots or downSpots based on if it's ambush)
        {
            if (i == index) continue; //If it's the same spot index as the enemy already had, continue to the next spot

            if (!RightSpotsTaken[i])//If the spot is free
            {
                if (IsPositionOnScreen(RightSpots[i].position))//If the spot is in screen boundries
                {
                    validSpotsFound++; //Increase the spots found counter
                    if (validSpotsFound >= 1) //Return true if found at least 3 valid spots. (3 because less than that might be problematic)
                        return true;
                }
            }
        }

        return false; //If didn't find enough valid spots, the function returns false
    }

    // Function to find a new spot for the enemy
    public int FindNewSpot(int previousSpotIndex = -1, bool insideArch = false)
    {
        // If it's circle spots, and the previous spot index is valid and taken
        if (previousSpotIndex > -1 && RightSpotsTaken[previousSpotIndex] && !isArches)
        {
            SetFree(previousSpotIndex); // Set the previous spot as free
        }

        int spotIndex = -1; // Initialize spot index

        if (isArches) // If this EnemySpots instance is for arches spots
        {
            if (insideArch) // If looking for an inner arch spot
            {
                // Save the closest index free spot index to the player on the inner arch using GetClosetSpotInInnerArch()
                int insideArchIndex = GetClosetSpotInInnerArch(previousSpotIndex);

                RightSpotsTaken[insideArchIndex] = true; // Set the spot as taken
                return insideArchIndex; // Return the index of the closest inner arch spot
            }
            else // If looking for an outer arch spot
            {
                
                for (int i = 0; i < SecondaryCircleStartIndex; i++) //Loop through the spots of the outer arch (finish loop when getting to the first inner arch index)
                {
                    if (!RightSpotsTaken[i]) // If the spot is free
                    {
                        spotIndex = i; // Set the spot index
                        RightSpotsTaken[i] = true; // Set the spot as taken
                        break; // Exit the loop
                    }
                }
            }
        }
        else // If this EnemySpots instance is for circle spots
        {
            // Loop through all spots
            for (int i = 0; i < RightSpots.Count; i++)
            {
                if (!RightSpotsTaken[i]) // If the spot is free
                {
                    // Check if the spot is within screen boundaries
                    if (IsPositionOnScreen(RightSpots[i].position))
                    {
                        spotIndex = i; // Set the spot index
                        RightSpotsTaken[i] = true; // Set the spot as taken
                        break; // Exit the loop
                    }
                }
            }
        }

        if (spotIndex == -1 && previousSpotIndex > -1) // If no valid spot found, and there was a valid spot index before
        {
            Debug.Log("No valid spot found."); // Log a message indicating no valid spot found
            spotIndex = previousSpotIndex; // Set the spot index to be the same as it was before
            RightSpotsTaken[spotIndex] = true; // Set the spot as taken
        }

        return spotIndex; // Return the index of the chosen spot
    }


    // Function to get the closest free spot index to the player for arches spots
    public int GetClosetSpotInInnerArch(int previousSpotIndex = -1) // We get the previous spot Index, and wether we search a spot on the inner arch
    {
        int closestIndex = -1; //Variable to update the closet spot found so far
        float closestDistance = float.MaxValue; //Variable to update the closest spot distance found so far (Staring at the max float value possible)

        if (!isArches) //If spots are not arches spots, exit the function and return the index we got.
            return previousSpotIndex;

            for (int i = SecondaryCircleStartIndex; i < RightSpots.Count; i++) //Looping from the index of the first inner arch spot to the rest of the inner spots
            {
                if (!RightSpotsTaken[i]) //If the spot is free
                {
                    float distanceFromPlayer = Vector2.Distance(RightSpots[i].position, player.transform.position); //Save spot distance to the player
                    if (distanceFromPlayer < closestDistance) // If its smaller than the closest distance so far
                    {
                        closestDistance = distanceFromPlayer; //Update closestDistance
                        closestIndex = i; //update closestIndex
                    }
                }
            }
            if (closestIndex == -1) //If closestIndex is still -1 (no free spot found)
                return previousSpotIndex; //Return the same spot index as the enemy already had
        

        return closestIndex; //Return the closest index after all the checks
    }

    // Function that gets spot index and sets it free
    public void SetFree(int spotIndex = -2)
    {
        if (spotIndex == -1) //If index is -1, exit the function
            return;

        if (spotIndex == -2) //If index is -2 (default value if no value given), set all the spots free.
        {
            for (int i = 0; i < RightSpotsTaken.Count; i++) //Looping through all the spots
            {
                RightSpotsTaken[i] = false; //Setting a spot free
            }
        }
        else
        {
            RightSpotsTaken[spotIndex] = false; //Else, set the given spot index free
        }
    }
}
