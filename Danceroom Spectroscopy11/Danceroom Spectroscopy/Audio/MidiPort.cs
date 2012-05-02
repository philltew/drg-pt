using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections;
using System.Text;
using System.Diagnostics;
using Rug.Cmd;

namespace Rvg.Win32.Midi
{

    /// <summary>
    /// Used by actions to receive an object as data
    /// </summary>
    public class ObjectEventArgs : EventArgs
    {
        public object obj;
        public ObjectEventArgs(object o)
        {
            obj = o;
        }
    }

	#region MidiPort
	//----------------------------------------------------------------------
	/// <summary>
	/// MidiPort is the parent class for MidiInputPort and MidiOutputPort.
	/// </summary>
	public class MidiPort : IDisposable
	{
		internal int m_PortIndex;
        /// <summary>
        /// Midi handle after open
        /// </summary>
        internal int m_MidiHandle = -1;		
		//protected bool alreadyDisposed = false;
        /// <summary>
        /// Name of this device. Name returned by MidiInCaps 
        /// </summary>
		protected string m_Name;	

		/// <summary>
		/// MidiPort Constructor
		/// </summary>
		/// <param name="i">Port index</param>
		public MidiPort(int i)
		{
			m_PortIndex = i;
            m_MidiHandle = -1;
		}
		/// <summary>
		/// Dispose method
		/// </summary>
		public void Dispose()
		{
			RC.WriteLine("MidiPort: Dispose called");
		}

		/// <summary>
		/// Name property
		/// </summary>
		public string Name
		{
			get { return m_Name; }
		}
	}
	#endregion

	//----------------------------------------------------------------------
	/// <summary>
	/// MidiInputPort represents a MIDI input port.
	/// It will establish a Win32 API MIDI callback.
	/// TODO: Add SysEx support
	/// </summary>
	public class MidiInputPort : MidiPort, IMidiDynamicEvent, IMidiInputPort
	{
		private const int HeaderCount = 4;
		private const int SysExBufferSize = 32000;

		static int NumOpenInputPorts = 0;
		internal static int TotalNumMidiCallbacksReceived = 0;
		internal int NumMidiCallbacksReceived;
		internal MidiInProc m_InputMethod;
		internal MidiData m_PreviousMidiData;
		internal MidiData m_LastMidiData;

		// Midi headers for storing system exclusive messages.
        private MidiLibWrap.MidiHeader[] m_Headers = new MidiLibWrap.MidiHeader[HeaderCount];

		// Pointers to headers. 
		private IntPtr[] m_HeadersPointers = new IntPtr[HeaderCount];        

		// Thread for processing system exclusive headers.
		private Thread m_SysExHeaderThread;

		// Queue for storing system exclusive headers ready to be processed.
		private Queue m_SysExHeaderQueue;

		private readonly object m_LockObject = new object();

        /// <summary>
        /// OnMidiInputPortData is fired whenever MIDI data is received for this port.
        /// </summary>
        /// <remarks>This event is created dynamically using the actual port name</remarks>
        //[ProjectMidiEventAttribute("MIDI input port",1,"",ConnectionType.Normal)]
        public event MidiEventHandler OnMidiInputPortData;

        // NOTE: Race condition.  Not being set during startup, but works ok if connected after started.
		#region IMidiDynamicEvent
        /// <summary>
        /// Set an event handler to receive input MIDI data.
        /// </summary>
        /// <param name="handler">The event handler to be called when MIDI data is received</param>
		public void SetEventHandler(MidiEventHandler handler )
		{
            //midi.TraceMessage("MidiPort: SetEventHandler()");
			OnMidiInputPortData += handler;
		}
        /// <summary>
        /// Remove a previously set Event Handler
        /// </summary>
        /// <param name="handler"></param>
		public void RemoveEventHandler(MidiEventHandler handler )
		{
            //midi.TraceMessage("MidiPort: RemoveEventHandler()");
			OnMidiInputPortData -= handler;
		}
		#endregion

