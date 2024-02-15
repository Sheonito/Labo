using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

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
    private ProcessUI curProcess;
    private bool isProcessSelected;
    void Start()
    {
        selectButton.onClick.AddListener(() => SelectProcess().Forget());
        unSelectButton.onClick.AddListener(UnSelectProcess);
        
        ShowProcesses();
    }

    private async UniTaskVoid SelectProcess()
    {
        if (curProcess == null)
            return;
        
        // 윈도우를 복원하고 최상위로 가져오기
        ShowWindow(curProcess.handle, SW_RESTORE);
        
        // 현재 프로세스 위치 및 width,height 변경
        // SetWindowPos(curProcess.handle, IntPtr.Zero, 0, 0, 0, 0, SWP_SHOWWINDOW);
        
        isProcessSelected = true;
        while (isProcessSelected)
        {
            GetWindowRect(curProcess.handle,out RECT rect);
            curProcess.rect = rect;
            Logger.Print($"Current Process: {curProcess.nameText.text} \nTop: {rect.Top}\nBottom: {rect.Bottom}\nLeft: {rect.Left}\nRight: {rect.Right}");
            
            await UniTask.Yield();
            
        }
    }

    private void UnSelectProcess()
    {
        isProcessSelected = false;
    }

    private void ShowProcesses()
    {
        List<Process> processes = GetProcess();
        foreach (Process process in processes)
        {
            // 결과 출력
            ProcessUI processUI = Instantiate(processUIPrefab, scrollRect.content);
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
        RECT rect = GetWindowRECT(process.handle);
        Logger.Print($"Current Process: {process.nameText.text} \nTop: {rect.Top}\nBottom: {rect.Bottom}\nLeft: {rect.Left}\nRight: {rect.Right}");
    }

    private RECT GetWindowRECT(IntPtr handle)
    {
        GetWindowRect(handle, out var rect);
        return rect;
    }
}
