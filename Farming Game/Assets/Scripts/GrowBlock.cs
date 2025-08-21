using UnityEngine;
using UnityEngine.InputSystem;

public class GrowBlock : MonoBehaviour
{
    public enum GrowStage
    {
        barren,
        ploughed,
        planted,
        growing1,
        growing2,
        ripe,
    }
    public GrowStage currentStage = GrowStage.barren;
    public SpriteRenderer theSR;
    public Sprite soilTilled;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            AdvanceStage();

            SetSoilSprite();
        }
    }

    public void AdvanceStage()
    {
        currentStage = currentStage + 1;

        if ((int)currentStage >= 6)
        {
            currentStage = GrowStage.barren;
        }
    }

    public void SetSoilSprite()
    {
        if(currentStage == GrowStage.barren)
        {
            theSR.sprite = null;
        }
        else
        {
            theSR.sprite = soilTilled;
        }

    }
}
