using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AutomationQA
{
    [System.Serializable]
    public class MouseData
    {
        public float x;
        public float y;
        public InputState inputState;
        public InputType inputType;

        public MouseData(float x, float y,InputState inputState)
        {
            this.x = x;
            this.y = y;
            this.inputState = inputState;
        }

        public MouseData()
        {
            
        }
    }

    [System.Serializable]
    public class RecordingData
    {
        public List<MouseData> mousePosList;
    }

}

