using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AutomationQA.Common
{
    public class UniTaskUtil
    {
        public static int ConvertSecToMilSec(float sec)
        {
            int milSec = (int)(sec * 1000);
            return milSec;
        }
    }

}
