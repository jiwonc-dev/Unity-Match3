using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Tile : MonoBehaviour
{
    // Properties
    public int m_ID;


    // methods
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnMouseDown()
    {
        Vector3 punch = new Vector3(0.1f, 0.1f);
        transform.DOPunchScale(punch, 1f);
    }
}
