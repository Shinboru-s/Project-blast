using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    //A class for holding sprites that different for colors
    [System.Serializable]
    public class DataForGenerate
    {
        public string TypeName;
        public Sprite spriteDefault;
        public Sprite spriteA;
        public Sprite spriteB;
        public Sprite spriteC;
    }

    //class to hold row and column information of blocks or grids
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

    //a array holding the data of the blocks in the scene
    //because it is a matrix, we can find blocks with row and column
    private GameObject[,] blockMatrix;

    private GameObject[,] blocksToMove;

    //int will storage witch blast list its belongs
    //-1 is means it doesnt belong to any list
    //this array created for quick access and control instead of reaching the Block script every time
    private int[,] blastableList;

    private List<int> uniqueBlastableListNumbers = new List<int>();

    private GameObject previousBlock;
    private int blastListNumberCounter = 0;

    private int[] createdNewBlockCounter;

    //to block operations like sprite changes during moving 
    private List<string> movingBlocks = new List<string>();
    public bool isWorkInProgress = false;


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

    //unlist all blocks
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

    #region Progress Check
    public void AddToList(string nameOfBlock)
    {
        movingBlocks.Add(nameOfBlock);
        CheckProgress();
    }
    public void RemoveFromList(string nameOfBlock)
    {
        movingBlocks.Remove(nameOfBlock);
        CheckProgress();
    }
    void CheckProgress()
    {
        if (movingBlocks.Count == 0)
        {
            isWorkInProgress = false;
        }
        else
        {
            isWorkInProgress = true;
        }
    }
    #endregion 



    //this method works from GridSystem to get total row and column count
    public void GetRowColumn(int row, int column)
    {
        rowCount = row;
        columnCount = column;
        blockMatrix = new GameObject[rowCount, columnCount];
        blocksToMove = new GameObject[rowCount, columnCount];
        blastableList = new int[rowCount, columnCount];
        createdNewBlockCounter = new int[column];
    }

    //also works from GridSystem to create random block on given position
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



    #region Blastable Check
    public IEnumerator CheckForBlastables()
    {
        while (isWorkInProgress==true)
        {
            yield return null;
        }

        ResetLists();

        //check every row from left to right
        for (int x = 0; x < rowCount; x++)
        {
            previousBlock = null;
            for (int y = 0; y < columnCount; y++)
            {
                //checking types of two block
                if (CompareBlockTypes(blockMatrix[x, y], previousBlock)) 
                {
                    //checking list number for blocks have any list
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

        //Checking all list numbers for sprite change if needed
        CheckBlastableListsForSprtie();

        //checking for deadlock to shuffle
        DeadlockCheck();
        
    }

    //checking types of two block
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
    //checking list number for blocks have any list
    void CheckIndex(int activeBlockRow, int activeBlockColumn, GameObject previousBlock)
    {
        previousBlockRow = previousBlock.GetComponent<Block>().rowNumber;
        previousBlockColumn = previousBlock.GetComponent<Block>().columnNumber;

        //this means is activeBlock belongs to any blastableList
        if (blastableList[activeBlockRow, activeBlockColumn] >= 0)
        {
            //previousBlock belongs to any blastableList
            if (blastableList[previousBlockRow, previousBlockColumn] >= 0)
            {
                //merge two list to get one list
                MergeBlastableLists(blastableList[activeBlockRow, activeBlockColumn], blastableList[previousBlockRow, previousBlockColumn]);
            }
            else
            {
                //active have list but previous dont have list
                //add previous to active's list
                SetListNumber(previousBlockRow, previousBlockColumn, blastableList[activeBlockRow, activeBlockColumn]);
            }
        }
        //previousBlock belongs to any blastableList
        else if (blastableList[previousBlockRow, previousBlockColumn] >= 0)
        {
            //add active to previous's list
            SetListNumber(activeBlockRow, activeBlockColumn, blastableList[previousBlockRow, previousBlockColumn]);
        }
        //neither block belongs to any blastableList
        else
        {
            //create new list and add both block
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

    #endregion

    #region Sprite Check
    int blastableListNumberCounter = 0;
    string currentBlockType;
    void CheckBlastableListsForSprtie()
    {
        //find out how many different lists there are
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
                        //count how many block with same list number
                        blastableListNumberCounter++;

                        //find type of block with same list number
                        currentBlockType = blockMatrix[x, y].GetComponent<Block>().blockType;
                    }
                }
            }

            //find which sprite to apply
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

    //apply default sprite to all blocks that dont belong to any list
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

    //apply selected sprite to all blocks with same list number
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

    //find sprite from block data list
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

    #endregion

    #region Blast blocks and fill grids

    //blast block at specified positon if block have any list number
    //if block have any list number this means stands with at least one other block of the same color
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
                        //destroy block with same list number other blocks
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
        //control each column separately
        for (int y = 0; y < columnCount; y++)
        {
            CheckColumn(y);
        }
        ResetCreatedNewBlockCounter();
        ResetLists();
        StartCoroutine(CheckForBlastables());
    }
    
    void ResetCreatedNewBlockCounter()
    {
        for (int y = 0; y < createdNewBlockCounter.Length; y++)
        {
            //to count how many blocks created in each column
            createdNewBlockCounter[y] = 0;
        }
    }

    void CheckColumn(int column)
    {
        //find null block in column if there is
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
        //else
        //{
        //    Debug.Log(column + ". column is complete");
        //}
    }

    //find null block in column if there is
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

    //are there any block top of null grid
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
        //send block to null grid
        GameObject gridGameObject = FindObjectOfType<GridSystem>().GetGridInfo(nullGridInfo.row, nullGridInfo.column);
        blockMatrix[blockInfo.row, blockInfo.column].GetComponent<Block>().MoveBlockToGrid(gridGameObject);

        //assign block information to the null grid
        blockMatrix[nullGridInfo.row, nullGridInfo.column] = blockMatrix[blockInfo.row, blockInfo.column];
        blockMatrix[nullGridInfo.row, nullGridInfo.column].GetComponent<Block>().SetRowColumnNumber(nullGridInfo.row, nullGridInfo.column);


        //assign the old block info to null (because new location is assigned)
        blockMatrix[blockInfo.row, blockInfo.column] = null;

    }

    //same like SendBlockToNullGrid method but its for newly created block
    void SendNewBlockToNullGrid(GameObject block, BlockRowAndColumn nullGridInfo)
    {
        blockMatrix[nullGridInfo.row, nullGridInfo.column] = block;
        GameObject gridGameObject = FindObjectOfType<GridSystem>().GetGridInfo(nullGridInfo.row, nullGridInfo.column);
        block.GetComponent<Block>().MoveBlockToGrid(gridGameObject);
        block.GetComponent<Block>().SetRowColumnNumber(nullGridInfo.row, nullGridInfo.column);

    }

    //create new block for null grids
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
    #endregion


    bool deadlockSolveStarted = false;
    void DeadlockCheck()
    {
        if(deadlockSolveStarted == false)
        {
            CheckUniqueListNumbers();

            //this means there is one unique number and none of the blocks belong to any list
            if (uniqueBlastableListNumbers.Count == 1 && uniqueBlastableListNumbers[0] == -1)
            {
                RandomizeArray();
            }
        }
        
    }

   
    void RandomizeArray()
    {
        deadlockSolveStarted = true;
        //a list to temporarily store all blocks
        List<GameObject> tempList = new List<GameObject>();

        //add all blocks of the original array to list
        for (int x = 0; x < rowCount; x++)
        {
            for (int y = 0; y < columnCount; y++)
            {
                tempList.Add(blockMatrix[x, y]);
            }
        }

        //send blocks in random locations
        for (int x = 0; x < rowCount; x++)
        {
            for (int y = 0; y < columnCount; y++)
            {
                int randomIndex = Random.Range(0, tempList.Count);

                BlockRowAndColumn nullGridInfo = new BlockRowAndColumn(x, y);

                SendNewBlockToNullGrid(tempList[randomIndex], nullGridInfo);

                tempList.RemoveAt(randomIndex);
            }
        }

        deadlockSolveStarted = false;
        StartCoroutine(CheckForBlastables());
    }
}


