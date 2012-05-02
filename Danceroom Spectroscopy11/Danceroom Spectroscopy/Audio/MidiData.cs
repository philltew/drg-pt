using System;
using System.Diagnostics;
using System.Text;

namespace Rvg.Win32.Midi
{
	#region MidiChannelMap
	public class MidiChannelMap : IMidiChannelMap, IFormattable
	{
		private uint bitmap;
		public uint Bitmap 
		{
			get { return bitmap; }
			set { bitmap = value; }
		}
		public void ClearChannel(int chan)
		{
			Debug.Assert(chan >= 1 && chan <= 16,"Invalid channel #","Channel must be 1 to 16");
			uint mask = (uint)(0xffff - (1 << (chan-1)));
			bitmap &= mask;
		}
		public void SetChannel(int chan)
		{
			Debug.Assert(chan >= 1 && chan <= 16,"Invalid channel #","Channel must be 1 to 16");
			bitmap |= (uint)(1 << (chan-1));
		}
		public int LowestChannelSet()
		{
			int chan = 0;
			if(bitmap!=0)
			{
				for(chan=1; chan<16; chan++)
					if( (bitmap & (1 << (chan-1)) ) != 0) break;
			}
			return chan;
		}
		public int HighestChannelSet()
		{
			int chan = 0;
			if(bitmap!=0)
			{
				for(chan=16; chan>0; chan--)
					if( (bitmap & (1 << (chan-1)) ) != 0) break;
			}
			return chan;
		}
		public bool IsChannelSet(int chan)
		{
			Debug.Assert(chan >= 1 && chan <= 16,"Invalid channel #","Channel must be 1 to 16");
			return (bitmap & (1 << (chan-1)))!=0;
		}
		public int NumChannelsSet()
		{
			int count = 0;
			for(int chan=1; chan<=16; chan++)
				if( (bitmap & (1 << (chan-1)) ) != 0) count++;
			return count;
		}
		public override string ToString()
		{
			return ToString("X",null);
		}
		// ToString supports "D" decimal, "H" or "X" hex
		public string ToString(string format, IFormatProvider formatProvider)
		{
			string result = null;
			int data = (int)bitmap;

			if(format == null) format = "X";

			switch(format.ToUpper()[0])
			{
				case 'D':			// Format decimal
					result = data.ToString();
					break;
				default:			// Format hex value
					string hexstr = data.ToString("X");
					result = "0x"+hexstr;
					break;
			}
			return result;
		}
	}
	#endregion

	#region MidiData
	/// <summary>
	/// This struct encapsulates MIDI data.  By design a value type.
	/// It is immutable and atomic.
	/// </summary>
	public struct MidiData : IMidiData, IFormattable
	{
		public enum ControlChange 
		{
			BankSelectMSB = 0,
			BankSelectLSB = 0x20
		}

		private byte status;
		private byte byte1;
		private byte byte2;
		private uint timestamp;

		// Constructors
		public MidiData( MidiStatus status, byte channel, byte byte1, byte byte2, uint ts )
		{
			this.status = (byte)(status+channel);
			this.byte1 = byte1;
			this.byte2 = byte2;
			timestamp = ts;
		}
		public MidiData( uint mididata, uint ts )
		{
			status = (byte)(mididata & 0xff);
			byte1 = (byte)((mididata>>8) & 0xff);
			byte2 = (byte)((mididata>>16) & 0xff);
			timestamp = ts;
		}
		public MidiData( byte status, byte byte1, byte byte2, uint ts )
		{
			this.status = status;
			this.byte1 = byte1;
			this.byte2 = byte2;
			timestamp = ts;
		}
		public MidiData( byte status, byte byte1, uint ts )
		{
			this.status = status;
			this.byte1 = byte1;
			this.byte2 = 0;
			timestamp = ts;
		}
		public MidiData( byte status, uint ts )
		{
			this.status = status;
			this.byte1 = 0;
			this.byte2 = 0;
			timestamp = ts;
		}

