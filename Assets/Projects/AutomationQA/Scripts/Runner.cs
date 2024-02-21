using System;
using System.Collections;
using System.Collections.Generic;
using AutomationQA;
using AutomationQA.Input;
using UnityEngine;

public class Runner : MonoBehaviour
{
    private void Update()
    {
        if (InputDetector.GetInputDown(InputType.LeftMouse))
        {
            Vector2 inputPos = InputDetector.GetInputPos(InputType.LeftMouse);
            // Debug.Log("DownInputPos: " + inputPos);
        }
        if (InputDetector.GetInput(InputType.LeftMouse))
        {
            Vector2 inputPos = InputDetector.GetInputPos(InputType.LeftMouse);
            // Debug.Log("StayInputPos: " + inputPos);
        }
        if (InputDetector.GetInputUp(InputType.LeftMouse))
        {
            Vector2 inputPos = InputDetector.GetInputPos(InputType.LeftMouse);
            // Debug.Log("UpInputPos: " + inputPos);
        }
    }
}
