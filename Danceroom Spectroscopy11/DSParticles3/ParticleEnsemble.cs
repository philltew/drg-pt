using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace DSParticles3
{
	public class ParticleEnsemble
	{
		public const int MAX_PARTICLE_COLLISIONS = 50;
        public const int MAX_CORR_FTN_SIZE = 2048;

        // needs to be internal static because particle classes need to know about it
        public readonly static int VelocityAutoCorrelationLength = 1024; // should be user-controlled
        // only multiples of 2^n allowed, SHOULDN'T be larger than MAX_CORR_FTN_SIZE

		#region Private Members

		private int m_NumberOfParticles;
		private int m_MaxNumberOfParticles;
		private int step;
		private int NeighborListSize;

		private double InitialMinVel, InitialMaxVel;

		//  box boundaries
		private int m_BoxWidth;
		private int m_BoxHeight;
		private int maxPixelIndex;

		//  stuff for BerendsenThermostatting...
		private double kb, m_BerendsenThermostatCoupling, m_EquilibriumTemperature;
		private double temperature, scaleFactor, InitialKE;
		private bool BerendsenThermostat;

		private bool ThereIsMidiOutput;
		private bool EnsembleReinitializationFlag;
		private bool NumberOfParticlesChangedFlag;
		private bool NumberOfParticlesIsGreaterFlag;

		//  stuff for stability
		private bool AllPositionsUnchangedFromLastStep;
		private double m_MaxForceThreshold, m_InitialMaxForceThreshold;

		// stuff for kinetic energies
		private double TotalKineticEnergy, AverageKineticEnergy, SDKineticEnergy;

		// the gradient scale factor...
		private double m_GradientScaleFactor;

		// the radius scale factor...
		private double m_RadiiScaleFactor;

		private double m_Timestep;
		private double m_PotentialEnergy, TotalEnergy;

		// pointer to a vector of ForceField objects
		private List<ForceField> ForceFields = new List<ForceField>();		

		private List<bool> InnerTurningPointCrossed = new List<bool>();

		// matrix with the interparticle separations of particle i-j at time t
		private DPMatrix distanceMatrix;
		// matrix with the interparticle separations of particle i-j at time t-1
		private DPMatrix distanceMatrixLastTime;
		// matrix with the interparticle separations of particle i-j at time t-2
		private DPMatrix distanceMatrixLastLastTime;

		// matrix of booleans that tell us if particles i-j are within range of one another
		private BoolMatrix particlesWithinRange;			
         
        // correlation function vector of size velocityAutoCorrelationLength; it would be good if the user could
        // dynamically choose the value of velocityAutoCorrelationLength
        private bool m_FFTenabled;              // should be user-controlled
        private List<double> m_FFTofCorrelationFunction;     
        private double[] m_VelocityAutoCorrelationFunction;
        private int m_NumberOfFFTAverages = 60;   // should be user controlled

        // this a little FFT test to be sure that it gives the correct answer
        public double[] xtest = new double[VelocityAutoCorrelationLength];
        public double[] FFTamplitudes = new double[VelocityAutoCorrelationLength / 2];
        double[] FFTperiods = new double[VelocityAutoCorrelationLength / 2];
        public double[] FFTfreqs = new double[VelocityAutoCorrelationLength / 2];
        List<double>[] FFTmatrixForMovingAverage = new List<double>[VelocityAutoCorrelationLength / 2];  // this is an array of lists, to hold the moving FFT average
        public double[] AveragedFFTamplitudes = new double[VelocityAutoCorrelationLength / 2];

		// these need moving to private, with appropriate set & get functions!!!!
		private int CollisionsCount;

		#endregion

		// pointer to a vector of particles
		public readonly ParticleCollection Particles;

		#region Public Access Methods and Properties

		#region Global Variables Access Methods

		public int BoxWidth
		{
			get { return m_BoxWidth; } 
		}

		public int BoxHeight
		{
			get { return m_BoxHeight; } 
		}		

		#endregion

		#region Particle Variable Access Methods

		#region Number Of Particles
		
		/// <summary>
		/// number of particles
		/// </summary>
		public int NumberOfParticles
		{
			//get { return m_NumberOfParticles; }
			//set { m_NumberOfParticles = value; }
			get { return Particles.Count; }
		}

		/// <summary>
		/// Maximum possible particles in current ensemble
		/// </summary>
		/// <returns></returns>
		public int MaxNumberOfParticles
		{
			//get { return m_MaxNumberOfParticles; } 
			get { return Particles.Maximum; } 
		}

		#endregion

		/// <summary>
		/// Get the particle at the given index 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public Particle GetParticle(int i)
		{
			if (i >= MaxNumberOfParticles)
			{
				throw new IndexOutOfRangeException(string.Format("The index exceded the maximum possible for this particle ensemble {{0}}", MaxNumberOfParticles));
			}
			else if (i >= NumberOfParticles)
			{
				throw new IndexOutOfRangeException(string.Format("The index exceded the number of valid particles for this particle ensemble {{0}}", NumberOfParticles));
			}

			return Particles[i];
		}

		#region Particle Position

		public void SetXParticlePosition(int i, double NewXposition)
		{
			Particles[i].Position.X = NewXposition;
		}

		public double GetXParticlePosition(int i)
		{
			return Particles[i].Position.X;
		}

		public void SetYParticlePosition(int i, double NewYposition)
		{
			Particles[i].Position.Y = NewYposition;
		}

		public double GetYParticlePosition(int i)
		{
			return Particles[i].Position.Y;
		}

		#endregion

		#region Particle Last Position

		public void SetLastXParticlePosition(int i, double Xposition)
		{
			Particles[i].PositionLast.X = Xposition;
		}

		public double GetLastXParticlePosition(int i)
		{
			return Particles[i].PositionLast.X;
		}

		public void SetLastYParticlePosition(int i, double Yposition)
		{
			Particles[i].PositionLast.Y = Yposition;
		}

		public double GetLastYParticlePosition(int i)
		{
			return Particles[i].PositionLast.Y;
		}

		#endregion

		#region Particle Velocity
		
		public void SetXParticleVelocity(int i, double NewXVelocity)
		{
			Particles[i].Velocity.X = NewXVelocity;
		}

		public double GetXParticleVelocity(int i)
		{
			return Particles[i].Velocity.X;
		}

		public void SetYParticleVelocity(int i, double NewYVelocity)
		{
			Particles[i].Velocity.Y = NewYVelocity;
		}

		public double GetYParticleVelocity(int i)
		{
			return Particles[i].Velocity.Y;
		}

		#endregion

		#region Particle Force
		
		public void SetXParticleForce(int i, double NewXforce)
		{
			Particles[i].Force.X = NewXforce;
		}

		public double GetXParticleForce(int i)
		{
			return Particles[i].Force.X;
		}

		public void SetYParticleForce(int i, double NewYforce)
		{
			Particles[i].Force.Y = NewYforce;
		}

		public double GetYParticleForce(int i)
		{
			return Particles[i].Force.Y;
		}

		#endregion

		#region Particle Last Force

		public void SetLastXParticleForce(int i, double Xforce)
		{
			Particles[i].ForceLast.X = Xforce;
		}

		public double GetLastXParticleForce(int i)
		{
			return Particles[i].ForceLast.X;
		}

		public void SetLastYParticleForce(int i, double Yforce)
		{
			Particles[i].ForceLast.Y = Yforce;
		}

		public double GetLastYParticleForce(int i)
		{
			return Particles[i].ForceLast.Y;
		}

		#endregion

		#region Particle Radius

		public double GetParticleRadius(int i)
		{
			return Particles[i].Radius;
		}

		public double ParticleScale 
		{
			get { return ParticleStaticObjects.AtomPropertiesDefinition.RadiiScaleFactor; }
			set { ParticleStaticObjects.AtomPropertiesDefinition.ScaleParticleRadii(value); } 
		}

		#endregion

		#region Particle Kinetic Energy
		
		public void SetParticleKineticEnergy(int i, double newKE)
		{
			Particles[i].KineticEnergy = newKE;
		}

		public double GetParticleKineticEnergy(int i)
		{
			return Particles[i].KineticEnergy;
		}

		#endregion

        public void SetWasReflectedByWall(int i, bool value)
		{
			Particles[i].WasReflectedByWall = value;
		}

		public double GetParticleMass(int i)
		{
			return Particles[i].Mass;
		}

		#region Defunct Access Methods

		/*
		public void SetParticleMass(int i, double newmass)
		{
			Particles[i].Mass = newmass;
		}

		public void SetParticleRadius(int i, double newradius)
		{
			//Particles[i].Radius = newradius;
		}
		*/

		#endregion

		#endregion							

		#region Force Field Objects
		
		/// <summary>
		/// add an external forcefield from the pixels on the end of the pForceFieldVector 
		/// </summary>
		/// <param name="pixels"></param>
		public void AddForceFieldObject(ForceField pixels)
		{
			ForceFields.Add(pixels);
		}

		public ForceField GetForceFieldObject(int i)
		{
			return ForceFields[i];
		}

		public int NumberOfForceFieldObjects
		{
			get { return ForceFields.Count; } 
		}

		#endregion

		#region Simulation Realtime Variables

		#region Equilibrium Temperature

		/// <summary>
		/// Equilibrium Temperature
		/// </summary>
		public double EquilibriumTemperature 
		{
			get { return m_EquilibriumTemperature; }
			set { m_EquilibriumTemperature = value; }
		}

		#endregion

		#region Berendsen Thermostat Coupling

		/// <summary>
		/// Berendsen Thermostat Coupling
		/// </summary>
		public double BerendsenThermostatCoupling 
		{
			get { return m_BerendsenThermostatCoupling; }
			set { m_BerendsenThermostatCoupling = value; }
		}

		#endregion

		#region Gradient Scale Factor

		/// <summary>
		/// Gradient Scale Factor
		/// </summary>
		public double GradientScaleFactor 
		{ 
			get { return m_GradientScaleFactor; } 
			set { m_GradientScaleFactor = value; } 
		}

		#endregion		

		#region Potential Energy
		
		/// <summary>
		/// Potential Energy
		/// </summary>
		public double PotentialEnergy 
		{ 
			get { return m_PotentialEnergy; } 
			set { m_PotentialEnergy = value; } 
		}

		/// <summary>
		/// Add Potential Energy
		/// </summary>
		/// <param name="amount"></param>
		public void AddPotentialEnergy(double amount)
		{
			m_PotentialEnergy += amount;
		}

		#endregion

		#region Max Force Threshold

		/// <summary>
		/// Max Force Threshold
		/// </summary>
		public double MaxForceThreshold 
		{ 
			get { return m_MaxForceThreshold; } 
			set { m_MaxForceThreshold = value; } 
		} 

		/// <summary>
		/// Initial Max Force Threshold
		/// </summary>
		public double InitialMaxForceThreshold
		{
			get { return m_InitialMaxForceThreshold; } 
		}

		#endregion	

		/// <summary>
		/// Timestep, the ammount to increment time by inside the simulation each frame / sub-frame
		/// </summary>
		public double Timestep
		{
			get { return m_Timestep; }
			set { m_Timestep = value; }
		}

		#endregion

		#region Soon to be defunct methods
		
		/* 
		public double GetRadiiScaleFactor()
		{
			return radiiScaleFactor;
		}				
		*/ 

		public void SetEnsembleReinitializationFlag()
		{
			EnsembleReinitializationFlag = true;
		}

		public void SetNumberOfParticlesChangedFlag()
		{
			NumberOfParticlesChangedFlag = true;
		}

		public void SetNumberOfParticlesIsGreater()
		{
			NumberOfParticlesIsGreaterFlag = true;
		}

		#endregion

		#region Forces

		public void AddXForces(double[] NewXforces)
		{
			for (int i = 0; i < NumberOfParticles; ++i)
			{
				double newforce = Particles[i].Force.X + NewXforces[i];
				Particles[i].Force.X = newforce;
			}
		}

		public void AddYForces(double[] NewYforces)
		{
			for (int i = 0; i < NumberOfParticles; ++i)
			{
				double newforce = Particles[i].Force.Y + NewYforces[i];
				Particles[i].Force.Y = newforce;
			}
		}

		public void ZeroXForces()
		{
			for (int i = 0; i < NumberOfParticles; ++i)
			{
				Particles[i].Force.X = 0.0;
			}
		}

		public void ZeroYForces()
		{
			for (int i = 0; i < NumberOfParticles; ++i)
			{
				Particles[i].Force.Y = 0.0;
			}
		}

		#endregion

		#endregion

		#region Constructor

		public ParticleEnsemble(int nparticles, double MinRad, double MaxRad, List<string> FFtype, int Height, int Width, double scaleFactor)
		{
			int i;
			
			// set the maximum number of particles to 1000
			m_MaxNumberOfParticles = 1000;

			Particles = new ParticleCollection(this, m_MaxNumberOfParticles); 

			// value of the boltzmann constant - we dont really care about units now...
			kb = 8.314;									
			// value of the berendsen coupling constant - set this to be controlled by the artist...
			m_BerendsenThermostatCoupling = 1.0;		
			
			m_EquilibriumTemperature = 20000;
			BerendsenThermostat = true;

			//  BerendsenThermostat=false;
			EnsembleReinitializationFlag = false;
			NumberOfParticlesChangedFlag = false;
			NumberOfParticlesIsGreaterFlag = false;

			m_RadiiScaleFactor = 1.0;
			InitialKE = 0.0;
			m_GradientScaleFactor = scaleFactor;

			step = 0;
			//NumberOfParticles = nparticles;
			m_BoxHeight = Height;
			m_BoxWidth = Width;

			m_MaxForceThreshold = 1.0e6;
			m_InitialMaxForceThreshold = m_MaxForceThreshold;

            // allocate maximum space for and initialize the velocity autocorrelation & FFT vectors
            m_FFTenabled = true;
            m_FFTofCorrelationFunction = new List<double>(new double[MAX_CORR_FTN_SIZE]);
            m_VelocityAutoCorrelationFunction = new double[MAX_CORR_FTN_SIZE];
            for (int ii = 0; ii < MAX_CORR_FTN_SIZE; ++ii)
            {
                m_VelocityAutoCorrelationFunction[ii] = 0.0;
                m_FFTofCorrelationFunction[ii] = 0.0;
            }

            for (int ii = 0; ii < FFTmatrixForMovingAverage.Count(); ++ii)
            {
                FFTmatrixForMovingAverage[ii]= new List<double>(new double[m_NumberOfFFTAverages]);
                for(int kk=0; kk < m_NumberOfFFTAverages; ++kk){
                    FFTmatrixForMovingAverage[ii][kk] = 0.0;
                }

            }

			InitialMinVel = 100.0;
			InitialMaxVel = 200.0;

			for (int ii = 0; ii < m_MaxNumberOfParticles; ++ii)
			{
				InitializeOneNewParticle();
			}

			for (int ii = nparticles; ii < m_MaxNumberOfParticles; ++ii)
			{
				Particles.Pop();
			}

			if (!EliminateParticleOverlap(Height, Width))
			{
				// adjust particle positions to eliminate overlap 
				do
				{
					// if there's too many particles to fit in the simulation box
					//NumberOfParticles -= 1;
					Particles.Pop();

					// decrement the particles till it's ok
					EliminateParticleOverlap(Height, Width);				
				}
				while (!EliminateParticleOverlap(Height, Width));
			}
			
			// create matrix to distanceMatrix & distanceMatrixLastTime
			distanceMatrix = new DPMatrix(0.0, MaxNumberOfParticles, MaxNumberOfParticles);
			distanceMatrixLastTime = new DPMatrix(0.0, MaxNumberOfParticles, MaxNumberOfParticles);
			distanceMatrixLastLastTime = new DPMatrix(0.0, MaxNumberOfParticles, MaxNumberOfParticles);
 
			// update the particlesWithinRange matrix
			particlesWithinRange = new BoolMatrix(false, MaxNumberOfParticles, MaxNumberOfParticles); 

			// update the interparticle separation matrix
			UpdateInterParticleSeparations();						

			m_PotentialEnergy = 0.0;

			// push back forceField objects onto pForceFieldVector - at present, we only have LJ forces, but this can be
			// easily expanded
			for (i = 0; i < FFtype.Count; ++i)
			{
				if (FFtype[i] == "SoftSpheres") { ForceFields.Add(new SoftSpheres(this)); }
			}
		}

		#endregion

		#region Init / Setup Particles

		public void InitializeOneNewParticle()
		{
			Particle pParticle;
			pParticle = new Particle();

			int type = 0;
			type = DetermineParticleType();
			pParticle.TypeID = type;

			AtomicInfo typeInfo = ParticleStaticObjects.AtomPropertiesDefinition.Lookup[type];

			//pParticle.Radius = typeInfo.Mass * 0.5;
			//pParticle.InitialRadius = typeInfo.Mass * 0.5;
			pParticle.Position.X = (ParticleStaticObjects.RandomDouble(0.0 + pParticle.Radius, BoxWidth - pParticle.Radius));
			pParticle.Position.Y = (ParticleStaticObjects.RandomDouble(0.0 + pParticle.Radius, BoxHeight - pParticle.Radius));
			pParticle.Velocity.X = ParticleStaticObjects.RandomDouble(-1.0 * InitialMinVel, InitialMaxVel);
			pParticle.Velocity.Y = ParticleStaticObjects.RandomDouble(-1.0 * InitialMinVel, InitialMaxVel);
			pParticle.Mass = (10);
			Particles.Add(pParticle);
		}

		public void SetPropertiesForParticle(int newParticleIdx)
		{
			int type = 0;
			type = DetermineParticleType();
			Particles[newParticleIdx].TypeID = type;

			AtomicInfo typeInfo = ParticleStaticObjects.AtomPropertiesDefinition.Lookup[type];

			//Particles[newParticleIdx].Radius = (typeInfo.Mass * 0.5);
			//Particles[newParticleIdx].InitialRadius = (typeInfo.Mass * 0.5);
			Particles[newParticleIdx].Mass = (10);
		}

		/// <summary>
		/// this function initializes a random particle with index particleIdx, giving it a position that does not overlap 
		/// with all particles whose index is less than or equal to particleIdx
		/// </summary>
		/// <param name="particleIdx"></param>
		/// <param name="Height"></param>
		/// <param name="Width"></param>
		/// <returns></returns>
		public bool InitializeRandomParticlePosition(int particleIdx, int Height, int Width)
		{

			int i, jj, iterations;
			double ijjSeparation = 0.0;
			bool ParticlesOverlap = true;

			//ofSeedRandom();   	//  Seed random number generator to clock time, so random numbers are always different    

			i = particleIdx;

			SetXParticleVelocity(i, ParticleStaticObjects.RandomDouble(-1.0 * InitialMinVel, InitialMaxVel));
			SetYParticleVelocity(i, ParticleStaticObjects.RandomDouble(-1.0 * InitialMinVel, InitialMaxVel));
			SetXParticlePosition(i, ParticleStaticObjects.RandomDouble(GetParticleRadius(i), BoxWidth - GetParticleRadius(i)));
			SetYParticlePosition(i, ParticleStaticObjects.RandomDouble(GetParticleRadius(i), BoxHeight - GetParticleRadius(i)));

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
// THIS ONE !UsePOW
#if !UsePOW
						ijjSeparation = Math.Sqrt(Math.Pow((GetXParticlePosition(i) - GetXParticlePosition(jj)), 2.0) + Math.Pow((GetYParticlePosition(i) - GetYParticlePosition(jj)), 2.0));
#else
						ijjSeparation = Math.Sqrt(MathHelper.Pow2((GetXParticlePosition(i) - GetXParticlePosition(jj))) + MathHelper.Pow2((GetYParticlePosition(i) - GetYParticlePosition(jj))));
#endif

						if (ijjSeparation <= (GetParticleRadius(i) + GetParticleRadius(jj))) { ParticlesOverlap = true; }
					}
					
					if (ParticlesOverlap)
					{
						SetXParticlePosition(i, ParticleStaticObjects.RandomDouble(GetParticleRadius(i), BoxWidth - GetParticleRadius(i)));
						SetYParticlePosition(i, ParticleStaticObjects.RandomDouble(GetParticleRadius(i), BoxHeight - GetParticleRadius(i)));
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
					type = ParticleStaticObjects.AtomPropertiesDefinition.Count- 1;
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

		#region Eliminate Particle Overlap

		/// <summary>
		/// function to initialize random velocities and non-overlapping positions for all the particles
		/// </summary>
		/// <param name="Height"></param>
		/// <param name="Width"></param>
		/// <returns></returns>
		bool EliminateParticleOverlap(int Height, int Width)
		{
			int i, jj, iterations;
			bool everythingOK = true;

			for (i = 0; i < NumberOfParticles; ++i)
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
				InitialKE = TotalKineticEnergy;
				return true;
			}
		}

		#endregion

		#endregion

		#region Resize

		public void Resize(int width, int height)
		{
			m_BoxWidth = width;
			m_BoxHeight = height;

			for (int i = 0; i < NumberOfParticles; ++i)
			{
				double pxnew = GetXParticlePosition(i);
				double pynew = GetYParticlePosition(i);

				if (pxnew < GetParticleRadius(i))
				{
					pxnew = GetParticleRadius(i); 
				}
				else if (pxnew > (BoxWidth - GetParticleRadius(i))) 
				{
					pxnew = BoxWidth - GetParticleRadius(i); 
				}

				if (pynew < GetParticleRadius(i)) 
				{
					pynew = GetParticleRadius(i); 
				}
				else if (pynew > (BoxHeight - GetParticleRadius(i)))
				{
					pynew = BoxHeight - GetParticleRadius(i); 
				}

				Particles[i].Position.X = pxnew;
				Particles[i].Position.Y = pynew; 
				Particles[i].GridSector.GetSector(Particles[i].Position);
			}
		}

		#endregion

		#region Simulation Frame

		/// <summary>
		/// velocity verlet routine to propagate the particle ensemble
		/// </summary>
		/// <param name="Height"></param>
		/// <param name="Width"></param>
		/// <param name="pExternalField"></param>
		public void VelocityVerletPropagation(ExternalField pExternalField)
		{
			int i, kk;
			double V = 0.0, T = 0.0, dt, pxnew, pynew, factor;

			AllPositionsUnchangedFromLastStep = true;

			//BoxWidth = Width;
			//BoxHeight = Height;

			if (NumberOfParticlesChangedFlag)
			{
				NumberOfParticlesChangedFlag = false;

				if (NumberOfParticlesIsGreaterFlag)
				{
					for (kk = 0; kk < NumberOfForceFieldObjects; ++kk)
					{				
						// calculate the ij energy terms for particle set including the new ones
						GetForceFieldObject(kk).UpdateEnergyTerms(this);
					}
				}
			}
			else
			{

				//  Timestep = 1.0/(double)ofGetFrameRate();
				Timestep = 0.005;

				dt = Timestep;
				
				// increment ParticleEnsemble Private data member step
				++step;                                 

				if (BerendsenThermostat)
				{                
					//  Berendsen Thermostat
					BerendsenVelocityRescaling();
				}

				// this loop uses verlet scheme (VV) to propagate the positions forward one step
				for (i = 0; i < NumberOfParticles; ++i)
				{
					SetLastXParticlePosition(i, GetXParticlePosition(i));
					SetLastYParticlePosition(i, GetYParticlePosition(i));

					factor = 0.5 * dt * dt / GetParticleMass(i);

					pxnew = GetXParticlePosition(i) + GetXParticleVelocity(i) * dt + GetXParticleForce(i) * factor;
					pynew = GetYParticlePosition(i) + GetYParticleVelocity(i) * dt + GetYParticleForce(i) * factor;

					if (pxnew > GetParticleRadius(i) && pxnew < (BoxWidth - GetParticleRadius(i)))
					{
						// this the standard VV code here
						SetXParticlePosition(i, pxnew);                              
					}
					else
					{ 
						// this is to reflect off the walls; added by DRG in lieu of soft walls to improve real time stability... not part of a standard VV scheme      
						SetXParticlePosition(i, GetLastXParticlePosition(i));
						SetXParticleVelocity(i, -1.0 * GetXParticleVelocity(i));
						SetWasReflectedByWall(i, true);
						calculateParticleVelocitiesInXWallFrame(i);
					}

					if (pynew > GetParticleRadius(i) && pynew < (BoxHeight - GetParticleRadius(i)))
					{     
						// this the standard VV code here
						SetYParticlePosition(i, pynew);
					}
					else
					{  
						// this is to reflect off the walls; added by DRG in lieu of soft walls to improve real time stability... not part of a standard VV scheme
						SetYParticlePosition(i, GetLastYParticlePosition(i));
						SetYParticleVelocity(i, -1.0 * GetYParticleVelocity(i));
						SetWasReflectedByWall(i, true);
						calculateParticleVelocitiesInYWallFrame(i);						
					}

					Particles[i].GridSector.GetSector(Particles[i].Position);
				}

				// check whether all the positions are changed from the last step
				for (i = 0; i < NumberOfParticles; ++i)
				{
					if (GetYParticlePosition(i) != GetLastYParticlePosition(i) || GetXParticlePosition(i) != GetLastXParticlePosition(i))
					{
						AllPositionsUnchangedFromLastStep = false;
					}
				}


				if (AllPositionsUnchangedFromLastStep)
				{    
					// this is a stability measure; if the frame is frozen wrt to the previous frame,					

					// adjust particle positions to eliminate overlap - this can cause the sim to freeze
					EliminateParticleOverlap(BoxHeight, BoxWidth);    

					for (i = 0; i < NumberOfParticles; ++i)
					{           
						//  then we zero out the forces and velocities & repropagate the positions						
						SetXParticleForce(i, 0.0);
						SetYParticleForce(i, 0.0);
						SetLastXParticlePosition(i, GetXParticlePosition(i));
						SetLastYParticlePosition(i, GetYParticlePosition(i));
						SetXParticlePosition(i, GetXParticlePosition(i) + GetXParticleVelocity(i) * dt + (GetXParticleForce(i) / GetParticleMass(i)) * dt * dt * 0.5);
						SetYParticlePosition(i, GetYParticlePosition(i) + GetYParticleVelocity(i) * dt + (GetYParticleForce(i) / GetParticleMass(i)) * dt * dt * 0.5);

						Particles[i].GridSector.GetSector(Particles[i].Position);
					}

					AllPositionsUnchangedFromLastStep = false;
				}

				UpdateInterParticleSeparations();

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
						// save the present forces to t-1 vectors
						SetLastXParticleForce(i, GetXParticleForce(i));
						SetLastYParticleForce(i, GetYParticleForce(i));
					}

					// zero out the force vectors & potential energy
					ZeroXForces();			
					ZeroYForces();
					PotentialEnergy = 0.0;

					for (kk = 0; kk < NumberOfForceFieldObjects; ++kk)
					{				
						// calculate & set the forces at the new positions
						GetForceFieldObject(kk).CalculateForceField(this);
					}

					for (i = 0; i < NumberOfParticles; ++i)
					{                  
						// use VV scheme to propagate the velocities forward
						factor = dt * 0.5 / GetParticleMass(i);
						SetXParticleVelocity(i, GetXParticleVelocity(i) + (GetXParticleForce(i) + GetLastXParticleForce(i)) * factor);
						SetYParticleVelocity(i, GetYParticleVelocity(i) + (GetYParticleForce(i) + GetLastYParticleForce(i)) * factor);
					}

                    // Update the ensemble velocity autocorrelation function
                    //if (m_FFTenabled)
                    //{
                    //    UpdateVelocityAutoCorrelationFunction();
                    //    FFTVelocityAutoCorrelationFunction();
                    //}

					// see whether any collisions occurred
					DetermineIfCollisionsOccurred();

				}
			}

		}

		/// <summary>
		/// this function is for the simple Berendsen Thermostat
		/// </summary>
		public void BerendsenVelocityRescaling()
		{
			double scaleFactor;
			//int i;

			// this is an extra velocity rescaling measure to improve real-time stability... not part of Berendsen!!!!
			// be sure that no single particle has a KE which differs from the average by 3 standard deviations (sigmas) 
			CalculateKineticEnergiesAndTemperature();
			double sigma = 2.0;
			for (int i = 0; i < NumberOfParticles; ++i)
			{
				if ((GetParticleKineticEnergy(i) - AverageKineticEnergy) > (sigma * SDKineticEnergy))
				{
					scaleFactor = (sigma * SDKineticEnergy) / (GetParticleKineticEnergy(i) - AverageKineticEnergy);
					SetXParticleVelocity(i, scaleFactor * GetXParticleVelocity(i));
					SetYParticleVelocity(i, scaleFactor * GetYParticleVelocity(i));
				}
			}

			// again, a real-time stability measure... not part of Berendsen!!!!
			// re-initialize the system if the temperature gets crazy
			if (temperature > 1.0e8)
			{
				//cout << " T = " << temperature << " scaleFactor " << scaleFactor << endl;
				
				// adjust particle positions to eliminate overlap - this can cause the sim to freeze
				EliminateParticleOverlap(BoxHeight, BoxWidth);    

				for (int i = 0; i < NumberOfParticles; ++i)
				{           
					//  then we zero out the forces and velocities 
					SetXParticleForce(i, 0.0);
					SetYParticleForce(i, 0.0);
					SetLastXParticlePosition(i, GetXParticlePosition(i));
					SetLastYParticlePosition(i, GetYParticlePosition(i));
				}
				CalculateKineticEnergiesAndTemperature();
			}

			// this code here is the bona fide Berendsen thermostat !!!!
			scaleFactor = Math.Sqrt(m_EquilibriumTemperature / (m_BerendsenThermostatCoupling * temperature));
			if (scaleFactor != 1.0)
			{
				for (int i = 0; i < NumberOfParticles; ++i)
				{						
					//rescale the velocities
					SetXParticleVelocity(i, scaleFactor * GetXParticleVelocity(i));
					SetYParticleVelocity(i, scaleFactor * GetYParticleVelocity(i));
				}
			}
		}

		#endregion		

		#region Calculate Kinetic Energies And Temperature
		
		public void CalculateKineticEnergiesAndTemperature()
		{
			// calculate the total, avg, & standard deviation of the kinetic energy

			double sum = 0.0;
			TotalKineticEnergy = 0.0;
			AverageKineticEnergy = 0.0;
			SDKineticEnergy = 0.0;
			double KE = 0.0;

			for (int i = 0; i < NumberOfParticles; ++i)
			{
// THIS ONE !UsePOW
#if !UsePOW
				KE = 0.5 * GetParticleMass(i) * (Math.Pow(GetXParticleVelocity(i), 2.0) + Math.Pow(GetYParticleVelocity(i), 2.0));
#else
				KE = 0.5 * GetParticleMass(i) * (MathHelper.Pow2(GetXParticleVelocity(i)) + MathHelper.Pow2(GetYParticleVelocity(i)));
#endif
				Particles[i].KineticEnergy = (KE);
				TotalKineticEnergy += KE;
			}

			AverageKineticEnergy = TotalKineticEnergy / NumberOfParticles;
			temperature = TotalKineticEnergy / (NumberOfParticles * kb);

			for (int i = 0; i < NumberOfParticles; ++i) 
			{
// THIS ONE !UsePOW
#if !UsePOW
				sum += Math.Pow((Particles[i].KineticEnergy - AverageKineticEnergy), 2.0); 
#else 
				sum += MathHelper.Pow2((Particles[i].KineticEnergy - AverageKineticEnergy)); 
#endif
			}

			SDKineticEnergy = Math.Sqrt(sum / NumberOfParticles);

		}

		#endregion

		#region Particle Collision Detection

		/// <summary>
		/// function to determine if a collision happened
		/// </summary>
		public void DetermineIfCollisionsOccurred()
		{
			for (int i = 0; i < NumberOfParticles; ++i)
			{
				for (int j = (i + 1); j < NumberOfParticles; ++j)
				{
					if (particlesWithinRange[i, j] == true)
					{
						if (distanceMatrixLastTime[i, j] < distanceMatrix[i, j] && distanceMatrixLastTime[i, j] < distanceMatrixLastLastTime[i, j])
						{
							Particles[i].CollisionOccurred = true;
							Particles[j].CollisionOccurred = true;

							calculateParticleVelocitiesInComFrame(i, j);
						}
					}
				}
			}
		}

		/// <summary>
		/// update the interparticle distanceMatrix
		/// </summary>
		public void UpdateInterParticleSeparations()
		{
			double ijSeparation = 0;

			DPMatrix temp = distanceMatrixLastLastTime;
			distanceMatrixLastLastTime = distanceMatrixLastTime;
			distanceMatrixLastTime = distanceMatrix;
			distanceMatrix = temp;

			int numberOfParticles = NumberOfParticles;

			for (int i = 0; i < numberOfParticles; ++i)
			{
				Particle particlei = Particles[i];

				for (int j = (i + 1); j < numberOfParticles; ++j)
				{
					Particle particlej = Particles[j];

					if (particlei.GridSector.IsInJoiningSectors(particlej.GridSector) == false)
					{
						continue;
					}

// THIS ONE !UsePOW`
#if !UsePOW
					// SQRT MOD
					//ijSeparation = Math.Sqrt(Math.Pow(Particles[i].Position.X - Particles[j].Position.X, 2.0) + Math.Pow(Particles[i].Position.Y - Particles[j].Position.Y, 2.0));
					ijSeparation = Math.Pow(Particles[i].Position.X - Particles[j].Position.X, 2.0) + Math.Pow(Particles[i].Position.Y - Particles[j].Position.Y, 2.0);
#else 
					// SQRT MOD
					//ijSeparation = Math.Sqrt(MathHelper.Pow2(Particles[i].Position.X - Particles[j].Position.X) + MathHelper.Pow2(Particles[i].Position.Y - Particles[j].Position.Y));
					ijSeparation = MathHelper.Pow2(Particles[i].Position.X - Particles[j].Position.X) + MathHelper.Pow2(Particles[i].Position.Y - Particles[j].Position.Y);
#endif
					// assign the present distances
					distanceMatrix[i, j] = ijSeparation;
					distanceMatrix[j, i] = ijSeparation;
				}
			}
		}		

		public void SetParticlesWithinRange(int i, int j)
		{
			particlesWithinRange[i, j] = true;
			particlesWithinRange[j, i] = true;
		}

		public void SetParticlesNotWithinRange(int i, int j)
		{
			particlesWithinRange[i, j] = false;
			particlesWithinRange[j, i] = false;
		}

		public void ResetAllParticlesNotWithinRange()
		{
			particlesWithinRange.SetValueRange(false, NumberOfParticles, NumberOfParticles);
		}				

		public void ResetParticleCollisions()
		{
			for (int i = 0; i < NumberOfParticles; ++i)
			{
				Particles[i].MarkParticleDidNotCollide();
			}
		}

		public double GetInterParticleSeparation(int i, int j)
		{
			return distanceMatrix[i, j];
		}

		#endregion

		#region Calculate Particle Velocities In Com Frame

		/// <summary>
		/// function to calculate the velocity of particles i & j in their center of mass frame
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		public void calculateParticleVelocitiesInComFrame(int i, int j)
		{
			int kk, dimensions = 3;
			double[] Vi = new double[dimensions], Qi = new double[dimensions], Vj = new double[dimensions], Qj = new double[dimensions];
			double[] Vcom = new double[dimensions], Vicom = new double[dimensions], Vjcom = new double[dimensions], n12 = new double[dimensions];
			double[] ViParProj = new double[dimensions], VjParProj = new double[dimensions];

			// velocity vectors for particle i
			Vi[0] = Particles[i].Velocity.X; 	
			Vi[1] = Particles[i].Velocity.Y;
			// position vectors for particle i
			Qi[0] = Particles[i].Position.X; 		
			Qi[1] = Particles[i].Position.Y;

			// velocity vectors for particle j
			Vj[0] = Particles[j].Velocity.X;		  
			Vj[1] = Particles[j].Velocity.Y;
			// position vectros for particle j
			Qj[0] = Particles[j].Position.X;			
			Qj[1] = Particles[j].Position.Y;

			// calculate interparticle separation distance
			// SQRT MOD
			//double ijSeparation = GetInterParticleSeparation(i, j);
			double ijSeparation = Math.Sqrt(GetInterParticleSeparation(i, j));   

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
#if !UsePOW
			double ViParMagnitude = Math.Sqrt(Math.Pow(ViParProj[0], 2.0) + Math.Pow(ViParProj[1], 2.0) + Math.Pow(ViParProj[2], 2.0));
			double VjParMagnitude = Math.Sqrt(Math.Pow(VjParProj[0], 2.0) + Math.Pow(VjParProj[1], 2.0) + Math.Pow(VjParProj[2], 2.0));
#else 
			double ViParMagnitude = Math.Sqrt(MathHelper.Pow2(ViParProj[0]) + MathHelper.Pow2(ViParProj[1]) + MathHelper.Pow2(ViParProj[2]));
			double VjParMagnitude = Math.Sqrt(MathHelper.Pow2(VjParProj[0]) + MathHelper.Pow2(VjParProj[1]) + MathHelper.Pow2(VjParProj[2]));
#endif
			Particles[i].VInCollisionFrame = ViParMagnitude;
			Particles[j].VInCollisionFrame = VjParMagnitude;

		}

		#endregion

		#region calculate Particle Velocities In Wall Frame
		
		/// <summary>
		/// function to calculate the velocity of particle i in the wall frame	
		/// </summary>
		/// <param name="i"></param>
		public void calculateParticleVelocitiesInXWallFrame(int i)
		{
			// velocity vectors for particle i in x direction
			double velx = Particles[i].Velocity.X; 		

			Particles[i].VInCollisionFrame = velx / 2.0;			
		}

		public void calculateParticleVelocitiesInYWallFrame(int i)
		{
			// velocity vectors for particle i in x direction
			double vely = Particles[i].Velocity.Y; 	
				
			Particles[i].VInCollisionFrame = vely / 2.0;
		}

		#endregion

		#region FFT

		public void UpdateVelocityAutoCorrelationFunction()
		{

            // zero out the velocityAutoCorrelationFunction Vector
            for (int i = 0; i < m_VelocityAutoCorrelationFunction.Count(); ++i)
            {
                m_VelocityAutoCorrelationFunction[i] = 0.0;
            }

            // now calculate the total ensemble's velocity autocorrelation function as:
            //        
            //             ____2n
            //             \     
            //  C(0)C(t) =  >   Vj(0) * Vj(t)
            //             /___
            //                 j=1
            //
            // where v is velocity and n is the total number of particles, multiplied by 2 because the velocity of each 
            //         particle has both an x and y component
            // this equation is effectively the dot product of the V(0) and V(t) vectors

            for (int j = 0; j < Particles.Count; ++j)
            {
                Particles[j].UpdateVelocityAutoCorrelationFunction();       // update the velocity history of each particle over the last 2^n timesteps

                double x0 = Particles[j].GetXVelocityCorrelationElement(0); // get x0 and v0 for each particle
                double y0 = Particles[j].GetYVelocityCorrelationElement(0);

                for (int i = 0; i < VelocityAutoCorrelationLength; ++i)
                {
                    m_VelocityAutoCorrelationFunction[i] += x0 * Particles[j].GetXVelocityCorrelationElement(i);
                    m_VelocityAutoCorrelationFunction[i] += y0 * Particles[j].GetYVelocityCorrelationElement(i);
                }

            }

            for (int jj = 0; jj < VelocityAutoCorrelationLength; ++jj)
            {
                xtest[jj] = m_VelocityAutoCorrelationFunction[jj];
//                xtest[jj] = Math.Cos(0.1 * (Math.PI) * jj) + Math.Cos(0.2 * (Math.PI) * jj);
            }

		}

        public void FFTVelocityAutoCorrelationFunction()
		{
            // zero out the FFT vector
            for(int jj = 0; jj < MAX_CORR_FTN_SIZE; ++jj)
			{
                m_FFTofCorrelationFunction[jj] = 0.0;
            }
            // copy the velocity autocorrelation function to the FFT vector
            for(int jj = 0; jj < VelocityAutoCorrelationLength; ++jj)
            {
				// for the sake of efficiency, the velocity autocorrelation functions on the particle are PUBLIC data... beware!!!
                m_FFTofCorrelationFunction[jj] = m_VelocityAutoCorrelationFunction[jj];
			}

/*          
            // all this stuff was simply to test out the hardcoded realft code below before alglib
            List<double> listtest = new List<double>(xtest); // Copy to List
            
            // the vector that goes into the FFT routine MUST be of dimension 2^n
            realft(listtest, 1);

            // note that to get frequencies, the result must be divided by the number of entries in the list
            double[] frequencies = new double[listtest.Count];
            
            for (int jj = 0; jj < listtest.Count; ++jj ){
                if (listtest[jj] != 0.0){frequencies[jj] = listtest.Count / listtest[jj];}
                else{frequencies[jj] = 0.0;}
            }
 
            realft(FFTofCorrelationFunction, 1);
*/

            alglib.complex[] z;             // set up an array of complex numbers
            alglib.fftr1d(xtest, out z);    // do a 1d FFT of a real data array, output is the complex coefficients

            for (int ii = 0; ii < VelocityAutoCorrelationLength/2; ++ii)
            {
                // FFTamplitudes contains the spectral amplitudes, calculated as magnitudes of the complex FFT coefficients
                FFTamplitudes[ii] = Math.Sqrt(z[ii].x * z[ii].x + z[ii].y * z[ii].y);

                // insert the FFT amplitudes into the list elements to keep track of the moving average in FFTmatrixForMovingAverage
                FFTmatrixForMovingAverage[ii].RemoveAt(m_NumberOfFFTAverages - 1);
                FFTmatrixForMovingAverage[ii].Insert(0, FFTamplitudes[ii]);

                // averagedFFTamplitudes contains the spectral amplitudes, calculated as magnitudes of the complex FFT coefficients
                AveragedFFTamplitudes[ii] = FFTmatrixForMovingAverage[ii].Average();

                // FFTperiods contains the periods to which the amplitudes correspond
                FFTperiods[ii] = ((double)(VelocityAutoCorrelationLength) / ii) / RugTech1.Framework.GameEnvironment.FramesPerSecond;
                // - units of FFTperiods is time (it's a period)

                // FFTfreqs contains the frequencies to which the amplitudes correspond (inverse of the periods)
                FFTfreqs[ii] = 1 / FFTperiods[ii];
                // - units of FFTfreqs is now in Hz
            }

            /* 
             * A note on the FFT data: 
             * a single dynamics step corresponds to (draw frequency)/(m_DrawFrequency).
             * Presently the draw frequency is 1/60 s, and m_DrawFrequency=1, so that 1 dynamics step is 1/60 s.
             * Hence, a velocity autocorrelation ftn with a length of, e.g., 512, 
             *        corresponds to 512/60, or ~ 8.5 s of particle action
             * 
             * So the slowest frequency we can detect is something that happens every (8.5/2), or ~4 seconds (~0.25 Hz).
             * The fastest frequency we can detect is something that happens every ~1/60 seconds (60 Hz).
             * This spans about 2.5 orders of magnitude. The human ear can approximately detect sounds between
             * 20 - 20,000 Hz, which is 3 orders of magnitude. This might inform how we scale the frequencies
             * to give something which makes audible sense.
            */


            int dummy = 0;
		}

        void RealFT(List<double> data, int isign)
		{
			int i, i1, i2, i3, i4;
			double c1 = 0.5, c2, h1r, h1i, h2r, h2i, wr, wi, wpr, wpi, wtemp, theta;

			int n = data.Count;
			theta = Math.PI / (double)(n >> 1);
			if (isign == 1)
			{
				c2 = -0.5;
				Four1(data, 1);
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
				Four1(data, -1);
			}
		}


		//#define SWAP(a,b) tempr=(a);(a)=(b);(b)=tempr
		void SwapIndexValues(List<double> data, int a, int b)
		{
			double temp = data[a];
			data[a] = data[b];
			data[b] = temp;
		}

		void Four1(List<double> data, int isign)
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

		/*
		/// <summary>
		/// this function allows dynamic scaling of the particle Radii
		/// </summary>
		/// <param name="newScaleFac"></param>
		public void ScaleParticleRadii(double newScaleFac)
		{
			// update the particle radii
			radiiScaleFactor = newScaleFac;
			for (int i = 0; i < NumberOfParticles; ++i)
			{
				SetParticleRadius(i, radiiScaleFactor * Particles[i].InitialRadius);
			}
			// update any necessary forceField terms (e.g., LJ terms for SoftSpheres)
			if (GetForceFieldObject(0).ForceFieldType == "SoftSphereForceField")
			{
				GetForceFieldObject(0).UpdateEnergyTerms(this);
			}
		}
		*/		
	}
}
