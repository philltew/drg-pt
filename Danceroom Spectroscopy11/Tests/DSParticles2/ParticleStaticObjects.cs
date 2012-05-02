using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticles2
{
	public static class ParticleStaticObjects
	{
		public static ParticlePalette AtomPropertiesDefinition = new ParticlePalette();

		public static Random Random = new Random();

		public static double RandomDouble(double min, double max)
		{
			return min + (Random.NextDouble() * (max - min)); 
		}

		public static int RandomInt(int min, int max)
		{
			return Random.Next(min, max); 
		}

		public static void ReSeedRandom(int seed)
		{
			Random = new Random(seed); 
		}
	}
}
