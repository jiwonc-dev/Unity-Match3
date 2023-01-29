using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    // Managed by Board;
    public int m_column;
    public int m_row;
    public Board.Direction m_dir;

    private int m_prevColumn;
    private int m_prevRow;
    private Board.Direction m_prevDir;

    // Target Position
    public float m_targetX;
    public float m_targetY;

    public bool m_isMatched = false;
    public bool m_isSelected = false;
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
        m_dir = Board.Direction.NONE;
    }

    private void Update()
    {
        Move();

        Disappear();
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

            if (m_isSelected && !IsMatched() && m_prevDir != Board.Direction.NONE) {
                m_isSelected = false;
                ChangeDotLocation(m_column, m_row, m_prevDir);
            }
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
            if (m_isSelected && !IsMatched() && m_prevDir != Board.Direction.NONE) {
                m_isSelected = false;
                ChangeDotLocation(m_column, m_row, m_prevDir);
            }
        }
    }

    private void Disappear()
    {
        float speed = 2f;
        
        if (m_isMatched && !m_isStopped)
            transform.localScale -= new Vector3(0.5f, 0.5f, 0) * Time.deltaTime * speed;

        if (transform.localScale.x < 0.3f)
        {
            // fix size
            transform.localScale = new Vector3(0.3f, 0.3f, 0);

            // init variables
            m_isStopped = true;
            m_isMatched = false;
            m_isSelected = false;
            m_prevColumn = m_column;
            m_prevRow = m_row;
            m_prevDir = Board.Direction.NONE;
            board.AddScore(1);
        }
    }

    private bool IsMatched() {
        if (m_isMatched) {
            Debug.Log( "(matched) m_colum : "+m_column+"m_row : "+m_row );
            return true;
        }
        Dot prevDot = dotsArray[m_prevColumn, m_prevRow].GetComponent<Dot>();
        if (prevDot.m_isMatched) {
            Debug.Log( "(matched) m_colum : "+m_column+"m_row : "+m_row+ " m_prevColumn : "+m_prevColumn+" m_prevRow : "+m_prevRow );
        } else {
            Debug.Log( "(not matched) m_colum : "+m_column+"m_row : "+m_row+ " m_prevColumn : "+m_prevColumn+" m_prevRow : "+m_prevRow );
        }
        return prevDot.m_isMatched;
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
        m_prevColumn = m_column;
        m_prevRow = m_row;
        m_isSelected = true;
        
        if ((-45 < m_angle && 45 >= m_angle) && Board.m_width - 1 > m_row)
        {
            m_prevDir = Board.Direction.LEFT;
            m_dir = Board.Direction.RIGHT;
        }
        else if ((45 < m_angle && 135 >= m_angle) && Board.m_height - 1 > m_column)
        {
            m_prevDir = Board.Direction.DOWN;
            m_dir = Board.Direction.UP;
        }
        else if ((135 < m_angle || -135 >= m_angle) && 0 < m_row)
        {
            m_prevDir = Board.Direction.RIGHT;
            m_dir = Board.Direction.LEFT;
        }
        else if ((-45 > m_angle && -135 <= m_angle) && 0 < m_column)
        {
            m_prevDir = Board.Direction.UP;
            m_dir = Board.Direction.DOWN;
        }
        ChangeDotLocation(m_column, m_row, m_dir);
    }

    private void ChangeDotLocation(int column, int row, Board.Direction dir ) {
        switch (dir){
            case Board.Direction.LEFT : 
                board.ChangeDotlocation(column, row, Board.Direction.LEFT);
                board.ChangeDotlocation(column, row - 1, Board.Direction.RIGHT);
                break;
            case Board.Direction.RIGHT :
                board.ChangeDotlocation(column, row, Board.Direction.RIGHT);
                board.ChangeDotlocation(column, row + 1, Board.Direction.LEFT);
                break;
            case Board.Direction.UP :
                board.ChangeDotlocation(column, row, Board.Direction.UP);
                board.ChangeDotlocation(column + 1, row, Board.Direction.DOWN);
                break;
            case Board.Direction.DOWN :
                board.ChangeDotlocation(column, row, Board.Direction.DOWN);
                board.ChangeDotlocation(column - 1, row, Board.Direction.UP);
                break;
        }
    }
}