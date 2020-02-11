using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    // Background
    public GameObject backgroundPrefab;
    public static int m_width = 7;
    public static int m_height = 11;
    public GameObject[,] m_slotArray = new GameObject[m_height, m_width];

    // Dots
    public GameObject[] dotType;
    public GameObject[,] m_dotsArray = new GameObject[m_height, m_width];

    void Start()
    {
        SetUp();
    }

    void Update()
    {
        FindMatch();
    }

    void SetUp()
    {
        for (int i = 0; i < m_height; ++i)
        {
            for (int j = 0; j < m_width; ++j)
            {
                // Make Background Array
                Vector3 position = this.transform.position;
                position += new Vector3(j, i, 0);
                m_slotArray[i, j] = Instantiate(backgroundPrefab, position, this.transform.rotation);
                m_slotArray[i, j].transform.name = "BG(" + i + ',' + j + ')';
                m_slotArray[i, j].transform.parent = this.transform;

                // Dots
                int random = Random.Range(0, dotType.Length);
                m_dotsArray[i, j] = Instantiate(dotType[random], position, this.transform.rotation);
                m_dotsArray[i, j].transform.parent = this.transform;
                m_dotsArray[i, j].name = "Dot (" + i + ',' + j + ')';
                m_dotsArray[i, j].GetComponent<Dot>().m_column = i;
                m_dotsArray[i, j].GetComponent<Dot>().m_row = j;
            }
        }
    }

    void FindMatch()
    {
        for (int i = 0; i < m_height; ++i)
        {
            for (int j = 0; j < m_width; ++j)
            {
                // Find Row Match
                if (j > 0 && j < m_width - 1)
                {
                    if ((m_dotsArray[i, j - 1].tag == m_dotsArray[i, j].tag)
                        && (m_dotsArray[i, j].tag == m_dotsArray[i, j + 1].tag))
                    {
                        m_dotsArray[i, j - 1].GetComponent<Dot>().isMatched = true;
                        m_dotsArray[i, j].GetComponent<Dot>().isMatched = true;
                        m_dotsArray[i, j + 1].GetComponent<Dot>().isMatched = true;
                    }
                }

                // Find Column Match
                if (i > 0 && i < m_height - 1)
                {
                    if ((m_dotsArray[i - 1, j].tag == m_dotsArray[i, j].tag)
                        && (m_dotsArray[i, j].tag == m_dotsArray[i + 1, j].tag))
                    {
                        m_dotsArray[i - 1, j].GetComponent<Dot>().isMatched = true;
                        m_dotsArray[i, j].GetComponent<Dot>().isMatched = true;
                        m_dotsArray[i + 1, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
    }
}
