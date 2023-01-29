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
    public static int m_width = 7;
    public static int m_height = 11;
    public Vector3[,] m_slotPosArr = new Vector3[m_width, m_height +1];

    // Dots
    private Transform dotPanel;
    public GameObject[] dotType;
    public Transform[,] m_dotTransformArr = new Transform[m_width, m_height];
    public Dot[,] m_dotsArray = new Dot[m_width, m_height];

    // Object Pooling
    private GameObjectQueue dotQueue;

    private bool m_isPulling = false;

    private void Start()
    {
        SetUp();
        FindMatch();
    }

    private void Update()
    {
        // PullDot();
        FindMatch();
    }

    private void SetUp()
    {
        // Object Pool
        Transform reuse = this.transform.Find("Reuse");
        dotQueue = new GameObjectQueue();
        dotQueue.Create(dotType, reuse);

        // Score Text
        scoreText = GameObject.Find("Canvas/Score").GetComponent<UnityEngine.UI.Text>();

        slotPanel = this.transform.Find("SlotPanel");
        dotPanel = this.transform.Find("DotPanel");
        
        Vector3 boardPosition = this.transform.position;
        for (int i = 0; i < m_width; ++i)
        {
            for (int j = 0; j <= m_height; ++j)
            {
                // Create Background Array
                Vector3 position = boardPosition + new Vector3(i, j, 0);
                CreateSlot(i, j, position);

                // Extra slot for background array
                if (j == m_height)
                    continue;

                // Create Dots
                CreateDot(i, j, position);
            }
        }
    }

    private void CreateSlot(int row, int column, Vector3 position) 
    {
        GameObject slotObj = Instantiate(backgroundPrefab, position, this.transform.rotation);
        slotObj.name = "BG(" + row + ',' + column + ')';
        slotObj.transform.parent = slotPanel;
        m_slotPosArr[row, column] = position;
    }

    private Dot CreateDot(int row, int column, Vector3 position)
    {
        int randomType = Random.Range(0, dotType.Length);
        GameObject dotObj = dotQueue.GetObjectWithType(randomType, position);
        dotObj.name = "Dot (" + row + ',' + column + ')';
        Transform dotTransform = dotObj.transform;
        dotTransform.parent = dotPanel;
        m_dotTransformArr[row, column] = dotTransform;
        
        Dot dot = dotObj.GetComponent<Dot>();
        dot.Initialize(row, column, randomType);
        m_dotsArray[row, column] = dot;
        return dot;
    }

    public void FindMatch()
    {
        if (m_isPulling) {
            return;
        }
        for (int i = 0; i < m_width; ++i)
        {
            for (int j = 0; j < m_height; ++j)
            {
                Dot dot = m_dotsArray[i, j];
                if (dot == null || dot.IsMoving()) {
                    // if the dot is moving, skip
                    continue;
                }

                int dotType = dot.GetType();
                // Find Row Match
                if (i > 0 && i < m_width - 1 ) {
                    Dot leftDot = GetNeighborDot(i, j, Enum.Direction.LEFT);
                    Dot rightDot = GetNeighborDot(i, j, Enum.Direction.RIGHT);

                    if ( leftDot != null && rightDot != null && 
                    !leftDot.IsMoving() && !rightDot.IsMoving() &&
                    leftDot.GetType() == dotType && rightDot.GetType() == dotType) {
                        // Match!
                        leftDot.Match();
                        dot.Match();
                        rightDot.Match();
                    }
                }

                // Find Column Match
                if ( j > 0 && j < m_height - 1 ) {
                    Dot upDot = GetNeighborDot(i, j, Enum.Direction.UP);
                    Dot downDot = GetNeighborDot(i, j, Enum.Direction.DOWN);

                    if ( upDot != null && downDot != null && !upDot.IsMoving() && !downDot.IsMoving() &&
                    upDot.GetType() == dotType && downDot.GetType() == dotType) {
                        // Match!
                        upDot.Match();
                        dot.Match();
                        downDot.Match();
                    }
                }
            }
        }
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

        // error
        Debug.Log( "error - row : "+row+" column : "+column+" dir : "+neighborDir);
        return null;
    }

    public void RemoveDot(int row, int column, int dotType)
    {
        // go to Dot Pool
        Transform dotTransform = m_dotTransformArr[row, column];
        if (dotTransform != null) {
            dotQueue.HideObject(m_dotTransformArr[row, column], dotType);
        }
        m_dotTransformArr[row, column] = null;
        m_dotsArray[row, column] = null;
    }

    private void SetPosition(int row, int column, Dot dot)
    {
        m_dotTransformArr[row, column] = dot.transform;
        m_dotsArray[row, column] = dot;
    }

    private void PullDot()
    {
        m_isPulling = true;
        // check every column first
        for (int i = 0; i < m_width; ++i)
        {
            for (int j = 0; j < m_height; ++j)
            {
                // if a slot is empty
                if (m_dotsArray[i,j] == null)
                {
                    for (int k = j + 1; k < m_height; ++k)
                    {
                        Debug.Log( "PullDot i : "+i+" j : "+j+" k : "+k);
                        Dot upDot = m_dotsArray[i, k];
                        if (upDot == null) 
                        {
                            // create Dot
                            upDot = CreateDot(i, k, m_slotPosArr[i, m_height]);
                        }

                        // move down
                        ChangeDotLocation(upDot, i, k-1, Enum.Direction.DOWN);
                    }
                }
            }
        }
        m_isPulling = false;
    }

    public void ChangeDotLocation(Dot dot, int row, int column, Enum.Direction dir)
    {
        switch (dir)
        {
            case Enum.Direction.LEFT:
                Debug.Log( "LEFT - row : "+(row-1)+" column : "+column);
                dot.SetTargetX(m_slotPosArr[row - 1, column].x);
                dot.SetRow(row - 1);
                SetPosition(row - 1, column, dot);
                break;
            case Enum.Direction.RIGHT:
                Debug.Log( "RIGHT - row : "+(row+1)+" column : "+column);
                dot.SetTargetX(m_slotPosArr[row + 1, column].x);
                dot.SetRow(row + 1);
                SetPosition(row + 1, column, dot);
                break;
            case Enum.Direction.UP:
                Debug.Log( "UP - row : "+row+" column : "+(column+1));
                dot.SetTargetY(m_slotPosArr[row, column + 1].y);
                dot.SetColumn(column + 1);
                SetPosition(row, column + 1, dot);
                break;
            case Enum.Direction.DOWN:
                Debug.Log( "DOWN - row : "+row+" column : "+(column-1));
                dot.SetTargetY(m_slotPosArr[row, column - 1].y);
                dot.SetColumn(column - 1);
                SetPosition(row, column - 1, dot);
                break;
        }
    }
}