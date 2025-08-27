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
    public Sprite soilTilled, soilWaterd;

    public SpriteRenderer cropSR;
    public Sprite cropPlanted, cropGrowing1, cropGrowing2, cropRipe;

    public bool isWatered;
    public bool preventUse;

    private Vector2Int gridPosition;
    public CropController.CropType cropType;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //if (Keyboard.current.eKey.wasPressedThisFrame)
        //{
        //    AdvanceStage();

//    SetSoilSprite();
//}
#if UNITY_EDITOR
        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            AdvanceCrop();
        }
#endif
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
        if (currentStage == GrowStage.barren)
        {
            theSR.sprite = null;
        }
        else
        {
            if (isWatered == true)
            {
                theSR.sprite = soilWaterd;
            }
            else
            {
                theSR.sprite = soilTilled;
            }

        }

        UpdateGridInfo();
    }
    public void PloughSoil()
    {
        if (currentStage == GrowStage.barren && preventUse == false)
        {
            currentStage = GrowStage.ploughed;

            SetSoilSprite();
        }
    }
    public void WaterSoil()
    {
        if(preventUse == false)
        {
            isWatered = true;
            SetSoilSprite();
        }        

    }

    public void PlantCrop(CropController.CropType cropToPlant)
    {
        if (currentStage == GrowStage.ploughed && isWatered == true && preventUse == false)
        {
            currentStage = GrowStage.planted;

            cropType = cropToPlant;

            UpdateCropSprite();
        }
    }

    public void UpdateCropSprite()
    {
        CropInfo activeCrop = CropController.instance.GetCropInfo(cropType);
        switch (currentStage)
        {
            case GrowStage.planted:
                //cropSR.sprite = cropPlanted;
                cropSR.sprite = activeCrop.planted;
                break;
            case GrowStage.growing1:
                //cropSR.sprite = cropGrowing1;
                cropSR.sprite = activeCrop.growStage1;
                break;
            case GrowStage.growing2:
                //cropSR.sprite = cropGrowing2;
                cropSR.sprite = activeCrop.growStage2;
                break;
            case GrowStage.ripe:
                //cropSR.sprite = cropRipe;
                cropSR.sprite = activeCrop.ripe;
                break;                
        }
        UpdateGridInfo();

    }
    public void AdvanceCrop()
    {
        if (isWatered == true && preventUse == false)
        {
            if (currentStage == GrowStage.planted ||
                currentStage == GrowStage.growing1 ||
                currentStage == GrowStage.growing2)
            {
                currentStage++;

                isWatered = false;
                SetSoilSprite();
                UpdateCropSprite();
            }
        }
    }

    public void HarvestCrop()
    {
        if(currentStage == GrowStage.ripe && preventUse == false)
        {
            currentStage = GrowStage.ploughed;

            SetSoilSprite();
            
            cropSR.sprite = null;
        }
    }
    public void SetGridPosition(int x, int y)
    {
        gridPosition = new Vector2Int(x, y);

    }

    void UpdateGridInfo()
    {
        GridInfo.instance.UpdateInfo(this, gridPosition.x, gridPosition.y);
    }
}
