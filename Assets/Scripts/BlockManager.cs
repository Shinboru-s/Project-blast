using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    [System.Serializable]
    public class DataForGenerate
    {
        public string TypeName;
        public Sprite spriteDefault;
        public Sprite spriteA;
        public Sprite spriteB;
        public Sprite spriteC;
    }

    public class BlockRowAndColumn
    {

        public int row;
        public int column;

        public BlockRowAndColumn(int row, int column)
        {
            this.row = row;
            this.column = column;
        }
    }

    [SerializeField] private int minBlastMemberForA;
    [SerializeField] private int minBlastMemberForB;
    [SerializeField] private int minBlastMemberForC;
    [SerializeField] private DataForGenerate[] dataList;
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private int numberOfColors;

    private int nameNumber = 0;
    private int rowCount;
    private int columnCount;

    private GameObject blockTemp;
    private int randomInt;

    private GameObject[,] blockMatrix;

    //int will storage witch blast list its belongs
    //-1 is for null
    //this array created for quick access and control instead of reaching the Block script every time
    private int[,] blastableList;
    private List<int> uniqueBlastableListNumbers = new List<int>();

    private GameObject previousBlock;
    private int blastListNumberCounter = 0;

    private int[] createdNewBlockCounter;

    void Awake()
    {
        if (numberOfColors > dataList.Length)
            numberOfColors = dataList.Length;
        else if (numberOfColors < 0)
            numberOfColors = 0;
    }

    void Start()
    {
        
    }

    void ResetLists()
    {
        for (int x = 0; x < rowCount; x++)
        {
            for (int y = 0; y < columnCount; y++)
            {
                blastableList[x, y] = -1;
                blockMatrix[x, y].GetComponent<Block>().SetListNumberDefault();
            }
        }
    }

    public void GetRowColumn(int row, int column)
    {
        rowCount = row;
        columnCount = column;
        blockMatrix = new GameObject[rowCount, columnCount];
        blastableList = new int[rowCount, columnCount];
        createdNewBlockCounter = new int[column];
    }

    string[] rowColumn;
    public void CreateRandomBlock(Vector3 blockPosition, string positionInfo)
    {
        blockTemp = Instantiate(blockPrefab, blockPosition, Quaternion.identity);
        blockTemp.name = "Block_" + nameNumber;
        blockTemp.transform.parent = transform;

        randomInt = Random.Range(0, numberOfColors);
        rowColumn = positionInfo.Split('_');

        blockTemp.GetComponent<Block>().blockType = dataList[randomInt].TypeName;
        blockTemp.GetComponent<Block>().SetSprite(dataList[randomInt].spriteDefault);
        blockTemp.GetComponent<Block>().SetRowColumnNumber(int.Parse(rowColumn[0]), int.Parse(rowColumn[1]));
        blockTemp.GetComponent<Block>().SetSortingLayer();

        blockMatrix[int.Parse(rowColumn[0]), int.Parse(rowColumn[1])] = blockTemp;
        blastableList[int.Parse(rowColumn[0]), int.Parse(rowColumn[1])] = -1;
        nameNumber++;

    }

    public void CheckForBlastables()
    {
        ResetLists();

        //check every row from left to right
        for (int x = 0; x < rowCount; x++)
        {
            previousBlock = null;
            for (int y = 0; y < columnCount; y++)
            {
                //turler uyusuyor mu diye kontrol edilir
                if (CompareBlockTypes(blockMatrix[x, y], previousBlock)) 
                {
                    //eger turler uyusuyorsa blocklar bir listeye ait mi diye kontrol edilir
                    CheckIndex(x, y, previousBlock);
                }
                previousBlock = blockMatrix[x, y];
            }
        }

        //check every column from up to down
        for (int y = 0; y < columnCount; y++)
        {
            previousBlock = null;
            for (int x = 0; x < rowCount; x++)
            {
                if (CompareBlockTypes(blockMatrix[x, y], previousBlock))
                {
                    CheckIndex(x, y, previousBlock);
                }
                previousBlock = blockMatrix[x, y];
            }
        }

        //Checking all list numbers for sprite change
        CheckBlastableListsForSprtie();
    }

    bool CompareBlockTypes(GameObject activeBlock, GameObject previousBlock)
    {
        if (previousBlock == null)
            return false;
        else if (activeBlock.GetComponent<Block>().blockType == previousBlock.GetComponent<Block>().blockType)
            return true;
        else
            return false;
    }

    int previousBlockRow;
    int previousBlockColumn;
    void CheckIndex(int activeBlockRow, int activeBlockColumn, GameObject previousBlock)
    {
        previousBlockRow = previousBlock.GetComponent<Block>().rowNumber;
        previousBlockColumn = previousBlock.GetComponent<Block>().columnNumber;

        //this means is activeBlock belongs to any blastableList
        if (blastableList[activeBlockRow, activeBlockColumn] >= 0)
        {
            if (blastableList[previousBlockRow, previousBlockColumn] >= 0)
            {
                //ikisinin de listesi varsa listeleri birlestir
                MergeBlastableLists(blastableList[activeBlockRow, activeBlockColumn], blastableList[previousBlockRow, previousBlockColumn]);
            }
            else
            {
                //active have list but previous dont have list
                //add previous to active's list
                SetListNumber(previousBlockRow, previousBlockColumn, blastableList[activeBlockRow, activeBlockColumn]);
            }
        }
        else if(blastableList[previousBlockRow, previousBlockColumn] >= 0)
        {
            //add active to previous's list
            SetListNumber(activeBlockRow, activeBlockColumn, blastableList[previousBlockRow, previousBlockColumn]);
        }
        else
        {
            //yeni liste numarasi olusturur ve ikisini de o listeye ekler
            CreateBlastableList(activeBlockRow, activeBlockColumn);
        }
    }

    void CreateBlastableList(int activeBlockRow, int activeBlockColumn)
    {
        SetListNumber(activeBlockRow, activeBlockColumn, blastListNumberCounter);
        SetListNumber(previousBlockRow, previousBlockColumn, blastListNumberCounter);

        blastListNumberCounter++;
    }

    void MergeBlastableLists(int oldListNumber, int newListNumber)
    {
        for (int y = 0; y < columnCount; y++)
        {
            for (int x = 0; x < rowCount; x++)
            {
                if (blastableList[x, y] == oldListNumber)
                {
                    SetListNumber(x, y, newListNumber);
                }
            }
        }

    }

    void SetListNumber(int row, int column, int listNumber)
    {
        blockMatrix[row, column].GetComponent<Block>().ChangeListNumber(listNumber);
        blastableList[row, column] = listNumber;
    }

    int blastableListNumberCounter = 0;
    string currentBlockType;
    void CheckBlastableListsForSprtie()
    {
        CheckUniqueListNumbers();
        for (int i = 0; i < uniqueBlastableListNumbers.Count; i++)
        {
            blastableListNumberCounter = 0;
            for (int y = 0; y < columnCount; y++)
            {
                for (int x = 0; x < rowCount; x++)
                {
                    if (uniqueBlastableListNumbers[i] == blastableList[x, y]) 
                    {
                        blastableListNumberCounter++;
                        currentBlockType = blockMatrix[x, y].GetComponent<Block>().blockType;
                    }
                }
            }

            CheckForMembership(blastableListNumberCounter, uniqueBlastableListNumbers[i], currentBlockType);
        }
        
    }

    void CheckUniqueListNumbers()
    {
        uniqueBlastableListNumbers.Clear();

        for (int y = 0; y < columnCount; y++)
        {
            for (int x = 0; x < rowCount; x++)
            {
                if (uniqueBlastableListNumbers.Contains(blastableList[x, y]) == false) 
                {
                    uniqueBlastableListNumbers.Add(blastableList[x, y]);
                }
            }
        }
        
    }

    void CheckForMembership(int listNumberCount, int listNumber, string blockType)
    {
        if (listNumber == -1)
        {
            SetDefaultSprite();
        }
        else if (listNumberCount > minBlastMemberForC)
        {
            SetAllSpritesToNew(listNumber, dataList[FindCorrectSprite(blockType)].spriteC);
        }
        else if(listNumberCount > minBlastMemberForB)
        {
            SetAllSpritesToNew(listNumber, dataList[FindCorrectSprite(blockType)].spriteB);
        }
        else if(listNumberCount > minBlastMemberForA)
        {
            SetAllSpritesToNew(listNumber, dataList[FindCorrectSprite(blockType)].spriteA);
        }
        else
        {
            SetAllSpritesToNew(listNumber, dataList[FindCorrectSprite(blockType)].spriteDefault);
        }
    }
    void SetDefaultSprite()
    {
        for (int y = 0; y < columnCount; y++)
        {
            for (int x = 0; x < rowCount; x++)
            {
                if (blastableList[x, y] == -1)
                {
                    blockMatrix[x, y].GetComponent<Block>().SetSprite(dataList[FindCorrectSprite(blockMatrix[x, y].GetComponent<Block>().blockType)].spriteDefault);
                }
            }
        }
    }
    void SetAllSpritesToNew(int listNumber, Sprite sprite)
    {
        for (int y = 0; y < columnCount; y++)
        {
            for (int x = 0; x < rowCount; x++)
            {
                if (listNumber == blastableList[x, y])
                {
                    blockMatrix[x, y].GetComponent<Block>().SetSprite(sprite);
                }
            }
        }
    }

    int FindCorrectSprite(string blockType)
    {
        for (int i = 0; i < dataList.Length; i++)
        {
            if (dataList[i].TypeName == blockType)
            {
                return i;
            }
        }
        return -1;
    }

    public void BlastBlocks(int row, int column)
    {
        if (blastableList[row, column] >= 0) 
        {

            for (int y = 0; y < columnCount; y++)
            {
                for (int x = 0; x < rowCount; x++)
                {
                    if (blastableList[row, column] == blastableList[x, y])
                    {
                        DestroyBlock(x, y);
                    }
                }
            }
            FillAllGrids();
        }
    }

    void DestroyBlock(int row, int column)
    {
        blockMatrix[row, column].GetComponent<Block>().BlastThisBlock();
        blockMatrix[row, column] = null;
    }

    void FillAllGrids()
    {
        //her bir column tek tek doldurularak gidecek
        for (int y = 0; y < columnCount; y++)
        {
            CheckColumn(y);
        }
        ResetCreatedNewBlockCounter();
        ResetLists();
        CheckForBlastables();
    }
    
    void ResetCreatedNewBlockCounter()
    {
        for (int y = 0; y < createdNewBlockCounter.Length; y++)
        {
            createdNewBlockCounter[y] = 0;
        }
    }

    void CheckColumn(int column)
    {
        BlockRowAndColumn nullblockInfo = FindNullBlock(column);
        if (nullblockInfo != null)
        {
            BlockRowAndColumn foundBlock = FindBlock(column, nullblockInfo.row);
            if (foundBlock != null)
            {
                SendBlockToNullGrid(foundBlock, nullblockInfo);
            }
            else
            {
                GameObject newBlock = CreateNewBlocks(column);
                SendNewBlockToNullGrid(newBlock, nullblockInfo);
            }

            CheckColumn(column);
        }
        else
        {
            Debug.Log(column + " sutununda bosluk kalmadi");
        }
    }


    BlockRowAndColumn FindNullBlock(int column)
    {
        for (int x = rowCount - 1; x >= 0; x--)
        {
            if (blockMatrix[x, column] == null) 
            {
                return new BlockRowAndColumn(x, column);
            }
                
        }
        return null;
    }


    BlockRowAndColumn FindBlock(int column, int searchStartRow)
    {
        for (int x = searchStartRow; x >= 0; x--)
        {
            if (blockMatrix[x, column] != null) 
            {
                return new BlockRowAndColumn(x, column);
            }
        }
        return null;
    }



    void SendBlockToNullGrid(BlockRowAndColumn blockInfo, BlockRowAndColumn nullGridInfo)
    {
        //bos gride blockun gonderilmesi
        GameObject gridGameObject = FindObjectOfType<GridSystem>().GetGridInfo(nullGridInfo.row, nullGridInfo.column);
        blockMatrix[blockInfo.row, blockInfo.column].GetComponent<Block>().MoveBlockToGrid(gridGameObject);

        //null gridin bulunduga kisima block bilgilerinin atanmasi
        blockMatrix[nullGridInfo.row, nullGridInfo.column] = blockMatrix[blockInfo.row, blockInfo.column];
        blockMatrix[nullGridInfo.row, nullGridInfo.column].GetComponent<Block>().SetRowColumnNumber(nullGridInfo.row, nullGridInfo.column);
        blockMatrix[nullGridInfo.row, nullGridInfo.column].GetComponent<Block>().SetSortingLayer();

        //blockun bulundugu kismin nulla atanmasi (cunku artik yeni konumu yukarida verildi)
        blockMatrix[blockInfo.row, blockInfo.column] = null;
    }

    void SendNewBlockToNullGrid(GameObject block, BlockRowAndColumn nullGridInfo)
    {
        blockMatrix[nullGridInfo.row, nullGridInfo.column] = block;
        GameObject gridGameObject = FindObjectOfType<GridSystem>().GetGridInfo(nullGridInfo.row, nullGridInfo.column);
        block.GetComponent<Block>().MoveBlockToGrid(gridGameObject);
        block.GetComponent<Block>().SetRowColumnNumber(nullGridInfo.row, nullGridInfo.column);
        blockMatrix[nullGridInfo.row, nullGridInfo.column].GetComponent<Block>().SetSortingLayer();
    }


    GameObject CreateNewBlocks(int column)
    {
        createdNewBlockCounter[column]++;

        Vector2 blockCreatePosition = FindObjectOfType<GridSystem>().GetGridInfo(0, column).transform.position;
        blockCreatePosition = blockCreatePosition + (Vector2.up * createdNewBlockCounter[column]);

        GameObject newBlockTemp = Instantiate(blockPrefab, blockCreatePosition, Quaternion.identity);
        newBlockTemp.name = "Block_" + nameNumber;
        newBlockTemp.transform.parent = transform;

        randomInt = Random.Range(0, numberOfColors);

        newBlockTemp.GetComponent<Block>().blockType = dataList[randomInt].TypeName;
        newBlockTemp.GetComponent<Block>().SetSprite(dataList[randomInt].spriteDefault);

        nameNumber++;

        return newBlockTemp;
    }

}


