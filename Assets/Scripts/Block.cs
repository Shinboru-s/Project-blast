using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public string blockType;
    public int rowNumber;
    public int columnNumber;
    public int blastableListNumber = -1;

    private SpriteRenderer spriteRenderer;
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void SetRowColumnNumber(int row, int column)
    {
        rowNumber = row;
        columnNumber = column;
    }

    public void SetSprite(Sprite newSprite)
    {
        spriteRenderer.sprite = newSprite;
    }

    public void SetListNumberDefault()
    {
        blastableListNumber = -1;
    }
    public void ChangeListNumber(int number)
    {
        blastableListNumber = number;
    }
    public void SetSortingLayer()
    {
        spriteRenderer.sortingOrder = rowNumber * -1;

    }
}
