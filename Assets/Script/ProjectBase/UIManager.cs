using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // UIManager 跟着 Canvas 走，不 DontDestroyOnLoad
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
