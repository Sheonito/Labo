using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AutomationQA.Common;
using UnityEngine;

namespace AutomationQA
{
    
    public class ProcessUIFactory
    {
        private ProcessUI _processUIPrefab;
        private List<ProcessUI> processUis;
        
        public ProcessUIFactory(ProcessUI processUIPrefab,Transform parent)
        {
            _processUIPrefab = processUIPrefab;
            processUis = new List<ProcessUI>();
            CreateProcesses(parent);
        }

        public List<ProcessUI> GetProcessUI()
        {
            return processUis;
        }

        private void CreateProcesses(Transform parent)
        {
            List<Process> processes = GetProcess();
            foreach (Process process in processes)
            {
                // 결과 출력
                ProcessUI processUI = GameObject.Instantiate(_processUIPrefab, parent);
                processUI.process = process;
                processUI.nameText.text = process.ProcessName;
                processUI.handle = process.MainWindowHandle;
                processUis.Add(processUI);
            }
        }

        private List<Process> GetProcess()
        {
            List<Process> processes = new List<Process>();

            // EnumWindows 함수를 사용하여 모든 창 열거
            WindowApi.EnumWindows((hWnd, lParam) =>
            {
                if (WindowApi.IsWindowVisible(hWnd))
                {
                    // 창이 보이는 경우 프로세스 ID 가져오기
                    uint processId;
                    WindowApi.GetWindowThreadProcessId(hWnd, out processId);

                    // 프로세스 ID로 프로세스 가져오기
                    Process windowProcess = Process.GetProcessById((int)processId);

                    // 창의 제목 가져오기
                    int titleLength = WindowApi.GetWindowTextLength(hWnd);
                    StringBuilder title = new StringBuilder(titleLength + 1);
                    WindowApi.GetWindowText(hWnd, title, title.Capacity);

                    processes.Add(windowProcess);
                }

                return true; // 계속 열거
            }, IntPtr.Zero);

            return processes;
        }
    }
}