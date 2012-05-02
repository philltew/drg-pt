using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticlesOpt3
{
	// Additional info shared across a class of particles  
	public class ParticleInfo
	{
		public readonly int ID;
		public readonly string Name;
		public readonly double Mass;
		
		public int Color;
		public double Radius;
		public double InitialRadius;

		public double AttractiveOrRepulsive;
		public bool IsSoundOn;
		public bool Enabled;
		public bool ActiveParticle;
		
		public int MidiOutput;
		public int Note;

		public readonly double[] MinimumDistance;
		public readonly double[] LJenergyTermA;
		public readonly double[] LJenergyTermB;
		public readonly double[] LJgradientTermA;
		public readonly double[] LJgradientTermB;


		public ParticleInfo(int id, string name, double mass)
		{
			this.ID = id; 

			this.Mass = mass;

			this.Radius = Mass * 0.5;
			this.InitialRadius = Mass * 0.5;

			this.MinimumDistance = new double[ParticlePalette.MAX_PARTICLE_TYPES];
			this.LJenergyTermA = new double[ParticlePalette.MAX_PARTICLE_TYPES];
			this.LJenergyTermB = new double[ParticlePalette.MAX_PARTICLE_TYPES];
			this.LJgradientTermA = new double[ParticlePalette.MAX_PARTICLE_TYPES];
			this.LJgradientTermB = new double[ParticlePalette.MAX_PARTICLE_TYPES];
		}
	}

}
