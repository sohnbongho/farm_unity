using System.Collections.Specialized;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public Transform minPoint, maxPoint;
    public GrowBlock baseGridBlock;

    private Vector2Int gridSize;

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

        // 블럭의 복사본을 생성
        //Instantiate(baseGridBlock, startpoint, Quaternion.identity);

        gridSize = new Vector2Int(
            Mathf.RoundToInt(maxPoint.position.x - minPoint.position.x),
            Mathf.RoundToInt(maxPoint.position.y - minPoint.position.y));

    }
}
