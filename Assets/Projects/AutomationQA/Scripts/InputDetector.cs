using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AutomationQA.Input
{
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
        private static Dictionary<InputType, InputState> inputsState;
        private static Dictionary<InputType, Vector2> inputsPos;

        public static void Init()
        {
            inputsState = new Dictionary<InputType, InputState>();
            inputsPos = new Dictionary<InputType, Vector2>();
            foreach (InputType inputType in Enum.GetValues(typeof(InputType)))
            {
                inputsState.Add(inputType, InputState.None);
                inputsPos.Add(inputType, Vector2.zero);
            }
        }

        public static bool GetInputDown(InputType inputType)
        {

            if (inputsState[inputType] == InputState.Down)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool GetInput(InputType inputType)
        {
            if (inputsState[inputType] == InputState.Stay)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool GetInputUp(InputType inputType)
        {
            if (inputsState[inputType] == InputState.Up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Vector2 GetInputPos(InputType inputType)
        {
            return inputsPos[inputType];
        }

        public static void SetHook(ProcessUI processUI)
        {
            if (isHookRegistered)
                return;

            curProcessUI = processUI;
            Process process = processUI.process;
            isHookRegistered = true;
            _mouseHookID = SetMouseHook(_mouseHookCallback, processUI.process);
            _keyboardHookID = SetKeboardHook(_keyboardHookCallback, process, WH_KEYBOARD_LL);
        }

        // UnHook 함수를 호출하지 않으면 크래시됨
        public static void UnHook()
        {
            isHookRegistered = false;
            UnhookWindowsHookEx(_mouseHookID);
            UnhookWindowsHookEx(_keyboardHookID);
        }

        private static IntPtr SetMouseHook(LowLevelMouseProc callback, Process curProcess)
        {
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, callback, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr SetKeboardHook(LowLevelKeyboardProc callback, Process curProcess, int hookType)
        {
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(hookType, callback, GetModuleHandle(curModule.ModuleName), 0);
            }
        }


        // 마우스 클릭 시 호출되는 콜백함수
        private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // 왼쪽 ClickDown
            if (nCode >= 0 && MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                int mouseX = hookStruct.pt.X;
                int mouseY = hookStruct.pt.Y;
                int processMouseX = mouseX - curProcessUI.rect.Left - 8;
                int processMouseY = mouseY - curProcessUI.rect.Top + 8;
                mousePos = new Vector2(processMouseX, processMouseY);

                inputsState[InputType.LeftMouse] = InputState.Down;
                inputsPos[InputType.LeftMouse] = new Vector2(processMouseX, processMouseY);
                ChangeInputState(InputType.LeftMouse, InputState.Stay);
            }
            // 왼쪽 ClickUp
            else if (nCode >= 0 && MouseMessages.WM_LBUTTONUP == (MouseMessages)wParam)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                int mouseX = hookStruct.pt.X;
                int mouseY = hookStruct.pt.Y;
                int processMouseX = mouseX - curProcessUI.rect.Left - 8;
                int processMouseY = mouseY - curProcessUI.rect.Top + 8;

                inputsState[InputType.LeftMouse] = InputState.Up;
                inputsPos[InputType.LeftMouse] = new Vector2(processMouseX, processMouseY);
                ChangeInputState(InputType.LeftMouse, InputState.None);
            }
            else if (nCode >= 0 && MouseMessages.WM_MOUSEMOVE == (MouseMessages)wParam)
            {
                // 왼쪽 Stay + 마우스 이동
                if (inputsState[InputType.LeftMouse] == InputState.Stay)
                {
                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                    int mouseX = hookStruct.pt.X;
                    int mouseY = hookStruct.pt.Y;
                    int processMouseX = mouseX - curProcessUI.rect.Left - 8;
                    int processMouseY = mouseY - curProcessUI.rect.Top + 8;

                    inputsPos[InputType.LeftMouse] = new Vector2(processMouseX, processMouseY);
                }
            }

            // 오른쪽 ClickDown
            if (nCode >= 0 && MouseMessages.WM_RBUTTONDOWN == (MouseMessages)wParam)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                int mouseX = hookStruct.pt.X;
                int mouseY = hookStruct.pt.Y;
                int processMouseX = mouseX - curProcessUI.rect.Left - 8;
                int processMouseY = mouseY - curProcessUI.rect.Top + 8;

                inputsState[InputType.RightMouse] = InputState.Down;
                inputsPos[InputType.RightMouse] = new Vector2(processMouseX, processMouseY);
                ChangeInputState(InputType.RightMouse, InputState.Stay);
            }
            // 오른쪽 ClickUp
            else if (nCode >= 0 && MouseMessages.WM_RBUTTONUP == (MouseMessages)wParam)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                int mouseX = hookStruct.pt.X;
                int mouseY = hookStruct.pt.Y;
                int processMouseX = mouseX - curProcessUI.rect.Left - 8;
                int processMouseY = mouseY - curProcessUI.rect.Top + 8;

                inputsState[InputType.RightMouse] = InputState.Up;
                inputsPos[InputType.RightMouse] = new Vector2(processMouseX, processMouseY);
                ChangeInputState(InputType.RightMouse, InputState.None);
            }
            else if (nCode >= 0 && MouseMessages.WM_MOUSEMOVE == (MouseMessages)wParam)
            {
                // 오른쪽 Stay + 마우스 이동
                if (inputsState[InputType.RightMouse] == InputState.Stay)
                {
                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                    int mouseX = hookStruct.pt.X;
                    int mouseY = hookStruct.pt.Y;
                    int processMouseX = mouseX - curProcessUI.rect.Left - 8;
                    int processMouseY = mouseY - curProcessUI.rect.Top + 8;

                    inputsPos[InputType.RightMouse] = new Vector2(processMouseX, processMouseY);
                }
            }


            return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
        }

        // 키보드 입력 시 호출되는 콜백함수 (해당 프로세스 안에서만 감지됨)
        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                string keyCode = ((InputType)vkCode).ToString();
                Debug.Log("keyCode: " + keyCode);
            }

            return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
        }

        // Window API에는 Stay를 가져오는 API가 없기 때문에 1프레임 뒤에 State를 변경해준다.
        private static async void ChangeInputState(InputType inputType, InputState state)
        {
            await UniTask.Yield();
            await UniTask.Yield();
            inputsState[inputType] = state;

            if (state == InputState.None)
                inputsPos[inputType] = Vector2.zero;
        }
    }
}