using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticles3
{
	public abstract class ForceField
	{
		public abstract string ForceFieldType { get; }
		public abstract void CalculateEnergyTerms(ParticleEnsemble ensemble);
		public abstract void UpdateEnergyTerms(ParticleEnsemble ensemble);
		public abstract void CalculateForceField(ParticleEnsemble ensemble);
	}
}
