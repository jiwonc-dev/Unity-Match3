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

    public bool isMatched = false;
    private bool isStopped = false;

    private Transform m_transform;

    private GameObject[,] backgroundArray;
    private GameObject[,] dotsArray;

    // Touch Position
    private Vector3 m_firstPosition;
    private Vector3 m_lastPosition;

    private float m_angle;
    private float m_swipeResist = 0.5f;

    void Start()
    {
        m_transform = GetComponent<Transform>();

        backgroundArray = GameObject.Find("BoardManager").GetComponent<Board>().m_slotArray;
        dotsArray = GameObject.Find("BoardManager").GetComponent<Board>().m_dotsArray;

        m_targetX = backgroundArray[m_column, m_row].transform.position.x;
        m_targetY = backgroundArray[m_column, m_row].transform.position.y;
    }

    void Update()
    {
        Move();

        Disappear();
    }

    void Move()
    {
        // Move Towards the target
        if (Mathf.Abs(m_targetX - transform.position.x) > .1)
        {
            Vector3 targetPos = new Vector3(m_targetX, transform.position.y, 0);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);

            //Debug.Log("My : " + transform.position);
            //Debug.Log("Target : " + targetPos);
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
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);
        }
        // Directly set the position
        else
        {
            Vector3 targetPos = new Vector3(transform.position.x, m_targetY, 0);
            transform.position = targetPos;
            dotsArray[m_column, m_row] = this.gameObject;
        }
    }

    void Disappear()
    {
        if (isMatched && !isStopped)
            m_transform.localScale -= new Vector3(0.5f, 0.5f, 0) * Time.deltaTime;

        if (m_transform.localScale.x < 0.3f)
        {
            // Fix size
            m_transform.localScale = new Vector3(0.29f, 0.29f, 0);
            isStopped = true;
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
        // Right
        if ((-45 < m_angle && 45 >= m_angle) && Board.m_width - 1 > m_row)
        {
            Dot rightDot = dotsArray[m_column, m_row + 1].GetComponent<Dot>();

            m_targetX = backgroundArray[m_column, m_row + 1].transform.position.x;
            rightDot.m_targetX = backgroundArray[m_column, m_row].transform.position.x;

            ++m_row;
            --rightDot.m_row;
        }
        // Up
        else if ((45 < m_angle && 135 >= m_angle) && Board.m_height - 1 > m_column)
        {
            Dot UpDot = dotsArray[m_column + 1, m_row].GetComponent<Dot>();

            m_targetY = backgroundArray[m_column + 1, m_row].transform.position.y;
            UpDot.m_targetY = backgroundArray[m_column, m_row].transform.position.y;

            ++m_column;
            --UpDot.m_column;
        }
        // Left
        else if ((135 < m_angle || -135 >= m_angle) && 0 < m_row)
        {
            Dot leftDot = dotsArray[m_column, m_row - 1].GetComponent<Dot>();

            m_targetX = backgroundArray[m_column, m_row - 1].transform.position.x;
            leftDot.m_targetX = backgroundArray[m_column, m_row].transform.position.x;

            --m_row;
            ++leftDot.m_row;
        }
        // Down
        else if ((-45 > m_angle && -135 <= m_angle) && 0 < m_column)
        {
            Dot DownDot = dotsArray[m_column - 1, m_row].GetComponent<Dot>();

            m_targetY = backgroundArray[m_column - 1, m_row].transform.position.y;
            DownDot.m_targetY = backgroundArray[m_column, m_row].transform.position.y;

            --m_column;
            ++DownDot.m_column;
        }
    }

}
