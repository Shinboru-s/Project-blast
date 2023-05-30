using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [SerializeField] private int rowCount;
    [SerializeField] private int columnCount;
    [SerializeField] private GameObject gridObjectPrefab;
    [SerializeField] private GameObject gridObjectTemp;
    [SerializeField] private GameObject spriteMask;

    private BlockManager blockManagerScript;
    private GameObject[,] gridMatrix;

    [Header("Borders")]
    [SerializeField] private GameObject topLeft;
    [SerializeField] private GameObject top;
    [SerializeField] private GameObject topRight;
    [SerializeField] private GameObject right;
    [SerializeField] private GameObject downRight;
    [SerializeField] private GameObject down;
    [SerializeField] private GameObject downLeft;
    [SerializeField] private GameObject left;

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
        if (Input.GetButtonDown("Fire1"))
        {
            if (FindObjectOfType<BlockManager>().isWorkInProgress == false)
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

                FindBorder(y, x);
            }
        }
        StartCoroutine(FindObjectOfType<BlockManager>().CheckForBlastables());

        spriteMask.transform.position = new Vector2(0, (_rowCount / 2f) + 0.15f);
        spriteMask.transform.localScale = new Vector2(columnCount * 2, rowCount);
    }

    Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(transform.position.x - columnCount / 2.0f + x + 0.5f, transform.position.y + rowCount / 2.0f - y - 0.5f);
    }

    void FindBorder(int row, int column)
    {
        //ilk satir
        if (row == 0)
        {
            //sol ust
            if (column == 0)
            {
                Instantiate(topLeft, GetWorldPosition(column, row), Quaternion.identity);
            }
            //sag ust
            else if(column==columnCount-1)
            {
                Instantiate(topRight, GetWorldPosition(column, row), Quaternion.identity);
            }
            else
            {
                Instantiate(top, GetWorldPosition(column, row), Quaternion.identity);
            }
        }
        //en alt satir
        else if (row == rowCount - 1)
        {
            //sol alt
            if (column == 0)
            {
                Instantiate(downLeft, GetWorldPosition(column, row), Quaternion.identity);
            }
            //sag alt
            else if (column == columnCount - 1)
            {
                Instantiate(downRight, GetWorldPosition(column, row), Quaternion.identity);
            }
            else
            {
                Instantiate(down, GetWorldPosition(column, row), Quaternion.identity);
            }
        }
        //sol sutun
        else if (column == 0)
        {
            Instantiate(left, GetWorldPosition(column, row), Quaternion.identity);
        }
        //sag sutun
        else if (column == columnCount - 1)
        {
            Instantiate(right, GetWorldPosition(column, row), Quaternion.identity);
        }
    }

    string[] rowColumn;
    //check if there is a grid there when clicking anywhere
    void SendRayToGrid()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null && hit.collider.CompareTag("Grid")) 
        {
            rowColumn = hit.collider.gameObject.name.Split('_');
            FindObjectOfType<BlockManager>().BlastBlocks(int.Parse(rowColumn[0]), int.Parse(rowColumn[1]));
        }
    }

    public GameObject GetGridInfo(int row,int column)
    {
        return gridMatrix[row, column];
    }
}
