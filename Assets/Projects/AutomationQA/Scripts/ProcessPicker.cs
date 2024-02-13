using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
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
    static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
    
    public ScrollRect scrollRect;
    public ProcessUI processUIPrefab;
    void Start()
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
        RECT rect = GetWindowRECT(process.handle);
        Logger.Print($"Current Process: {process.nameText.text} \nTop: {rect.Top}\nBottom: {rect.Bottom}\nLeft: {rect.Left}\nRight: {rect.Right}");
    }

    private RECT GetWindowRECT(IntPtr handle)
    {
        GetWindowRect(handle, out var rect);
        return rect;
    }
}
