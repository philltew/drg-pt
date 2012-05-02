using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticlesOpt3
{
	public class SoftSpheres : ForceField
	{
		#region Private Members
		
		private DPVector[] LJforces;
		private List<double> WallDistance;
		private double epsilon;

		//private DPMatrix MinimumDistance;
		//private DPMatrix LJenergyTermA;
		//private DPMatrix LJenergyTermB;
		//private DPMatrix LJgradientTermA;
		//private DPMatrix LJgradientTermB;

		#endregion

		#region Public Properties
		
		public override string ForceFieldType { get { return "SoftSphereForceField"; } }

		#endregion

		#region Constructor

		public SoftSpheres(ParticleEnsemble pParticleSet)
		{  
			epsilon = 10.0;
	
			// allocate vectors holding positions & forces
			LJforces = new DPVector[pParticleSet.MaxNumberOfParticles];			
  
			// allocate vector holding cutoff distance for calculating Wall-Particle interactions
			WallDistance = new List<double>(new double[pParticleSet.MaxNumberOfParticles]);
  		
			// allocate vector holding cutoff distance for calculating particle-particle interaction
			//MinimumDistance = new DPMatrix(0, pParticleSet.MaxNumberOfParticles,pParticleSet.MaxNumberOfParticles);
	
			// allocate vectors holding particle-particle LJ energy terms
			//LJenergyTermA = new DPMatrix(0, pParticleSet.MaxNumberOfParticles,pParticleSet.MaxNumberOfParticles); 
			//LJenergyTermB = new DPMatrix(0, pParticleSet.MaxNumberOfParticles, pParticleSet.MaxNumberOfParticles);
			//LJgradientTermA = new DPMatrix(0, pParticleSet.MaxNumberOfParticles, pParticleSet.MaxNumberOfParticles);
			//LJgradientTermB = new DPMatrix(0, pParticleSet.MaxNumberOfParticles, pParticleSet.MaxNumberOfParticles);
  
			CalculateEnergyTerms(pParticleSet);
	
			// calculate initial forcefield
			CalculateForceField(pParticleSet);  
		}

		#endregion

		#region Calculate Energy Terms

		public override void CalculateEnergyTerms(ParticleEnsemble ensemble)
		{
			ParticleStaticObjects.AtomPropertiesDefinition.CalculateEnergyTerms(); 

			/*for (int i = 0; i < ensemble.MaxNumberOfParticles; ++i)
			{
				double radiusi = ensemble.GetParticle(i).Radius; 

				for (int j = (i + 1); j < ensemble.MaxNumberOfParticles; ++j)
				{
					double radiusj = ensemble.GetParticle(j).Radius;

					double MinDist = 2.0 * (radiusi + radiusj);

					MinimumDistance[i, j] = MinDist * MinDist;
					MinimumDistance[j, i] = MinimumDistance[i, j];
					LJenergyTermA[i, j] = epsilon * Math.Pow(MinDist, 12.0);
					LJenergyTermA[j, i] = LJenergyTermA[i, j];
					LJenergyTermB[i, j] = -2.0 * epsilon * Math.Pow(MinDist, 6.0);
					LJenergyTermB[j, i] = LJenergyTermB[i, j];
					LJgradientTermA[i, j] = -12.0 * LJenergyTermA[i, j];
					LJgradientTermA[j, i] = LJgradientTermA[i, j];
					LJgradientTermB[i, j] = -6.0 * LJenergyTermB[i, j];
					LJgradientTermB[j, i] = LJgradientTermB[i, j];
				}
			} */ 
		}

		#endregion

		#region Update Energy Terms

		public override void UpdateEnergyTerms(ParticleEnsemble ensemble)
		{
			ParticleStaticObjects.AtomPropertiesDefinition.CalculateEnergyTerms();

			/*
			for (int i = 0; i < ensemble.NumberOfParticles; ++i)
			{
				double radiusi = ensemble.GetParticle(i).Radius; 

				for (int j = (i + 1); j < ensemble.NumberOfParticles; ++j)
				{
					double radiusj = ensemble.GetParticle(j).Radius;

					double MinDistance = 2.0 * (radiusi + radiusj);

					MinimumDistance[i, j] = MinDistance * MinDistance;
					MinimumDistance[j, i] = MinimumDistance[i, j];
					LJenergyTermA[i, j] = epsilon * Math.Pow(MinDistance, 12.0);
					LJenergyTermA[j, i] = LJenergyTermA[i, j];
					LJenergyTermB[i, j] = -2.0 * epsilon * Math.Pow(MinDistance, 6.0);
					LJenergyTermB[j, i] = LJenergyTermB[i, j];
					LJgradientTermA[i, j] = -12.0 * LJenergyTermA[i, j];
					LJgradientTermA[j, i] = LJgradientTermA[i, j];
					LJgradientTermB[i, j] = -6.0 * LJenergyTermB[i, j];
					LJgradientTermB[j, i] = LJgradientTermB[i, j];
				}
			}
			*/ 
		}

		#endregion

		#region Calculate Force Field

		public override void CalculateForceField(ParticleEnsemble ensemble)
		{
			// variable declarations
			double posXi, posYi;
			double posXj, posYj;
			double ijSeparation = 0.0, LJforce = 0.0, PotentialEnergy = 0.0;
  
			// variable initializations
			int BoxHeight = ensemble.BoxHeight;
			int BoxWidth = ensemble.BoxWidth;
			double MaxForce = ensemble.MaxForceThreshold;
			double MinForce = -1.0 * (ensemble.MaxForceThreshold);

			DPVector zero = new DPVector(0, 0);

			// initialize vectors holding forces
			for (int i = 0; i < ensemble.NumberOfParticles; ++i)
			{
				LJforces[i] = zero;				
			}

			ensemble.ResetAllParticlesNotWithinRange(); 

			for (int i = 0; i < ensemble.NumberOfParticles; ++i)
			{
				Particle particlei = ensemble.GetParticle(i); 

				for (int j = (i + 1); j < ensemble.NumberOfParticles; ++j)
				{				
					Particle particlej = ensemble.GetParticle(j); 

					// get the interparticle separation distances
					ijSeparation = ensemble.GetInterParticleSeparation(i, j);

					double LJgradientTermA = particlei.ParticleType.LJgradientTermA[particlej.TypeID];
					double LJgradientTermB = particlei.ParticleType.LJgradientTermB[particlej.TypeID]; 
						
					// update the radial distribution function
					double cutoffDistance = particlei.ParticleType.MinimumDistance[particlej.TypeID]; // MinimumDistance[i, j];
			
					if (ijSeparation < cutoffDistance) 
					{						
						// for each particle, change the appropriate element of the setWithinRangeOfAnotherParticle vector to true
						posXi = particlei.Position.X; 
						posYi = particlei.Position.Y; 

						posXj = particlej.Position.X; 
						posYj = particlej.Position.Y; 
												
						LJforce = (posXj - posXi) * (LJgradientTermA / Math.Pow(ijSeparation, 13.0) + LJgradientTermB / Math.Pow(ijSeparation, 7.0)) / ijSeparation;

						if (Math.Abs(LJforce) > MaxForce || Math.Abs(LJforce) < MinForce)
						{
							// error check for real-time stability...
							LJforce = 0.0; 
						}    
						else if(double.IsNaN(LJforce) || double.IsInfinity(LJforce))
						{
							// error check for real-time stability...
							LJforce = 0.0;
						}

						LJforces[i].X += LJforce;
						LJforces[j].X += -1.0 * LJforce;
						LJforce = (posYj - posYi) * (LJgradientTermA / Math.Pow(ijSeparation, 13.0) + LJgradientTermB / Math.Pow(ijSeparation, 7.0)) / ijSeparation;

						if (Math.Abs(LJforce) > MaxForce || Math.Abs(LJforce) < MinForce) 
						{
							// error check for real-time stability...
							LJforce = 0.0; 
						}    
						else if (double.IsNaN(LJforce) || double.IsInfinity(LJforce)) 
						{
							// error check for real-time stability...
							LJforce = 0.0; 
						} 

						LJforces[i].Y += LJforce;
						LJforces[j].Y += -1.0 * LJforce;

						ensemble.SetParticlesWithinRange(i, j);
						ensemble.SetParticlesWithinRange(j, i);					
					}
					//else
					//{
					//	ensemble.SetParticlesNotWithinRange(i, j);
						//ensemble.SetParticlesNotWithinRange(j, i);			
					//}
				}        
			}
	
			// set the forces in the Particle Ensemble Object
			ensemble.AddForces(LJforces);

			// set the potential energy
			ensemble.AddPotentialEnergy(PotentialEnergy);
		}

		#endregion
	}
}
