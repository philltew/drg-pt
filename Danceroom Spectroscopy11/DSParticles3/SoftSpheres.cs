using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticles3
{
	public class SoftSpheres : ForceField
	{
		#region Private Members
		
		private double[] LJxforces;
		private double[] LJyforces;
		private double[] WallDistance;
		// private double epsilon;

		//private DPMatrix MinimumDistance;
		//private DPMatrix LJenergyTermA;
		//private DPMatrix LJenergyTermB;
		//private DPMatrix LJgradientTermA;
		//private DPMatrix LJgradientTermB;	

		#endregion

		#region Public Properties
		
		public override string ForceFieldType
		{
			get { return "SoftSphereForceField"; }
		}

		#endregion

		#region Constructor

		// constructor
		public SoftSpheres(ParticleEnsemble pParticleSet)
		{  
			//epsilon=10.0;
	
			// allocate vectors holding positions & forces
			LJxforces = new double[pParticleSet.MaxNumberOfParticles];
			LJyforces = new double[pParticleSet.MaxNumberOfParticles];
  
			// allocate vector holding cutoff distance for calculating Wall-Particle interactions
			WallDistance = new double[pParticleSet.MaxNumberOfParticles];
  		
			// allocate vector holding cutoff distance for calculating particle-particle interaction
			//Mat_DP tmp(0.0,pParticleSet->MaxNumberOfParticles,pParticleSet->MaxNumberOfParticles);
			//MinimumDistance = new DPMatrix(0, pParticleSet.MaxNumberOfParticles,pParticleSet.MaxNumberOfParticles);
	
			// allocate vectors holding particle-particle LJ energy terms
			//LJenergyTermA = new DPMatrix(0, pParticleSet.MaxNumberOfParticles,pParticleSet.MaxNumberOfParticles); // =tmp;
			//LJenergyTermB = new DPMatrix(0, pParticleSet.MaxNumberOfParticles, pParticleSet.MaxNumberOfParticles); // =tmp;
			//LJgradientTermA = new DPMatrix(0, pParticleSet.MaxNumberOfParticles, pParticleSet.MaxNumberOfParticles); // =tmp;
			//LJgradientTermB = new DPMatrix(0, pParticleSet.MaxNumberOfParticles, pParticleSet.MaxNumberOfParticles); // =tmp;
  
			CalculateEnergyTerms(pParticleSet);
	
			// calculate initial forcefield
			CalculateForceField(pParticleSet);
  
		}

		#endregion

		#region Energy Terms (Moved to Particle Pallette)

		public override void CalculateEnergyTerms(ParticleEnsemble ensemble)
		{
			ParticleStaticObjects.AtomPropertiesDefinition.CalculateEnergyTerms(); 

			/* 
			for (int i = 0; i < ensemble.MaxNumberOfParticles; ++i)
			{
				for (int j = (i + 1); j < ensemble.MaxNumberOfParticles; ++j)
				{
					double MinDistance = 2.0 * (ensemble.GetParticleRadius(i) + ensemble.GetParticleRadius(j));

					MinimumDistance[i, j] = MinDistance;
					MinimumDistance[j, i] = MinDistance;
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

		public override void UpdateEnergyTerms(ParticleEnsemble ensemble)
		{
			ParticleStaticObjects.AtomPropertiesDefinition.CalculateEnergyTerms(); 

			/*
			for (int i = 0; i < ensemble.NumberOfParticles; ++i)
			{
				for (int j = (i + 1); j < ensemble.NumberOfParticles; ++j)
				{
					double MinDistance = 2.0 * (ensemble.GetParticleRadius(i) + ensemble.GetParticleRadius(j));

					MinimumDistance[i, j] = MinDistance;
					MinimumDistance[j, i] = MinDistance;
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
			 * */ 
		}

		#endregion

		#region Calculate Force Field

		public override void CalculateForceField(ParticleEnsemble ensemble)
		{
			// variable declarations
			double posXi, posYi;
			double posXj, posYj;
			double LJxf,  LJyf;
			double ijSeparation = 0.0, LJforce = 0.0, PotentialEnergy = 0.0;
			int j, kk, ctr, dimensions = 2;
  
			// variable initializations			
			int BoxWidth = ensemble.BoxWidth;
			int BoxHeight = ensemble.BoxHeight;
			double MaxForce = ensemble.MaxForceThreshold;
			double MinForce = -1.0 * (ensemble.MaxForceThreshold);
  
			// initialize vectors holding forces
			for (int i = 0; i < ensemble.NumberOfParticles; ++i)
			{
				LJxforces[i]=0.0;
				LJyforces[i]=0.0;
			}

			ensemble.ResetAllParticlesNotWithinRange();

			int numberOfParticles = ensemble.NumberOfParticles;

			for (int i = 0; i < numberOfParticles; ++i)
			{
				Particle particlei = ensemble.Particles[i];

				for (j = (i + 1); j < numberOfParticles; ++j)
				{

					Particle particlej = ensemble.Particles[j];

					if (particlei.GridSector.IsInJoiningSectors(particlej.GridSector) == false)
					{
						continue;
					}

					//    get the interparticle separation distances
					ijSeparation = ensemble.GetInterParticleSeparation(i, j);

					//		update the radial distribution function
					//			pParticleSet->UpdateRadialDistributionFunction((int)(ijSeparation));

					double cutoffDistance = particlei.ParticleType.MinimumDistance[particlej.TypeID]; // MinimumDistance[i, j];
					//double cutoffDistance = MinimumDistance[i, j];

					if (ijSeparation < cutoffDistance)
					{
						// SQRT MOD
						ijSeparation = Math.Sqrt(ijSeparation); 

						double LJgradientTermA = particlei.ParticleType.LJgradientTermA[particlej.TypeID];
						double LJgradientTermB = particlei.ParticleType.LJgradientTermB[particlej.TypeID];

						// for each particle, change the appropriate element of the setWithinRangeOfAnotherParticle vector to true

						posXi = ensemble.GetXParticlePosition(i);
						posYi = ensemble.GetYParticlePosition(i);

						posXj = ensemble.GetXParticlePosition(j);
						posYj = ensemble.GetYParticlePosition(j);

						//        PotentialEnergy += LJenergyTermA[i][j]/(pow(ijSeparation,12.0))+LJenergyTermB[i][j]/pow(ijSeparation,6.0)+epsilon;
						//LJforce = (posXj - posXi) * (LJgradientTermA[i, j] / Math.Pow(ijSeparation, 13.0) + LJgradientTermB[i, j] / Math.Pow(ijSeparation, 7.0)) / ijSeparation;
#if !UsePOW
						LJforce = (posXj - posXi) * (LJgradientTermA / Math.Pow(ijSeparation, 13.0) + LJgradientTermB / Math.Pow(ijSeparation, 7.0)) / ijSeparation;
#else
						LJforce = (posXj - posXi) * (LJgradientTermA / MathHelper.Pow13(ijSeparation) + LJgradientTermB / MathHelper.Pow7(ijSeparation)) / ijSeparation;
#endif
						if (Math.Abs(LJforce) > MaxForce || Math.Abs(LJforce) < MinForce) { LJforce = 0.0; }    // error check for real-time stability...
						else if (double.IsNaN(LJforce) || double.IsInfinity(LJforce)) { LJforce = 0.0; } // error check for real-time stability...

						LJxforces[i] += LJforce;
						LJxforces[j] += -1.0 * LJforce;
						//        cout << "x "<< i << " " << j << " " << LJforce << endl; 
						//				cout << "i " << i << " LJxforces[i] " << LJxforces[i] << " j " << j <<  " LJxforces[j] " << LJxforces[j] << endl;
						//        cout << "xi:=" << posXi << ";" << "xj:=" << posXj << ";" << "yi:=" << posYi << ";" << "yj:=" << posYj << ";" << LJxforces[i] << endl;
						//LJforce = (posYj - posYi) * (LJgradientTermA[i, j] / Math.Pow(ijSeparation, 13.0) + LJgradientTermB[i, j] / Math.Pow(ijSeparation, 7.0)) / ijSeparation;
#if !UsePOW
						LJforce = (posYj - posYi) * (LJgradientTermA / Math.Pow(ijSeparation, 13.0) + LJgradientTermB / Math.Pow(ijSeparation, 7.0)) / ijSeparation;
#else
						LJforce = (posYj - posYi) * (LJgradientTermA / MathHelper.Pow13(ijSeparation) + LJgradientTermB / MathHelper.Pow7(ijSeparation)) / ijSeparation;
#endif
						if (Math.Abs(LJforce) > MaxForce || Math.Abs(LJforce) < MinForce) { LJforce = 0.0; }    // error check for real-time stability...
						else if (double.IsNaN(LJforce) || double.IsInfinity(LJforce)) { LJforce = 0.0; } // error check for real-time stability...

						LJyforces[i] += LJforce;
						LJyforces[j] += -1.0 * LJforce;

						ensemble.SetParticlesWithinRange(i, j);
						//ensemble.SetParticlesWithinRange(j, i);
					}
				}
			}
	
			// set the forces in the Particle Ensemble Object
			ensemble.AddXForces(LJxforces);
			ensemble.AddYForces(LJyforces);
			// set the potential energy
			ensemble.AddPotentialEnergy(PotentialEnergy);
		}

		#endregion
	}
}
