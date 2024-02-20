using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;
using LogTextType = Define.LogTextType;

public enum MouseMessages
{
    WM_LBUTTONDOWN = 0x0201,
    WM_LBUTTONUP = 0x0202,
    WM_MOUSEMOVE = 0x0200,
    WM_MOUSEWHEEL = 0x020A,
    WM_RBUTTONDOWN = 0x0204,
    WM_RBUTTONUP = 0x0205
}

public struct POINT
{
    public int X;
    public int Y;

    public POINT(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}

public struct MSLLHOOKSTRUCT
{
    public POINT pt;
    public uint mouseData;
    public uint flags;
    public uint time;
    public IntPtr dwExtraInfo;
}

public class InputDetector
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    
    
    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static LowLevelMouseProc _mouseHookCallback = MouseHookCallback;
    private static LowLevelKeyboardProc _keyboardHookCallback = KeyboardHookCallback;
    private static IntPtr _mouseHookID = IntPtr.Zero;
    private static IntPtr _keyboardHookID = IntPtr.Zero;
    private const int WH_MOUSE_LL = 14;
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;

    private static bool isHookRegistered;
    private static ProcessUI curProcessUI;
    public static Vector2 mousePos = new Vector2();
    public static void SetHook(ProcessUI processUI)
    {
        if (isHookRegistered)
            return;

        curProcessUI = processUI;
        Process process = processUI.process;
        isHookRegistered = true;
        _mouseHookID = SetMouseHook(_mouseHookCallback,processUI.process);
        _keyboardHookID = SetKeboardHook(_keyboardHookCallback,process, WH_KEYBOARD_LL);
        
    }

    // UnHook 함수를 호출하지 않으면 크래시됨
    public static void UnHook()
    {
        isHookRegistered = false;
        UnhookWindowsHookEx(_mouseHookID);
        UnhookWindowsHookEx(_keyboardHookID);
    }

    private static IntPtr SetMouseHook(LowLevelMouseProc callback,Process curProcess)
    {
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_MOUSE_LL, callback, GetModuleHandle(curModule.ModuleName), 0);
        }
    }
    
    private static IntPtr SetKeboardHook(LowLevelKeyboardProc callback, Process curProcess,int hookType)
    {
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(hookType, callback, GetModuleHandle(curModule.ModuleName), 0);
        }
    }


    
    // 마우스 클릭 시 호출되는 콜백함수
    private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        // 왼쪽 클릭
        if (nCode >= 0 && MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
        {
            MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            Debug.Log($"MousePos: {hookStruct.pt.X},{hookStruct.pt.Y}");
            int mouseX = hookStruct.pt.X;
            int mouseY = hookStruct.pt.Y;
            int processMouseX = mouseX - curProcessUI.rect.Left -8;
            int processMouseY = mouseY - curProcessUI.rect.Top +8;
            mousePos = new Vector2(processMouseX,processMouseY);
            Debug.Log("mousePos: " + mousePos);
        }
        // 오른쪽 클릭
        else if (nCode >= 0 && MouseMessages.WM_RBUTTONDOWN == (MouseMessages)wParam)
        {
            MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            Debug.Log($"MousePos: {hookStruct.pt.X},{hookStruct.pt.Y}");
        }
        
        return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
    }
    
    // 키보드 입력 시 호출되는 콜백함수 (해당 프로세스 안에서만 감지됨)
    private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            string keyCode = ((Define.WindowKeys)vkCode).ToString();
            Debug.Log("keyCode: " + keyCode);
        }
        return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
    }
}