		public override string ToString()
		{
			return ToString("G",null);
		}
		public string ToString(string format, IFormatProvider formatProvider)
		{
			string result = null;

			if(format == null) format = "G";

			switch(format.ToUpper()[0])
			{
				case 'T':			// Timestamp only
					result = formatTimeStamp();
					break;
				case 'S':			// Status only
					result = formatStatus();
					break;
				case 'C':			// Channel only
					result = formatChannel();
					break;
				case 'D':			// Data only
					result = formatChannel();
					break;
				default:			// Format Status, Channel, Data
					result = formatTimeStamp()+": "+formatStatus()+", "+formatChannel()+", "+formatData();
					break;
			}
			return result;
		}
		private string formatTimeStamp()
		{
			string result = timestamp.ToString("X");
			return result;
		}
		private string formatStatus()
		{
			string result = "";
			if( IsNoteOn ) result = "Note On";
			else if( IsNoteOff ) result = "Note Off";
			else if( IsPolyKeyPressure ) result = "PolyKeyPressure";
			else if( IsControlChange ) result = "ControlChange";
			else if( IsProgramChange ) result = "ProgramChange";
			else if( IsChannelPressure ) result = "ChannelPressure";
			else if( IsPitchBend ) result = "PitchBend";
			else if( IsTimingClock ) result = "TimingClock";
			else if( IsStart ) result = "Start";
			else if( IsContinue ) result = "Continue";
			else if( IsStop ) result = "Stop";
			else if( IsActiveSense ) result = "ActiveSense";
			else if( IsSystemReset ) result = "SystemReset";
			return result;
		}
		private string formatChannel()
		{
			string result = "";
			if( IsNoteOn ) result = "Channel = "+(status & 0x0f);
			else if( IsNoteOff ) result = "Channel = "+(status & 0x0f);
			else if( IsPolyKeyPressure ) result = "Channel = "+(status & 0x0f);
			else if( IsControlChange ) result = "Channel = "+(status & 0x0f);
			else if( IsProgramChange ) result = "Channel = "+(status & 0x0f);
			else if( IsChannelPressure ) result = "Channel = "+(status & 0x0f);
			else if( IsPitchBend ) result = "Channel = "+(status & 0x0f);
			return result;
		}
		private string formatData()
		{
			string result = "";
			if( IsNoteOn ) result = "Key = "+byte1+", Velocity = "+byte2;
			else if( IsNoteOff ) result = "Key = "+byte1+", Velocity = "+byte2;
			else if( IsPolyKeyPressure ) result = "Key = "+byte1+", Pressure = "+byte2;
			else if( IsControlChange )
			{
				result = "Control = ";
				switch( byte1 )
				{
					case 0: result += "Bank select"; break;
					case 1:	result += "Mod wheel"; break;
					case 2:	result += "Breath controller"; break;
					case 4: result += "Foot controller"; break;
					case 5: result += "Portamento time"; break;
					case 6: result += "Data entry MSB"; break;
					case 7: result += "Main volume"; break;
					case 8: result += "Balance"; break;
					case 10: result += "Pan"; break;
					case 16: result += "GPC 1"; break;
					case 17: result += "GPC 2"; break;
					case 18: result += "GPC 3"; break;
					case 19: result += "GPC 4"; break;
					case 20: result += "Bank select"; break;
					case 64: result += "Sustain"; break;
					case 65: result += "Portamento"; break;
					case 66: result += "Sostenudo"; break;
					case 67: result += "Soft pedal"; break;
					case 68: result += "Legato footswitch"; break;
					case 69: result += "Hold 2"; break;
					case 78: result += "All sounds off"; break;
					case 91: result += "External effects depth"; break;
					case 92: result += "Tremolo depth"; break;
					case 93: result += "Chorus depth"; break;
					case 94: result += "Celeste (detune) depth"; break;
					case 95: result += "Phaser depth"; break;
					case 96: result += "Data increment"; break;
					case 97: result += "Data decrement"; break;
					case 98: result += "NRPN LSB"; break;
					case 99: result += "NRPN MSB"; break;
					case 100: result += "RPN LSB"; break;
					case 101: result += "RPN MSB"; break;
					case 121: result += "Reset all controllers"; break;
					case 122: result += "Local control"; break;
					case 123: result += "All notes off"; break;
					case 124: result += "Omni mode off"; break;
					case 125: result += "Omni Mode on"; break;
					case 126: result += "Mono mode on"; break;
					case 127: result += "Poly mode on"; break;
					default:
						result += byte1.ToString();
						break;
				}
				result += ", Value = "+byte2;
			}
			else if( IsProgramChange ) result = "Program = "+byte1;
			else if( IsChannelPressure ) result = "Pressure = "+byte1;
			else if( IsPitchBend ) result = "Value = "+((byte1*128)+byte2);
			return result;
		}

