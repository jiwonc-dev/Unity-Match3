using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    public RectTransform rect;
    private int m_column;
    private int m_row;
    private int m_type;
    private Enum.Direction m_dir = Enum.Direction.NONE;

    private int m_prevColumn;
    private int m_prevRow;

    private Dot m_prevDot;

    // Target Position
    private float m_targetX;
    private float m_targetY;

    private bool m_rowMatched = false;
    private bool m_colMatched = false;
    public bool m_isSelected = false;
    private bool m_needToMoveX = false;
    private bool m_needToMoveY = false;
    private bool m_isMoving = false;

    private Board board;

    // Touch Position
    private Vector3 m_firstPosition;
    private Vector3 m_lastPosition;

    private float m_angle;
    private float m_swipeResist = 0.5f;

    private const float m_speed = 0.5f;

    private void Start()
    {
        board = GameObject.Find("BoardManager").GetComponent<Board>();
        rect = this.GetComponent<RectTransform>();
    }

    private void Update()
    {
        Move();

        Disappear();

        //board.FindMatch();
    }

    public void OnEnable()
    {
        transform.localScale = new Vector3(1f, 1f, 0);
        rect = this.GetComponent<RectTransform>();

        Vector2 startPos = rect.anchoredPosition;
        m_targetX = startPos.x;
        m_targetY = startPos.y;
    }

    public void OnDisable() {
        m_rowMatched = false;
        m_colMatched = false;
        m_isSelected = false;
        m_needToMoveX = false;
        m_needToMoveY = false;
        m_isMoving = false;
        m_dir = Enum.Direction.NONE;
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

    public int GetRow() {
        return m_row;
    }

    public int GetColumn() {
        return m_column;
    }

    public void RowMatch() {
        m_rowMatched = true;
    }
    public void ColumnMatch() {
        m_colMatched = true;
    }

    public bool IsMoving() {
        return m_isMoving;
    }

    public void SetTargetX(float x) {
        m_targetX = x;
        m_needToMoveX = true;
    }

    public void SetTargetY(float y) {
        m_targetY = y;
        m_needToMoveY = true;
    }

    public void SetRow(int row) {
        m_row = row;
    }
    public void SetColumn(int column) {
        m_column = column;
    }

    private void Move()
    {
        m_isMoving = false;

        Vector2 curPos = rect.anchoredPosition;

        // Move Towards the target
        if (m_needToMoveX && Mathf.Abs(m_targetX - curPos.x) > .1)
            {
                m_isMoving = true;
                Vector2 targetPos = new Vector2(m_targetX, curPos.y);
                rect.anchoredPosition = Vector2.Lerp(curPos, targetPos, Time.deltaTime * m_speed);
            }
            // Directly set the position
        else if (m_needToMoveX)
            {
                Vector2 targetPos = new Vector2(m_targetX, curPos.y);
                rect.anchoredPosition = targetPos;
                m_needToMoveX = false;
            }

        // Move Towards the target

        if ( m_needToMoveY ) {
            string timerID = transform.name;
            board.AddTimer( timerID, 1, () => MoveDown( timerID ));
            m_needToMoveY = false;
        }
    }

    private void MoveDown(string timerID) {
        Vector2 curPos = rect.anchoredPosition;
        if (Mathf.Abs(m_targetY - curPos.y) > .1){
            m_isMoving = true;
            float nextPosY = curPos.y - m_speed;

            if (nextPosY < m_targetY ) {
                nextPosY = m_targetY;
            }
            rect.anchoredPosition = new Vector2(curPos.x, nextPosY);
            Debug.Log("Move - "+transform.name+" curPosY : "+curPos.y+" targetY : "+m_targetY + " nextPosY : "+nextPosY);
        } else {
            Vector2 targetPos = new Vector2(curPos.x, m_targetY);
            rect.anchoredPosition = targetPos;
            board.RemoveTimer(timerID);
        }
    }

    private void Disappear()
    {
        float speed = 2f;
        
        if (IsAnyMatched())
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

    public bool IsAnyMatched() {
        return (m_rowMatched || m_colMatched);
    }

    public bool IsRowMatched() {
        return m_rowMatched;
    }

    public bool IsColMatched() {
        return m_colMatched;
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
        // board.FindMatch();
    }

    private void SwapDot(int row, int column, Enum.Direction dir ) {
        switch (dir){
            case Enum.Direction.LEFT :
                board.ChangeDotLocation(this, row, column, Enum.Direction.LEFT);
                break;
            case Enum.Direction.RIGHT :
                board.ChangeDotLocation(this, row, column, Enum.Direction.RIGHT);
                break;
            case Enum.Direction.UP :
                board.ChangeDotLocation(this, row, column, Enum.Direction.UP);
                break;
            case Enum.Direction.DOWN :
                board.ChangeDotLocation(this, row, column, Enum.Direction.DOWN);
                break;
        }
    }
}