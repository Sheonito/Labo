using System;
using System.Collections;
using System.Collections.Generic;
using AutomationQA;
using AutomationQA.Input;
using UnityEngine;

namespace AutomationQA
{
    public class Runner : MonoBehaviour
    {
        private void Update()
        {
            if (WindowInputSystem.GetInputDown(InputType.LeftMouse))
            {
                Vector2 inputPos = WindowInputSystem.GetInputPos(InputType.LeftMouse);
                // Debug.Log("DownInputPos: " + inputPos);
            }
            if (WindowInputSystem.GetInput(InputType.LeftMouse))
            {
                Vector2 inputPos = WindowInputSystem.GetInputPos(InputType.LeftMouse);
                // Debug.Log("StayInputPos: " + inputPos);
            }
            if (WindowInputSystem.GetInputUp(InputType.LeftMouse))
            {
                Vector2 inputPos = WindowInputSystem.GetInputPos(InputType.LeftMouse);
                // Debug.Log("UpInputPos: " + inputPos);
            }
        
        
        }
    }
}


