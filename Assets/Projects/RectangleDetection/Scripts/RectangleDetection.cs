using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class RectangleDetection : MonoBehaviour
{
    [DllImport("user32.dll")]
    static extern bool GetCursorPos(out Point lpPoint);

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    // IsIconic 함수 정의
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindow(IntPtr hWnd, int wCmd);

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

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    // SetWindowPos 플래그
    const uint SWP_NOSIZE = 0x0001;
    const uint SWP_NOMOVE = 0x0002;
    const uint SWP_NOZORDER = 0x0004;
    const uint SWP_NOACTIVATE = 0x0010;

    private bool isWindow;
    private IntPtr curWindow;
    private IntPtr curProcess;
    private RECT curRect;

    private void Update()
    {
        Detect();

        if (isWindow)
        {
            // 메인 윈도우의 화면 영역 가져오기
            if (GetWindowRect(curWindow, out RECT windowRect))
            {
                // SyncWindowRect(windowRect);
            }

            SyncProcessRect(curRect);
        }
    }

    private void SyncWindowRect(RECT rect)
    {
        SetWindowPos(curProcess, IntPtr.Zero, rect.Left, rect.Top, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
        Debug.Log("SyncWindowRect: " + rect.Top);
    }
    
    private void SyncProcessRect(RECT rect)
    {
        SetWindowPos(curWindow, IntPtr.Zero, rect.Left, rect.Top, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
        Debug.Log("SyncWindowRect: " + rect.Top);
    }

    private void Detect()
    {
        // 현재 실행 중인 모든 프로세스 가져오기
        Process[] processes = Process.GetProcesses();

        // 마우스 위치 가져오기
        GetCursorPos(out Point mousePoint);

        Process smallestProcess = null;
        int biggestZOrder = 0;

        // 각 프로세스의 정보 확인
        foreach (Process process in processes)
        {
            // 프로세스의 메인 윈도우 핸들 가져오기
            IntPtr mainWindowHandle = process.MainWindowHandle;

            if (process.MainWindowHandle != IntPtr.Zero && IsWindowVisible(mainWindowHandle) && IsIconic(mainWindowHandle) == false)
            {
                // 메인 윈도우의 화면 영역 가져오기
                if (GetWindowRect(mainWindowHandle, out RECT windowRect))
                {
                    int zOrder = GetZOrder(mainWindowHandle);
                    // 마우스 위치가 해당 프로세스의 화면 영역 안에 있는지 확인
                    if (mousePoint.X >= windowRect.Left && mousePoint.X <= windowRect.Right && mousePoint.Y >= windowRect.Top && mousePoint.Y <= windowRect.Bottom)
                    {
                        if (biggestZOrder < zOrder)
                        {
                            if(process.ProcessName == "Unity")
                                continue;

                            if (process.ProcessName == "notepad++")
                            {
    
                            }
                            
                            biggestZOrder = zOrder;
                            smallestProcess = process;
                            curRect = windowRect;
                            curProcess = mainWindowHandle;
                                
              
                        }
                    }
                }
            }
        }

        if (smallestProcess != null)
        {
            if (!isWindow)
            {
                isWindow = true;
                curWindow = WindowCreator.CreateWindow(curRect);
            }

            Debug.Log($"마우스는 {smallestProcess.ProcessName} 프로세스의 영역에 있습니다.");
        }
    }

    // Z-order를 가져오는 메서드
    public static int GetZOrder(IntPtr windowHandle)
    {
        const int GW_HWNDNEXT = 2;

        int zOrder = 0;
        IntPtr currentWindow = windowHandle;

        while ((currentWindow = GetWindow(currentWindow, GW_HWNDNEXT)) != IntPtr.Zero)
        {
            zOrder++;
        }

        return zOrder;
    }
}