using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    [System.Serializable]
    public class BlockData
    {
        public string TypeName;
        public Sprite spriteDefault;
        public Sprite spriteA;
        public Sprite spriteB;
        public Sprite spriteC;
    }

    [SerializeField] private BlockData[] dataList;
    [SerializeField] private GameObject blockPrefab;

    private GameObject blockTemp;
    private int randomInt;
     
    public void CreateRandomBlock(Vector3 blockPosition,string blockName)
    {
        blockTemp = Instantiate(blockPrefab, blockPosition, Quaternion.identity);
        blockTemp.name = blockName;
        blockTemp.transform.parent = transform;

        randomInt = Random.Range(0, dataList.Length);

        string[] rowColumn = blockName.Split('_');

        blockTemp.GetComponent<Block>().blockType = dataList[randomInt].TypeName;
        blockTemp.GetComponent<Block>().rowNumber = rowColumn[0];
        blockTemp.GetComponent<Block>().columnNumber = rowColumn[1];
        blockTemp.GetComponent<SpriteRenderer>().sprite = dataList[randomInt].spriteDefault;
        blockTemp.GetComponent<SpriteRenderer>().sortingOrder = int.Parse(rowColumn[0]) * -1;

    }

    public void CheckForBlastables()
    {

    }
}