		#region CTOR
        /// <summary>
        /// MIDI Input Port object constructor
        /// </summary>
        /// <param name="i"></param>
		public MidiInputPort(int i /*,Midi midi*/) : base(i /*, midi*/)
		{
			int dwInstance = i;			// This can be any data we want
			NumMidiCallbacksReceived = 0;
			int dwFlags = MidiLibWrap.CALLBACK_FUNCTION;
			try
			{
				MidiLibWrap.MidiInCapabilities midiInCaps = new MidiLibWrap.MidiInCapabilities();
				MidiLibWrap.midiInGetDevCaps(i, out midiInCaps, Marshal.SizeOf( typeof(MidiLibWrap.MidiInCapabilities)));					
				m_Name = midiInCaps.Name;
				RC.WriteLine(string.Format("Input port name = {0}", m_Name));

				m_SysExHeaderQueue = Queue.Synchronized(new Queue());

				m_InputMethod = new MidiInProc( MidiCallback );
				int rc = MidiLibWrap.midiInOpen(out m_MidiHandle, m_PortIndex, m_InputMethod, dwInstance, dwFlags);

				CreateHeaders();

				// Initializes headers for system exclusive messages.
				for(int j = 0; j < HeaderCount; j++)
				{ 
					// Reset flags.
					m_Headers[j].flags = 0;

					// Imprint header structure onto raw memory.
					Marshal.StructureToPtr(m_Headers[j], m_HeadersPointers[j], false); 

					// Prepare header.
					MidiLibWrap.midiInPrepareHeader(m_MidiHandle, m_HeadersPointers[j], Marshal.SizeOf(typeof(MidiLibWrap.MidiHeader)));

					// Add header to buffer.
					MidiLibWrap.midiInAddBuffer(m_MidiHandle, m_HeadersPointers[j], 
						Marshal.SizeOf(typeof(MidiLibWrap.MidiHeader)));                  
				}

				// Clear system exclusive header queue.
				m_SysExHeaderQueue.Clear();

				// Create thread for processing system exclusive headers.
				m_SysExHeaderThread = 
					new Thread(new ThreadStart(ManageSysExHeaders));

				// Start worker thread.
				m_SysExHeaderThread.Start();

				rc = MidiLibWrap.midiInStart( m_MidiHandle );
			}
			catch( Exception e )
			{
				RC.WriteLine(string.Format("Error: {0}", e.Message));
			}
			NumOpenInputPorts++;
		}
		// TODO: create a Dispose method to close ports.
        /// <summary>
        /// MIDI Input Port object dispose method
        /// </summary>
		~MidiInputPort()
		{
			Dispose();
			GC.KeepAlive( m_InputMethod );
		}
		#endregion

        /// <summary>
        /// Static MIDI callback. We'll convert this to an instance method "inPort.InstanceMidiCallback"
        /// </summary>
        /// <param name="hMidiIn">MIDI port handle</param>
        /// <param name="Msg">MIDI message</param>
        /// <param name="dwInstance">Instance data</param>
        /// <param name="dwParam1"></param>
        /// <param name="dwParam2"></param>
        /// <remarks>This is static because this is a callback</remarks>
		static void MidiCallback( int hMidiIn, uint Msg, uint dwInstance, uint dwParam1, uint dwParam2  )
		{
			try
			{
				TotalNumMidiCallbacksReceived++;
				//MidiInputPort inPort = MidiMaster.midiInputPorts[dwInstance];
				//if(inPort != null) inPort.InstanceMidiCallback( hMidiIn, Msg, dwInstance, dwParam1, dwParam2 );
			}
			catch(Exception e)
			{
				RC.WriteLine(string.Format("Error in static MidiCallback: {0}", e.Message));
			}
		}
        /// <summary>
        /// Instance MIDI callback. Called from the static MIDI callback.
        /// </summary>
        /// <param name="hMidiIn"></param>
        /// <param name="msg"></param>
        /// <param name="dwInstance"></param>
        /// <param name="dwParam1"></param>
        /// <param name="dwParam2"></param>
		void InstanceMidiCallback(int hMidiIn, uint msg, uint dwInstance, uint dwParam1, uint dwParam2)
		{
			try
			{
				NumMidiCallbacksReceived++;
				if(msg == MidiLibWrap.MIM_DATA)
				{
					// Handle input data
					m_PreviousMidiData = m_LastMidiData;
					m_LastMidiData = new MidiData( dwParam1, dwParam2 );
					if(m_LastMidiData.IsActiveSense==false && m_LastMidiData.IsTimingClock==false)
					{
						RC.WriteLine(string.Format("InstanceMidiCallback: {0:A}", m_LastMidiData));
					}

					// Fire our specific Input Data Event
					if(OnMidiInputPortData!=null)
					{
						ObjectEventArgs e = new ObjectEventArgs( m_LastMidiData );
						OnMidiInputPortData( this, e );
					}

					// Fire the All Midi Input Data event
                    //ProjectMidi.ProjectMidiParent.FireAllMidiInputData(lastMidiData as IMidiData, dwParam1);

				}
				//else if(msg == MIM_ERROR)
				//{
				//	DispatchInvalidShortMsg(param1, param2);
				//}
				else if(msg == MidiLibWrap.MIM_LONGDATA)
				{
					ManageSysExMessage(dwParam1, dwParam2);
				}
			}
			catch(Exception e)
			{
				RC.WriteLine(string.Format("Error in InstanceMidiCallback: {0}", e.Message));
			}
		}