		#region Properties
		public int RawData { get { return (int)(status + (byte1<<8) + (byte2<<16)); } }
		public byte Byte1 { get { return byte1; } }
		public byte Byte2 { get { return byte2; } }
		public byte KeyNum { get { return byte1; } }
		public byte Velocity { get { return byte2; } }
		public byte ProgramNum { get { return byte1; } }
		public MidiStatus Status { get { return (MidiStatus)((byte)status & 0xf0); } }
		public byte Channel { get { return (byte) (status & 0x0f); } }
		public uint TimeStamp { get { return timestamp; } }
		public bool IsNoteOn { get { return ((byte)status & 0xf0) == 0x90; } }
		public bool IsNoteOff { get { return ((status & 0xf0) == 0x80) || ((status & 0xf0) == 0x90 && byte2==0); } }
		public bool IsPolyKeyPressure { get { return (status & 0xf0) == 0xa0; } }
		public bool IsControlChange { get { return (status & 0xf0) == 0xb0; } }
		public bool IsProgramChange { get { return (status & 0xf0) == 0xc0; } }
		public bool IsChannelPressure { get { return (status & 0xf0) == 0xd0; } }
		public bool IsPitchBend { get { return (status & 0xf0) == 0xe0; } }
		public bool	IsMTC { get{ return status == (byte)MidiStatus.MidiTimeCode; } }
		public bool	IsSongPositionPtr { get{ return status == (byte)MidiStatus.SongPositionPtr; } }
		public bool	IsSongSelect { get{ return status == (byte)MidiStatus.SongSelect; } }
		public bool	IsTuneRequest { get{ return status == (byte)MidiStatus.TuneRequest; } }
		public bool	IsTimingClock { get{ return status == (byte)MidiStatus.TimingClock; } }
		public bool	IsStart { get{ return status == (byte)MidiStatus.Start; } }
		public bool	IsContinue { get{ return status == (byte)MidiStatus.Continue; } }
		public bool	IsStop { get{ return status == (byte)MidiStatus.Stop; } }
		public bool	IsActiveSense { get{ return status == (byte)MidiStatus.ActiveSense; } }
		public bool	IsSystemReset { get{ return status == (byte)MidiStatus.SystemReset; } }
		#endregion
	}
	#endregion

	#region SysExMessage
	public class SysExMessage : ISysExMessage //: IMidiMessage
	{
		#region SysExMessage Members

		#region Fields

		// The system exclusive type.
		private SysExType type;

		// The system exclusive message data.
		private StringBuilder message;

		#endregion

		#region Construction

		/// <summary>
		/// Initializes a new instance of the SysExMessage class with the
		/// specified system exclusive type and data.
		/// </summary>
		/// <param name="type">
		/// The type of system exclusive message.
		/// </param>
		/// <param name="data">
		/// The system exclusive data.
		/// </param>
		public SysExMessage(SysExType type, byte[] data)
		{
			this.type = type;

			// If this is a regular system exclusive message.
			if(this.type == SysExType.Start)
			{
				// Create storage for message data which includes an extra byte
				// for the status value.
				message = new StringBuilder(data.Length + 1);
				message.Length = message.Capacity;

				// Store status value.
				message[0] = (char)this.type;
			}
				// Else this is a continuation message or an escaped message.
			else
			{
				message = new StringBuilder(data.Length);
			}

			// Copy data into message.
			for(int i = 0; i < data.Length; i++)
			{
				this[i] = data[i];
			}
		}

