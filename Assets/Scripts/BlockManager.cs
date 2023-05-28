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

    public void GetRowColumn(int row, int column)
    {
        rowCount = row;
        columnCount = column;
        blockMatrix = new GameObject[rowCount, columnCount];
        blastableList = new int[rowCount, columnCount];
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
                if (blastableList[x, y] >= 0 && uniqueBlastableListNumbers.Contains(blastableList[x, y]) == false) 
                {
                    uniqueBlastableListNumbers.Add(blastableList[x, y]);
                }
            }
        }
        
    }

    void CheckForMembership(int listNumberCount, int listNumber, string blockType)
    {
        if (listNumberCount > minBlastMemberForC)
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

}