        /// <summary>
        /// Manage System Exclusive messages
        /// </summary>
        /// <param name="param1"></param>
        /// <param name="timeStamp"></param>
		private void ManageSysExMessage(uint param1, uint timeStamp)
		{
			// Get pointer to header.
			IntPtr ptrHeader = new IntPtr(param1);

			// Imprint raw pointer on to structure.
			MidiLibWrap.MidiHeader header = (MidiLibWrap.MidiHeader)Marshal.PtrToStructure(ptrHeader, typeof(MidiLibWrap.MidiHeader));
                
			// Dispatches system exclusive messages.
			DispatchSysExMessage(header, timeStamp);

			// Enqueue next system exclusive header and signal the worker queue
			// that another header is ready to be processed.
			lock(m_LockObject)
			{
				m_SysExHeaderQueue.Enqueue(ptrHeader);
				Monitor.Pulse(m_LockObject);
			}
		}

        /// <summary>
        /// Dispatch a MIDI System Exclusive message
        /// </summary>
        /// <param name="header"></param>
        /// <param name="timeStamp"></param>
        private void DispatchSysExMessage(MidiLibWrap.MidiHeader header, uint timeStamp)
		{
			// Create array for holding system exclusive data.
			byte[] data = new byte[header.bytesRecorded - 1];

			// Get status byte.
			byte status = Marshal.ReadByte(header.data);
                
			// Copy system exclusive data into array (status byte is excluded).
			for(int i = 1; i < header.bytesRecorded; i++)
			{
				data[i - 1] = Marshal.ReadByte(header.data, i);
			}

			// Create message.
			SysExMessage msg = new SysExMessage((SysExType)status, data);

			// Raise event.
			//SysExReceived(this, new SysExEventArgs(msg, timeStamp));

			// Fire our specific Input Data Event
			if(OnMidiInputPortData!=null)
			{
				ObjectEventArgs e = new ObjectEventArgs( msg );
				OnMidiInputPortData( this, e );
			}

			// Fire the All Midi Input Data event
            //ProjectMidi.ProjectMidiParent.FireAllSysExInputData(msg as ISysExMessage, timeStamp);
		}

