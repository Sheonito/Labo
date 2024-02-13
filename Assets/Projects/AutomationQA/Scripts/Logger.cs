using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public TMP_Text logText;
    private static TMP_Text s_logText;
    
    private void Awake()
    {
        s_logText = logText;
    }

    public static void Print(string log)
    {
        s_logText.text = log;
    }
}