		/// <summary>
		/// Initializes a new instance of the SysExMessage class with 
		/// another instance of the SysExMessage class.
		/// </summary>
		/// <param name="message">
		/// The SysExMessage instance to use for initialization.
		/// </param>
		public SysExMessage(SysExMessage message)
		{
			type = message.type;

			this.message = new StringBuilder(message.Message);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Tests to see if a status value belongs to a system exclusive 
		/// message.
		/// </summary>
		/// <param name="status">
		/// The status value to test.
		/// </param>
		/// <returns>
		/// <b>true</b> if the status value belongs to a system exclusive 
		/// message; otherwise, <b>false</b>.
		/// </returns>
		public static bool IsSysExMessage(int status)
		{
			if(status == (int)SysExType.Start ||
				status == (int)SysExType.Special)
				return true;

			return false;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <remarks>
		/// Indexing this class allows access to the system exclusive data. 
		/// This is any element other than the status value, which cannot 
		/// be changed.
		/// </remarks>
		public byte this[int index]
		{
			get
			{
				// Enforce preconditions.
				if(index < 0 || index >= Length)
					throw new ArgumentOutOfRangeException("index", index,
						"Index into system exclusive message out of range.");

				byte value;

				// If this is a regular system exclusive message.
				if(Type == SysExType.Start)
				{
					// Offset by one so that the status byte isn't 
					// included.
					value = (byte)message[index + 1];
				}
					// Else this is a continuation or escaped system exclusive 
					// message.
				else
				{
					// No offset necessary.
					value = (byte)message[index];
				}

				return value;
			}            
			set
			{
				// Enforce preconditions.
				if(index < 0 || index >= Length)
					throw new ArgumentOutOfRangeException("index", index,
						"Index into system exclusive message out of range.");

				// If this is a regular system exclusive message.
				if(Type == SysExType.Start)
				{
					// Offset by one so that the status byte isn't 
					// overwritten.
					message[index + 1] = (char)value;
				}
					// Else this is a continuation or escaped system exclusive 
					// message.
				else
				{
					// Assign value at the specified index.
					message[index] = (char)value;
				}
			}
		}

		/// <summary>
		/// Gets the length of the system exclusive data.
		/// </summary>
		/// <remarks>
		/// The status byte is not included in the length of the system 
		/// exclusive message.
		/// </remarks>
		public int Length
		{
			get
			{
				int length;

				// If this is a regular system exclusive message
				if(Type == SysExType.Start)
				{
					// Do not include status value in data length.
					length = message.Length - 1;
				}
					// Else this is a continuation or escaped system exclusive 
					// message.
				else
				{
					// Data length is the length of the message.
					length = message.Length;
				}

				return length;
			}
		}

		/// <summary>
		/// Gets the system exclusive data.
		/// </summary>
		public string Message
		{
			get
			{
				return message.ToString();
			}
		}        

		/// <summary>
		/// Gets the system exclusive type.
		/// </summary>
		public SysExType Type
		{
			get
			{
				return type;
			}
		}

		#endregion

		#endregion

		#region ICloneable Members

		/// <summary>
		/// Creates a deep copy of this message.
		/// </summary>
		/// <returns>
		/// A deep copy of this message.
		/// </returns>
		public object Clone()
		{
			return new SysExMessage(this);
		}

		#endregion

		#region IMidiMessage Members

		//        /// <summary>
		//        /// Accepts a MIDI message visitor.
		//        /// </summary>
		//        /// <param name="visitor">
		//        /// The visitor to accept.
		//        /// </param>
		//        public void Accept(MidiMessageVisitor visitor)
		//        {
		//            visitor.Visit(this);
		//        }

		/// <summary>
		/// Gets the status value.
		/// </summary>
		public int Status
		{
			get
			{
				return (int)Type;
			}
		}

		#endregion
	}
	#endregion

}
