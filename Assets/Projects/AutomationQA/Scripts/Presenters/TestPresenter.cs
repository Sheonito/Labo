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
        private string savePath;

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            testView.SelectButton.onClick.AddListener(() => SelectProcess().Forget());
            testView.RecordButton.onClick.AddListener(StartRecord);
            testView.StopRecordButton.onClick.AddListener(StopRecord);
            testView.PlayButton.onClick.AddListener(Play);
            Logger.Log(LogTextType.DeviceResolution,Screen.currentResolution.ToString());
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
            
            savePath = Path.Combine(Application.persistentDataPath, "recordingData.txt");
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
            
            RECT curProcessRECT = ResolutionUtil.GetProcessResolutionRECT(curProcess.handle, false);
            Logger.Log(LogTextType.ProcessName,curProcess.nameText.text);
            Logger.Log(LogTextType.ProcessRect,curProcess.nameText.text);
            Logger.Log(LogTextType.ProcessRect,$"{curProcessRECT.Top}(Top) {curProcessRECT.Bottom}(Bottom)\n{curProcessRECT.Left}(Left) {curProcessRECT.Right}(Right)");
        }

        private float recordElapsedTime;
        private async void StartRecord()
        {
            isRecording = true;
            testView.RecordButton.gameObject.SetActive(false);
            testView.StopRecordButton.gameObject.SetActive(true);

            WindowInputSystem.onMouseInput += RecordMouseData;
            while (isRecording)
            {
                recordElapsedTime += Time.deltaTime;
                await UniTask.Yield();
            }
        }

        private void RecordMouseData()
        {
            RECT curProcessRECT = ResolutionUtil.GetProcessResolutionRECT(curProcess.handle, false);
            curProcess.rect = curProcessRECT;
            bool isOnSelectedProcess = WindowInputSystem.IsMouseClickedOnSelectedProcess(curProcess.process.Id);
            if (isOnSelectedProcess)
            {
                float processMousePosX = WindowInputSystem.processMouseData.x;
                float processMousePosY = WindowInputSystem.processMouseData.y;
                if ((processMousePosX != 0 || processMousePosY != 0) && processMousePosX > 0 && processMousePosY > 0)
                {
                    MouseData saveMouseData = new MouseData(processMousePosX, processMousePosY,
                        WindowInputSystem.processMouseData.inputState,recordElapsedTime);
                    saveMousePosList.Add(saveMouseData);
                    
                    Logger.Log(LogTextType.TouchProcessPos, $"{processMousePosX},{processMousePosY}");
                }
            }
            else
            {
                Logger.Log(LogTextType.TouchProcessPos, "No detected");
            }
        }

        private void StopRecord()
        {
            // RecordingData에 Data 할당
            RecordingData recordingData = new RecordingData();
            recordingData.mousePosList = saveMousePosList;
            Vector2 processResolution = ResolutionUtil.GetProcessResolution(curProcess.handle, false);
            recordingData.processWidth = processResolution.x;
            recordingData.processHeight = processResolution.y;
            string saveData = JsonConvert.SerializeObject(recordingData);
            
            // 로컬에 녹화 데이터 저장하기
            File.WriteAllText(savePath, saveData);

            // 데이터 리셋
            isRecording = false;
            recordElapsedTime = 0;
            testView.StopRecordButton.gameObject.SetActive(false);
            testView.RecordButton.gameObject.SetActive(true);
            
            WindowInputSystem.onMouseInput -= RecordMouseData;
            saveMousePosList.Clear();
            Logger.ResetLog();
        }

        private async void Play()
        {
            string saveData = File.ReadAllText(savePath);
            RecordingData recordingData = JsonConvert.DeserializeObject<RecordingData>(saveData);
            List<MouseData> mouseDataList = recordingData.mousePosList;
            int preMilElapsedTime = 0;
            for (int i = 0; i < mouseDataList.Count; i++)
            {
                // 마우스 좌표 프로세스 해상도 대응
                Vector2 originProcessResolution = new Vector2(recordingData.processWidth, recordingData.processHeight);
                Vector2 originProcessMousePos = new Vector2(mouseDataList[i].x,mouseDataList[i].y);
                Vector2 synchronizedMousePos =  WindowInputSystem.GetSynchronizedMousePos(originProcessResolution,originProcessMousePos);

                // 프로세스 마우스 좌표가 아닌 실제 마우스 좌표 구하기
                RECT curProcessRECT = ResolutionUtil.GetProcessResolutionRECT(curProcess.handle,true);
                int mouseX = (int)(curProcessRECT.Left + synchronizedMousePos.x);
                int mouseY = (int)(curProcessRECT.Top + synchronizedMousePos.y);
                
                // 화면 해상도에 따라 mouseX와 mouseY를 조정
                int scaledMouseX = (mouseX * 65535) / Screen.currentResolution.width;
                int scaledMouseY = (mouseY * 65535) / Screen.currentResolution.height;

                // 프로세스를 Foreground로 이동
                WindowApi.SetForegroundWindow(curProcess.handle);

                // 프로세스를 최상단으로 이동
                WindowApi.ShowWindow(curProcess.handle, WindowApi.SW_RESTORE);
                
                // 마우스 이벤트 실행
                float elapsedTime = mouseDataList[i].elapsedTime;
                int milElapsedTime = UniTaskUtil.ConvertSecToMilSec(elapsedTime);
                await UniTask.Delay(milElapsedTime - preMilElapsedTime);
                preMilElapsedTime = milElapsedTime;

                WindowApi.mouse_event(WindowApi.MOUSEEVENTF_ABSOLUTE | WindowApi.MOUSEEVENTF_MOVE, scaledMouseX, scaledMouseY, 0, 0);
                if (mouseDataList[i].inputState == InputState.Down)
                {
                    WindowApi.mouse_event(WindowApi.MOUSEEVENTF_LEFTDOWN, scaledMouseX, scaledMouseY, 0, 0);
                }
                else if (mouseDataList[i].inputState == InputState.Stay)
                {
                    
                }
                else // Up
                {
                    WindowApi.mouse_event(WindowApi.MOUSEEVENTF_LEFTUP, scaledMouseX, scaledMouseY, 0, 0);
                }
            }
        }

        // 선택된 녹화할 프로세스 해제
        private void DeSelectProcess()
        {
            WindowInputSystem.UnHook();
        }
    }
}