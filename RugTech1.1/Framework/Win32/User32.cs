using System;
using System.Runtime.InteropServices;

namespace RugTech1.Win32
{
#if !__MonoCS__
    /// <summary>
    /// This class shall keep the User32 APIs being used in 
    /// our program.
    /// </summary>
    internal class User32
    {
        #region Class Variables
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;

        public const int ENUM_CURRENT_SETTINGS = -1;
        //public const int CDS_UPDATEREGISTRY = 0x01;
        //public const int CDS_TEST = 0x02;

        public const int CDS_UPDATEREGISTRY = 0x00000001;
        public const int CDS_TEST           = 0x00000002;
        public const int CDS_FULLSCREEN     = 0x00000004;
        public const int CDS_GLOBAL         = 0x00000008;
        public const int CDS_SET_PRIMARY    = 0x00000010;
        public const int CDS_VIDEOPARAMETERS = 0x00000020;
        public const int CDS_RESET          = 0x40000000;
        public const int CDS_NORESET       =  0x10000000;


//        public const int DISP_CHANGE_SUCCESSFUL = 0;
  //      public const int DISP_CHANGE_RESTART = 1;
    //    public const int DISP_CHANGE_FAILED = -1;

        public const int DISP_PRIMARY_DEVICE = 0x04;        

        public const int DISP_CHANGE_SUCCESSFUL = 0;
        public const int DISP_CHANGE_RESTART = 1;
        public const int DISP_CHANGE_FAILED = -1;
        public const int DISP_CHANGE_BADMODE = -2;
        public const int DISP_CHANGE_NOTUPDATED = -3;
        public const int DISP_CHANGE_BADFLAGS = -4;
        public const int DISP_CHANGE_BADPARAM = -5;
        public const int DISP_CHANGE_BADDUALVIEW = -6;

        #endregion

        #region Class Functions
        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", EntryPoint = "GetDC")]
        public static extern IntPtr GetDC(IntPtr ptr);

        [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        public static extern int GetSystemMetrics(int abc);

        [DllImport("user32.dll", EntryPoint = "GetWindowDC")]
        public static extern IntPtr GetWindowDC(Int32 ptr);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(IntPtr lpDeviceName, int deviceNumber, [In, Out] Display device, int flags);

        [DllImport("user32.dll")]
        // [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DeviceMode32 devMode);
        //        public static extern int EnumDisplaySettings(IntPtr lpDeviceName, int modeNum, ref DeviceMode32 devMode);
        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettings(ref DeviceMode32 devMode, int flags);

        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettingsEx(string lpszDeviceName, ref DeviceMode32 lpDevMode, IntPtr hwnd, uint dwflags, IntPtr lParam);
        #endregion

        #region Public Constructor
        public User32()
        {
            // 
            // TODO: Add constructor logic here
            //
        }
        #endregion
    }

    //This structure shall be used to keep the size of the screen.
    internal struct SIZE
    {
        public int cx;
        public int cy;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct DeviceMode32
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;//
        public short SpecVersion;
        public short DriverVersion;
        public short Size;
        public short DriverExtra;
        public int Fields;

        public short Orientation;
        public short PaperSize;
        public short PaperLength;
        public short PaperWidth;

        public short Scale;
        public short Copies;
        public short DefaultSource;
        public short PrintQuality;
        public short Color;
        public short Duplex;
        public short YResolution;
        public short TTOption;
        public short Collate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string FormName;// = new string('\0', 32);
        public short LogPixels;
        public short BitsPerPel;
        public int PelsWidth;
        public int PelsHeight;

        public int DisplayFlags;
        public int DisplayFrequency;

        public int ICMMethod;
        public int ICMIntent;
        public int MediaType;
        public int DitherType;
        public int Reserved1;
        public int Reserved2;

        public int PanningWidth;
        public int PanningHeight;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal class Display
    {
        public int cb = 0;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName = new string('\0', 32);

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString = new string('\0', 128);

        public int Flags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID = new string('\0', 128);

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey = new string('\0', 128);

        public override string ToString()
        {
            return DeviceName;
        }
    }
#endif
}
