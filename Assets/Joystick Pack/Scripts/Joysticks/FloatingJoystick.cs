using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FloatingJoystick : Joystick
{

    Vector3 startPos;
    Player player;
    protected override void Start()
    {
        base.Start();
        startPos = background.anchoredPosition;
        player = FindObjectOfType<Player>();
       
        background.gameObject.SetActive(false);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        
        background.gameObject.SetActive(true);
        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        base.OnPointerDown(eventData);

    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (player.joystickDecoy)
            Destroy(player.joystickDecoy);
        background.anchoredPosition = Vector3.zero;
        base.OnPointerUp(eventData);
        background.anchoredPosition = startPos;
    }



    public static void ResetJoystickPosition()
    {
        FloatingJoystick joystick = FindObjectOfType<FloatingJoystick>();
        joystick.input = Vector2.zero;
        joystick.handle.anchoredPosition = Vector3.zero;
        joystick.background.anchoredPosition = joystick.startPos;
    }


}