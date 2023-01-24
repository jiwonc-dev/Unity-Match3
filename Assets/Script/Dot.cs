using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    // Managed by Board;
    public int m_column;
    public int m_row;

    // Target Position
    public float m_targetX;
    public float m_targetY;

    public bool m_isMatched = false;
    public bool m_isStopped = false;

    private Board board;
    private GameObject[,] dotsArray;

    // Touch Position
    private Vector3 m_firstPosition;
    private Vector3 m_lastPosition;

    private float m_angle;
    private float m_swipeResist = 0.5f;

    private void Start()
    {
        board = GameObject.Find("BoardManager").GetComponent<Board>();
        dotsArray = board.m_dotsArray;

        m_targetX = transform.position.x;
        m_targetY = transform.position.y;

    }

    private void Update()
    {
        Move();

        Disappear();

        //board.FindMatch();
    }

    public void OnEnable()
    {
        m_isMatched = false;
        m_isStopped = false;
        transform.localScale = new Vector3(1f, 1f, 0);
    }

    private void Move()
    {
        float speed = 5f;

        // Move Towards the target
        if (Mathf.Abs(m_targetX - transform.position.x) > .1)
        {
            Vector3 targetPos = new Vector3(m_targetX, transform.position.y, 0);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);
        }
        // Directly set the position
        else
        {
            Vector3 targetPos = new Vector3(m_targetX, transform.position.y, 0);
            transform.position = targetPos;
            dotsArray[m_column, m_row] = this.gameObject;
        }

        // Move Towards the target
        if (Mathf.Abs(m_targetY - transform.position.y) > .1)
        {
            Vector3 targetPos = new Vector3(transform.position.x, m_targetY, 0);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);
        }
        // Directly set the position
        else
        {
            Vector3 targetPos = new Vector3(transform.position.x, m_targetY, 0);
            transform.position = targetPos;
            dotsArray[m_column, m_row] = this.gameObject;
        }
    }

    private void Disappear()
    {
        float speed = 2f;
        
        if (m_isMatched && !m_isStopped)
            transform.localScale -= new Vector3(0.5f, 0.5f, 0) * Time.deltaTime * speed;

        if (transform.localScale.x < 0.3f)
        {
            // Fix size
            transform.localScale = new Vector3(0.29f, 0.29f, 0);
            m_isStopped = true;
        }
    }

    private void OnMouseDown()
    {
        m_firstPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        m_lastPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        CalculateAngle();
    }

    private void CalculateAngle()
    {
        if (Mathf.Abs(m_lastPosition.y - m_firstPosition.y) > m_swipeResist
            || Mathf.Abs(m_lastPosition.x - m_firstPosition.x) > m_swipeResist)
        {
            // Polar Coordinates
            m_angle = Mathf.Atan2(m_lastPosition.y - m_firstPosition.y, m_lastPosition.x - m_firstPosition.x) * 180 / Mathf.PI;

            SetTarget();
        }
    }

    private void SetTarget()
    {
        int prevColumn = m_column;
        int prevRow = m_row;
        
        // Right
        if ((-45 < m_angle && 45 >= m_angle) && Board.m_width - 1 > m_row)
        {
            board.ChangeDotlocation(prevColumn, prevRow, Board.Direction.RIGHT);
            board.ChangeDotlocation(prevColumn, prevRow + 1, Board.Direction.LEFT);
        }
        // Up
        else if ((45 < m_angle && 135 >= m_angle) && Board.m_height - 1 > m_column)
        {
            board.ChangeDotlocation(prevColumn, prevRow, Board.Direction.UP);
            board.ChangeDotlocation(prevColumn + 1, prevRow, Board.Direction.DOWN);
        }
        // Left
        else if ((135 < m_angle || -135 >= m_angle) && 0 < m_row)
        {
            board.ChangeDotlocation(prevColumn, prevRow, Board.Direction.LEFT);
            board.ChangeDotlocation(prevColumn, prevRow - 1, Board.Direction.RIGHT);
        }
        // Down
        else if ((-45 > m_angle && -135 <= m_angle) && 0 < m_column)
        {
            board.ChangeDotlocation(prevColumn, prevRow, Board.Direction.DOWN);
            board.ChangeDotlocation(prevColumn - 1, prevRow, Board.Direction.UP);
        }
    }
}