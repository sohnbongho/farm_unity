using UnityEngine;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AdvanceStage();

        AdvanceStage();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AdvanceStage()
    {
        currentStage = currentStage + 6;

        if((int)currentStage >= 6)
        {
            currentStage = GrowStage.barren;
        }
    }
}
