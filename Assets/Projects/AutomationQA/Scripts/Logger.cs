using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using LogTextType = Define.LogTextType;

public class Logger : MonoBehaviour
{
    public TMP_Text processRectText;
    private static TMP_Text s_ProcessRectText;
    public TMP_Text keyboardInputText;
    private static TMP_Text s_keyboardInputText;
    
    private void Awake()
    {
        s_ProcessRectText = processRectText;
        s_keyboardInputText = keyboardInputText;
    }

    public static void Print(LogTextType textType,string log)
    {
        switch (textType)
        {
            case LogTextType.ProcessRect:
                s_ProcessRectText.text = log;
                break;
            
            case LogTextType.KeyboardInput:
                s_keyboardInputText.text = log;
                break;
        }
        
    }

    public static void Append(LogTextType textType,string log)
    {
        switch (textType)
        {
            case LogTextType.ProcessRect:
                s_ProcessRectText.text += log;
                break;
            
            case LogTextType.KeyboardInput:
                s_keyboardInputText.text += log;
                break;
        }
        
    }
}
