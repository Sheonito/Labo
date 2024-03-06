using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace AutomationQA
{
    public class Logger : MonoBehaviour
    {
        public TMP_Text processRectText;
        private static TMP_Text s_ProcessRectText;
        [FormerlySerializedAs("keyboardInputText")] public TMP_Text processMouseText;
        private static TMP_Text s_ProcessMouseText;
        public TMP_Text syncMousePosText;
        private static TMP_Text s_SyncMousePos;
    
        private void Awake()
        {
            s_ProcessRectText = processRectText;
            s_ProcessMouseText = processMouseText;
            s_SyncMousePos = syncMousePosText;
        }

        public static void Print(LogTextType textType,string log)
        {
            switch (textType)
            {
                case LogTextType.ProcessRect:
                    s_ProcessRectText.text = log;
                    break;
            
                case LogTextType.ProcessMousePos:
                    s_ProcessMouseText.text = log;
                    break;
                
                case LogTextType.SyncMousePos:
                    s_SyncMousePos.text = log;
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
            
                case LogTextType.ProcessMousePos:
                    s_ProcessMouseText.text += log;
                    break;
                
                case LogTextType.SyncMousePos:
                    s_SyncMousePos.text += log;
                    break;
            }
        
        }
    }

}

