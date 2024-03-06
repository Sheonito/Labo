using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using AutomationQA.Input;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace AutomationQA
{
    public class ProcessPicker : MonoBehaviour
    {
        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT Point);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        const uint SW_RESTORE = 9;
        const uint SWP_SHOWWINDOW = 0x40;

        public ScrollRect scrollRect;
        public ProcessUI processUIPrefab;
        public Button selectButton;
        public Button unSelectButton;
        public Button saveResolutionButton;
        public Button syncMousePosButton;
        private static ProcessUI curProcess;
        private bool isProcessSelected;

        private void Awake()
        {
            InputDetector.Init();
        }

        void Start()
        {
            selectButton.onClick.AddListener(() => SelectProcess().Forget());
            unSelectButton.onClick.AddListener(UnSelectProcess);
            saveResolutionButton.onClick.AddListener(SaveResolution);
            syncMousePosButton.onClick.AddListener(SyncMousePos);

            ShowProcesses();
        }

        private static RECT GetResolutionRECT(IntPtr handle)
        {
            GetWindowRect(handle, out RECT windowRect);
            GetClientRect(handle, out RECT clientRect);

            RECT rect = new RECT();
            int titlebarHeight = (windowRect.Bottom - windowRect.Top) - clientRect.Bottom;
            rect.Top = windowRect.Top + titlebarHeight;
            rect.Bottom = windowRect.Bottom;
            rect.Left = windowRect.Left;
            rect.Right = windowRect.Right;

            return rect;
        }

        private static void SyncMousePos()
        {
            int savedResolutionWidth = PlayerPrefs.GetInt("ResolutionWidth");
            int savedResolutionHeight = PlayerPrefs.GetInt("ResolutionHeight");
            int savedProcessMouseX = PlayerPrefs.GetInt("SelectedMousePosX");
            int savedProcessMouseY = PlayerPrefs.GetInt("SelectedMousePosY");
            Vector2 currentResolution = GetCurrentResolution();
            
            Vector2 targetResolution = new Vector2(savedResolutionWidth, savedResolutionHeight);
            Vector2 targetMousePos = new Vector2(savedProcessMouseX, savedProcessMouseY);
            
            Vector2 syncMousePos = GetSyncMousePos(targetResolution, currentResolution, targetMousePos);
            Logger.Print(LogTextType.SyncMousePos,$"syncMousePos: {syncMousePos}\nsavedMousePos: {targetMousePos}");
        }

        private static Vector2 GetSyncMousePos(Vector2 targetResolution, Vector2 curResolution, Vector2 targetMousePos)
        {
            float ratioX = targetMousePos.x / targetResolution.x;
            float ratioY = targetMousePos.y / targetResolution.y;
            float synchronizedMousePosX = curResolution.x * ratioX;
            float synchronizedMousePosY = curResolution.y * ratioY;
            Vector2 synchronizedMousePos = new Vector2(synchronizedMousePosX, synchronizedMousePosY);

            return synchronizedMousePos;
        }

        private void SaveResolution()
        {
            if (curProcess == null)
            {
                Debug.Log("현재 선택된 프로세스가 없습니다.");
                return;
            }

            Vector2 resolutionSize = GetCurrentResolution();

            PlayerPrefs.SetInt("ResolutionWidth", (int)resolutionSize.x);
            PlayerPrefs.SetInt("ResolutionHeight", (int)resolutionSize.y);
        }

        private static Vector2 GetCurrentResolution()
        {
            RECT resolutionRect = GetResolutionRECT(curProcess.handle);
            int resolutionWidth = resolutionRect.Right - resolutionRect.Left;
            int resolutionHeight = resolutionRect.Bottom - resolutionRect.Top;
            Vector2 resolutionSize = new Vector2(resolutionWidth, resolutionHeight);

            return resolutionSize;
        }

        private async UniTaskVoid SelectProcess()
        {
            if (curProcess == null)
                return;

            InputDetector.SetHook(curProcess);

            // 윈도우를 복원하고 최상위로 가져오기
            ShowWindow(curProcess.handle, SW_RESTORE);

            // 현재 프로세스 위치 및 width,height 변경
            // SetWindowPos(curProcess.handle, IntPtr.Zero, 0, 0, 0, 0, SWP_SHOWWINDOW);

            isProcessSelected = true;
            while (isProcessSelected)
            {
                GetWindowRect(curProcess.handle, out RECT windowRect);
                GetClientRect(curProcess.handle, out RECT clientRect);

                RECT rect = new RECT();
                int titlebarHeight = (windowRect.Bottom - windowRect.Top) - clientRect.Bottom;
                rect.Top = windowRect.Top + titlebarHeight;
                rect.Bottom = windowRect.Bottom;
                rect.Left = windowRect.Left;
                rect.Right = windowRect.Right;

                curProcess.rect = rect;

                Logger.Print(LogTextType.ProcessRect, $"Current Process: {curProcess.nameText.text} \nTop: {rect.Top}\nBottom: {rect.Bottom}\nLeft: {rect.Left}\nRight: {rect.Right}");
                bool isOnSelectedProcess = IsMouseClickedOnSelectedProcess(curProcess.process.Id);
                if (isOnSelectedProcess)
                {
                    if (InputDetector.processMousePos != Vector2.zero && InputDetector.processMousePos.x > 0 && InputDetector.processMousePos.y > 0)
                    {
                        PlayerPrefs.SetInt("SelectedMousePosX",(int)InputDetector.processMousePos.x);
                        PlayerPrefs.SetInt("SelectedMousePosY",(int)InputDetector.processMousePos.y);                        
                    }

                    Logger.Print(LogTextType.ProcessMousePos, $"ClientMouseX: {InputDetector.processMousePos.x}\nClientMouseY: {InputDetector.processMousePos.y}");
                }
                else
                {
                    Logger.Print(LogTextType.ProcessMousePos, $"ClientMouseX: No detected\nClientMouseY: No detected");
                }


                await UniTask.Yield();
            }
        }

        private bool IsMouseClickedOnSelectedProcess(int processId)
        {
            POINT mousePOINT = new POINT();
            mousePOINT.X = (int)InputDetector.mousePos.x;
            mousePOINT.Y = (int)InputDetector.mousePos.y;
            IntPtr handle = WindowFromPoint(mousePOINT);

            // 창 핸들을 사용하여 프로세스 ID를 가져옵니다.
            uint pid;
            GetWindowThreadProcessId(handle, out pid);

            // 프로세스 ID를 사용하여 프로세스 정보를 가져옵니다.
            Process proc = Process.GetProcessById((int)pid);

            if (processId == proc.Id)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void UnSelectProcess()
        {
            isProcessSelected = false;
            InputDetector.UnHook();
        }

        private void ShowProcesses()
        {
            List<Process> processes = GetProcess();
            foreach (Process process in processes)
            {
                // 결과 출력
                ProcessUI processUI = Instantiate(processUIPrefab, scrollRect.content);
                processUI.process = process;
                processUI.nameText.text = process.ProcessName;
                processUI.handle = process.MainWindowHandle;
                processUI.button.onClick.AddListener(() => PickProcess(processUI));
            }
        }

        private List<Process> GetProcess()
        {
            List<Process> processes = new List<Process>();

            // EnumWindows 함수를 사용하여 모든 창 열거
            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    // 창이 보이는 경우 프로세스 ID 가져오기
                    uint processId;
                    GetWindowThreadProcessId(hWnd, out processId);

                    // 프로세스 ID로 프로세스 가져오기
                    Process windowProcess = Process.GetProcessById((int)processId);

                    // 창의 제목 가져오기
                    int titleLength = GetWindowTextLength(hWnd);
                    StringBuilder title = new StringBuilder(titleLength + 1);
                    GetWindowText(hWnd, title, title.Capacity);

                    processes.Add(windowProcess);
                }

                return true; // 계속 열거
            }, IntPtr.Zero);

            return processes;
        }

        private void PickProcess(ProcessUI process)
        {
            curProcess = process;

            GetWindowRect(curProcess.handle, out RECT windowRect);
            GetClientRect(curProcess.handle, out RECT clientRect);

            RECT rect = new RECT();
            int titlebarHeight = (windowRect.Bottom - windowRect.Top) - clientRect.Bottom;
            rect.Top = windowRect.Top + titlebarHeight;
            rect.Bottom = windowRect.Bottom;
            rect.Left = windowRect.Left;
            rect.Right = windowRect.Right;

            curProcess.rect = rect;

            Logger.Print(LogTextType.ProcessRect, $"Current Process: {process.nameText.text} \nTop: {rect.Top}\nBottom: {rect.Bottom}\nLeft: {rect.Left}\nRight: {rect.Right}");
        }

        private RECT GetWindowRECT(IntPtr handle)
        {
            GetWindowRect(handle, out var rect);
            return rect;
        }

        private void OnApplicationQuit()
        {
            InputDetector.UnHook();
        }
    }
}