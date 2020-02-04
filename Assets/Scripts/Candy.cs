using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candy : MonoBehaviour
{
    [Header("Board Variables")]
    public int column;
    public int row;
    public int prevColumn;
    public int prevRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false;
    
    private GameObject otherCandy;
    private Board board;
    private SpriteRenderer spriteRenderer;

    private Vector3 tempPosition;
    private Vector3 firstTouchPosition;
    private Vector3 finalTouchPosition;
    public float swipeAngle = 0;
    public float swipeResist = 1f;

    void Start()
    {
        board = FindObjectOfType<Board>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        targetX = (int)transform.position.x;
        targetY = (int)transform.position.y;
        row = targetY;
        column = targetX;
        prevRow = row;
        prevColumn = column;
    }

    void Update()
    {
        FindMatches();

        if (isMatched)
            spriteRenderer.color = new Color(0f, 0f, 0f, .2f);

        targetX = column;
        targetY = row;

        Move();
    }

    private void Move()
    {
        // Move Towards the target
        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            tempPosition = new Vector3(targetX, transform.position.y, 0);
            transform.position = Vector3.Lerp(transform.position, tempPosition, Time.deltaTime * 10f);
        }
        // Directly set the position
        else
        {
            tempPosition = new Vector3(targetX, transform.position.y, 0);
            transform.position = tempPosition;
            board.allCandies[column, row] = this.gameObject;
        }

        // Move Towards the target
        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            tempPosition = new Vector3(transform.position.x, targetY, 0);
            transform.position = Vector3.Lerp(transform.position, tempPosition, Time.deltaTime * 10f);
        }
        // Directly set the position
        else
        {
            tempPosition = new Vector3(transform.position.x, targetY, 0);
            transform.position = tempPosition;
            board.allCandies[column, row] = this.gameObject;
        }
    }

    public IEnumerator CheckMoveCo()
    {
        yield return new WaitForSeconds(.5f);

        if (null != otherCandy)
        {
            if (!isMatched && !otherCandy.GetComponent<Candy>().isMatched)
            {
                otherCandy.GetComponent<Candy>().row = row;
                otherCandy.GetComponent<Candy>().column = column;
                row = prevRow;
                column = prevColumn;
            }

            otherCandy = null;
        }
    }

    private void OnMouseDown()
    {
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculateAngle();
    }

    private void CalculateAngle()
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist
            || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces();
        }
    }

    private void MovePieces()
    {
        // Right Swipe
        if (-45 < swipeAngle && 45 >= swipeAngle && column < board.m_width - 1)
        {
            prevColumn = column;
            prevRow = row;

            otherCandy = board.allCandies[column + 1, row];
            --(otherCandy.GetComponent<Candy>().column);
            ++column;
        }
        // Up Swipe
        else if(45 < swipeAngle && 135 >= swipeAngle && row < board.m_height - 1)
        {
            prevColumn = column;
            prevRow = row;

            otherCandy = board.allCandies[column, row + 1];
            --(otherCandy.GetComponent<Candy>().row);
            ++row;
        }   
        // Left Swipe
        else if ((135 < swipeAngle || -135 >= swipeAngle) && column > 0)
        {
            prevColumn = column;
            prevRow = row;

            otherCandy = board.allCandies[column - 1, row];
            ++(otherCandy.GetComponent<Candy>().column);
            --column;
        }
        // Down Swipe
        else if ((-45 > swipeAngle && -135 <= swipeAngle) && row > 0)
        {
            prevColumn = column;
            prevRow = row;

            otherCandy = board.allCandies[column, row-1];
            ++(otherCandy.GetComponent<Candy>().row);
            --row;
        }

        StartCoroutine(CheckMoveCo());
    }

    private void FindMatches()
    {
        // column match
        if (0 < column && board.m_width - 1 > column)
        {
            GameObject leftCandy = board.allCandies[column - 1, row];
            GameObject rightCandy = board.allCandies[column + 1, row];

            if (null != leftCandy && null != rightCandy)
            {
                if (leftCandy.tag == gameObject.tag && rightCandy.tag == gameObject.tag)
                {
                    isMatched = true;
                    leftCandy.GetComponent<Candy>().isMatched = true;
                    rightCandy.GetComponent<Candy>().isMatched = true;
                }
            }
        }

        // row match
        if (0 < row && board.m_height - 1 > row)
        {
            GameObject upCandy = board.allCandies[column, row + 1];
            GameObject downCandy = board.allCandies[column, row - 1];

            if (null != upCandy && null != downCandy)
            {
                if (upCandy.tag == gameObject.tag && downCandy.tag == gameObject.tag)
                {
                    isMatched = true;
                    upCandy.GetComponent<Candy>().isMatched = true;
                    downCandy.GetComponent<Candy>().isMatched = true;
                }
            }
        }
    }
}
