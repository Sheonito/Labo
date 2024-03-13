using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AutomationQA.Common;
using AutomationQA.Input;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AutomationQA
{
    public class TestPresenter : MonoBehaviour
    {
        [SerializeField] private TestView testView;
        private ProcessUIFactory processUIFactory;
        private ProcessUI curProcess;
        private bool isProcessSelected;
        private bool isRecording;
        private List<MouseData> saveMousePosList = new List<MouseData>();

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            testView.SelectButton.onClick.AddListener(() => SelectProcess().Forget());
            testView.RecordButton.onClick.AddListener(StartRecord);
            testView.StopRecordButton.onClick.AddListener(StopRecord);
        }

        private void OnApplicationQuit()
        {
            WindowInputSystem.UnHook();
        }

        private void Init()
        {
            WindowInputSystem.Init();
            processUIFactory = new ProcessUIFactory(testView.ProcessUIPrefab, testView.AppsScrollRect.content);
            List<ProcessUI> processUiList = processUIFactory.GetProcessUI();
            processUiList.ForEach(processUi => processUi.button.onClick.AddListener(() => curProcess = processUi));
        }


        // 녹화할 프로세스 선택
        // While문 안에 있는 기능들을 함수로 빼서 update문으로 이동시켜야 할듯
        private async UniTaskVoid SelectProcess()
        {
            if (curProcess == null)
                return;

            WindowInputSystem.SetHook(curProcess);

            // 프로세스를 Foreground로 이동
            WindowApi.SetForegroundWindow(curProcess.handle);

            // 프로세스를 최상단으로 이동
            WindowApi.ShowWindow(curProcess.handle, WindowApi.SW_RESTORE);
        }

        private void StartRecord()
        {
            isRecording = true;
            testView.RecordButton.gameObject.SetActive(false);
            testView.StopRecordButton.gameObject.SetActive(true);

            WindowInputSystem.onMouseInput += RecordMouseData;
        }

        private void RecordMouseData()
        {
            RECT curProcessRECT = ResolutionUtil.GetProcessResolutionRECT(curProcess.handle, false);
            curProcess.rect = curProcessRECT;

            Logger.Print(LogTextType.ProcessRect,
                $"Current Process: {curProcess.nameText.text} \nTop: {curProcessRECT.Top}\nBottom: {curProcessRECT.Bottom}\nLeft: {curProcessRECT.Left}\nRight: {curProcessRECT.Right}");
            bool isOnSelectedProcess = WindowInputSystem.IsMouseClickedOnSelectedProcess(curProcess.process.Id);
            if (isOnSelectedProcess)
            {
                if ((WindowInputSystem.processMouseData.x != 0 || WindowInputSystem.processMouseData.y != 0) &&
                    WindowInputSystem.processMouseData.x > 0 &&
                    WindowInputSystem.processMouseData.y > 0)
                {
                    MouseData saveMouseData = new MouseData(WindowInputSystem.processMouseData.x,
                        WindowInputSystem.processMouseData.y, WindowInputSystem.processMouseData.inputState);
                    saveMousePosList.Add(saveMouseData);
                }
            }
            else
            {
                Logger.Print(LogTextType.ProcessMousePos, $"ClientMouseX: No detected\nClientMouseY: No detected");
            }
        }

        private void StopRecord()
        {
            isRecording = false;
            testView.StopRecordButton.gameObject.SetActive(false);
            testView.RecordButton.gameObject.SetActive(true);

            // 로컬에 녹화 데이터 저장하기
            RecordingData recordingData = new RecordingData();
            recordingData.mousePosList = saveMousePosList;
            string saveData = JsonConvert.SerializeObject(recordingData);
            string savePath = Path.Combine(Application.persistentDataPath, "recordingData.txt");
            File.WriteAllText(savePath, saveData);

            WindowInputSystem.onMouseInput -= RecordMouseData;
        }

        // 선택된 녹화할 프로세스 해제
        private void DeSelectProcess()
        {
            WindowInputSystem.UnHook();
        }
    }
}