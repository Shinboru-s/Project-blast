using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [SerializeField] private int rowCount;
    [SerializeField] private int columnCount;
    [SerializeField] private GameObject gridObjectPrefab;
    [SerializeField] private GameObject gridObjectTemp;

    private BlockManager blockManagerScript;
    private GameObject[,] gridMatrix;

    void Awake()
    {
        blockManagerScript = FindObjectOfType<BlockManager>();

        blockManagerScript.GetRowColumn(rowCount, columnCount);
        gridMatrix = new GameObject[rowCount, columnCount];
    }
    void Start()
    {
        CreateGrid(rowCount, columnCount);
    }

    void Update()
    {
        //yeni blocklar olusturulurken yok etme olmamali
        if (Input.GetButtonDown("Fire1"))
        {
            SendRayToGrid();
        }
    }

    void CreateGrid(int _rowCount, int _columnCount)
    {

        for (int x = 0; x < _columnCount; x++)
        {
            for (int y = 0; y < _rowCount; y++)
            {
                
                gridObjectTemp = Instantiate(gridObjectPrefab, GetWorldPosition(x, y), Quaternion.identity);
                gridObjectTemp.name = y + "_" + x;
                gridObjectTemp.transform.parent = transform;
                gridMatrix[y, x] = gridObjectTemp;
                blockManagerScript.CreateRandomBlock(GetWorldPosition(x, y), gridObjectTemp.name);

            }
        }
        FindObjectOfType<BlockManager>().CheckForBlastables();

    }

    Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(transform.position.x - columnCount / 2.0f + x + 0.5f, transform.position.y + rowCount / 2.0f - y - 0.5f);
    }

    string[] rowColumn;
    void SendRayToGrid()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null && hit.collider.CompareTag("Grid")) 
        {
            rowColumn = hit.collider.gameObject.name.Split('_');
            Debug.Log("Blast this row: " + rowColumn[0] + " column: " + rowColumn[1]);
            FindObjectOfType<BlockManager>().BlastBlocks(int.Parse(rowColumn[0]), int.Parse(rowColumn[1]));
        }
    }

    public GameObject GetGridInfo(int row,int column)
    {

        Debug.Log("istenilen row: " + row + " istenilen column: " + column + ""+gridMatrix[row, column].transform.position);
        return gridMatrix[row, column];
    }
}
