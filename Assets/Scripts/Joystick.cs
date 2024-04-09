//using UnityEngine;
//using UnityEngine.EventSystems;

//public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
//{
//    public RectTransform joystickBackground;
//    public RectTransform joystickHandle;
//    public Vector2 joystickPosition;

//    public bool isDragging = false; // Flag to indicate if the joystick is being dragged

//    void Start()
//    {
//        joystickBackground = GetComponent<RectTransform>();
//        joystickPosition = joystickHandle.anchoredPosition;
//    }

//    public void OnPointerDown(PointerEventData eventData)
//    {
//        isDragging = true; // Set the flag to true when the joystick is pressed
//        OnDrag(eventData);
//    }

//    public void OnPointerUp(PointerEventData eventData)
//    {
//        isDragging = false; // Set the flag to false when the joystick is released
//        joystickHandle.anchoredPosition = Vector2.zero;
//        joystickPosition = Vector2.zero;
//    }

 

   

//    public void OnDrag(PointerEventData eventData)
//    {
//        Vector2 direction = eventData.position - (Vector2)joystickBackground.position;
//        joystickHandle.anchoredPosition = Vector2.ClampMagnitude(direction, joystickBackground.sizeDelta.x * 0.5f);

//        float x = joystickHandle.anchoredPosition.x / (joystickBackground.sizeDelta.x * 0.5f);
//        float y = joystickHandle.anchoredPosition.y / (joystickBackground.sizeDelta.y * 0.5f);
//        joystickPosition = new Vector2(x, y);
//    }

  

//}
