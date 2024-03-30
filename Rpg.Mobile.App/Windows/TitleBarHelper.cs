//using System.Runtime.InteropServices;

//namespace Rpg.Mobile.App.Windows;

//public static class TitleBarUser32
//{
//    [DllImport("user32.dll")]
//    private static extern int GetWindowRect(IntPtr hWnd, out RECT rect);

//    [DllImport("user32.dll")]
//    private static extern int GetClientRect(IntPtr hWnd, out RECT rect);

//    [DllImport("user32.dll")]
//    private static extern bool ClientToScreen(IntPtr hWnd, ref PointI lpPoint);

//    [StructLayout(LayoutKind.Sequential)]
//    public struct RECT
//    {
//        public int Left;
//        public int Top;
//        public int Right;
//        public int Bottom;
//    }

//    [DllImport("user32.dll")]
//    private static extern bool GetTitleBarInfo(IntPtr hwnd, ref TITLEBARINFO pti);

//    [StructLayout(LayoutKind.Sequential)]
//    public struct TITLEBARINFO
//    {
//        public int cbSize;
//        public RECT rcTitleBar;
//        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
//        public uint[] rgstate;
//    }

//    //public static RECT GetTitleBarRect()
//    //{
//    //    var hWnd = GetForegroundWindow();
//    //    TITLEBARINFO titleBarInfo = new TITLEBARINFO();
//    //    titleBarInfo.cbSize = Marshal.SizeOf(titleBarInfo);

//    //    if (GetTitleBarInfo(hWnd, ref titleBarInfo))
//    //    {
//    //        return titleBarInfo.rcTitleBar;
//    //    }
//    //    else
//    //    {
//    //        throw new InvalidOperationException("Failed to get title bar information.");
//    //    }
//    //}

//    public static int GetTitleBarHeight()
//    {
//        return 32;
//        //   thorw ne
//        //var rect = GetForegroundWindow();
//        //return rect.
//        // GetWindowRect(hWnd, out RECT windowRect);
//        // GetClientRect(hWnd, out RECT clientRect);

//        // var topLeft = new PointI { X = clientRect.Left, Y = clientRect.Top };
//        // ClientToScreen(hWnd, ref topLeft);

//        // return topLeft.Y - windowRect.Top;
//        ////// var topDistance = windowRect.Top;
//        // return 0;


//        // RECT wrect;
//        // GetWindowRect(hwnd, &wrect);
//        // RECT crect;
//        // GetClientRect(hwnd, &crect);
//        // POINT lefttop = { crect.left, crect.top }; // Practicaly both are 0
//        // ClientToScreen(hwnd, &lefttop);
//        // POINT rightbottom = { crect.right, crect.bottom };
//        // ClientToScreen(hwnd, &rightbottom);

//        // int left_border = lefttop.x - wrect.left; // Windows 10: includes transparent part
//        // int right_border = wrect.right - rightbottom.x; // As above
//        // int bottom_border = wrect.bottom - rightbottom.y; // As above
//        // int top_border_with_title_bar = lefttop.y - wrect.top;

//    }
//}