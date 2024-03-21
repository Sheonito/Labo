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
        public TMP_Text deviceResolutionText;
        public TMP_Text processNameText;
        public TMP_Text processRectText;
        public TMP_Text touchDevicePosText;
        public TMP_Text touchProcessPosText;
        public TMP_Text pressedKeyText;
        private static TMP_Text s_DeviceResolutionText;
        private static TMP_Text s_ProcessNameText;
        private static TMP_Text s_ProcessRectText;
        private static TMP_Text s_TouchDevicePosText;
        private static TMP_Text s_TouchProcessPosText;
        private static TMP_Text s_PressedKeyText;
    
        private void Awake()
        {
            s_DeviceResolutionText = deviceResolutionText;
            s_ProcessNameText = processNameText;
            s_ProcessRectText = processRectText;
            s_TouchDevicePosText = touchDevicePosText;
            s_TouchProcessPosText = touchProcessPosText;
            s_PressedKeyText = pressedKeyText;
        }

        public static void Log(LogTextType textType,string log)
        {
            switch (textType)
            {
                case LogTextType.DeviceResolution:
                    s_DeviceResolutionText.text = "DeviceResolution: " + log;
                    break;
                
                case LogTextType.ProcessName:
                    s_ProcessNameText.text = "ProcessName: " + log;
                    break;
                
                case LogTextType.ProcessRect:
                    s_ProcessRectText.text = "ProcessRect: " + log;
                    break;
            
                case LogTextType.TouchDevicePos:
                    s_TouchDevicePosText.text = "TouchDevicePos: " + log;
                    break;
                
                case LogTextType.TouchProcessPos:
                    s_TouchProcessPosText.text = "TouchProcessPos: " + log;
                    break;
                
                case LogTextType.PressedKey:
                    s_PressedKeyText.text = "PressedKey: " + log;
                    break;
            }
        }

        public static void ResetLog()
        {
            s_DeviceResolutionText.text = "DeviceResolution: ";
            s_ProcessNameText.text = "ProcessName: ";
            s_ProcessRectText.text = "ProcessRect: ";
            s_TouchDevicePosText.text = "TouchDevicePos: ";
            s_TouchProcessPosText.text = "TouchProcessPos: ";
            s_PressedKeyText.text = "PressedKey: ";
        }
    }

}

