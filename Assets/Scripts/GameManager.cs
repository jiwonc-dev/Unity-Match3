using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    // Properties
    public static GameManager m_instance;


    // Methods
    void Awake()
    {
        // Singleton
        if (null == m_instance)
            m_instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        DOTween.Init();
    }

    void Update()
    {

    }
}
