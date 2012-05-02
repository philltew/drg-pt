using System;
//using System.Collections;
//using System.Collections.Specialized;
//using System.Xml;
//using System.Drawing;

namespace Rvg.Win32.Midi
{
    #region Midi Data
    // MIDI STATUS
	public enum MidiStatus : byte 
	{ 
		None			= 0,
		NoteOff         = 0x80,
		NoteOn          = 0x90,
		PolyKeyPressure = 0xa0,
		ControlChange   = 0xb0,
		ProgramChange   = 0xc0,
		AfterTouch      = 0xd0,
		PitchBend       = 0xe0,
		SystemMsgs      = 0xf0,
		MidiTimeCode	= 0xf1,
		SongPositionPtr	= 0xf2,
		SongSelect		= 0xf3,
		TuneRequest		= 0xf6,
		EOX				= 0xf7,
		TimingClock		= 0xf8,
		Start			= 0xfa,
		Continue		= 0xfb,
		Stop			= 0xfc,
		ActiveSense		= 0xfe,
		SystemReset		= 0xff
	}
	public interface IMidiChannelMap
	{
		uint	Bitmap { get; set; }		// Bitmap of enabled channels (1 - 16 = bits d0 - d15)
		bool	IsChannelSet(int chan);		// Query by 1-based ordinal if a particular channel bit is set.
		void	SetChannel(int chan);		// Set a channel bit by 1-based ordinal.
		void	ClearChannel(int chan);		// Clear a channel bit by 1-based ordinal.
		int		LowestChannelSet();			// Return the 1-based ordinal of the lowest channel set
		int		HighestChannelSet();		// Return the 1-based ordinal of the highest channel set
		int		NumChannelsSet();			// Return the # of channel bits set (0 - 16).
	}
	public interface IMidiData
	{
		int RawData { get; }
		byte Byte1 { get; }
		byte Byte2 { get; }
		byte KeyNum { get; }
		byte Velocity { get; }
		byte ProgramNum { get; }
		MidiStatus Status { get; }
		byte Channel { get; }
		uint TimeStamp { get; }
		bool IsNoteOn { get; }
		bool IsNoteOff { get; }
		bool IsPolyKeyPressure { get; }
		bool IsControlChange { get; }
		bool IsProgramChange { get; }
		bool IsChannelPressure { get; }
		bool IsPitchBend { get; }
		bool	IsMTC { get;  }
		bool	IsSongPositionPtr { get; }
		bool	IsSongSelect { get; }
		bool	IsTuneRequest { get; }
		bool	IsTimingClock { get; }
		bool	IsStart { get; }
		bool	IsContinue { get; }
		bool	IsStop { get; }
		bool	IsActiveSense { get; }
		bool	IsSystemReset { get; }
	}

	/// <summary>
	/// Provides data for the <b>SysExReceived</b> event.
	/// </summary>
	public interface IMidiDataEventArgs // : EventArgs
	{
		IMidiData Data { get; }
		uint TimeStamp { get; }
	}
	#endregion

	#region Midi Ports
    /// <summary>
    /// Midi port interface
    /// </summary>
	public interface IMidiPort
	{
        /// <summary>
        /// Return the name provided by MidiInCaps
        /// </summary>
		string Name { get; }
	}
	//----------------------------------------------------------------------
	/// <summary>
	/// IMidiInputPort represents a MIDI input port.
	/// It will establish a Win32 API MIDI callback.
	/// </summary>
	public interface IMidiInputPort : IMidiPort
	{

	}
	//----------------------------------------------------------------------
	/// <summary>
	/// IMidiOutputPort represents a MIDI output port.
	/// </summary>
	public interface IMidiOutputPort : IMidiPort
	{
        /// <summary>
        /// Write data to this MIDI output port
        /// </summary>
        /// <param name="status">MIDI status byte</param>
        /// <param name="b1">1st MIDI data byte</param>
        /// <param name="b2">2nd MIDI data byte</param>
        /// <returns></returns>
		int Write(byte status, byte b1, byte b2);
        /// <summary>
        /// Write a MIDI System Exclusive message to this MIDI output port
        /// </summary>
        /// <param name="message">MIDI System Exclusive message</param>
        /// <returns></returns>
		int WriteSysex(ISysExMessage message);
        /// <summary>
        /// Set true after port has been opened.
        /// </summary>
		bool IsOpen { get; }
	}
	#endregion

	#region SYSTEM EXCLUSIVES
	public enum SysExType
	{
		/// <summary>
		/// Represents the start of system exclusive message type.
		/// </summary>
		Start = 0xF0,

		/// <summary>
		/// Represents either the continuation or the escape system 
		/// exclusive message type.
		/// </summary>
		Special = 0xF7
	}

	public interface ISysExMessage //: IMidiMessage
	{
		byte this[int index] { get; set; }
		int Length { get; }
		string Message { get; }
		SysExType Type { get; }
		object Clone();
		int Status { get; }
	}
	public interface ISysExEventArgs // : EventArgs
	{
		ISysExMessage Message { get; }
		uint TimeStamp { get; }
	}

	#endregion
}
