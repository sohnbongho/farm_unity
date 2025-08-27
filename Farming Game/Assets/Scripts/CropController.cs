using System.Collections.Generic;
using UnityEngine;

public class CropController : MonoBehaviour
{
    public static CropController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public enum CropType
    {
        pumpkin,
        lettuce,
        carrot,
        hay,
        potato,
        strawberry,
        tomato,
        avocado
    }

    public List<CropInfo> cropList = new List<CropInfo>();

    public CropInfo GetCropInfo(CropType cropToGet)
    {
        int position = -1;

        for (int i = 0; i < cropList.Count; i++)
        {
            if (cropList[i].cropType == cropToGet)
            {
                position = i;

            }
        }
        if(position >= 0)
        {
            return cropList[position];
        }
        else
        {
            return null;
        }
    }
}

[System.Serializable]
public class CropInfo
{
    public CropController.CropType cropType;
    public Sprite finalCrop, seedType, planted, growStage1, growStage2, ripe;

    public int seedAmount, cropAmount;

}