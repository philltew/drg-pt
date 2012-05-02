using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticlesOpt3
{
	public class ParticleEnsemble
	{
		public const int MAX_PARTICLE_COLLISIONS = 50;

		#region Private Members
		
		private int m_NumberOfParticles;
		private int m_MaxNumberOfParticles;
		private int m_Step;
		private int m_NeighborListSize;

		private double m_InitialMinVel, m_InitialMaxVel;

		//  box boundaries
		private int m_BoxWidth;
		private int m_BoxHeight;
		private int m_MaxPixelIndex;

		//  stuff for BerendsenThermostatting...
		private double m_KB, m_BerendsenCoupling, m_Tequilibrium;
		private double m_Temperature, m_ScaleFactor, m_InitialKE;
		private bool m_BerendsenThermostat;

		private bool m_ThereIsMidiOutput;
		private bool m_EnsembleReinitializationFlag;
		private bool m_NumberOfParticlesChangedFlag;
		private bool m_NumberOfParticlesIsGreaterFlag;

		//  stuff for stability
		private bool m_AllPositionsUnchangedFromLastStep;
		private double m_MaxForceThreshold, m_InitialMaxForceThreshold;

		// stuff for kinetic energies
		private double m_TotalKineticEnergy, m_AverageKineticEnergy, m_SDKineticEnergy;

		// the gradient scale factor...
		private double m_GradientScaleFactor;

		// the radius scale factor...
		private double m_RadiiScaleFactor;

		private double m_Timestep;
		private double m_PotentialEnergy, m_TotalEnergy;

		// pointer to a vector of ForceField objects
		private List<ForceField> m_ForceFields = new List<ForceField>();		

		private List<double> m_AverageVelocityAutoCorrelationFunction;

		private List<bool> m_InnerTurningPointCrossed = new List<bool>();

		// matrix with the interparticle separations of particle i-j at time t
		private DPMatrix m_DistanceMatrix;
		// matrix with the interparticle separations of particle i-j at time t-1
		private DPMatrix m_DistanceMatrixLastTime;
		// matrix with the interparticle separations of particle i-j at time t-2
		private DPMatrix m_DistanceMatrixLastLastTime;

		// matrix of booleans that tell us if particles i-j are within range of one another
		private BoolMatrix m_ParticlesWithinRange;			

		// these need moving to private, with appropriate set & get functions!!!!
		private int m_CollisionsCount;

		string debugMessage = "";

		#endregion

		#region Public Properties
		
		/// <summary>
		/// Pointer to a vector of particles
		/// </summary>
		public readonly List<Particle> Particles = new List<Particle>();

		/// <summary>
		/// Potential Energy
		/// </summary>
		public double PotentialEnergy { get { return m_PotentialEnergy; } set { m_PotentialEnergy = value; } }

		/// <summary>
		/// Simulation Box Width
		/// </summary>
		public int BoxWidth { get { return m_BoxWidth; } set { m_BoxWidth = value; } }

		/// <summary>
		/// Simulation Box Height
		/// </summary>
		public int BoxHeight { get { return m_BoxHeight; } set { m_BoxHeight = value; } }

		/// <summary>
		/// Eq Temperature
		/// </summary>
		public double EqTemperature { get { return m_Tequilibrium; } set { m_Tequilibrium = value; } }

		/// <summary>
		/// Berendsen Thermostat Coupling
		/// </summary>
		public double BerendsenThermostatCoupling { get { return m_BerendsenCoupling; } set { m_BerendsenCoupling = value; } }

		/// <summary>
		/// Gradient Scale Factor
		/// </summary>
		public double GradientScaleFactor { get { return m_GradientScaleFactor; } set { m_GradientScaleFactor = value; } }

		/// <summary>
		/// Radii Scale Factor
		/// </summary>
		public double RadiiScaleFactor { get { return m_RadiiScaleFactor; } }

		/// <summary>
		/// Time Step
		/// </summary>
		public double Timestep { get { return m_Timestep; } }

		/// <summary>
		/// Max Force Threshold
		/// </summary>
		public double MaxForceThreshold { get { return m_MaxForceThreshold; } set { m_MaxForceThreshold = value; } }

		/// <summary>
		/// Initial Max Force Threshold
		/// </summary>
		public double InitialMaxForceThreshold { get { return m_InitialMaxForceThreshold; } }

		/// <summary>
		/// Number Of Force Field Objects
		/// </summary>
		public int NumberOfForceFieldObjects { get { return m_ForceFields.Count; } }

		/// <summary>
		/// Number Of Particles
		/// </summary>
		public int NumberOfParticles { get { return m_NumberOfParticles; } }

		/// <summary>
		/// Max Number Of Particles
		/// </summary>
		public int MaxNumberOfParticles { get { return m_MaxNumberOfParticles; } }

		public ForceField GetForceFieldObject(int i)
		{
			return m_ForceFields[i];
		}

		public Particle GetParticle(int i)
		{
			return Particles[i];
		}

		#endregion	
	
		#region Constructor

		public ParticleEnsemble(int nparticles, double MinRad, double MaxRad, List<string> FFtype, int Height, int Width, double scaleFactor)
		{
			int i;
			// set the maximum number of particles to 1000
			m_MaxNumberOfParticles = 1000;

			// value of the boltzmann constant - we dont really care about units now...
			m_KB = 8.314;
			// value of the berendsen coupling constant - set this to be controlled by the artist...	
			m_BerendsenCoupling = 1.0;		
			m_Tequilibrium = 20000;
			m_BerendsenThermostat = true;
			//  BerendsenThermostat=false;
			m_EnsembleReinitializationFlag = false;
			m_NumberOfParticlesChangedFlag = false;
			m_NumberOfParticlesIsGreaterFlag = false;

			m_RadiiScaleFactor = 1.0;
			m_InitialKE = 0.0;
			m_GradientScaleFactor = scaleFactor;

			m_Step = 0;
			m_NumberOfParticles = nparticles;
			m_BoxHeight = Height;
			m_BoxWidth = Width;

			m_MaxForceThreshold = 1.0e6;
			m_InitialMaxForceThreshold = m_MaxForceThreshold;

			m_AverageVelocityAutoCorrelationFunction = new List<double>(new double[1024]);

			m_InitialMinVel = 100.0;
			m_InitialMaxVel = 200.0;

			for (int ii = 0; ii < m_MaxNumberOfParticles; ++ii)
			{
				InitializeOneNewParticle();
			}

			if (!EliminateParticleOverlap(Height, Width))
			{
				// adjust particle positions to eliminate overlap 
				do
				{
					//cout << "Can't fit " << NumberOfParticles << " particles into the simulation box " << endl;
					
					// if there's too many particles to fit in the simulation box
					m_NumberOfParticles -= 1;                       
					
					//cout << "Decrementing the number of particles to " << NumberOfParticles << endl;
					
					// decrement the particles till it's ok
					EliminateParticleOverlap(Height, Width);				
				}
				while (!EliminateParticleOverlap(Height, Width));
			}
			
			// assign tmp1 matrix to distanceMatrix & distanceMatrixLastTime
			m_DistanceMatrix = new DPMatrix(0.0, m_MaxNumberOfParticles, m_MaxNumberOfParticles);
			m_DistanceMatrixLastTime = new DPMatrix(0.0, m_MaxNumberOfParticles, m_MaxNumberOfParticles);
			m_DistanceMatrixLastLastTime = new DPMatrix(0.0, m_MaxNumberOfParticles, m_MaxNumberOfParticles);

			// update the particlesWithinRange matrix
			m_ParticlesWithinRange = new BoolMatrix(false, m_MaxNumberOfParticles, m_MaxNumberOfParticles); 

			// update the interparticle separation matrix
			UpdateInterParticleSeparations();						

			m_PotentialEnergy = 0.0;

			// push back forceField objects onto pForceFieldVector - at present, we only have LJ forces, but this can be easily expanded
			for (i = 0; i < FFtype.Count; ++i)
			{
				if (FFtype[i] == "SoftSpheres") 
				{ 
					m_ForceFields.Add(new SoftSpheres(this)); 
				}
			}
		}

		#endregion

		#region Particle Setup Methods

		public void InitializeOneNewParticle()
		{
			Particle pParticle;
			pParticle = new Particle();

			int type = 0;
			type = DetermineParticleType();
			pParticle.TypeID = type;

			//ParticleInfo typeInfo = ParticleStaticObjects.AtomPropertiesDefinition.Lookup[type];

			//pParticle.Radius = typeInfo.Mass * 0.5;
			//pParticle.InitialRadius = typeInfo.Mass * 0.5;
			pParticle.Position.X = ParticleStaticObjects.RandomDouble(0.0 + pParticle.Radius, m_BoxWidth - pParticle.Radius);
			pParticle.Position.Y = ParticleStaticObjects.RandomDouble(0.0 + pParticle.Radius, m_BoxHeight - pParticle.Radius);
			//pParticle.Mass = 10;
			Particles.Add(pParticle);
		}

		public void SetPropertiesForParticle(int newParticleIdx)
		{
			int type = 0;
			type = DetermineParticleType();
			Particles[newParticleIdx].TypeID = type;

			//ParticleInfo typeInfo = ParticleStaticObjects.AtomPropertiesDefinition.Lookup[type];

			//Particles[newParticleIdx].Radius = typeInfo.Mass * 0.5;
			//Particles[newParticleIdx].InitialRadius = typeInfo.Mass * 0.5;
			//Particles[newParticleIdx].Mass = 10;
		}

		public int DetermineParticleType()
		{
			int type = 0;

			if (ParticleStaticObjects.AtomPropertiesDefinition.Active.Count == 0)
			{
				// if the size of the Active vector is zero
				type = ParticleStaticObjects.RandomInt(0, ParticleStaticObjects.AtomPropertiesDefinition.Count);

				// not sure about the implementation of ofxRandom, is it inclusive? clip it anyway 
				if (type >= ParticleStaticObjects.AtomPropertiesDefinition.Count)
				{
					type = ParticleStaticObjects.AtomPropertiesDefinition.Count - 1;
				}
			}
			else
			{
				// if the size of the active vector isn't zero
				int activeType = ParticleStaticObjects.RandomInt(0, ParticleStaticObjects.AtomPropertiesDefinition.Active.Count);

				// not sure about the implementation of ofxRandom, is it inclusive? clip it anyway 
				if (activeType >= ParticleStaticObjects.AtomPropertiesDefinition.Active.Count)
				{
					activeType = ParticleStaticObjects.AtomPropertiesDefinition.Active.Count - 1;
				}

				type = ParticleStaticObjects.AtomPropertiesDefinition.Active[activeType];
			}

			return type;
		}		

		// function to initialize random velocities and non-overlapping positions for all the particles
		bool EliminateParticleOverlap(int Height, int Width)
		{
			int i, jj, iterations;
			bool everythingOK = true;

			for (i = 0; i < m_NumberOfParticles; ++i)
			{
				if (InitializeRandomParticlePosition(i, Height, Width)) { everythingOK = true; }
				else { everythingOK = false; }
			}

			if (!everythingOK)
			{
				return false;
			}
			else
			{
				CalculateKineticEnergiesAndTemperature();
				m_InitialKE = m_TotalKineticEnergy;
				return true;
			}
		}

		// this function initializes a random particle with index particleIdx, giving it a position that does not overlap 
		//	with all particles whose index is less than or equal to particleIdx
		public bool InitializeRandomParticlePosition(int particleIdx, int Height, int Width)
		{

			int i, jj, iterations;
			double ijjSeparation = 0.0;
			bool ParticlesOverlap = true;

			//ofSeedRandom();   	//  Seed random number generator to clock time, so random numbers are always different    

			i = particleIdx;

			Particle particlei = GetParticle(i);

			particlei.Velocity = new DPVector(ParticleStaticObjects.RandomDouble(-1.0 * m_InitialMinVel, m_InitialMaxVel), ParticleStaticObjects.RandomDouble(-1.0 * m_InitialMinVel, m_InitialMaxVel));
			particlei.Position = new DPVector(ParticleStaticObjects.RandomDouble(particlei.Radius, m_BoxWidth - particlei.Radius), ParticleStaticObjects.RandomDouble(particlei.Radius, m_BoxHeight - particlei.Radius));

			#if DEBUG 

			if (particlei.Velocity.IsNanOrInfinate == true)
			{
				throw new Exception("Particle Velocity Is Nan Or Infinate");
			}
			
			#endif

			// what follows is for making sure that the initial particles dont overlap
			if (i != 0)
			{ 
				// only execute if it's the first and only particle
				// the do-while loop below reselects the particle px & py until it no longer overlaps with any other particle
				iterations = 1;

				do
				{
					ParticlesOverlap = false;
					
					for (jj = 0; jj < i; ++jj)
					{
						Particle particlejj = GetParticle(jj);
						ijjSeparation = Math.Sqrt(Math.Pow((particlei.Position.X - particlejj.Position.X), 2.0) + Math.Pow((particlei.Position.Y - particlejj.Position.Y), 2.0));
						if (ijjSeparation <= (particlei.Radius + particlejj.Radius)) { ParticlesOverlap = true; }
					}
					
					if (ParticlesOverlap)
					{						
						particlei.Position = new DPVector(ParticleStaticObjects.RandomDouble(particlei.Radius, m_BoxWidth - particlei.Radius), ParticleStaticObjects.RandomDouble(particlei.Radius, m_BoxHeight - particlei.Radius));
					}
					
					++iterations;

					if (iterations > 10000)
					{
						return false;
					}
				} 
				while (ParticlesOverlap);
			}
			return true;
		}

		#endregion

		#region Methods For Forces

		public void ZeroForces()
		{
			for (int i = 0; i < m_NumberOfParticles; ++i)
			{
				Particle part = Particles[i];

				part.Force.X = 0.0;
				part.Force.Y = 0.0;
			}
		}

		public void AddForces(DPVector[] Newforces)
		{
			for (int i = 0; i < m_NumberOfParticles; ++i)
			{
				Particle part = Particles[i];
				DPVector vect = Newforces[i];

				double newforceX = part.Force.X + vect.X;
				double newforceY = part.Force.Y + vect.Y;

				part.Force.X = newforceX;
				part.Force.Y = newforceY;
			}
		}

		#endregion

		#region Misc Methods

		/// <summary>
		/// add an external forcefield from the pixels on the end of the pForceFieldVector 
		/// </summary>
		/// <param name="pixels"></param>
		public void AddAPixelField(ForceField pixels)
		{
			m_ForceFields.Add(pixels);
		}

		public void SetEnsembleReinitializationFlag()
		{
			m_EnsembleReinitializationFlag = true;
		}

		public void SetNumberOfParticlesChangedFlag()
		{
			m_NumberOfParticlesChangedFlag = true;
		}

		public void SetNumberOfParticlesIsGreater()
		{
			m_NumberOfParticlesIsGreaterFlag = true;
		}

		public void SetNumberOfParticles(int newnumber)
		{
			m_NumberOfParticles = newnumber; // number of particles
		}

		public void SetParticleKineticEnergy(int i, double newKE)
		{
			Particles[i].KineticEnergy = newKE;
		}

		public void AddPotentialEnergy(double NewPotentialEnergy)
		{
			m_PotentialEnergy += NewPotentialEnergy;
		}

		public void SetParticlesWithinRange(int i, int j)
		{
			m_ParticlesWithinRange[i, j] = true;
			m_ParticlesWithinRange[j, i] = true;
		}

		public void SetParticlesNotWithinRange(int i, int j)
		{
			m_ParticlesWithinRange[i, j] = false;
			m_ParticlesWithinRange[j, i] = false;
		}

		public void ResetAllParticlesNotWithinRange()
		{
			m_ParticlesWithinRange.SetValueRange(false, NumberOfParticles, NumberOfParticles);
		}


		#endregion

		#region Simulation Frame Methods

		#region Velocity Verlet Propagation

		/// <summary>
		/// Velocity verlet routine to propagate the particle ensemble
		/// </summary>
		/// <param name="Height"></param>
		/// <param name="Width"></param>
		/// <param name="pExternalField"></param>
		public void VelocityVerletPropagation(int Height, int Width, ExternalField pExternalField)
		{
			int i, kk;
			double V = 0.0, T = 0.0, dt, pxnew, pynew, factor;

			m_AllPositionsUnchangedFromLastStep = true;

			m_BoxWidth = Width;
			m_BoxHeight = Height;

			if (m_NumberOfParticlesChangedFlag)
			{
				#region Number Of Particles has been adjusted 
				
				m_NumberOfParticlesChangedFlag = false;
				
				if (m_NumberOfParticlesIsGreaterFlag)
				{
					for (kk = 0; kk < NumberOfForceFieldObjects; ++kk)
					{				
						// calculate the ij energy terms for particle set including the new ones
						GetForceFieldObject(kk).UpdateEnergyTerms(this);
					}
				}

				#endregion
			}
			else
			{
				#region Regular Time Step
				
				#region Increment Time Steps

				//  Timestep = 1.0/(double)ofGetFrameRate();
				m_Timestep = 0.005;

				dt = m_Timestep;
				
				// increment ParticleEnsemble Private data member step
				++m_Step;
				
				#endregion

				#region Berendsen Velocity Rescaling

				if (m_BerendsenThermostat)
				{                
					//  Berendsen Thermostat
					BerendsenVelocityRescaling();
				}

				#endregion

				#region Move all particles

				// this loop uses verlet scheme (VV) to propagate the positions forward one step
				for (i = 0; i < NumberOfParticles; ++i)
				{
					Particle particlei = GetParticle(i);
					
					particlei.PositionLast = particlei.Position; 

					factor = 0.5 * dt * dt / particlei.Mass;
					
					pxnew = particlei.Position.X + particlei.Velocity.X * dt + particlei.Force.X * factor;
					pynew = particlei.Position.Y + particlei.Velocity.Y * dt + particlei.Force.Y * factor;

					#region Check for colitions with the wall (left or right)

					if (pxnew > particlei.Radius && pxnew < (m_BoxWidth - particlei.Radius))
					{
						particlei.Position.X = pxnew; 
					}
					else
					{  
						// this is to reflect off the walls; added by DRG in lieu of soft walls to improve real time stability... not part of a standard VV scheme      
						particlei.Position.X = particlei.PositionLast.X;
						particlei.Velocity.X = -1.0 * particlei.Velocity.X;

						#if DEBUG 

						if (particlei.Velocity.IsNanOrInfinate == true)
						{
							throw new Exception("Particle Velocity Is Nan Or Infinate");
						}
			
						#endif

						particlei.WasReflectedByWall = true; 

						CalculateParticleVelocitiesInXWallFrame(i);
					}

					#endregion 

					#region Check for colitions with the wall (top or bottom)					

					if (pynew > particlei.Radius && pynew < (m_BoxHeight - particlei.Radius))
					{    
						// this the standard VV code here						
						particlei.Position.Y = pynew; 
					}
					else
					{  
						// this is to reflect off the walls; added by DRG in lieu of soft walls to improve real time stability... not part of a standard VV scheme						
						particlei.Position.Y = particlei.PositionLast.Y;
						particlei.Velocity.Y = -1.0 * particlei.Velocity.Y;

						#if DEBUG 

						if (particlei.Velocity.IsNanOrInfinate == true)
						{
							throw new Exception("Particle Velocity Is Nan Or Infinate");
						}
			
						#endif

						particlei.WasReflectedByWall = true; 

						CalculateParticleVelocitiesInYWallFrame(i);						
					}

					#endregion

					// check whether all the positions are changed from the last step
					if (particlei.Position.Y != particlei.PositionLast.Y || particlei.Position.X != particlei.PositionLast.X)
					{
						m_AllPositionsUnchangedFromLastStep = false;
					}
				}

				#endregion

				#region If all the particles have frozen then we need to rerandomise (should be rare)
				
				// this is a stability measure; if the frame is frozen wrt to the previous frame,
				if (m_AllPositionsUnchangedFromLastStep)
				{
					// adjust particle positions to eliminate overlap - this can cause the sim to freeze
					EliminateParticleOverlap(m_BoxHeight, m_BoxWidth);    

					for (i = 0; i < NumberOfParticles; ++i)
					{
						Particle particlei = GetParticle(i);

						//  then we zero out the forces and velocities & repropagate the positions
						particlei.Force.X = 0;
						particlei.Force.Y = 0;

						particlei.PositionLast.X = particlei.Position.X;
						particlei.PositionLast.Y = particlei.Position.Y;

						particlei.Position.X = particlei.Position.X + particlei.Velocity.X * dt + (particlei.Force.X / particlei.Mass) * dt * dt * 0.5; 
						particlei.Position.Y = particlei.Position.Y + particlei.Velocity.Y * dt + (particlei.Force.Y / particlei.Mass) * dt * dt * 0.5; 
					}

					m_AllPositionsUnchangedFromLastStep = false;
				}

				#endregion

				#region Determin Inter-particle collions
				
				UpdateInterParticleSeparations();

				#endregion

				#region Calculate Force Fields
				
				if (pExternalField != null)
				{
					pExternalField.CalculateForceField(this);
				}

				if (GetForceFieldObject(0).ForceFieldType == "HardSphereForceField")
				{
					GetForceFieldObject(0).CalculateForceField(this);
				}
				else
				{
					for (i = 0; i < NumberOfParticles; ++i)
					{
						Particle particlei = GetParticle(i);
						// save the present forces to t-1 vectors
						particlei.ForceLast.X = particlei.Force.X;
						particlei.ForceLast.Y = particlei.Force.Y;
					}

					// zero out the force vectors & potential energy					
					ZeroForces();			
					PotentialEnergy = 0.0;

					for (kk = 0; kk < NumberOfForceFieldObjects; ++kk)
					{				// calculate & set the forces at the new positions
						GetForceFieldObject(kk).CalculateForceField(this);
					}

					for (i = 0; i < NumberOfParticles; ++i)
					{
						Particle particlei = GetParticle(i);
						// use VV scheme to propagate the velocities forward
						factor = dt * 0.5 / particlei.Mass; 						

						particlei.Velocity.X = particlei.Velocity.X + (particlei.Force.X + particlei.ForceLast.X * factor);
						particlei.Velocity.Y = particlei.Velocity.Y + (particlei.Force.Y + particlei.ForceLast.Y * factor);

						#if DEBUG 

						if (particlei.Velocity.IsNanOrInfinate == true)
						{
							throw new Exception("Particle Velocity Is Nan Or Infinate");
						}
			
						#endif
					}

					// see whether any collisions occurred
					DetermineIfCollisionsOccurred();
				}

				#endregion 

				#endregion
			}
		}

		#endregion

		#region Berendsen Velocity Rescaling

		/// <summary>
		/// This function is for the simple Berendsen Thermostat
		/// </summary>
		private void BerendsenVelocityRescaling()
		{
			double scaleFactor;			

			// this is an extra velocity rescaling measure to improve real-time stability... not part of Berendsen!!!!
			// be sure that no single particle has a KE which differs from the average by 3 standard deviations (sigmas) 
			CalculateKineticEnergiesAndTemperature();

			#region Rescale each particle velocity
			
			double sigma = 2.0;

			for (int i = 0; i < NumberOfParticles; ++i)
			{
				Particle particlei = GetParticle(i);

				if ((particlei.KineticEnergy - m_AverageKineticEnergy) > (sigma * m_SDKineticEnergy))
				{				
					scaleFactor = (sigma * m_SDKineticEnergy) / (particlei.KineticEnergy - m_AverageKineticEnergy);

					particlei.Velocity.X = scaleFactor * particlei.Velocity.X;
					particlei.Velocity.Y = scaleFactor * particlei.Velocity.Y;

					#if DEBUG 

					if (particlei.Velocity.IsNanOrInfinate == true)
					{
						throw new Exception("Particle Velocity Is Nan Or Infinate");
					}
			
					#endif
				}
			}

			#endregion

			#region real-time stability measure
			
			// again, a real-time stability measure... not part of Berendsen!!!!
			// re-initialize the system if the temperature gets crazy
			if (m_Temperature > 1.0e8)
			{
				// adjust particle positions to eliminate overlap - this can cause the sim to freeze
				EliminateParticleOverlap(m_BoxHeight, m_BoxWidth);    
				
				for (int i = 0; i < NumberOfParticles; ++i)
				{
					Particle particlei = GetParticle(i);

					//  then we zero out the forces and velocities 
					particlei.Force.X = 0.0;
					particlei.Force.Y = 0.0; 

					particlei.PositionLast.X = particlei.Position.X;
					particlei.PositionLast.Y = particlei.Position.Y;
				}
	
				CalculateKineticEnergiesAndTemperature();
			}

			#endregion

			#region Apply Berendsen thermostat

			// this code here is the bona fide Berendsen thermostat !!!!
			scaleFactor = Math.Sqrt(m_Tequilibrium / (m_BerendsenCoupling * m_Temperature));

			if (scaleFactor != 1.0)
			{
				for (int i = 0; i < NumberOfParticles; ++i)
				{
					Particle particlei = GetParticle(i);

					//rescale the velocities
					particlei.Velocity.X = scaleFactor * particlei.Velocity.X;
					particlei.Velocity.Y = scaleFactor * particlei.Velocity.Y;

					#if DEBUG 

					if (particlei.Velocity.IsNanOrInfinate == true)
					{
						throw new Exception("Particle Velocity Is Nan Or Infinate");
					}
			
					#endif
				}
			}

			#endregion
		}

		#endregion

		#region Update Particle Velocity Auto Correlation Functions

		public void UpdateParticleVelocityAutoCorrelationFunctions()
		{
			for (int i = 0; i < Particles.Count; ++i)
			{
				// update each particle's velocity autocorrelation function...
				Particles[i].UpdateVelocityAutoCorrelationFunction();
			}
		}

		#endregion	

		#region Calculate Kinetic Energies And Temperature
		
		public void CalculateKineticEnergiesAndTemperature()
		{
			// calculate the total, avg, & standard deviation of the kinetic energy

			double sum = 0.0;
			m_TotalKineticEnergy = 0.0;
			m_AverageKineticEnergy = 0.0;
			m_SDKineticEnergy = 0.0;
			double KE = 0.0;

			for (int i = 0; i < NumberOfParticles; ++i)
			{
				Particle particlei = GetParticle(i);

				//KE = 0.5 * GetParticleMass(i) * (Math.Pow(GetXParticleVelocity(i), 2.0) + Math.Pow(GetYParticleVelocity(i), 2.0));
				KE = 0.5 * particlei.Mass * (Math.Pow(particlei.Velocity.X, 2.0) + Math.Pow(particlei.Velocity.Y, 2.0));
				//pParticleVector[i].KineticEnergy = KE;
				particlei.KineticEnergy = KE;
				m_TotalKineticEnergy += KE;
			}

			m_AverageKineticEnergy = m_TotalKineticEnergy / m_NumberOfParticles;
			m_Temperature = m_TotalKineticEnergy / (m_NumberOfParticles * m_KB);

			for (int i = 0; i < NumberOfParticles; ++i) 
			{
				sum += Math.Pow((Particles[i].KineticEnergy - m_AverageKineticEnergy), 2.0); 
			}

			m_SDKineticEnergy = Math.Sqrt(sum / m_NumberOfParticles);

		}

		#endregion

		#region Update Inter Particle Separations
		
		/// <summary>
		/// update the interparticle distanceMatrix
		/// </summary>
		public void UpdateInterParticleSeparations()
		{
			double ijSeparation = 0;

			DPMatrix temp = m_DistanceMatrixLastLastTime; 
			m_DistanceMatrixLastLastTime = m_DistanceMatrixLastTime;
			m_DistanceMatrixLastTime = m_DistanceMatrix;
			m_DistanceMatrix = temp; 
			
			for (int i = 0; i < m_NumberOfParticles; ++i)
			{
				for (int j = (i + 1); j < m_NumberOfParticles; ++j)
				{
					//distanceMatrixLastLastTime[i, j] = distanceMatrixLastTime[i, j];
					//distanceMatrixLastLastTime[j, i] = distanceMatrixLastTime[j, i];
					//distanceMatrixLastTime[i, j] = distanceMatrix[i, j];   // move the present distances to the last
					//distanceMatrixLastTime[j, i] = distanceMatrix[j, i];
					//ijSeparation = Math.Sqrt(Math.Pow(pParticleVector[i].getpx() - pParticleVector[j].getpx(), 2.0) + Math.Pow(pParticleVector[i].getpy() - pParticleVector[j].getpy(), 2.0));
					DPVector posi = Particles[i].Position;
					DPVector posj = Particles[j].Position;

					ijSeparation = Math.Sqrt(Math.Pow(posi.X - posj.X, 2.0) + Math.Pow(posi.Y - posj.Y, 2.0));

					m_DistanceMatrix[i, j] = ijSeparation;
					m_DistanceMatrix[j, i] = ijSeparation;                   // assign the present distances
				}
			}
		}

		#endregion

		#endregion

		#region Collisions

		/// <summary>
		/// function to determine if a collision happened
		/// </summary>
		public void DetermineIfCollisionsOccurred()
		{
			for (int i = 0; i < m_NumberOfParticles; ++i)
			{
				for (int j = (i + 1); j < m_NumberOfParticles; ++j)
				{
					if (m_ParticlesWithinRange[i, j] == true)
					{
						if (m_DistanceMatrixLastTime[i, j] < m_DistanceMatrix[i, j] && m_DistanceMatrixLastTime[i, j] < m_DistanceMatrixLastLastTime[i, j])
						{
							Particles[i].CollisionOccurred = true;
							Particles[j].CollisionOccurred = true;
							CalculateParticleVelocitiesInComFrame(i, j);
							// debug code
							//						cout << "particle " << i << " - " << j << " distanceMatrixLastLastTime " << distanceMatrixLastLastTime[i][j] 
							//								 << " distanceMatrixLastTime " << distanceMatrixLastTime[i][j] << " distanceMatrix " << distanceMatrix[i][j] <<  endl;
						}
					}
				}
			}
		}

		public void ResetParticleCollisions()
		{
			for (int i = 0; i < m_NumberOfParticles; ++i)
			{
				Particles[i].MarkParticleDidNotCollide(); ;
			}
		}

		public double GetInterParticleSeparation(int i, int j)
		{
			return m_DistanceMatrix[i, j];
		}

		// function to calculate the velocity of particles i & j in their center of mass frame
		public void CalculateParticleVelocitiesInComFrame(int i, int j)
		{
			int kk, dimensions = 3;
			double[] Vi = new double[dimensions], Qi = new double[dimensions], Vj = new double[dimensions], Qj = new double[dimensions];
			double[] Vcom = new double[dimensions], Vicom = new double[dimensions], Vjcom = new double[dimensions], n12 = new double[dimensions];
			double[] ViParProj = new double[dimensions], VjParProj = new double[dimensions];

			Vi[0] = Particles[i].Velocity.X; 		// velocity vectors for particle i
			Vi[1] = Particles[i].Velocity.Y;
			Qi[0] = Particles[i].Position.X; 		// position vectors for particle i
			Qi[1] = Particles[i].Position.Y;

			Vj[0] = Particles[j].Velocity.X;		  // velocity vectors for particle j
			Vj[1] = Particles[j].Velocity.Y;
			Qj[0] = Particles[j].Position.X;			// position vectros for particle j
			Qj[1] = Particles[j].Position.Y;

			double ijSeparation = GetInterParticleSeparation(i, j);   // calculate interparticle separation distance

			double MassA = Particles[i].Mass;
			double MassB = Particles[j].Mass;

			for (kk = 0; kk < dimensions; ++kk)
			{
				//	calculate center of mass (COM) velocity
				Vcom[kk] = (MassA * Vi[kk] + MassB * Vj[kk]) / (MassA + MassB);
			}

			for (kk = 0; kk < dimensions; ++kk)
			{
				//	calculate velocity of particle i & j in COM frame
				Vicom[kk] = Vi[kk] - Vcom[kk];
				Vjcom[kk] = Vj[kk] - Vcom[kk];
			}

			for (kk = 0; kk < dimensions; ++kk)
			{
				//	calculate unit vector, n12, pointing from i to j
				n12[kk] = (Qi[kk] - Qj[kk]) / ijSeparation;
			}

			for (kk = 0; kk < dimensions; ++kk)
			{
				//	calculate the parallel projection of i & j onto n12
				ViParProj[kk] = n12[kk] * (Vicom[0] * n12[0] + Vicom[1] * n12[1] + Vicom[2] * n12[2]);
				VjParProj[kk] = n12[kk] * (Vjcom[0] * n12[0] + Vjcom[1] * n12[1] + Vjcom[2] * n12[2]);
			}

			double ViParMagnitude = Math.Sqrt(Math.Pow(ViParProj[0], 2.0) + Math.Pow(ViParProj[1], 2.0) + Math.Pow(ViParProj[2], 2.0));
			double VjParMagnitude = Math.Sqrt(Math.Pow(VjParProj[0], 2.0) + Math.Pow(VjParProj[1], 2.0) + Math.Pow(VjParProj[2], 2.0));

			//debug code
			//	cout << " Collision velocity for particle " << i << " " << j << " " << ViParMagnitude << endl; 

			Particles[i].VInCollisionFrame = ViParMagnitude;
			Particles[j].VInCollisionFrame = VjParMagnitude;

		}

		// function to calculate the velocity of particle i in the wall frame	
		public void CalculateParticleVelocitiesInXWallFrame(int i)
		{
			double velx = Particles[i].Velocity.X; 		// velocity vectors for particle i in x direction
			Particles[i].VInCollisionFrame = velx / 2.0;
			//	cout << " Wall Collision velocity for particle " << i << " " << pParticleVector[i]->getvInCollisionFrame() << endl; 
		}

		public void CalculateParticleVelocitiesInYWallFrame(int i)
		{
			double vely = Particles[i].Velocity.Y; 		// velocity vectors for particle i in x direction
			//	cout << " Wall Collision velocity for particle " << i << " " << pParticleVector[i]->getvInCollisionFrame() << endl; 
			Particles[i].VInCollisionFrame = vely / 2.0;
		}

		#endregion

		#region FFT Methods

		private void FFTVelocityAutoCorrelationFunction()
		{
			// before FFT, we need to update the ensemble averaged autocorrelation function 
			m_AverageVelocityAutoCorrelationFunction = new List<double>(new double[1024]);  
			
			double avgfac = 1.0 / Particles.Count;

			for (int i = 0; i < Particles.Count; ++i)
			{
				for (int jj = 0; jj < 1024; ++jj)
				{
					// for the sake of efficiency, the velocity autocorrelation functions on the particle are PUBLIC data... beware!!!
					m_AverageVelocityAutoCorrelationFunction[jj] += avgfac * (Particles[i].VelocityAutoCorrelationFunction[0]) * (Particles[i].VelocityAutoCorrelationFunction[jj]);
				}
			}

			realft(m_AverageVelocityAutoCorrelationFunction, 1);
		}

		private void realft(List<double> data, int isign)
		{
			int i, i1, i2, i3, i4;
			double c1 = 0.5, c2, h1r, h1i, h2r, h2i, wr, wi, wpr, wpi, wtemp, theta;

			int n = data.Count;
			theta = Math.PI / (double)(n >> 1);

			if (isign == 1)
			{
				c2 = -0.5;
				four1(data, 1);
			}
			else
			{
				c2 = 0.5;
				theta = -theta;
			}

			wtemp = Math.Sin(0.5 * theta);
			wpr = -2.0 * wtemp * wtemp;
			wpi = Math.Sin(theta);
			wr = 1.0 + wpr;
			wi = wpi;

			for (i = 1; i < (n >> 2); i++)
			{
				i2 = 1 + (i1 = i + i);
				i4 = 1 + (i3 = n - i1);
				h1r = c1 * (data[i1] + data[i3]);
				h1i = c1 * (data[i2] - data[i4]);
				h2r = -c2 * (data[i2] + data[i4]);
				h2i = c2 * (data[i1] - data[i3]);
				data[i1] = h1r + wr * h2r - wi * h2i;
				data[i2] = h1i + wr * h2i + wi * h2r;
				data[i3] = h1r - wr * h2r + wi * h2i;
				data[i4] = -h1i + wr * h2i + wi * h2r;
				wr = (wtemp = wr) * wpr - wi * wpi + wr;
				wi = wi * wpr + wtemp * wpi + wi;
			}

			if (isign == 1)
			{
				data[0] = (h1r = data[0]) + data[1];
				data[1] = h1r - data[1];
			}
			else
			{
				data[0] = c1 * ((h1r = data[0]) + data[1]);
				data[1] = c1 * (h1r - data[1]);
				four1(data, -1);
			}
		}

		private void SwapIndexValues(List<double> data, int a, int b)
		{
			double temp = data[a];
			data[a] = data[b];
			data[b] = temp;
		}

		private void four1(List<double> data, int isign)
		{
			int n, mmax, m, j, istep, i;
			double wtemp, wr, wpr, wpi, wi, theta, tempr, tempi;

			int nn = data.Count / 2;

			n = nn << 1;

			j = 1;

			for (i = 1; i < n; i += 2)
			{
				if (j > i)
				{
					SwapIndexValues(data, j - 1, i - 1);
					SwapIndexValues(data, j, i);
				}

				m = nn;

				while (m >= 2 && j > m)
				{
					j -= m;
					m >>= 1;
				}

				j += m;
			}

			mmax = 2;

			while (n > mmax)
			{
				istep = mmax << 1;
				theta = isign * (2.0 * Math.PI / mmax);
				wtemp = Math.Sin(0.5 * theta);
				wpr = -2.0 * wtemp * wtemp;
				wpi = Math.Sin(theta);
				wr = 1.0;
				wi = 0.0;

				for (m = 1; m < mmax; m += 2)
				{
					for (i = m; i <= n; i += istep)
					{
						j = i + mmax;
						tempr = wr * data[j - 1] - wi * data[j];
						tempi = wr * data[j] + wi * data[j - 1];
						data[j - 1] = data[i - 1] - tempr;
						data[j] = data[i] - tempi;
						data[i - 1] += tempr;
						data[i] += tempi;
					}

					wr = (wtemp = wr) * wpr - wi * wpi + wr;
					wi = wi * wpr + wtemp * wpi + wi;
				}

				mmax = istep;
			}
		}

		#endregion
	}
}
