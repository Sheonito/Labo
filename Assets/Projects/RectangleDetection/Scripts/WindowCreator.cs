using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class WindowCreator : MonoBehaviour
{
[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern ushort RegisterClassEx([In] ref WNDCLASSEX lpwcx);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr CreateWindowEx(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool UpdateWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WNDCLASSEX
    {
        public uint cbSize;
        public uint style;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public WndProcDelegate lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszMenuName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszClassName;
        public IntPtr hIconSm;
    }

    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    const int WS_OVERLAPPEDWINDOW = 0x00CF;
    const int SW_SHOWNORMAL = 1;
    const int WM_DESTROY = 0x0002;
    const int WM_NCHITTEST = 0x0084;
    const int HTCAPTION = 0x02;

    static WndProcDelegate customWndProc;
    static IntPtr defaultWndProc;


    public static IntPtr CreateWindow(RectangleDetection.RECT rect)
    {
        customWndProc = CustomWndProc;
        defaultWndProc = IntPtr.Zero;
        int x = rect.Left;
        int y = rect.Top;
        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;
        height = 80;

        WNDCLASSEX wndClass = new WNDCLASSEX
        {
            cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX)),
            style = 0,
            lpfnWndProc = customWndProc,
            hInstance = Marshal.GetHINSTANCE(AppDomain.CurrentDomain.GetAssemblies()[0].GetModules()[0]),
            hbrBackground = (IntPtr)5, // COLOR_WINDOW
            lpszClassName = "CustomWindowClass"
        };

        RegisterClassEx(ref wndClass);

        IntPtr hWnd = CreateWindowEx(
            0,
            "CustomWindowClass",
            "Custom Window",
            WS_OVERLAPPEDWINDOW,
            x,
            y,
            width,
            height,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);

        ShowWindow(hWnd, SW_SHOWNORMAL);
        UpdateWindow(hWnd);

        return hWnd;
    }

    static IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WM_NCHITTEST:
                return (IntPtr)HTCAPTION;

            case WM_DESTROY:
                break;
        }

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

}
