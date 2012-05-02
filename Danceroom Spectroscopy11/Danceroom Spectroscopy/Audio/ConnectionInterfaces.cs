using System;
using System.Collections;
using System.Reflection;

namespace Rvg.Win32.Midi
{
    public delegate void MidiEventHandler(object sender, EventArgs e);

	//public interface IPMAssemblyCollection : ICollection, IEnumerable {}

	#region IMidiDynamicEvent
    /// <summary>
    /// Interface used if the assembly contains dynamically created events
    /// </summary>
	public interface IMidiDynamicEvent
	{
        /// <summary>
        /// Method used to attach an action to the dynamically created event.
        /// </summary>
        /// <param name="handler"></param>
		void SetEventHandler(MidiEventHandler handler);
        /// <summary>
        /// Method used to remove a previously attached action.
        /// </summary>
        /// <param name="handler">MidiEventHandler</param>
		void RemoveEventHandler(MidiEventHandler handler);
	}
	#endregion

	#region	IMidiDynamicAction
    /// <summary>
    /// Interface used if the assembly contains dynamically created actions
    /// </summary>
    /// <remarks>Not used?</remarks>
	public interface IMidiDynamicAction
	{
        /// <summary>
        /// ActionHandler for dynamically created actions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		void ActionHandler( object sender, EventArgs e );
	}
	#endregion

    /*
	#region IPMAssembly
	public interface IPMAssembly
	{
		PMEventInfoCollection Events { get; }
		PMActionInfoCollection Actions { get; }
		Assembly Asm { get; }
		string Name { get; }
		string FilePath { get; }
		IProjectMidi IProjMidi { get; }
	}
	#endregion
     * */ 
}
