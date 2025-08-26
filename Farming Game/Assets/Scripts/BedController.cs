using UnityEngine;
using UnityEngine.InputSystem;

public class BedController : MonoBehaviour
{
    private bool canSleep;

    // Update is called once per frame
    void Update()
    {
        if (canSleep == true)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame || 
                Keyboard.current.spaceKey.wasPressedThisFrame || 
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                GridInfo.instance.GrowCrop();
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            canSleep = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            canSleep = false;
        }
    }

}
