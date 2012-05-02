using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticlesOpt
{
	// Additional info shared across a class of particles  
	public class ParticleInfo
	{
		public int ID;
		//string Name; 
		public double Mass;
		public int Color;
		public double AttractiveOrRepulsive;
		public bool soundIsOn;

		public int MidiOutput;
		public int Note;

		public bool Enabled;
		public bool activeParticle;
	}

}
