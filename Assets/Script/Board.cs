using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    // score text
    private UnityEngine.UI.Text scoreText;
    private int score;
    
    private bool m_isDotMatched = false;
    private bool m_isPulling = false;

    // Background
    public GameObject backgroundPrefab;
    public static int m_width = 7;
    public static int m_height = 11;
    public GameObject[,] m_slotArray = new GameObject[m_height + 1, m_width];

    // Dots
    public GameObject[] dotType;
    public GameObject[,] m_dotsArray = new GameObject[m_height, m_width];

    // Object Pooling
    private static int m_poolSize = 30;
    private LinkedList<GameObject> m_dotsPool = new LinkedList<GameObject>();
    private Vector3 m_poolPosition = new Vector3(0f, 13f, 0f);

    // dot's moving direction
    public enum Direction { NONE, LEFT, RIGHT, UP, DOWN };

    private void Start()
    {
        scoreText = GameObject.Find("Canvas/Score").GetComponent<UnityEngine.UI.Text>();
        SetUp();
        FindMatch();
    }

    private void Update()
    {
        RemoveDot();
        PullDot();
        FindMatch();
    }

    private void SetUp()
    {
        for (int i = 0; i < m_height + 1; ++i)
        {
            for (int j = 0; j < m_width; ++j)
            {
                // Make Background Array
                Vector3 position = this.transform.position;
                position += new Vector3(j, i, 0);
                m_slotArray[i, j] = Instantiate(backgroundPrefab, position, this.transform.rotation);
                m_slotArray[i, j].transform.name = "BG(" + i + ',' + j + ')';
                m_slotArray[i, j].transform.parent = this.transform;

                // Extra slot for background array
                if (m_height == i)
                    continue;

                // Dots
                int random = Random.Range(0, dotType.Length);
                m_dotsArray[i, j] = Instantiate(dotType[random], position, this.transform.rotation);
                m_dotsArray[i, j].transform.parent = this.transform;
                m_dotsArray[i, j].name = "Dot (" + i + ',' + j + ')';
                m_dotsArray[i, j].GetComponent<Dot>().m_column = i;
                m_dotsArray[i, j].GetComponent<Dot>().m_row = j;
            }
        }

        // Object Pool
        m_poolPosition += this.transform.position;
        for (int i = 0; i < m_poolSize; ++i)
        {
            int random = Random.Range(0, dotType.Length);
            GameObject extraDot = Instantiate(dotType[random], m_poolPosition, this.transform.rotation);
            extraDot.transform.parent = this.transform;
            m_dotsPool.AddLast(extraDot);
        }
    }

    public void FindMatch()
    {
        if (m_isPulling)
            return;

        for (int i = 0; i < m_height; ++i)
        {
            for (int j = 0; j < m_width; ++j)
            {
                // Find Row Match
                if (j > 0 && j < m_width - 1)
                {
                    if ((m_dotsArray[i, j - 1].tag == m_dotsArray[i, j].tag)
                        && (m_dotsArray[i, j].tag == m_dotsArray[i, j + 1].tag)
                        && (m_dotsArray[i, j].tag == m_dotsArray[i, j + 1].tag))
                    {
                        m_dotsArray[i, j - 1].GetComponent<Dot>().m_isMatched = true;
                        m_dotsArray[i, j].GetComponent<Dot>().m_isMatched = true;
                        m_dotsArray[i, j + 1].GetComponent<Dot>().m_isMatched = true;

                        m_isDotMatched = true;
                    }
                }

                // Find Column Match
                if (i > 0 && i < m_height - 1)
                {
                    if ((m_dotsArray[i - 1, j].tag == m_dotsArray[i, j].tag)
                        && (m_dotsArray[i, j].tag == m_dotsArray[i + 1, j].tag))
                    {
                        m_dotsArray[i - 1, j].GetComponent<Dot>().m_isMatched = true;
                        m_dotsArray[i, j].GetComponent<Dot>().m_isMatched = true;
                        m_dotsArray[i + 1, j].GetComponent<Dot>().m_isMatched = true;

                        m_isDotMatched = true;
                    }
                }
            }
        }
    }

    private void RemoveDot()
    {
        for (int i = 0; i < m_height; ++i)
        {
            for (int j = 0; j < m_width; ++j)
            {
                // Dot finished Match and Resize
                if (true == m_dotsArray[i, j].GetComponent<Dot>().m_isStopped)
                {
                    // go to Dot Pool

                    int randomIndex = Random.Range(0, 4);

                    // 4 choices
                    {
                        switch (randomIndex)
                        {
                            case 0:
                                m_dotsPool.AddBefore(m_dotsPool.First, m_dotsArray[i, j]);
                                break;
                            case 1:
                                m_dotsPool.AddAfter(m_dotsPool.First, m_dotsArray[i, j]);
                                break;
                            case 2:
                                m_dotsPool.AddBefore(m_dotsPool.Last, m_dotsArray[i, j]);
                                break;
                            default:
                                m_dotsPool.AddAfter(m_dotsPool.Last, m_dotsArray[i, j]);
                                break;
                        }
                    }
                    m_dotsArray[i, j].SetActive(false);
                    m_dotsArray[i, j] = null;
                }
            }
        }
    }

    private void PullDot()
    {
        // check every column first
        for (int j = 0; j < m_width; ++j)
        {
            for (int i = 0; i < m_height; ++i)
            {
                // if a slot has empty
                if (null == m_dotsArray[i,j])
                {
                    for (int k = i + 1; k < m_height + 1; ++k)
                    {
                        // if a dot in top row is null
                        if (k == m_height)
                        {
                            m_dotsArray[i, j] = m_dotsPool.First.Value;
                            m_dotsArray[i, j].transform.position = m_slotArray[k, j].transform.position;
                            m_dotsArray[i, j].GetComponent<Dot>().m_targetX = m_slotArray[i, j].transform.position.x;
                            m_dotsArray[i, j].GetComponent<Dot>().m_targetY = m_slotArray[i, j].transform.position.y;
                            m_dotsArray[i, j].GetComponent<Dot>().m_column = i;
                            m_dotsArray[i, j].GetComponent<Dot>().m_row = j;
                            m_dotsArray[i, j].SetActive(true);

                            m_dotsArray[i, j].name = "Clone (" + i + ',' + j + ")";

                            m_dotsPool.RemoveFirst();
                            break;
                        }
                        else if (null != m_dotsArray[k, j])
                        {
                            Dot dot = m_dotsArray[k, j].GetComponent<Dot>();
                            FallDownDot(dot, i, j);
                            m_dotsArray[i, j] = m_dotsArray[k, j];
                            m_dotsArray[i, j].name = "Modified (" + i + ',' + j + ")";

                            m_dotsArray[k, j] = null;
                            break;
                        }
                    }
                }
            }
        }
        m_isPulling = false;
    }

    private void FallDownDot(Dot dot, int column, int row)
    {
        dot.m_targetY = m_slotArray[column, row].transform.position.y;
        dot.m_column = column;
    }

    public void ChangeDotlocation(int column, int row, Direction dir)
    {
        Dot dot = m_dotsArray[column, row].GetComponent<Dot>();

        switch (dir)
        {
            case Direction.LEFT:
                dot.m_targetX = m_slotArray[column, row - 1].transform.position.x;
                --(dot.m_row);
                break;
            case Direction.RIGHT:
                dot.m_targetX = m_slotArray[column, row + 1].transform.position.x;
                ++(dot.m_row);
                break;
            case Direction.UP:
                dot.m_targetY = m_slotArray[column + 1, row].transform.position.y;
                ++(dot.m_column);
                break;
            case Direction.DOWN:
                dot.m_targetY = m_slotArray[column - 1, row].transform.position.y;
                --(dot.m_column);
                break;
            default:
                break;
        }
    }

    public void AddScore(int num) {
        score += num;
        scoreText.text = score.ToString();
    }
}