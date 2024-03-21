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
        public float elapsedTime;
        public InputState inputState;
        public InputType inputType;

        public MouseData(float x, float y,InputState inputState,float elapsedTime)
        {
            this.x = x;
            this.y = y;
            this.inputState = inputState;
            this.elapsedTime = elapsedTime;
        }

        public MouseData()
        {
            
        }
    }

    [System.Serializable]
    public class RecordingData
    {
        public List<MouseData> mousePosList;
        public float processWidth;
        public float processHeight;
    }

}

