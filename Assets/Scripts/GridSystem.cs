using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [SerializeField] private int rowCount;
    [SerializeField] private int columnCount;
    [SerializeField] private GameObject gridObjectPrefab;
    [SerializeField] private GameObject gridObjectTemp;

    void Start()
    {
        CreateGrid(rowCount, columnCount);
        Debug.Log(columnCount / 2.0f);
    }


    void Update()
    {
        
    }

    public void CreateGrid(int _rowCount, int _columnCount)
    {

        for (int x = 0; x < _columnCount; x++)
        {
            for (int y = 0; y < _rowCount; y++)
            {
                gridObjectTemp = Instantiate(gridObjectPrefab, GetWorldPosition(x, y), Quaternion.identity);
                gridObjectTemp.name = y + "_" + x;
                gridObjectTemp.transform.parent = transform;

                FindObjectOfType<BlockManager>().CreateRandomBlock(GetWorldPosition(x, y), gridObjectTemp.name);

            }
        }
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(transform.position.x - columnCount / 2.0f + x + 0.5f, transform.position.y + rowCount / 2.0f - y - 0.5f);
    }
}
