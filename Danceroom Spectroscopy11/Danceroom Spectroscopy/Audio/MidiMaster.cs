using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Rvg.Win32.Midi
{
    public static class MidiMaster
    {
        #region Inputs

        private static object m_OpenInputsLock = new object();
        private static Dictionary<string, MidiInputPort> m_OpenInputs = new Dictionary<string, MidiInputPort>(); 

        public static string[] GetInDeviceNames()
        {
            int inDevices = MidiLibWrap.midiInGetNumDevs();

            string[] InputDevices = new string[inDevices];

            for (int i = 0; i < inDevices; i++)
            {
                MidiLibWrap.MidiInCapabilities caps = new MidiLibWrap.MidiInCapabilities();

                MidiLibWrap.midiInGetDevCaps(i, out caps, Marshal.SizeOf(typeof(MidiLibWrap.MidiInCapabilities)));

                InputDevices[i] = caps.Name;
            }

            return InputDevices;
        }

        public static MidiInputPort GetInputPort(string deviceName)
        {
            lock (m_OpenInputsLock)
            {
                MidiInputPort port = null;

                if (m_OpenInputs.TryGetValue(deviceName, out port))
                {
                    return port;
                }

                string[] allNames = GetInDeviceNames();

                for (int i = 0; i < allNames.Length; i++)
                {
                    if (allNames[i] == deviceName)
                    {
                        port = new MidiInputPort(i);

                        m_OpenInputs.Add(deviceName, port);

                        return port;
                    }
                }
            }

            return null;
        }

        #endregion

        #region Outputs

        private static object m_OpenOutputsLock = new object();
        private static Dictionary<string, MidiOutputPort> m_OpenOutputs = new Dictionary<string, MidiOutputPort>();

        public static string[] GetOutDeviceNames()
        {
            int outDevices = MidiLibWrap.midiOutGetNumDevs();

            string[] OutDevices = new string[outDevices];

            for (int i = 0; i < outDevices; i++)
            {
                MidiLibWrap.MidiOutCapabilities caps = new MidiLibWrap.MidiOutCapabilities();

                MidiLibWrap.midiOutGetDevCaps(i, out caps, Marshal.SizeOf(typeof(MidiLibWrap.MidiOutCapabilities)));

                OutDevices[i] = caps.Name;
            }

            return OutDevices;
        }

        public static MidiOutputPort GetOutputPort(string deviceName) 
        {
            lock (m_OpenOutputsLock)
            {
                MidiOutputPort port = null;

                if (m_OpenOutputs.TryGetValue(deviceName, out port))
                {
                    if (port.IsOpen)
                    {
                        port.AddReference();
                        return port;
                    }
                    
                    port.Dispose();
                    port = null; 
                    m_OpenOutputs.Remove(deviceName);
                }

                string[] allNames = GetOutDeviceNames();

                for (int i = 0; i < allNames.Length; i++)
                {
                    if (allNames[i] == deviceName)
                    {
                        port = new MidiOutputPort(i);

                        if (port != null && port.IsOpen)
                        {
                            port.AddReference();
                            m_OpenOutputs.Add(deviceName, port);
                        }
                        else if (port != null)
                        {
                            port.Dispose();
                            port = null;
                        }
                        return port;
                    }
                }
            }

            return null;
        }

        public static void ReleaseOutputPort(MidiOutputPort port)
        {
            if (port != null)
            {
                lock (m_OpenOutputsLock)
                {
                    port.RemoveReference();

                    if (port.References <= 0)
                    {
                        port.Dispose();
                        m_OpenOutputs.Remove(port.Name); 
                    }
                }
            }
        }

        #endregion
    }
}
