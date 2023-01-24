using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int m_width;
    public int m_height;
    private BackgroundTile[,] allTiles;

    public GameObject tilePrefab;
    public GameObject[] candies;
    public GameObject[,] allCandies;

    public GameObject reuse;
    private GameObjectQueue candyQueue;

    void Start()
    {
        allTiles = new BackgroundTile[m_width, m_height];
        allCandies = new GameObject[m_width, m_height];

        candyQueue = new GameObjectQueue();
        candyQueue.Create(candies, reuse);

        SetUp();
    }

    private void SetUp()
    {
        for (int i = 0; i < m_width; ++i)
        {
            for (int j = 0; j < m_height; ++j)
            {
                Vector3 tempPosition = new Vector3(i, j, 0);
                GameObject backgroundTile  = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "( " + i + ", " + j + " )";

                int candyToUse = Random.Range(0, candies.Length);
                int maxIteration = 0;
                while (MatchesAt(i, j, candies[candyToUse]) && maxIteration < 100)
                {
                    candyToUse = Random.Range(0, candies.Length);
                    ++maxIteration;
                }
                maxIteration = 0;

                GameObject candy = candyQueue.GetObjectWithType(candyToUse, tempPosition);
                candy.transform.parent = this.transform;
                candy.name = "( " + i + ", " + j + " )";

                allCandies[i, j] = candy;
            }
        }
    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {
        if (1 < column && 1 < row)
        {
            if ((allCandies[column - 1, row].tag == piece.tag && allCandies[column - 2, row].tag == piece.tag)
                || (allCandies[column, row - 1].tag == piece.tag && allCandies[column, row - 2].tag == piece.tag))
                return true;
        }
        else if (1 >= column || 1 >= row)
        {
            // first column (0, n)
            if (row > 1)
            {
                if (allCandies[column, row - 1].tag == piece.tag && allCandies[column, row - 2].tag == piece.tag)
                    return true;
            }
            // first row (n, 0)
            else if (column > 1)
            {
                if (allCandies[column - 1, row].tag == piece.tag && allCandies[column-2, row].tag == piece.tag)
                    return true;
            }
        }
        return false;
    }
}