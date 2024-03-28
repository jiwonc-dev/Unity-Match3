using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Board : MonoBehaviour
{
    // Score text
    private UnityEngine.UI.Text scoreText;
    private int score;

    // Background (for position of dots)
    public GameObject backgroundPrefab;
    private Transform slotPanel;
    // public static int m_width = 7;
    public static int m_width = 4;
    // public static int m_height = 11;
    public static int m_height = 4;
    public Vector3[,] m_slotPosArr = new Vector3[m_width, m_height +1];

    // Dots
    private Transform dotPanel;
    public GameObject[] dotType;
    public RectTransform[,] m_dotTransformArr = new RectTransform[m_width, m_height];
    public Dot[,] m_dotsArray = new Dot[m_width, m_height];

    // Object Pooling
    private GameObjectQueue dotQueue;

    // Timer
    private Timer timer;

    private bool m_isPulling = false;
    private bool m_needPull = false;

    private void Start()
    {
        SetUp();
        FindMatch();
    }

    private void Update()
    {
        UpdateTimer();
        PullDot();
        // FindMatch();
    }

    private void SetUp()
    {
        // Object Pool
        Transform reuse = this.transform.Find("Reuse");
        dotQueue = new GameObjectQueue();
        dotQueue.Create(dotType, reuse);

        // Timer
        timer = new Timer();

        // Set variables
        scoreText = GameObject.Find("Canvas/Score").GetComponent<UnityEngine.UI.Text>();
        slotPanel = this.transform.Find("SlotPanel");
        dotPanel = this.transform.Find("DotPanel");

        // Create Slots and Dots
        Vector2 boardPosition = this.GetComponent<RectTransform>().anchoredPosition ;
        for (int col = 0; col <= m_height; ++col)
        {
            for (int row = 0; row < m_width; ++row)
            {
                Vector2 position = boardPosition + new Vector2(row, col);

                // Create Background Array
                CreateSlot(row, col, position);

                // Extra slot for background array
                if (col == m_height)
                    continue;

                // Create Dots
                CreateDot(row, col, position);
            }
        }
    }

    private void CreateSlot(int row, int column, Vector2 position) 
    {
        GameObject slotObj = Instantiate(backgroundPrefab, position, this.transform.rotation);
        slotObj.name = "BG(" + row + ',' + column + ')';
        slotObj.transform.SetParent(slotPanel);
        m_slotPosArr[row, column] = position;
    }

    private Dot CreateDot(int row, int column, Vector2 position)
    {
        int randomType = Random.Range(0, dotType.Length);
        GameObject dotObj = dotQueue.GetObjectWithType(randomType, position);
        dotObj.name = "Dot (" + row + ',' + column + ')';
        dotObj.transform.SetParent(dotPanel);
        
        Dot dot = dotObj.GetComponent<Dot>();
        dot.Initialize(row, column, randomType);
        m_dotsArray[row, column] = dot;
        m_dotTransformArr[row, column] = dot.rect;
        return dot;
    }

    public void FindMatch()
    {
        if (m_isPulling) {
            return;
        }
        for (int col = 0; col < m_height; ++col)
        {
            for (int row = 0; row < m_width; ++row)
            {
                Dot dot = m_dotsArray[row, col];
                if (!dot || dot.IsMoving()) {
                    // if the dot is moving, skip
                    continue;
                }

                int dotType = dot.GetType();
                // Find Row Match
                Dot rightDot = GetNeighborDot(row, col, Enum.Direction.RIGHT);
                if (rightDot && !(rightDot.IsRowMatched())) {
                    List<Dot> rowMatchList = new List<Dot>();
                    rowMatchList.Add(dot);
                    // Debug.Log( "colMatchList.Add() dot : "+row+ ", type : "+dotType);
                    while (rightDot && !rightDot.IsMoving() && rightDot.GetType() == dotType) {
                        int rightDotRow = rightDot.GetRow();
                        // Debug.Log( "colMatchList.Add() rightColumn : "+rightDotRow+ ", type : "+rightDot.GetType());
                        rowMatchList.Add(rightDot);
                        rightDot = GetNeighborDot(rightDotRow, col, Enum.Direction.RIGHT);
                    }
                    if (rowMatchList.Count >= 3) {
                        foreach (Dot matchedDot in rowMatchList) {
                            // Debug.Log( "cur Row Match : []"+ matchedDot.GetRow()+ ", "+matchedDot.GetColumn()+ "]");
                            matchedDot.RowMatch();
                        }
                    }
                }

                // Find Column Match
                Dot upDot = GetNeighborDot(row, col, Enum.Direction.UP);
                if (upDot && !(upDot.IsColMatched())) {
                    List<Dot> colMatchList = new List<Dot>();
                    colMatchList.Add(dot);
                    while (upDot != null && !upDot.IsMoving() && upDot.GetType() == dotType) {
                        int upDotColumn = upDot.GetColumn();
                        colMatchList.Add(upDot);
                        upDot = GetNeighborDot(row, upDotColumn,  Enum.Direction.UP);
                    }
                    if (colMatchList.Count >= 3) {
                        foreach (Dot matchedDot in colMatchList) {
                            matchedDot.ColumnMatch();
                        }
                    }
                }
            }
        }
    }

    public void AddScore() 
    {
        score++;
        scoreText.text = score.ToString();
    }

    public Dot GetNeighborDot( int row, int column, Enum.Direction neighborDir ) {
        switch (neighborDir) {
            case Enum.Direction.LEFT : 
                if (row > 0) {
                    return m_dotsArray[row - 1, column];
                }
                break;
            case Enum.Direction.RIGHT :
                if (row < m_width - 1) {
                    return m_dotsArray[row+1, column];
                }
                break;
            case Enum.Direction.UP :
                if (column < m_height - 1) {
                    return m_dotsArray[row, column+1];
                }
                break;
            case Enum.Direction.DOWN :
                if (column > 0) {
                    return m_dotsArray[row, column-1];
                }
                break;
        }

        return null;
    }

    public void RemoveDot(int row, int column, int dotType)
    {
        // go to Dot Pool
        RectTransform dotTransform = m_dotTransformArr[row, column];
        if (dotTransform != null) {
            dotQueue.HideObject(m_dotTransformArr[row, column], dotType);
        }
        m_dotTransformArr[row, column] = null;
        m_dotsArray[row, column] = null;
        m_needPull = true;
    }

    private void SetPosition(int row, int column, Dot dot)
    {
        m_dotTransformArr[row, column] = dot.rect;
        m_dotsArray[row, column] = dot;
    }

    private void PullDot()
    {
        if (m_isPulling || !m_needPull) {
            return;
        }
        m_isPulling = true;

        // check every column first
        for (int i = 0; i < m_width; ++i)
        {
            for (int j = 0; j < m_height; ++j)
            {
                // if a slot is empty
                if (m_dotsArray[i,j] == null)
                {
                    int newJ = j;
                    for (int k = j + 1; k <= m_height; ++k)
                    {
                        Dot upDot = null;
                        bool breaktheloop = false;
                        if (k >= m_height - 1) {
                            // create Dot
                            upDot = CreateDot(i, newJ, m_slotPosArr[i, m_height]);
                            breaktheloop = true;
                        } else {
                            upDot = m_dotsArray[i, k];
                        }

                        if (upDot != null) {
                            upDot.transform.name = "Dot ("+i+","+newJ+") new";
                            upDot.SetTargetY(m_slotPosArr[i, newJ].y);
                            upDot.SetColumn(newJ);
                            SetPosition(i, newJ, upDot);

                            newJ++;
                        }
                        if (breaktheloop) {
                            break;
                        }
                    }
                }
            }
        }
        m_isPulling = false;
        m_needPull = false;
    }

    public void ChangeDotLocation(Dot dot, int row, int column, Enum.Direction dir)
    {
        switch (dir)
        {
            case Enum.Direction.LEFT:
                dot.SetTargetX(m_slotPosArr[row - 1, column].x);
                dot.SetRow(row - 1);
                SetPosition(row - 1, column, dot);
                break;
            case Enum.Direction.RIGHT:
                dot.SetTargetX(m_slotPosArr[row + 1, column].x);
                dot.SetRow(row + 1);
                SetPosition(row + 1, column, dot);
                break;
            case Enum.Direction.UP:
                dot.SetTargetY(m_slotPosArr[row, column + 1].y);
                dot.SetColumn(column + 1);
                SetPosition(row, column + 1, dot);
                break;
            case Enum.Direction.DOWN:
                dot.SetTargetY(m_slotPosArr[row, column - 1].y);
                dot.SetColumn(column - 1);
                SetPosition(row, column - 1, dot);
                break;
        }
    }

    public void AddTimer(string timerID, float durationSec, System.Action timeF)
    {
        timer.AddTimer(timerID, durationSec, timeF);
    }

    public void RemoveTimer(string timerID)
    {
        timer.RemoveTimer(timerID);
    }

    private void UpdateTimer()
    {
        timer.Update();
    }
}