		/// <summary>
		/// Unprepares/prepares and adds a MIDIHDR header back to the buffer to
		/// record another system exclusive message.
		/// </summary>
		private void ManageSysExHeaders()
		{
			lock(m_LockObject)
			{
				Monitor.Wait(m_LockObject);

				while(m_SysExHeaderQueue.Count > 0)
				{
					IntPtr header = (IntPtr)m_SysExHeaderQueue.Dequeue();

					// Unprepare header.
					int result = MidiLibWrap.midiInUnprepareHeader(m_MidiHandle, header, 
						Marshal.SizeOf(typeof(MidiLibWrap.MidiHeader))); 
        
					if(result == MidiLibWrap.MMSYSERR_NOERROR)
					{
						// Prepare header to be used again.
						result = MidiLibWrap.midiInPrepareHeader(m_MidiHandle, header, 
							Marshal.SizeOf(typeof(MidiLibWrap.MidiHeader))); 
					}

					if(result == MidiLibWrap.MMSYSERR_NOERROR)
					{ 
						// Add header back to buffer.
						result = MidiLibWrap.midiInAddBuffer(m_MidiHandle, header, 
							Marshal.SizeOf(typeof(MidiLibWrap.MidiHeader)));
					}

					if(result != MidiLibWrap.MMSYSERR_NOERROR)
					{
						// Raise event letting clients know an error has occurred.
//						if(SysExHeaderErrorOccurred != null)
//						{
//							InputDeviceException ex = new InputDeviceException(result);
//							SysExHeaderErrorOccurred(this, 
//								new SysExHeaderErrorEventArgs(ex.Message));
//						}
					}
				}
			}
		}

        /// <summary>
        /// Dispose method. Unprepare and free headers.
        /// </summary>
		public new void Dispose()
		{
            if (/*alreadyDisposed==false &&*/ m_MidiHandle != -1)
			{
				int rc = MidiLibWrap.midiInStop( m_MidiHandle );
				RC.WriteLine(string.Format("midiInClose {0}",m_PortIndex));
				UnprepareHeaders();
				DestroyHeaders();
				rc = MidiLibWrap.midiInClose(m_MidiHandle);
                m_MidiHandle = -1;
			}
			if(m_InputMethod==null)
				RC.WriteLine("Uh-oh, inProc was released");
			NumOpenInputPorts--;
			GC.KeepAlive( m_InputMethod );
		}

