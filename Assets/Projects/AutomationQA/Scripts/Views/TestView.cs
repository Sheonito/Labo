using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AutomationQA
{
    public class TestView : MonoBehaviour
    {
        [SerializeField] private ScrollRect appsScrollRect;
        [SerializeField] private ProcessUI processUIPrefab;
        [SerializeField] private Button selectButton;
        [SerializeField] private Button recordButton;
        [SerializeField] private Button stopRecordButton;
        [SerializeField] private Button playButton;

        public ScrollRect AppsScrollRect => appsScrollRect;
        public ProcessUI ProcessUIPrefab => processUIPrefab;
        public Button SelectButton => selectButton;
        public Button RecordButton => recordButton;
        public Button StopRecordButton => stopRecordButton;
        public Button PlayButton => playButton;
    }

}
