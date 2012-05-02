//  http://msdn.microsoft.com/en-us/library/dd757277(VS.85).aspx
// 	DaveyM69
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Rvg.Win32.Midi
{   
    public delegate void MidiInProc(int hMidiIn, uint Msg, uint dwInstance, uint dwParam1, uint dwParam2);

    public class MidiLibWrap
    {
        public const int CALLBACK_FUNCTION = 0x00030000;
        public const int MMSYSERR_NOERROR = 0;
        public const int MIM_DATA = 0x3C3;
        public const int MIM_ERROR = 0x3C5;
        public const int MIM_LONGDATA = 0x3C4;
        public const int MHDR_DONE = 0x00000001;

        // Represents the method handles messages from Windows.
        public delegate void MidiOutProc(int handle, int msg, int instance, int param1, int param2);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MidiInCapabilities
        {
            public ushort ManufacturerID;
            public ushort ProductID;
            public uint DriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string Name;		// size s/b MAXPNAMELEN (32)
            public uint Support;
        };	// 44 bytes

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MidiOutCapabilities
        {
            public ushort ManufacturerID;
            public ushort ProductID;
            public uint DriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string Name; //public char[32] szPname;
            public ushort Technology;
            public ushort Voices;
            public ushort Notes;
            public ushort ChannelMask;
            public uint Support;
        }; // 52 bytes

        [StructLayout(LayoutKind.Sequential)]
        public struct MidiHeader
        {
            #region MidiHeader Members

            public IntPtr data;
            public int bufferLength;
            public int bytesRecorded;
            public int user;
            public int flags;
            public IntPtr next;
            public int reserved;
            public int offset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public int[] reservedArray;

            #endregion
        };

        [DllImport("User32.dll", EntryPoint = "MessageBox", CharSet = CharSet.Auto)]
        public static extern int MsgBox(int hWnd, String text, String caption, uint type);

        [DllImport("winmm.dll")]
        public static extern int midiInGetNumDevs();

        [DllImport("winmm")]
        public static extern int midiOutGetNumDevs();

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        public static extern int midiInGetDevCaps(int uDeviceID,
                out MidiInCapabilities lpCaps, int cbMidiInCaps);

        [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
        public static extern int midiOutGetDevCaps(int uDeviceID,
                out MidiOutCapabilities lpMidiOutCaps, int cbMidiOutCaps);

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        public static extern int midiInOpen(out int lphMidiIn, int uDeviceID,
                        MidiInProc dwCallback, int dwInstance, int dwFlags);

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        public static extern int midiOutOpen(out int lphMidiOut, int uDeviceID,
                        MidiOutProc proc, int dwInstance, int dwFlags);

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        public static extern int midiOutShortMsg(int hMidiOut, int dwMsg);

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        public static extern int midiInStart(int hMidiIn);

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        public static extern int midiInStop(int hMidiIn);

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        public static extern int midiInClose(int hMidiIn);

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        public static extern int midiOutClose(int hMidiIn);

        [DllImport("winmm.dll")]
        public static extern int midiOutLongMsg(int hMidiOut, ref MidiHeader lpMidiOutHdr, int uSize);

        [DllImport("winmm.dll")]
        public static extern int midiOutPrepareHeader(int hMidiOut, ref MidiHeader lpMidiOutHdr, int uSize);

        [DllImport("winmm.dll")]
        public static extern int midiOutUnprepareHeader(int hMidiOut, ref MidiHeader lpMidiOutHdr, int uSize);

        [DllImport("winmm.dll")]
        public static extern int midiInPrepareHeader(int hMidiIn, IntPtr lpMidiInHdr, int uSize);

        [DllImport("winmm.dll")]
        public static extern int midiInAddBuffer(int hMidiIn, IntPtr lpMidiInHdr, int uSize);

        [DllImport("winmm.dll")]
        public static extern int midiInUnprepareHeader(int hMidiIn, IntPtr lpMidiInHdr, int uSize);
    }
}
