using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace DSParticles3
{
	/// <summary>
	/// Additional info shared across all particles of the same atomic type
	/// </summary>
	public class AtomicInfo
	{
		public readonly int ID;
		public readonly string Name;
		public readonly double Mass;

		// the 3 component color for the particle 
		public Color3 Color;
		// the 4 component render color, the alpha channel (the last one) is the radius
		public Vector4 RenderColor;
		// the 4 component render color for when the particle collides with another particle, the alpha channel (the last one) is the radius
		public Vector4 ParticleCollisionColor;
		// the 4 component render color for when the particle collides with a wall, the alpha channel (the last one) is the radius
		public Vector4 WallCollisionColor;	

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
			

		public AtomicInfo(int id, string name, double mass)
		{
			this.ID = id;
			this.Name = name;
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
