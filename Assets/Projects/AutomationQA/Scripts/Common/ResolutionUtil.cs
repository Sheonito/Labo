using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AutomationQA.Common
{
    public static class ResolutionUtil
    {
        // 해상도 가져오기
        public static Vector2 GetProcessResolution(IntPtr handle,bool isIncludedTitleBar)
        {
            RECT resolutionRect = GetProcessResolutionRECT(handle,isIncludedTitleBar);
            int resolutionWidth = resolutionRect.Right - resolutionRect.Left;
            int resolutionHeight = resolutionRect.Bottom - resolutionRect.Top;
            Vector2 resolutionSize = new Vector2(resolutionWidth, resolutionHeight);

            return resolutionSize;
        }
        
        // 해상도 RECT 가져오기
        public static RECT GetProcessResolutionRECT(IntPtr handle,bool isIncludedTitleBar)
        {
            WindowApi.GetWindowRect(handle, out RECT windowRect);
            WindowApi.GetClientRect(handle, out RECT clientRect);

            RECT rect = new RECT();
            int titlebarHeight = (windowRect.Bottom - windowRect.Top) - clientRect.Bottom;
            rect.Top = isIncludedTitleBar ? windowRect.Top : windowRect.Top + titlebarHeight;
            rect.Bottom = windowRect.Bottom;
            rect.Left = windowRect.Left;
            rect.Right = windowRect.Right;

            return rect;
        }
        
    }

}

