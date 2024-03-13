using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutomationQA
{
    public class ProcessUI : MonoBehaviour
    {
        public Process process;
        public TMP_Text nameText;
        public Button button;
        public IntPtr handle;
        public RECT rect;
    }
}
