using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public static GridController instance;
    private void Awake()
    {
        instance = this;
    }

    public Transform minPoint, maxPoint;
    public GrowBlock baseGridBlock;

    private Vector2Int gridSize;

    public List<BlockRow> blockRows = new List<BlockRow>();

    public LayerMask gridBlockers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateGrid();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void GenerateGrid()
    {
        minPoint.position = new Vector3(
            Mathf.Round(minPoint.position.x),
            Mathf.Round(minPoint.position.y),
            0f);

        maxPoint.position = new Vector3(
            Mathf.Round(maxPoint.position.x),
            Mathf.Round(maxPoint.position.y),
            0f);

        //Vector3 startpoint = minPoint.position + new Vector3(.5f, .5f, 0f);
        Vector3 startpoint = minPoint.position + new Vector3(1f, 1f, 0f);

        // ���� ���纻�� ����
        //Instantiate(baseGridBlock, startpoint, Quaternion.identity);

        gridSize = new Vector2Int(
            Mathf.RoundToInt(maxPoint.position.x - minPoint.position.x),
            Mathf.RoundToInt(maxPoint.position.y - minPoint.position.y));

        for (int y = 0; y < gridSize.y; y++)
        {
            blockRows.Add(new BlockRow());

            for (int x = 0; x < gridSize.x; x++)
            {
                GrowBlock newBlock = Instantiate(baseGridBlock, startpoint + new Vector3(x, y, 0f), Quaternion.identity);
                
                newBlock.transform.SetParent(transform);
                newBlock.theSR.sprite = null;

                newBlock.SetGridPosition(x, y);

                blockRows[y].blocks.Add(newBlock);

                if (Physics2D.OverlapBox(newBlock.transform.position, new Vector2(.9f, .9f), 0f, gridBlockers))
                {
                    newBlock.theSR.sprite = null;
                    newBlock.preventUse = true;
                }
            }
        }

        if(GridInfo.instance.hasGrid == false)
        {
            GridInfo.instance.CreateGrid();
        }

        baseGridBlock.gameObject.SetActive(false);
    }

    public GrowBlock GetBlock(float x, float y)
    {
        x = Mathf.RoundToInt(x);
        y = Mathf.RoundToInt(y);

        x -= minPoint.position.x;
        y -= minPoint.position.y;

        int intX = Mathf.RoundToInt(x);
        int intY = Mathf.RoundToInt(y);

        if(intX < gridSize.x && intY < gridSize.y)
        {
            return blockRows[intY].blocks[intX];
        }

        return null;
    }
}

[System.Serializable]
public class BlockRow
{

    public List<GrowBlock> blocks = new List<GrowBlock>();

}