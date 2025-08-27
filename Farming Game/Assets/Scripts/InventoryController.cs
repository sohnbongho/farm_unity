using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public void OpenClose()
    {
        if(gameObject.activeSelf == false)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

}