		/// <summary>
		/// Create headers for system exclusive messages.
		/// </summary>
		private void CreateHeaders()
		{
			// Create headers.
			for(int i = 0; i < HeaderCount; i++)
			{
				// Initialize headers and allocate memory for system exclusive data.
				m_Headers[i].bufferLength = SysExBufferSize;
				m_Headers[i].data = Marshal.AllocHGlobal(SysExBufferSize);

				// Allocate memory for pointers to headers. This is necessary 
				// to insure that garbage collection doesn't move the memory 
				// for the headers around while the input device is open.
				m_HeadersPointers[i] = 
					Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MidiLibWrap.MidiHeader)));
			}
		}

		/// <summary>
		/// Destroy headers.
		/// </summary>
		private void DestroyHeaders()
		{
			// Free memory for headers.
			for(int i = 0; i < HeaderCount; i++)
			{
				Marshal.FreeHGlobal(m_Headers[i].data);
				Marshal.FreeHGlobal(m_HeadersPointers[i]);
			}
		}

		/// <summary>
		/// Unprepares headers.
		/// </summary>
		private void UnprepareHeaders()
		{
			// Unprepare each Midi header.
			for(int i = 0; i < HeaderCount; i++)
			{
				MidiLibWrap.midiInUnprepareHeader(m_MidiHandle, m_HeadersPointers[i], 
					Marshal.SizeOf(typeof(MidiLibWrap.MidiHeader)));
			}
		}        
	
	
	}

	#region MidiOutputPort
	//----------------------------------------------------------------------
	/// <summary>
	/// MidiOutputPort represents a MIDI output port.
	/// </summary>
	public class MidiOutputPort : MidiPort, IMidiDynamicAction
	{
		private const int MOM_DONE = 0x3C9;
		private const int MMSYSERR_NOERROR = 0;
		private const int CALLBACK_FUNCTION = 0x30000;

		// Indicates whether or not the device is open.
		private bool m_Opened = false;

		// Represents the method handles messages from Windows.
		private delegate void MidiOutProc(int hMidi, 
											int msg, 
											int instance,
											int param1, 
											int param2); 

		// Represents the method that handles messages from Windows.
        private MidiLibWrap.MidiOutProc m_MessageHandler;

		// Thread for managing headers.
		private Thread m_HeaderManager;

		// Event used to signal when the device is done with a header.
		private AutoResetEvent m_ResetEvent = new AutoResetEvent(false);

		// Queue for storing headers.
		private Queue m_HeaderQueue = new Queue();

		// Synchronized queue.
		private Queue m_SyncHeaderQueue; 

        /// <summary>
        /// MIDI Output Port
        /// </summary>
        /// <param name="i">Port Index</param>
		public MidiOutputPort(int i) : base(i)
		{
			int dwInstance = 0;			// TODO:
			//int dwFlags = 0;			// TODO:
			RC.WriteLine(string.Format("midiOutOpen {0}",m_PortIndex));

			// Create delegate for handling messages from Windows.
			m_MessageHandler = new MidiLibWrap.MidiOutProc(OnMessage);

			// Create synchronized queue for holding headers.
			m_SyncHeaderQueue = Queue.Synchronized(m_HeaderQueue);

			MidiLibWrap.MidiOutCapabilities midiOutCaps = new MidiLibWrap.MidiOutCapabilities();
			MidiLibWrap.midiOutGetDevCaps(i, out midiOutCaps, Marshal.SizeOf(typeof(MidiLibWrap.MidiOutCapabilities)));
			m_Name = midiOutCaps.Name;
			RC.WriteLine(string.Format("Output port name = {0}", m_Name));

			int rc = MidiLibWrap.midiOutOpen(out m_MidiHandle, m_PortIndex, 
				m_MessageHandler, dwInstance, MidiLibWrap.CALLBACK_FUNCTION);
				//0, dwInstance, dwFlags);

			// Indicate the the device is now open.
			m_Opened = true;

			// Create thread for managing headers.
			m_HeaderManager = new Thread(new ThreadStart(ManageHeaders));

			// Start thread.
			m_HeaderManager.Start();

			// Wait for thread to become active.
			while(!m_HeaderManager.IsAlive)
				continue;

			// Keep track of device Identifier.
			//this.deviceID = deviceID;            

		}

        /// <summary>
        /// MIDI output port Dispose method
        /// </summary>
		~MidiOutputPort()
		{
			Dispose();
		}

		/// <summary>
		/// Handles messages from Windows.
		/// </summary>
		private void OnMessage(int hMidi, int msg, int instance, int param1, int param2)
		{
			// If the device has finished sending a system exclusive 
			// message.
			if(msg == MOM_DONE)
			{
				// If the device is open
				if(hMidi != -1)
				{
					// Signal header thread that the device has finished with 
					// a header.
					m_ResetEvent.Set();
				}
			}
		}

        /// <summary>
        /// MIDI Output Port Write method
        /// </summary>
        /// <param name="status">MIDI Status byte</param>
        /// <param name="b1">Optional 1st MIDI data byte</param>
        /// <param name="b2">Optional 2nd MIDI data byte</param>
        /// <returns></returns>
		public int Write(byte status, byte b1, byte b2)
		{
			int dwData = status + (b1<<8) + (b2<<16);
            return MidiLibWrap.midiOutShortMsg(m_MidiHandle, dwData);
		}
        /// <summary>
        /// MIDI Output Port Write method
        /// </summary>
        /// <param name="data">IMidiData data object</param>
        /// <returns></returns>
		public int Write(IMidiData data)
		{
			int dwData = data.RawData;
            return MidiLibWrap.midiOutShortMsg(m_MidiHandle, dwData);
		}

        /// <summary>
        /// IMidiDynamicAction
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
		public void ActionHandler( object source, EventArgs e )
		{
			ObjectEventArgs eObj = e as ObjectEventArgs;
			if(eObj==null) return;
			IMidiData midiData = eObj.obj as IMidiData;
			if(midiData != null)
			{
				Write( midiData );
			}

			SysExMessage sysEx = eObj.obj as SysExMessage;
			if(sysEx != null)
			{
				WriteSysex( sysEx );
			}
		}

		/// <summary>
		/// WriteSysex
		/// </summary>
		/// <returns>int count of data bytes written</returns>
		public int WriteSysex(ISysExMessage message)
		{
			int rc = 0;
			RC.WriteLine("WriteSysex");

			// Guard.
			if(!IsOpen)
				return rc;

			// Create header.
			MidiLibWrap.MidiHeader header = new MidiLibWrap.MidiHeader();

			// System exclusive message data.
			string msg = message.Message;

			//
			// Initialize header.
			//
			header.data = Marshal.StringToHGlobalAnsi(msg);
			header.bufferLength = msg.Length;
			header.flags = 0;

			// Prepare header.
			ThrowOnError(MidiLibWrap.midiOutPrepareHeader(m_MidiHandle, ref header, 
				Marshal.SizeOf(header)));
                
			// Place header in queue to be retrieved later.
			m_SyncHeaderQueue.Enqueue(header);

			// Send message.
			ThrowOnError(MidiLibWrap.midiOutLongMsg(m_MidiHandle, ref header, 
				Marshal.SizeOf(header)));

			return rc;
		}

		/// <summary>
		/// Throws exception on error.
		/// </summary>
		/// <param name="errCode">
		/// The error code. 
		/// </param>
		private static void ThrowOnError(int errCode)
		{
			// If an error occurred
			if(errCode != MMSYSERR_NOERROR)
			{
				// Throw exception.
				throw new OutputDeviceException(errCode);
			}
		}

		// 
		public new void Dispose()
		{
            if (m_MidiHandle != -1 /*&& alreadyDisposed==false*/)
			{
				m_Opened = false;
				//Console.WriteLine("midiOutClose {0}",PortIndex);
				m_ResetEvent.Set();				// Call the thread to run so it can terminate
				int rc = MidiLibWrap.midiOutClose(m_MidiHandle);
                m_MidiHandle = -1;
			}
		}

		/// <summary>
		/// Thread method for managing headers.
		/// </summary>
		private void ManageHeaders()
		{
			// While the device is open.
			while(IsOpen)
			{
				// Wait to be signalled when a header had finished being used.
				m_ResetEvent.WaitOne();

				// While there are still headers in the queue.
				while(m_SyncHeaderQueue.Count > 0)
				{
					RC.WriteLine("Thread running: processing header");
					// Get header from the front of the queue.
                    MidiLibWrap.MidiHeader header = (MidiLibWrap.MidiHeader)m_SyncHeaderQueue.Dequeue();

					// Unprepare header.
					int result = MidiLibWrap.midiOutUnprepareHeader(m_MidiHandle, ref header, 
						Marshal.SizeOf(header));

					// If an error occurred with unpreparing the system 
					// exclusive header.
					if(result != MMSYSERR_NOERROR)
					{
                        RC.WriteLine(string.Format("Error in ManageHeaders {0}", result));
//						if(SysExHeaderErrorOccurred != null)
//						{
//							OutputDeviceException ex = 
//								new OutputDeviceException(result);
//
//							SysExHeaderErrorOccurred(this, 
//								new SysExHeaderErrorEventArgs(ex.Message));
//						}
					}

					// Free memory allocated for the system exclusive data.
					Marshal.FreeHGlobal(header.data);
				}
			}
		}

		public bool IsOpen { get { return m_Opened; } }

        private int m_References = 0;

        public int References 
        {
            get
            {
                return m_References;
            }
        }

        public void AddReference()
        {
            m_References++;
        }

        public void RemoveReference()
        {
            m_References--;
        }
    }
	/// <summary>
	/// The exception that is thrown when a error occurs with the OutputDevice
	/// class.
	/// </summary>
	public class OutputDeviceException : ApplicationException
	{
		#region OutputDeviceException Members

		#region Win32 Midi Output Error Function

		[DllImport("winmm.dll")]
		private static extern int midiOutGetErrorText(int errCode, 
			StringBuilder message, int sizeOfMessage);

		#endregion

		#region Fields

		// Error message.
		private StringBuilder message = new StringBuilder(128);

		#endregion

		#region Construction

		/// <summary>
		/// Initializes a new instance of the OutputDeviceException class with
		/// the specified error code.
		/// </summary>
		/// <param name="errCode">
		/// The error code.
		/// </param>
		public OutputDeviceException(int errCode)
		{
			// Get error message.
			midiOutGetErrorText(errCode, message, message.Capacity);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets a message that describes the current exception.
		/// </summary>
		public override string Message
		{
			get
			{
				return message.ToString();
			}
		}

		#endregion

		#endregion
	}
	#endregion
}
