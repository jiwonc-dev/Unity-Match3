using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    private int m_column;
    private int m_row;
    private int m_type;
    private Enum.Direction m_dir;

    private int m_prevColumn;
    private int m_prevRow;

    private Dot m_prevDot;

    // Target Position
    private float m_targetX;
    private float m_targetY;

    private bool m_isMatched = false;
    public bool m_isSelected = false;
    private bool m_isMoving = false;

    private Board board;

    // Touch Position
    private Vector3 m_firstPosition;
    private Vector3 m_lastPosition;

    private float m_angle;
    private float m_swipeResist = 0.5f;

    private const float m_speed = 5f;

    private void Start()
    {
        board = GameObject.Find("BoardManager").GetComponent<Board>();

        m_targetX = transform.position.x;
        m_targetY = transform.position.y;
        m_dir = Enum.Direction.NONE;
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
        m_isMoving = false;
        transform.localScale = new Vector3(1f, 1f, 0);
    }
    public void Initialize(int row, int column, int type)
    {
        m_row = row;
        m_column = column;
        m_type = type;
    }

    public int GetType() {
        return m_type;
    }

    public void Match() {
        m_isMatched = true;
    }

    public bool IsMoving() {
        return m_isMoving;
    }

    public void SetTargetX(float x) {
        m_targetX = x;
    }

    public void SetTargetY(float y) {
        m_targetY = y;
    }

    public void SetRow(int row) {
        m_row = row;
    }
    public void SetColumn(int column) {
        m_column = column;
    }
    // public int GetRow() {
    //     return m_row;
    // }
    // public int GetColumn() {
    //     return m_column;
    // }


    private void Move()
    {
        m_isMoving = false;

        // Move Towards the target
        if (Mathf.Abs(m_targetX - transform.position.x) > .1)
        {
            m_isMoving = true;
            Vector3 targetPos = new Vector3(m_targetX, transform.position.y, 0);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * m_speed);
        }
        // Directly set the position
        else
        {
            Vector3 targetPos = new Vector3(m_targetX, transform.position.y, 0);
            transform.position = targetPos;

            // if (m_isSelected && !IsMatched() && m_prevDir != Enum.Direction.NONE) {
            //     m_isSelected = false;
            //     ChangeDotLocation(m_column, m_row, m_prevDir);
            // }
        }

        // Move Towards the target
        if (Mathf.Abs(m_targetY - transform.position.y) > .1)
        {
            m_isMoving = true;
            Vector3 targetPos = new Vector3(transform.position.x, m_targetY, 0);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * m_speed);
        }
        // Directly set the position
        else
        {
            Vector3 targetPos = new Vector3(transform.position.x, m_targetY, 0);
            transform.position = targetPos;

            // if (m_isSelected && !IsMatched() && m_prevDir != Enum.Direction.NONE) {
            //     m_isSelected = false;
            //     ChangeDotLocation(m_column, m_row, m_prevDir);
            // }
        }
    }

    private void Disappear()
    {
        float speed = 2f;
        
        if (m_isMatched)
            transform.localScale -= new Vector3(0.5f, 0.5f, 0) * Time.deltaTime * speed;

        if (transform.localScale.x < 0.3f)
        {
            // fix size
            transform.localScale = new Vector3(0.3f, 0.3f, 0);

            // release()
            board.AddScore();
            board.RemoveDot(m_row, m_column, m_type);
        }
    }

    private bool IsMatched() {
        if (m_isMatched) {
            Debug.Log( "(matched) m_colum : "+m_column+"m_row : "+m_row );
            return true;
        }
        if (m_prevDot.IsMatched()) {
            Debug.Log( "(matched) m_colum : "+m_column+"m_row : "+m_row+ " m_prevColumn : "+m_prevColumn+" m_prevRow : "+m_prevRow );
        } else {
            Debug.Log( "(not matched) m_colum : "+m_column+"m_row : "+m_row+ " m_prevColumn : "+m_prevColumn+" m_prevRow : "+m_prevRow );
        }
        return m_prevDot.IsMatched();
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
            m_dir = Enum.Direction.RIGHT;
        }
        // Up
        else if ((45 < m_angle && 135 >= m_angle) && Board.m_height - 1 > m_column)
        {
            m_dir = Enum.Direction.UP;
        }
        // Left
        else if ((135 < m_angle || -135 >= m_angle) && 0 < m_row)
        {
            m_dir = Enum.Direction.LEFT;
        }
        // Down
        else if ((-45 > m_angle && -135 <= m_angle) && 0 < m_column)
        {
            m_dir = Enum.Direction.DOWN;
        } 
        else 
        {
            return;
        }
        SwapDot(m_row, m_column, m_dir);
    }

    private void SwapDot(int row, int column, Enum.Direction dir ) {
        switch (dir){
            case Enum.Direction.LEFT :
                board.ChangeDotLocation(this, row, column, Enum.Direction.LEFT);

                m_prevDot = board.GetNeighborDot(row, column, Enum.Direction.LEFT);
                board.ChangeDotLocation(m_prevDot, row-1, column, Enum.Direction.RIGHT);
                break;
            case Enum.Direction.RIGHT :
                board.ChangeDotLocation(this, row, column, Enum.Direction.RIGHT);

                m_prevDot = board.GetNeighborDot(row, column, Enum.Direction.RIGHT);
                board.ChangeDotLocation(m_prevDot, row+1, column, Enum.Direction.LEFT);
                break;
            case Enum.Direction.UP :
                board.ChangeDotLocation(this, row, column, Enum.Direction.UP);

                m_prevDot = board.GetNeighborDot(row, column, Enum.Direction.UP);
                board.ChangeDotLocation(m_prevDot, row, column+1, Enum.Direction.DOWN);
                break;
            case Enum.Direction.DOWN :
                board.ChangeDotLocation(this, row, column, Enum.Direction.DOWN);

                m_prevDot = board.GetNeighborDot(row, column, Enum.Direction.DOWN);
                board.ChangeDotLocation(m_prevDot, row, column-1, Enum.Direction.UP);
                break;
        }
    }
}