using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Block : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float destroySpeed;
    [SerializeField] float dropAnimationSpeed;
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
    public void BlastThisBlock()
    {
        StartCoroutine(Destroy(destroySpeed));
    }

    Coroutine moveCorutine;
    public void MoveBlockToGrid(GameObject grid)
    {
        if (moveCorutine != null) 
            StopCoroutine(moveCorutine);

        moveCorutine = StartCoroutine(MoveToPosition(grid.transform.position, moveSpeed));
    }
    private IEnumerator MoveToPosition(Vector3 position,float moveSpeed)
    {
        FindObjectOfType<BlockManager>().AddToList(name);

        yield return new WaitForSeconds(destroySpeed);
        SetSortingLayer();
        float time = Vector2.Distance(transform.position, position) / moveSpeed;
        transform.DOMove(position, time);

        StartCoroutine(DropAnimation(dropAnimationSpeed));

        
    }

    private IEnumerator Destroy(float time)
    {
        Vector3 startingScale = transform.localScale;
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            transform.localScale = Vector3.Lerp(startingScale, new Vector3(0f, 0f, 0f), (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    private IEnumerator DropAnimation(float time)
    {
        Vector3 startingScale = transform.localScale;
        Vector3 squishyScale = new Vector3(transform.localScale.x * 1.2f, transform.localScale.y * 0.8f, transform.localScale.z);

        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            transform.localScale = Vector3.Lerp(startingScale, squishyScale, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0;
        while (elapsedTime < (time / 2)) 
        {
            transform.localScale = Vector3.Lerp(squishyScale, startingScale, (elapsedTime / (time / 2)));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = startingScale;
        FindObjectOfType<BlockManager>().RemoveFromList(name);
    }
}
