// Debug playerspeed when played in build
using UnityEngine;

public class DEBUG_SPEED : MonoBehaviour
{
    public PlayerController my_PlayerController_script;
    private int numberPressed;
    
    // Put keycodes in an array to make them easier to track within a for-loop.
    private KeyCode[] keyCodes = {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
        KeyCode.Alpha0,
    };

    void LateUpdate()
    {
        for (int i = 0; i < keyCodes.Length; i++)
        {
            if (Input.GetKeyDown(keyCodes[i]))
            {
                numberPressed = i + 1;
            }
        }
        
        switch (numberPressed)
        {
            case 1:
                my_PlayerController_script.maxMovement = 5;
                break;
            case 2: my_PlayerController_script.maxMovement = 10;
                break;
            case 3: my_PlayerController_script.maxMovement = 15;
                break;
            case 4: my_PlayerController_script.maxMovement = 20;
                break;
            case 5: my_PlayerController_script.maxMovement = 25;
                break;
            case 6: my_PlayerController_script.maxMovement = 30;
                break;
            case 7: my_PlayerController_script.maxMovement = 35;
                break;
            case 8: my_PlayerController_script.maxMovement = 40;
                break;
            case 9: my_PlayerController_script.maxMovement = 45;
                break;
            case 10: my_PlayerController_script.maxMovement = 50;
                break;
        }
    }
}
