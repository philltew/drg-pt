using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticlesOpt2
{
	public class ParticleEnsemble
	{
		public const int MAX_PARTICLE_COLLISIONS = 50;


		private int NumberOfParticles;
		private int maxNumberOfParticles;
		private int step;
		private int NeighborListSize;

		private double InitialMinVel, InitialMaxVel;

		//  box boundaries
		private int BoxWidth;
		private int BoxHeight;
		private int maxPixelIndex;

		//  stuff for BerendsenThermostatting...
		private double kb, BerendsenCoupling, Tequilibrium;
		private double temperature, scaleFactor, InitialKE;
		private bool BerendsenThermostat;

		private bool ThereIsMidiOutput;
		private bool EnsembleReinitializationFlag;
		private bool NumberOfParticlesChangedFlag;
		private bool NumberOfParticlesIsGreaterFlag;

		//  stuff for stability
		private bool AllPositionsUnchangedFromLastStep;
		private double MaxForceThreshold, initialMaxForceThreshold;

		// stuff for kinetic energies
		private double TotalKineticEnergy, AverageKineticEnergy, SDKineticEnergy;

		// the gradient scale factor...
		private double gradientScaleFactor;

		// the radius scale factor...
		private double radiiScaleFactor;

		private double Timestep;
		private double PotentialEnergy, TotalEnergy;

		private List<ForceField> pForceFieldVector = new List<ForceField>();		// pointer to a vector of ForceField objects

		private List<double> AverageVelocityAutoCorrelationFunction;

		private List<bool> InnerTurningPointCrossed = new List<bool>();

		private DPMatrix distanceMatrix;				// matrix with the interparticle separations of particle i-j at time t
		private DPMatrix distanceMatrixLastTime;		// matrix with the interparticle separations of particle i-j at time t-1
		private DPMatrix distanceMatrixLastLastTime;	// matrix with the interparticle separations of particle i-j at time t-2

		private BoolMatrix particlesWithinRange;			// matrix of booleans that tell us if particles i-j are within range of one another

		// these need moving to private, with appropriate set & get functions!!!!
		private int CollisionsCount;

		// pointer to a vector of particles
		public List<Particle> pParticleVector = new List<Particle>();

		public ParticleEnsemble(int nparticles, double MinRad, double MaxRad, List<string> FFtype, int Height, int Width, double scaleFactor)
		{
			int i;
			maxNumberOfParticles = 1024;	// set the maximum number of particles to 1000

			kb = 8.314;									// value of the boltzmann constant - we dont really care about units now...
			BerendsenCoupling = 1.0;		// value of the berendsen coupling constant - set this to be controlled by the artist...
			Tequilibrium = 20000;
			BerendsenThermostat = true;
			//  BerendsenThermostat=false;
			EnsembleReinitializationFlag = false;
			NumberOfParticlesChangedFlag = false;
			NumberOfParticlesIsGreaterFlag = false;

			radiiScaleFactor = 1.0;
			InitialKE = 0.0;
			gradientScaleFactor = scaleFactor;

			step = 0;
			NumberOfParticles = nparticles;
			BoxHeight = Height;
			BoxWidth = Width;

			MaxForceThreshold = 1.0e6;
			initialMaxForceThreshold = MaxForceThreshold;

			AverageVelocityAutoCorrelationFunction = new List<double>(new double[1024]);

			InitialMinVel = 100.0;
			InitialMaxVel = 200.0;

			for (int ii = 0; ii < maxNumberOfParticles; ++ii)
			{
				initializeOneNewParticle();
			}

			if (!EliminateParticleOverlap(Height, Width))
			{
				// adjust particle positions to eliminate overlap 
				do
				{
					//cout << "Can't fit " << NumberOfParticles << " particles into the simulation box " << endl;
					NumberOfParticles -= 1;                       // if there's too many particles to fit in the simulation box
					//cout << "Decrementing the number of particles to " << NumberOfParticles << endl;
					EliminateParticleOverlap(Height, Width);				// decrement the particles till it's ok
				}
				while (!EliminateParticleOverlap(Height, Width));
			}

			//DPMatrix tmp1 = new DPMatrix(0.0, maxNumberOfParticles, maxNumberOfParticles);			// assign tmp1 matrix to distanceMatrix & distanceMatrixLastTime
			distanceMatrix = new DPMatrix(0.0, maxNumberOfParticles, maxNumberOfParticles);
			distanceMatrixLastTime = new DPMatrix(0.0, maxNumberOfParticles, maxNumberOfParticles);
			distanceMatrixLastLastTime = new DPMatrix(0.0, maxNumberOfParticles, maxNumberOfParticles);

			particlesWithinRange = new BoolMatrix(false, maxNumberOfParticles, maxNumberOfParticles); // update the particlesWithinRange matrix

			UpdateInterParticleSeparations();						// update the interparticle separation matrix

			PotentialEnergy = 0.0;

			//initialize forcefield Object
			//	cout << FFtype[0] << endl;
			//if(FFtype[0]=="HardSpheres"){ 
			//	pForceFieldVector.push_back(new HardSpheres(this));
			//}  
			//else{

			// push back forceField objects onto pForceFieldVector - at present, we only have LJ forces, but this can be
			// easily expanded
			for (i = 0; i < FFtype.Count; ++i)
			{
				if (FFtype[i] == "SoftSpheres") { pForceFieldVector.Add(new SoftSpheres(this)); }
				//		if(FFtype[i]=="ExternalField"){ pForceFieldVector.push_back(new ExternalField(this));}
			}
			//}

			// test of the matrix class implementation 
			/*
			Mat_DP AA(1.0,3,3), BB(2.0,3,3), CC(0.0,3,3);
	
			NR::matadd(AA, BB, CC); // matrix addition

			for(int i=0; i<3; ++i){ // print out
				for(int j=0; j<3; ++j){
					cout << CC[i][j] << endl;
				}
			}
			*/


		}

		public void initializeOneNewParticle()
		{
			Particle pParticle;
			pParticle = new Particle();

			int type = 0;
			type = determineParticleType();
			pParticle.TypeID = type;

			ParticleInfo typeInfo = ParticleStaticObjects.AtomPropertiesDefinition.Lookup[type];

			pParticle.InitialRadius = typeInfo.Mass * 0.5;
			pParticle.Radius = typeInfo.Mass * 0.5;			
			//pParticle.PX = ParticleStaticObjects.RandomDouble(0.0 + pParticle.Radius, BoxWidth - pParticle.Radius);
			//pParticle.PY = ParticleStaticObjects.RandomDouble(0.0 + pParticle.Radius, BoxHeight - pParticle.Radius);
			pParticle.Position = new DPVector(ParticleStaticObjects.RandomDouble(0.0 + pParticle.Radius, BoxWidth - pParticle.Radius),
											  ParticleStaticObjects.RandomDouble(0.0 + pParticle.Radius, BoxHeight - pParticle.Radius)); 
			pParticle.Mass = 10; // typeInfo.Mass);
			pParticleVector.Add(pParticle);
		}

		public void setPropertiesForParticle(int newParticleIdx)
		{
			int type = 0;
			type = determineParticleType();
			pParticleVector[newParticleIdx].TypeID = type;

			ParticleInfo typeInfo = ParticleStaticObjects.AtomPropertiesDefinition.Lookup[type];

			pParticleVector[newParticleIdx].InitialRadius = typeInfo.Mass * 0.5;
			pParticleVector[newParticleIdx].Radius = typeInfo.Mass * 0.5;			
			pParticleVector[newParticleIdx].Mass = 10;
		}

		public int determineParticleType()
		{
			int type = 0;

			if (ParticleStaticObjects.AtomPropertiesDefinition.Active.Count == 0)
			{
				// if the size of the Active vector is zero
				type = ParticleStaticObjects.RandomInt(0, ParticleStaticObjects.AtomPropertiesDefinition.getCount());

				// not sure about the implementation of ofxRandom, is it inclusive? clip it anyway 
				if (type >= ParticleStaticObjects.AtomPropertiesDefinition.getCount())
				{
					type = ParticleStaticObjects.AtomPropertiesDefinition.getCount() - 1;
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

		// this function initializes a random particle with index particleIdx, giving it a position that does not overlap 
		//	with all particles whose index is less than or equal to particleIdx
		public bool InitializeRandomParticlePosition(int particleIdx, int Height, int Width)
		{

			int i, jj, iterations;
			double ijjSeparation = 0.0;
			bool ParticlesOverlap = true;

			//ofSeedRandom();   	//  Seed random number generator to clock time, so random numbers are always different    

			i = particleIdx;
			Particle particle = pParticleVector[i];

			particle.Velocity = new DPVector(ParticleStaticObjects.RandomDouble(-1.0 * InitialMinVel, InitialMaxVel), 
											 ParticleStaticObjects.RandomDouble(-1.0 * InitialMinVel, InitialMaxVel));
			//SetXParticleVelocity(i, ParticleStaticObjects.RandomDouble(-1.0 * InitialMinVel, InitialMaxVel));
			//SetYParticleVelocity(i, ParticleStaticObjects.RandomDouble(-1.0 * InitialMinVel, InitialMaxVel));
			
			particle.Position = new DPVector(ParticleStaticObjects.RandomDouble(GetParticleRadius(i), BoxWidth - GetParticleRadius(i)),
											 ParticleStaticObjects.RandomDouble(GetParticleRadius(i), BoxHeight - GetParticleRadius(i)));
			//SetXParticlePosition(i, ParticleStaticObjects.RandomDouble(GetParticleRadius(i), BoxWidth - GetParticleRadius(i)));
			//SetYParticlePosition(i, ParticleStaticObjects.RandomDouble(GetParticleRadius(i), BoxHeight - GetParticleRadius(i)));

			// what follows is for making sure that the initial particles dont overlap
			if (i != 0)
			{ // only execute if it's the first and only particle
				// the do-while loop below reselects the particle px & py until it no longer overlaps with any other particle
				iterations = 1;
				do
				{
					ParticlesOverlap = false;
					for (jj = 0; jj < i; ++jj)
					{
						Particle otherParticle = pParticleVector[jj];

						//ijjSeparation = Math.Sqrt(Math.Pow((GetXParticlePosition(i) - GetXParticlePosition(jj)), 2.0) + Math.Pow((GetYParticlePosition(i) - GetYParticlePosition(jj)), 2.0));
						
						ijjSeparation = DPVector.Seperation(particle.Position, otherParticle.Position);

						//if (ijjSeparation <= (GetParticleRadius(i) + GetParticleRadius(jj)))
						if (ijjSeparation <= (particle.Radius + otherParticle.Radius)) 
						{ 
							ParticlesOverlap = true; 
						}
					}
					
					if (ParticlesOverlap)
					{
						//SetXParticlePosition(i, ParticleStaticObjects.RandomDouble(GetParticleRadius(i), BoxWidth - GetParticleRadius(i)));
						//SetYParticlePosition(i, ParticleStaticObjects.RandomDouble(GetParticleRadius(i), BoxHeight - GetParticleRadius(i)));
						particle.Position = new DPVector(ParticleStaticObjects.RandomDouble(particle.Radius, BoxWidth - particle.Radius),
														 ParticleStaticObjects.RandomDouble(particle.Radius, BoxHeight - particle.Radius)); 
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

		/* 
		//  function to propagate the particle ensemble
		public void VelocityVerletPropagation(int, int, ExternalField) 
		{
  

		}
	*/

		// velocity verlet routine to propagate the particle ensemble
		public void VelocityVerletPropagation(int Height, int Width, ExternalField pExternalField)
		{
			int i, kk;
			double V = 0.0, T = 0.0, dt, pxnew, pynew, factor;
			DPVector pnew;
			DPVector vnew; 

			AllPositionsUnchangedFromLastStep = true;

			BoxWidth = Width;
			BoxHeight = Height;

			if (NumberOfParticlesChangedFlag)
			{
				NumberOfParticlesChangedFlag = false;
				if (NumberOfParticlesIsGreaterFlag)
				{
					for (kk = 0; kk < GetNumberOfForceFieldObjects(); ++kk)
					{				// calculate the ij energy terms for particle set including the new ones
						GetForceFieldObject(kk).UpdateEnergyTerms(this);
					}
				}
			}
			else
			{

				//  Timestep = 1.0/(double)ofGetFrameRate();
				Timestep = 0.005;

				dt = Timestep;
				++step;                                 // increment ParticleEnsemble Private data member step

				if (BerendsenThermostat)
				{                //  Berendsen Thermostat
					BerendsenVelocityRescaling();
				}

				//  T=GetKineticEnergy();
				//  V=GetPotentialEnergy();                // get the potential energy - pretty useless when the external field is time dependent - but useful for 
				//  TotalEnergy=T+V;                       // testing that new interparticle forceFields conserve energy

				//this loop uses verlet scheme (VV) to propagate the positions forward one step
				for (i = 0; i < GetNumberOfParticles(); ++i)
				{
					Particle particle = pParticleVector[i];

					//SetLastXParticlePosition(i, GetXParticlePosition(i));
					//SetLastYParticlePosition(i, GetYParticlePosition(i));

					particle.PositionLast = particle.Position; 

					//factor = 0.5 * dt * dt / GetParticleMass(i);
					factor = 0.5 * dt * dt / particle.Mass; // GetParticleMass(i);

					//pxnew = GetXParticlePosition(i) + GetXParticleVelocity(i) * dt + GetXParticleForce(i) * factor;
					//pynew = GetYParticlePosition(i) + GetYParticleVelocity(i) * dt + GetYParticleForce(i) * factor;

					//pxnew = particle.Position.X + GetXParticleVelocity(i) * dt + GetXParticleForce(i) * factor;
					//pynew = GetYParticlePosition(i) + GetYParticleVelocity(i) * dt + GetYParticleForce(i) * factor;
					pnew = particle.Position + particle.Velocity * dt + particle.Force * factor;
					vnew = particle.Velocity; 

					//		cout << "x " << pxnew << " y " << pynew << endl;
					//if (pxnew > GetParticleRadius(i) && pxnew < (BoxWidth - GetParticleRadius(i)))
					if (pnew.X <= particle.Radius || pnew.X >= (BoxWidth - particle.Radius))
					{  
						// this is to reflect off the walls; added by DRG in lieu of soft walls to improve real time stability... not part of a standard VV scheme      
						//SetXParticlePosition(i, GetLastXParticlePosition(i));
						//SetXParticleVelocity(i, -1.0 * GetXParticleVelocity(i));
						
						pnew.X = particle.PositionLast.X;
						vnew.X *= -1;

						particle.WasReflectedByWall = true; 

						//SetWasReflectedByWall(i, true);
						calculateParticleVelocitiesInXWallFrame(i);
						//			cout << "X Wall Reflection, Particle " << i << endl;
					}

					//if (pynew > GetParticleRadius(i) && pynew < (BoxHeight - GetParticleRadius(i)))
					if (pnew.Y <= particle.Radius && pnew.Y >= (BoxHeight - particle.Radius))					
					{  // this is to reflect off the walls; added by DRG in lieu of soft walls to improve real time stability... not part of a standard VV scheme
						//SetYParticlePosition(i, GetLastYParticlePosition(i));
						//SetYParticleVelocity(i, -1.0 * GetYParticleVelocity(i));

						pnew.Y = particle.PositionLast.Y;
						vnew.Y *= -1;

						particle.WasReflectedByWall = true; 
						// SetWasReflectedByWall(i, true);

						calculateParticleVelocitiesInYWallFrame(i);
						//			cout << "Y Wall Reflection, Particle " << i << endl;
					}

					particle.Position = pnew;
					particle.Velocity = vnew;

					//check whether all the positions are changed from the last step
					if (particle.Position != particle.PositionLast)
					{
						AllPositionsUnchangedFromLastStep = false;
					}
				}

				if (AllPositionsUnchangedFromLastStep)
				{    
					// this is a stability measure; if the frame is frozen wrt to the previous frame,
					//cout << "Positions unchanged" << endl;

					EliminateParticleOverlap(BoxHeight, BoxWidth);    // adjust particle positions to eliminate overlap - this can cause the sim to freeze

					for (i = 0; i < GetNumberOfParticles(); ++i)
					{           //  then we zero out the forces and velocities & repropagate the positions
						//      cout << px[i] << " " << py[i] << " " << vx[i] << " " << vy[i] << " " << fx[i] << " " << fy[i] << " " << fxLast[i] << " " << fyLast[i] << endl; 
						Particle particle = pParticleVector[i];

						particle.Force = new DPVector(0, 0);
						//SetXParticleForce(i, 0.0);
						//SetYParticleForce(i, 0.0);
						particle.PositionLast = particle.Position;
						//SetLastXParticlePosition(i, GetXParticlePosition(i));
						//SetLastYParticlePosition(i, GetYParticlePosition(i));
						particle.Position = particle.Position + particle.Velocity * dt + (particle.Force / particle.Mass) * dt * dt * 0.5;
						//SetXParticlePosition(i, GetXParticlePosition(i) + GetXParticleVelocity(i) * dt + (GetXParticleForce(i) / GetParticleMass(i)) * dt * dt * 0.5);
						//SetYParticlePosition(i, GetYParticlePosition(i) + GetYParticleVelocity(i) * dt + (GetYParticleForce(i) / GetParticleMass(i)) * dt * dt * 0.5);
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
					for (i = 0; i < GetNumberOfParticles(); ++i)
					{
						Particle particle = pParticleVector[i];

						// save the present forces to t-1 vectors
						//SetLastXParticleForce(i, GetXParticleForce(i));
						//SetLastYParticleForce(i, GetYParticleForce(i));
						particle.ForceLast = particle.Force; 
					}


					ZeroForces();			// zero out the force vectors & potential energy
					SetPotentialEnergy(0.0);

					for (kk = 0; kk < GetNumberOfForceFieldObjects(); ++kk)
					{				
						// calculate & set the forces at the new positions
						GetForceFieldObject(kk).CalculateForceField(this);
					}

					for (i = 0; i < GetNumberOfParticles(); ++i)
					{                  
						// use VV scheme to propagate the velocities forward
						Particle particle = pParticleVector[i];

						//factor = dt * 0.5 / GetParticleMass(i);
						factor = dt * 0.5 / particle.Mass;
						
						//SetXParticleVelocity(i, GetXParticleVelocity(i) + (GetXParticleForce(i) + GetLastXParticleForce(i)) * factor);
						//SetYParticleVelocity(i, GetYParticleVelocity(i) + (GetYParticleForce(i) + GetLastYParticleForce(i)) * factor);
						particle.Velocity = particle.Velocity + (particle.Force + particle.ForceLast) * factor;
					}

					// see whether any collisions occurred
					DetermineIfCollisionsOccurred();
				}
			}
		}


		// this function is for the simple Berendsen Thermostat
		public void BerendsenVelocityRescaling()
		{

			double scaleFactor;
			//int i;

			// this is an extra velocity rescaling measure to improve real-time stability... not part of Berendsen!!!!
			//    be sure that no single particle has a KE which differs from the average by 3 standard deviations (sigmas) 
			CalculateKineticEnergiesAndTemperature();
			double sigma = 2.0;
			for (int i = 0; i < GetNumberOfParticles(); ++i)
			{
				Particle particle = pParticleVector[i];

				//if ((GetParticleKineticEnergy(i) - AverageKineticEnergy) > (sigma * SDKineticEnergy))
				if ((particle.KineticEnergy - AverageKineticEnergy) > (sigma * SDKineticEnergy))
				{
					//scaleFactor = (sigma * SDKineticEnergy) / (GetParticleKineticEnergy(i) - AverageKineticEnergy);
					scaleFactor = (sigma * SDKineticEnergy) / (particle.KineticEnergy - AverageKineticEnergy);

					particle.Velocity = scaleFactor * particle.Velocity;					

					//SetXParticleVelocity(i, scaleFactor * GetXParticleVelocity(i));
					//SetYParticleVelocity(i, scaleFactor * GetYParticleVelocity(i));
				}
			}

			// again, a real-time stability measure... not part of Berendsen!!!!
			// re-initialize the system if the temperature gets crazy
			if (temperature > 1.0e8)
			{
				//cout << " T = " << temperature << " scaleFactor " << scaleFactor << endl;
				EliminateParticleOverlap(BoxHeight, BoxWidth);    // adjust particle positions to eliminate overlap - this can cause the sim to freeze
				
				for (int i = 0; i < GetNumberOfParticles(); ++i)
				{
					//  then we zero out the forces and velocities 
					Particle particle = pParticleVector[i];

					//SetXParticleForce(i, 0.0);
					//SetYParticleForce(i, 0.0);
					particle.Force = new DPVector(0, 0);

					//SetLastXParticlePosition(i, GetXParticlePosition(i));
					//SetLastYParticlePosition(i, GetYParticlePosition(i));
					particle.PositionLast = particle.Position; 
				}
				CalculateKineticEnergiesAndTemperature();
			}

			// this code here is the bona fide Berendsen thermostat !!!!
			scaleFactor = Math.Sqrt(Tequilibrium / (BerendsenCoupling * temperature));
			
			if (scaleFactor != 1.0)
			{
				for (int i = 0; i < GetNumberOfParticles(); ++i)
				{						
					//rescale the velocities
					Particle particle = pParticleVector[i];

					particle.Velocity = scaleFactor * particle.Velocity; 
					//SetXParticleVelocity(i, scaleFactor * GetXParticleVelocity(i));
					//SetYParticleVelocity(i, scaleFactor * GetYParticleVelocity(i));
				}
			}
		}

		// this function allows dynamic scaling of the particle Radii
		public void ScaleParticleRadii(double newScaleFac)
		{
			// update the particle radii
			radiiScaleFactor = newScaleFac;
			
			for (int i = 0; i < GetNumberOfParticles(); ++i)
			{
				Particle particle = pParticleVector[i];

				//SetParticleRadius(i, radiiScaleFactor * pParticleVector[i].InitialRadius);
				particle.Radius = radiiScaleFactor * particle.InitialRadius; 
			}

			// update any necessary forceField terms (e.g., LJ terms for SoftSpheres)
			if (GetForceFieldObject(0).ForceFieldType == "SoftSphereForceField")
			{
				GetForceFieldObject(0).UpdateEnergyTerms(this);
			}
		}
		/*
		 // this function allows dynamic scaling of the particle Radii
void ParticleEnsemble::ScaleParticleRadii(double newScaleFac){
  // update the particle radii
  radiiScaleFactor = newScaleFac;
  for(int i=0; i<GetNumberOfParticles(); ++i){
	SetParticleRadius(i,radiiScaleFactor*pParticleVector[i]->getInitialRadius());
  }
  // update any necessary forceField terms (e.g., LJ terms for SoftSpheres)
  if((GetForceFieldObject(0)->getForceFieldType())=="SoftSphereForceField"){
	GetForceFieldObject(0)->updateEnergyTerms(this);      
  }
};
		 */

		// add an external forcefield from the pixels on the end of the pForceFieldVector 
		public void AddAPixelField(ForceField pixels)
		{
			pForceFieldVector.Add(pixels);
		}

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

		public void SetNumberOfParticles(int newnumber)
		{
			NumberOfParticles = newnumber; // number of particles
		}

		//  The necessary Set functions
		public void SetEqTemperature(double newTemp)
		{
			Tequilibrium = newTemp;
		}

		public void SetBerendsenThermostatCoupling(double newCoupling)
		{
			BerendsenCoupling = newCoupling;
		}

		public void SetGradientScaleFactor(double newScaleFac)
		{
			gradientScaleFactor = newScaleFac;
		}

		public void AddForces(DPVector[] NewForces)
		{
			for (int i = 0; i < NumberOfParticles; ++i)
			{
				DPVector newforce = pParticleVector[i].Force + NewForces[i];
		
				pParticleVector[i].Force = newforce;
			}
		}

		/*public void ZeroXForces()
		{
			for (int i = 0; i < NumberOfParticles; ++i)
			{
				pParticleVector[i].FX = 0.0;
			}
		}*/

		//public void SetXParticlePosition(int i, double NewXposition)
		//{
		//	pParticleVector[i].PX = NewXposition;
		//}

		//public void SetLastXParticlePosition(int i, double Xposition)
		//{
		//	pParticleVector[i].PXLast = Xposition;
		//}

		//public void SetXParticleVelocity(int i, double NewXVelocity)
		//{
		//	pParticleVector[i].VX = NewXVelocity;
		//}

		//public void SetXParticleForce(int i, double NewXforce)
		//{
		//	pParticleVector[i].FX = NewXforce;
		//}

		//public void SetLastXParticleForce(int i, double Xforce)
		//{
		//	pParticleVector[i].FXLast = Xforce;
		//}

		//public void AddYForces(double[] NewYforces)
		//{
		//	for (int i = 0; i < NumberOfParticles; ++i)
		//	{
		//		double newforce = pParticleVector[i].FY + NewYforces[i];
		//		pParticleVector[i].FY = newforce;
		//	}
		//}

		public void ZeroForces()
		{
			for (int i = 0; i < NumberOfParticles; ++i)
			{
				pParticleVector[i].Force = new DPVector(0, 0);
			}
		}

		//public void SetYParticlePosition(int i, double NewYposition)
		//{
		//	pParticleVector[i].PY = NewYposition;
		//}

		//public void SetLastYParticlePosition(int i, double Yposition)
		//{
		//	pParticleVector[i].PYLast = Yposition;
		//}

		//public void SetYParticleVelocity(int i, double NewYVelocity)
		//{
		//	pParticleVector[i].VY = NewYVelocity;
		//}

		//public void SetYParticleForce(int i, double NewYforce)
		//{
		//	pParticleVector[i].FY = NewYforce;
		//}

		//public void SetLastYParticleForce(int i, double Yforce)
		//{
		//	pParticleVector[i].FYLast = Yforce;
		//}

		public void SetWasReflectedByWall(int i, bool value)
		{
			pParticleVector[i].WasReflectedByWall = value;
		}

		public void SetParticleRadius(int i, double newradius)
		{
			pParticleVector[i].Radius = newradius;
		}

		public void SetParticleMass(int i, double newmass)
		{
			pParticleVector[i].Mass = newmass;
		}

		public void SetParticleKineticEnergy(int i, double newKE)
		{
			pParticleVector[i].KineticEnergy = newKE;
		}

		public void SetPotentialEnergy(double NewPotentialEnergy)
		{
			PotentialEnergy = NewPotentialEnergy;
		}

		public void AddPotentialEnergy(double NewPotentialEnergy)
		{
			PotentialEnergy += NewPotentialEnergy;
		}

		public void SetBoxWidth(int Xmin)
		{
			BoxWidth = Xmin;
		}

		public void SetBoxHeight(int Xmax)
		{
			BoxHeight = Xmax;
		}

		public void SetMaxForceThreshold(double newMaxForceThreshold)
		{
			MaxForceThreshold = newMaxForceThreshold;
		}

		public void UpdateParticleVelocityAutoCorrelationFunctions()
		{
			for (int i = 0; i < pParticleVector.Count; ++i)
			{
				// update each particle's velocity autocorrelation function...
				pParticleVector[i].UpdateVelocityAutoCorrelationFunction();
			}
		}

		public void FFTVelocityAutoCorrelationFunction()
		{
			AverageVelocityAutoCorrelationFunction = new List<double>(new double[1024]);  // before FFT, we need to update the ensemble averaged autocorrelation function 
			double avgfac = 1.0 / pParticleVector.Count;
			for (int i = 0; i < pParticleVector.Count; ++i)
			{
				for (int jj = 0; jj < 1024; ++jj)
				{
					// for the sake of efficiency, the velocity autocorrelation functions on the particle are PUBLIC data... beware!!!
					AverageVelocityAutoCorrelationFunction[jj] += avgfac * (pParticleVector[i].VelocityAutoCorrelationFunction[0]) * (pParticleVector[i].VelocityAutoCorrelationFunction[jj]);
					//	cout << jj << " "<< AverageVelocityAutoCorrelationFunction[jj] << endl;
				}
			}

			/*
			cout << "correlation function" << endl;
			for(int i=0;i<AverageVelocityAutoCorrelationFunction.size();++i){
			if(AverageVelocityAutoCorrelationFunction[i] != 0.0){
			cout << i << " " << AverageVelocityAutoCorrelationFunction[i] << endl;
			}
			}
			*/

			realft(AverageVelocityAutoCorrelationFunction, 1);
			/*
			cout << "FFT" << endl;	
			for(int i=0;i<AverageVelocityAutoCorrelationFunction.size();++i){
			cout << i << " " << AverageVelocityAutoCorrelationFunction[i] << endl;
			}
			*/
		}

		void realft(List<double> data, int isign)
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


		//#define SWAP(a,b) tempr=(a);(a)=(b);(b)=tempr
		void SwapIndexValues(List<double> data, int a, int b)
		{
			double temp = data[a];
			data[a] = data[b];
			data[b] = temp;
		}

		void four1(List<double> data, int isign)
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

		//  All the Get functions 
		public double GetEqTemperature()
		{
			return Tequilibrium;
		}

		public double GetBerendsenThermostatCoupling()
		{
			return BerendsenCoupling;
		}

		public double GetGradientScaleFactor()
		{
			return gradientScaleFactor;
		}

		public double GetRadiiScaleFactor()
		{
			return radiiScaleFactor;
		}

		public void CalculateKineticEnergiesAndTemperature()
		{
			// calculate the total, avg, & standard deviation of the kinetic energy

			double sum = 0.0;
			TotalKineticEnergy = 0.0;
			AverageKineticEnergy = 0.0;
			SDKineticEnergy = 0.0;
			double KE = 0.0;

			for (int i = 0; i < GetNumberOfParticles(); ++i)
			{
				Particle particle = pParticleVector[i];

				KE = 0.5 * particle.Mass * (Math.Pow(particle.Velocity.X, 2.0) + Math.Pow(particle.Velocity.Y, 2.0));

				particle.KineticEnergy = KE;

				TotalKineticEnergy += KE;
			}
			
			AverageKineticEnergy = TotalKineticEnergy / NumberOfParticles;
			temperature = TotalKineticEnergy / (NumberOfParticles * kb);
			
			for (int i = 0; i < GetNumberOfParticles(); ++i) 
			{ 
				sum += Math.Pow((pParticleVector[i].KineticEnergy - AverageKineticEnergy), 2.0); 
			}

			SDKineticEnergy = Math.Sqrt(sum / NumberOfParticles);

		}

		//public double GetXParticlePosition(int i)
		//{
		//	return pParticleVector[i].PX;
		//}

		//public double GetYParticlePosition(int i)
		//{
		//	return pParticleVector[i].PY;
		//}

		//public double GetLastXParticlePosition(int i)
		//{
		//	return pParticleVector[i].PXLast;
		//}

		//public double GetLastYParticlePosition(int i)
		//{
		//	return pParticleVector[i].PYLast;
		//}

		//public double GetXParticleVelocity(int i)
		//{
		//	return pParticleVector[i].VX;
		//}

		//public double GetYParticleVelocity(int i)
		//{
			//return pParticleVector[i].VY;
		//}

		//public double GetXParticleForce(int i)
		//{
		//	return pParticleVector[i].FX;
		//}

		//public double GetYParticleForce(int i)
		//{
		//	return pParticleVector[i].FY;
		//}

		//public double GetLastXParticleForce(int i)
		//{
		//	return pParticleVector[i].FXLast;
		//}

		//public double GetLastYParticleForce(int i)
		//{
		//	return pParticleVector[i].FYLast;
		//}

		public double GetParticleMass(int i)
		{
			return pParticleVector[i].Mass;
		}

		public double GetParticleRadius(int i)
		{
			return pParticleVector[i].Radius;
		}

		public double GetParticleKineticEnergy(int i)
		{
			return pParticleVector[i].KineticEnergy;
		}

		public double GetTimestep()
		{
			// timestep
			return Timestep;
		}

		public double GetPotentialEnergy()
		{
			// Potential Energy
			return PotentialEnergy;
		}

		public int GetNumberOfForceFieldObjects()
		{
			return pForceFieldVector.Count;
		}

		public double GetMaxForceThreshold()
		{
			return MaxForceThreshold;
		}

		public double GetinitialMaxForceThreshold()
		{
			return initialMaxForceThreshold;
		}

		public ForceField GetForceFieldObject(int i)
		{
			return pForceFieldVector[i];
		}

		public Particle GetParticle(int i)
		{
			return pParticleVector[i];
		}

		public int GetNumberOfParticles()
		{
			// number of particles
			return NumberOfParticles;
		}

		public int GetMaxNumberOfParticles()
		{
			// number of particles
			return maxNumberOfParticles;
		}

		public int GetBoxWidth()
		{
			return BoxWidth;
		}

		public int GetBoxHeight()
		{
			return BoxHeight;
		}

		//  int GetGrabberCalls(){return GrabberCalls;}

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

		// this stuff is for the FFT - move this stuff to a ParticleMath class
		//void four1(std::vector<double> &data, const int isign);
		//void realft(std::vector<double> &data, const int isign);

		// update the interparticle distanceMatrix
		public void UpdateInterParticleSeparations()
		{
			double ijSeparation = 0;

			DPMatrix temp = distanceMatrixLastLastTime; 
			distanceMatrixLastLastTime = distanceMatrixLastTime;
			distanceMatrixLastTime = distanceMatrix;
			distanceMatrix = temp; 
			
			for (int i = 0; i < NumberOfParticles; ++i)
			{
				for (int j = (i + 1); j < NumberOfParticles; ++j)
				{
					//distanceMatrixLastLastTime[i, j] = distanceMatrixLastTime[i, j];
					//distanceMatrixLastLastTime[j, i] = distanceMatrixLastTime[j, i];
					//distanceMatrixLastTime[i, j] = distanceMatrix[i, j];   // move the present distances to the last
					//distanceMatrixLastTime[j, i] = distanceMatrix[j, i];
					//ijSeparation = Math.Sqrt(Math.Pow(pParticleVector[i].getpx() - pParticleVector[j].getpx(), 2.0) + Math.Pow(pParticleVector[i].getpy() - pParticleVector[j].getpy(), 2.0));
					//ijSeparation =  Math.Pow(pParticleVector[i].PX - pParticleVector[j].PX, 2.0) + Math.Pow(pParticleVector[i].PY - pParticleVector[j].PY, 2.0);
					ijSeparation = DPVector.SeperationSquared(pParticleVector[i].Position, pParticleVector[j].Position);
					distanceMatrix[i, j] = ijSeparation;
					distanceMatrix[j, i] = ijSeparation;                   // assign the present distances
				}
			}
		}

		// function to determine if a collision happened
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
							pParticleVector[i].CollisionOccurred = true;
							pParticleVector[j].CollisionOccurred = true;
							calculateParticleVelocitiesInComFrame(i, j);
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
			for (int i = 0; i < NumberOfParticles; ++i)
			{
				pParticleVector[i].MarkParticleDidNotCollide();
			}
		}

		public double GetInterParticleSeparation(int i, int j)
		{
			return distanceMatrix[i, j];
		}

		// function to calculate the velocity of particles i & j in their center of mass frame
		public void calculateParticleVelocitiesInComFrame(int i, int j)
		{

		}

		// function to calculate the velocity of particle i in the wall frame	
		public void calculateParticleVelocitiesInXWallFrame(int i)
		{

		}

		public void calculateParticleVelocitiesInYWallFrame(int i)
		{

		}




		/* 
		internal int GetBoxHeight()
		{
			throw new NotImplementedException();
		}

		internal int GetBoxWidth()
		{
			throw new NotImplementedException();
		}

		internal int GetNumberOfParticles()
		{
			throw new NotImplementedException();
		}

		internal double GetXParticlePosition(int i)
		{
			throw new NotImplementedException();
		}

		internal double GetYParticlePosition(int i)
		{
			throw new NotImplementedException();
		}

		internal double GetParticleRadius(int i)
		{
			throw new NotImplementedException();
		}

		internal double GetGradientScaleFactor()
		{
			throw new NotImplementedException();
		}

		internal void AddXForces(List<double> xforces)
		{
			throw new NotImplementedException();
		}

		internal void AddYForces(List<double> yforces)
		{
			throw new NotImplementedException();
		}

		internal void AddPotentialEnergy(double PotentialEnergy)
		{
			throw new NotImplementedException();
		}
		 * */
	}
}

/* 

#define MAX_PARTICLE_COLLISIONS 50

class ParticleEnsemble {
  
  public :
  
  //  constructor
  ParticleEnsemble(int, double, double, vector <string>,int, int, double);
  
  //  destructor - delete the dynamically allocated pForceFieldVector
  ~ParticleEnsemble();
	
	void initializeOneNewParticle();

	void setPropertiesForParticle(int);
	
	int determineParticleType();
	
  //  function to adjust particle positions to eliminate Overlap
  bool EliminateParticleOverlap(int, int);
  
	bool InitializeRandomParticlePosition(int, int, int);
	
  //  function to propagate the particle ensemble
  void VelocityVerletPropagation(int, int, ExternalField*);
  
  // this function is for the simple Berendsen Thermostat
  void BerendsenVelocityRescaling();   
  
  // this function allows dynamic scaling of the particle Radii
  void ScaleParticleRadii(double);
	
	// add an external forcefield from the pixels on the end of the pForceFieldVector 
	void AddAPixelField(ForceField* pixels){		
		pForceFieldVector.push_back(pixels);
	}
	 	  
	void SetEnsembleReinitializationFlag(){EnsembleReinitializationFlag = true;}
	void SetNumberOfParticlesChangedFlag(){NumberOfParticlesChangedFlag = true;}
	void SetNumberOfParticlesIsGreater(){NumberOfParticlesIsGreaterFlag = true;}
		
  void SetNumberOfParticles(int newnumber){NumberOfParticles=newnumber;}		// number of particles
	
  //  The necessary Set functions
  void SetEqTemperature(double newTemp){Tequilibrium=newTemp;}
  void SetBerendsenThermostatCoupling(double newCoupling){BerendsenCoupling=newCoupling;}
  
  void SetGradientScaleFactor(double newScaleFac){gradientScaleFactor=newScaleFac;}
  
  void AddXForces(const vector <double> &NewXforces){
    for(int i=0;i<NumberOfParticles;++i){
      double newforce = pParticleVector[i]->getfx() + NewXforces[i];
      pParticleVector[i]->setfx(newforce);
    }
  }
	
  void ZeroXForces(){for(int i=0;i<NumberOfParticles;++i){pParticleVector[i]->setfx(0.0);}}
  
  void SetXParticlePosition(int i, double NewXposition){pParticleVector[i]->setpx(NewXposition);}
  void SetLastXParticlePosition(int i, double Xposition){pParticleVector[i]->setpxLast(Xposition);}
  void SetXParticleVelocity(int i, double NewXVelocity){pParticleVector[i]->setvx(NewXVelocity);}
  void SetXParticleForce(int i, double NewXforce){pParticleVector[i]->setfx(NewXforce);}
  void SetLastXParticleForce(int i, double Xforce){pParticleVector[i]->setfxLast(Xforce);}
  
  void AddYForces(const vector <double> &NewYforces){
    for(int i=0;i<NumberOfParticles;++i){
      double newforce = pParticleVector[i]->getfy() + NewYforces[i];
      pParticleVector[i]->setfy(newforce);
    }
  }
  void ZeroYForces(){
    for(int i=0;i<NumberOfParticles;++i){pParticleVector[i]->setfy(0.0);}
  }
  
  void SetYParticlePosition(int i, double NewYposition){pParticleVector[i]->setpy(NewYposition);}
  void SetLastYParticlePosition(int i, double Yposition){pParticleVector[i]->setpyLast(Yposition);}
  void SetYParticleVelocity(int i, double NewYVelocity){pParticleVector[i]->setvy(NewYVelocity);}
  void SetYParticleForce(int i, double NewYforce){pParticleVector[i]->setfy(NewYforce);}  
  void SetLastYParticleForce(int i, double Yforce){pParticleVector[i]->setfyLast(Yforce);}  
	void SetWasReflectedByWall(int i, bool value){pParticleVector[i]->setWasReflectedByWall(value);}

  void SetParticleRadius(int i, double newradius){pParticleVector[i]->setRadius(newradius);}
  void SetParticleMass(int i, double newmass){pParticleVector[i]->setMass(newmass);}
  
  void SetParticleKineticEnergy(int i, double newKE){pParticleVector[i]->setKineticEnergy(newKE);}
  
  void SetPotentialEnergy(double NewPotentialEnergy){PotentialEnergy = NewPotentialEnergy;}
  void AddPotentialEnergy(double NewPotentialEnergy){PotentialEnergy += NewPotentialEnergy;}
  
  void SetBoxWidth(int Xmin){BoxWidth=Xmin;}
  void SetBoxHeight(int Xmax){BoxHeight=Xmax;}
    
  void SetMaxForceThreshold(double newMaxForceThreshold){MaxForceThreshold = newMaxForceThreshold;}
	void UpdateParticleVelocityAutoCorrelationFunctions(){
		for(int i=0; i<pParticleVector.size(); ++i){			// update each particle's velocity autocorrelation function...
			pParticleVector[i]->UpdateVelocityAutoCorrelationFunction();
		}
	}
	
	void FFTVelocityAutoCorrelationFunction();
	
  //  All the Get functions 
  double GetEqTemperature(){return Tequilibrium;}
  double GetBerendsenThermostatCoupling(){return BerendsenCoupling;}
  
  double GetGradientScaleFactor(){return gradientScaleFactor;}
  double GetRadiiScaleFactor(){return radiiScaleFactor;}
  
  void CalculateKineticEnergiesAndTemperature();
	
  double GetXParticlePosition(int i){return pParticleVector[i]->getpx();}
  double GetYParticlePosition(int i){return pParticleVector[i]->getpy();}
  
  double GetLastXParticlePosition(int i){return pParticleVector[i]->getpxLast();}
  double GetLastYParticlePosition(int i){return pParticleVector[i]->getpyLast();}
  
  double GetXParticleVelocity(int i){return pParticleVector[i]->getvx();}
  double GetYParticleVelocity(int i){return pParticleVector[i]->getvy();}
  
  double GetXParticleForce(int i){return pParticleVector[i]->getfx();}
  double GetYParticleForce(int i){return pParticleVector[i]->getfy();}
	
  double GetLastXParticleForce(int i){return pParticleVector[i]->getfxLast();}
  double GetLastYParticleForce(int i){return pParticleVector[i]->getfyLast();}
  
  double GetParticleMass(int i){return pParticleVector[i]->getMass();}
  
  double GetParticleRadius(int i){return pParticleVector[i]->getRadius();}
  
  double GetParticleKineticEnergy(int i){return pParticleVector[i]->getKineticEnergy();}
  
  double GetTimestep(){return Timestep;}						// timestep
  
  double GetPotentialEnergy(){return PotentialEnergy;}        // Potential Energy
  
  int GetNumberOfForceFieldObjects(){return pForceFieldVector.size();}
  
  double GetMaxForceThreshold(){return MaxForceThreshold;}
  
  double GetinitialMaxForceThreshold(){return initialMaxForceThreshold;}
  
  ForceField *GetForceFieldObject(int i){return pForceFieldVector[i];}
	
	Particle *GetParticle(int i){return pParticleVector[i];}
  
  int GetNumberOfParticles(){return NumberOfParticles;}		// number of particles
  int GetMaxNumberOfParticles(){return maxNumberOfParticles;}		// number of particles
  
  int GetBoxWidth(){return BoxWidth;}
  int GetBoxHeight(){return BoxHeight;}
  
//  int GetGrabberCalls(){return GrabberCalls;}
  
  void SetParticlesWithinRange(int i,int j){
		particlesWithinRange[i][j]=true;
		particlesWithinRange[j][i]=true;
	}

	void SetParticlesNotWithinRange(int i,int j){
		particlesWithinRange[i][j]=false;
		particlesWithinRange[j][i]=false;
	}
	
	// this stuff is for the FFT - move this stuff to a ParticleMath class
	void four1(std::vector<double> &data, const int isign);
	void realft(std::vector<double> &data, const int isign);
	
	// update the interparticle distanceMatrix
	void UpdateInterParticleSeparations(){
		double ijSeparation;
		for(int i=0; i<NumberOfParticles; ++i){
			for(int j=(i+1); j<NumberOfParticles; ++j){
				distanceMatrixLastLastTime[i][j]=distanceMatrixLastTime[i][j];
				distanceMatrixLastLastTime[j][i]=distanceMatrixLastTime[j][i];
				distanceMatrixLastTime[i][j]=distanceMatrix[i][j];   // move the present distances to the last
				distanceMatrixLastTime[j][i]=distanceMatrix[j][i];
				ijSeparation = sqrt(pow(pParticleVector[i]->getpx() - pParticleVector[j]->getpx(),2.0) + pow(pParticleVector[i]->getpy() - pParticleVector[j]->getpy(),2.0));
				distanceMatrix[i][j]=ijSeparation;
				distanceMatrix[j][i]=ijSeparation;                   // assign the present distances
			}
		}
	}
	
	// function to determine if a collision happened
	void DetermineIfCollisionsOccurred(){
		for(int i=0; i<NumberOfParticles; ++i){
			for(int j=(i+1); j<NumberOfParticles; ++j){
				if(particlesWithinRange[i][j]==true){
					if(distanceMatrixLastTime[i][j] < distanceMatrix[i][j] && distanceMatrixLastTime[i][j] < distanceMatrixLastLastTime[i][j] ){
						pParticleVector[i]->setCollisionOccurred();
						pParticleVector[j]->setCollisionOccurred();
						calculateParticleVelocitiesInComFrame(i,j);
						// debug code
//						cout << "particle " << i << " - " << j << " distanceMatrixLastLastTime " << distanceMatrixLastLastTime[i][j] 
//								 << " distanceMatrixLastTime " << distanceMatrixLastTime[i][j] << " distanceMatrix " << distanceMatrix[i][j] <<  endl;
					}
				}
			}
		}					
	}
	
	void ResetParticleCollisions(){
		for(int i=0;i<NumberOfParticles;++i){
      pParticleVector[i]->setParticleDidNotCollide();
    }
	} 	
		
	double GetInterParticleSeparation(int i, int j){return distanceMatrix[i][j];}
	
	// function to calculate the velocity of particles i & j in their center of mass frame
	void calculateParticleVelocitiesInComFrame(int i, int j);

	// function to calculate the velocity of particle i in the wall frame	
	void calculateParticleVelocitiesInXWallFrame(int i);
	void calculateParticleVelocitiesInYWallFrame(int i);
	
	// these need moving to private, with appropriate set & get functions!!!!
  int CollisionsCount;   

  vector< Particle * > pParticleVector;      // pointer to a vector of particles
	
private:
  
  int			NumberOfParticles;
	int     maxNumberOfParticles;
	int			step;
	int     NeighborListSize;
  
  double   InitialMinVel, InitialMaxVel;
  
  //  box boundaries
  int			BoxWidth;
  int			BoxHeight;
  int     maxPixelIndex;
  
  //  stuff for BerendsenThermostatting...
  double  kb, BerendsenCoupling, Tequilibrium;
  double  temperature, scaleFactor, InitialKE;
  bool    BerendsenThermostat;
  
  bool    ThereIsMidiOutput;
	bool    EnsembleReinitializationFlag;
	bool		NumberOfParticlesChangedFlag;
	bool		NumberOfParticlesIsGreaterFlag;
	
  //  stuff for stability
  bool  AllPositionsUnchangedFromLastStep;
  double MaxForceThreshold, initialMaxForceThreshold;
  
  // stuff for kinetic energies
  double TotalKineticEnergy, AverageKineticEnergy, SDKineticEnergy;
  
  // the gradient scale factor...
  double  gradientScaleFactor;
  
  // the radius scale factor...
  double  radiiScaleFactor;
	
  double  Timestep;
  double  PotentialEnergy, TotalEnergy;
	
  vector< ForceField * > pForceFieldVector;		// pointer to a vector of ForceField objects
  	
	vector< double > AverageVelocityAutoCorrelationFunction;  
	  
  vector< bool > InnerTurningPointCrossed;
	
	Mat_DP distanceMatrix;							// matrix with the interparticle separations of particle i-j at time t
	Mat_DP distanceMatrixLastTime;			// matrix with the interparticle separations of particle i-j at time t-1
	Mat_DP distanceMatrixLastLastTime;	// matrix with the interparticle separations of particle i-j at time t-2
	
	Mat_BOOL particlesWithinRange;			// matrix of booleans that tell us if particles i-j are within range of one another
	
}; // end of class ParticleEnsemble

#endif
*/ 

/* 

// constructor
ParticleEnsemble::ParticleEnsemble(int nparticles, double MinRad, double MaxRad, vector <string> FFtype,
																	 int Height, int Width, double scaleFactor)
{
  int i;
	maxNumberOfParticles=1000;	// set the maximum number of particles to 1000
	
  kb = 8.314;									// value of the boltzmann constant - we dont really care about units now...
  BerendsenCoupling = 1.0;		// value of the berendsen coupling constant - set this to be controlled by the artist...
  Tequilibrium = 20000;				
  BerendsenThermostat=true;
  //  BerendsenThermostat=false;
	EnsembleReinitializationFlag=false;
	NumberOfParticlesChangedFlag=false;
	NumberOfParticlesIsGreaterFlag=false;
  
  radiiScaleFactor = 1.0;
  InitialKE = 0.0;
	gradientScaleFactor = scaleFactor;
  
	step = 0;
	NumberOfParticles = nparticles;
	BoxHeight = Height;
	BoxWidth = Width;
    
  MaxForceThreshold = 1.0e6;
  initialMaxForceThreshold = MaxForceThreshold;
		
	AverageVelocityAutoCorrelationFunction.assign(1024,0.0);
	  
  InitialMinVel = 100.0;
  InitialMaxVel = 200.0;
  	
	for(int ii=0;ii<maxNumberOfParticles; ++ii){
		initializeOneNewParticle();
	}
	
	if(!EliminateParticleOverlap(Height,Width)){			// adjust particle positions to eliminate overlap 
		do{
			cout << "Can't fit " << NumberOfParticles << " particles into the simulation box " << endl;
			NumberOfParticles -= 1;                       // if there's too many particles to fit in the simulation box
			cout << "Decrementing the number of particles to " << NumberOfParticles << endl;
			EliminateParticleOverlap(Height,Width);				// decrement the particles till it's ok
		}
		while(!EliminateParticleOverlap(Height,Width));
	}
	
	Mat_DP tmp1(0.0,maxNumberOfParticles,maxNumberOfParticles);			// assign tmp1 matrix to distanceMatrix & distanceMatrixLastTime
	distanceMatrix=tmp1;
	distanceMatrixLastTime=tmp1;
	distanceMatrixLastLastTime=tmp1;
	
	Mat_BOOL tmp2(false,maxNumberOfParticles,maxNumberOfParticles); // update the particlesWithinRange matrix
	particlesWithinRange=tmp2;
	
  UpdateInterParticleSeparations();						// update the interparticle separation matrix
	
  PotentialEnergy=0.0;
  
  //initialize forcefield Object
  //	cout << FFtype[0] << endl;
	//if(FFtype[0]=="HardSpheres"){ 
	//	pForceFieldVector.push_back(new HardSpheres(this));
	//}  
	//else{
	
	// push back forceField objects onto pForceFieldVector - at present, we only have LJ forces, but this can be
	// easily expanded
	for(i=0;i<FFtype.size();++i){
		if(FFtype[i]=="SoftSpheres"){ pForceFieldVector.push_back(new SoftSpheres(this));}
//		if(FFtype[i]=="ExternalField"){ pForceFieldVector.push_back(new ExternalField(this));}
	}
	//}
	
  // test of the matrix class implementation 
	/*
 	Mat_DP AA(1.0,3,3), BB(2.0,3,3), CC(0.0,3,3);
	
	NR::matadd(AA, BB, CC); // matrix addition

	for(int i=0; i<3; ++i){ // print out
		for(int j=0; j<3; ++j){
			cout << CC[i][j] << endl;
		}
	}
	* /
  
}

// particle ensemble destructor
ParticleEnsemble::~ParticleEnsemble(){
	
	for(vector< ForceField * >::iterator iter=pForceFieldVector.begin();iter!=pForceFieldVector.end();++iter){
		delete *iter;
	}
	pForceFieldVector.clear();
	
	//    for(int i=0;i<=pMidiOutputVector.size();++i){pMidiOutputVector.clear(); }
	
	for(vector< Particle * >::iterator iter=pParticleVector.begin();iter!=pParticleVector.end();++iter){
		delete *iter;
	}
	pParticleVector.clear();

}

void ParticleEnsemble::initializeOneNewParticle(){
	
	Particle *pParticle;		
	pParticle = new Particle;
	
	int type = 0; 			
	type = determineParticleType(); 
	pParticle->TypeID = type; 
	
	ParticleInfo typeInfo = AtomPropertiesDefinition.Lookup[type];
	
	pParticle->setRadius(typeInfo.Mass * 0.5);
	pParticle->setInitialRadius(typeInfo.Mass * 0.5);
	pParticle->setpx(ofRandom(0.0 + pParticle->getRadius(),BoxWidth - pParticle->getRadius()));
	pParticle->setpy(ofRandom(0.0 + pParticle->getRadius(),BoxHeight- pParticle->getRadius()));
	pParticle->setMass(10); // typeInfo.Mass);
	pParticleVector.push_back(pParticle);	
}

void ParticleEnsemble::setPropertiesForParticle(int newParticleIdx){

	int type = 0; 			
	type = determineParticleType(); 
	pParticleVector[newParticleIdx]->TypeID = type; 
	
	ParticleInfo typeInfo = AtomPropertiesDefinition.Lookup[type];
	
	pParticleVector[newParticleIdx]->setRadius(typeInfo.Mass * 0.5);
	pParticleVector[newParticleIdx]->setInitialRadius(typeInfo.Mass * 0.5);
	pParticleVector[newParticleIdx]->setMass(10); 

}

int ParticleEnsemble::determineParticleType(){
	
  int type = 0; 			
	
	if (AtomPropertiesDefinition.Active.size() == 0) {   // if the size of the Active vector is zero
		type = (int)ofRandom(0, AtomPropertiesDefinition.getCount()); 
		// not sure about the implementation of ofxRandom, is it inclusive? clip it anyway 
		if (type >= AtomPropertiesDefinition.getCount()){ type = AtomPropertiesDefinition.getCount() - 1;}
	}
	else { // if the size of the active vector isn't zero
		int activeType = (int)ofRandom(0, AtomPropertiesDefinition.Active.size());
		// not sure about the implementation of ofxRandom, is it inclusive? clip it anyway 
		if (activeType >= AtomPropertiesDefinition.Active.size()) { activeType = AtomPropertiesDefinition.Active.size() - 1;}
		type = AtomPropertiesDefinition.Active[activeType]; 
	}
	return type;
}

// function to initialize random velocities and non-overlapping positions for all the particles
bool ParticleEnsemble::EliminateParticleOverlap(int Height,int Width){
	
	int i, jj, iterations;
	bool everythingOK = true;

	for(i=0;i<NumberOfParticles;++i){
    if(InitializeRandomParticlePosition(i,Height,Width)){everythingOK = true;}
		else {everythingOK = false;}
	}
	
	if(!everythingOK){
		return false;
	}
	else {
		CalculateKineticEnergiesAndTemperature();
		InitialKE = TotalKineticEnergy;
		return true;
	}	
}

// this function initializes a random particle with index particleIdx, giving it a position that does not overlap 
//	with all particles whose index is less than or equal to particleIdx

bool ParticleEnsemble::InitializeRandomParticlePosition(int particleIdx, int Height, int Width){
	
	int i, jj, iterations;
	double ijjSeparation(0.0);
	bool ParticlesOverlap(true);
  
	ofSeedRandom();   	//  Seed random number generator to clock time, so random numbers are always different    
  	
	i=particleIdx;
	
	SetXParticleVelocity(i,ofRandom(-1.0*InitialMinVel,InitialMaxVel));
	SetYParticleVelocity(i,ofRandom(-1.0*InitialMinVel,InitialMaxVel));
	SetXParticlePosition(i,ofRandom(GetParticleRadius(i),BoxWidth-GetParticleRadius(i)));
	SetYParticlePosition(i,ofRandom(GetParticleRadius(i),BoxHeight-GetParticleRadius(i)));
	// what follows is for making sure that the initial particles dont overlap
	if(i!=0){ // only execute if it's the first and only particle
		// the do-while loop below reselects the particle px & py until it no longer overlaps with any other particle
		iterations=1;
		do{
			ParticlesOverlap=false;
			for(jj=0;jj<i;++jj){
				ijjSeparation=sqrt(pow((GetXParticlePosition(i)-GetXParticlePosition(jj)),2.0)+pow((GetYParticlePosition(i)-GetYParticlePosition(jj)),2.0));
				if(ijjSeparation<=(GetParticleRadius(i)+GetParticleRadius(jj))) {ParticlesOverlap=true;}
			}
			if(ParticlesOverlap){
				SetXParticlePosition(i,ofRandom(GetParticleRadius(i),BoxWidth-GetParticleRadius(i)));
				SetYParticlePosition(i,ofRandom(GetParticleRadius(i),BoxHeight-GetParticleRadius(i)));
			}
			++iterations;
			if(iterations > 10000){
				return false;
			}
		}	while(ParticlesOverlap);
	}
	return true;
}

// this function allows dynamic scaling of the particle Radii
void ParticleEnsemble::ScaleParticleRadii(double newScaleFac){
  // update the particle radii
  radiiScaleFactor = newScaleFac;
  for(int i=0; i<GetNumberOfParticles(); ++i){
    SetParticleRadius(i,radiiScaleFactor*pParticleVector[i]->getInitialRadius());
  }
  // update any necessary forceField terms (e.g., LJ terms for SoftSpheres)
  if((GetForceFieldObject(0)->getForceFieldType())=="SoftSphereForceField"){
    GetForceFieldObject(0)->updateEnergyTerms(this);      
  }
};

// velocity verlet routine to propagate the particle ensemble
void ParticleEnsemble::VelocityVerletPropagation(int Height, int Width, ExternalField* pExternalField)
{
	int i, kk;
 	double V(0.0), T(0.0), dt, pxnew, pynew, factor;
  
  AllPositionsUnchangedFromLastStep = true;
  
  BoxWidth=Width;
  BoxHeight=Height;
		
	if(NumberOfParticlesChangedFlag){
		NumberOfParticlesChangedFlag = false;
		if(NumberOfParticlesIsGreaterFlag){
			for(kk=0;kk<GetNumberOfForceFieldObjects();++kk){				// calculate the ij energy terms for particle set including the new ones
				GetForceFieldObject(kk)->updateEnergyTerms(this);		
			}
		}
	}
	
	else{
		
		//  Timestep = 1.0/(double)ofGetFrameRate();
		Timestep = 0.005;
		
		dt = Timestep;
		++step;                                 // increment ParticleEnsemble Private data member step
		
		if(BerendsenThermostat){                //  Berendsen Thermostat
			BerendsenVelocityRescaling();
		}
		
		//  T=GetKineticEnergy();
		//  V=GetPotentialEnergy();                // get the potential energy - pretty useless when the external field is time dependent - but useful for 
		//  TotalEnergy=T+V;                       // testing that new interparticle forceFields conserve energy
		
		//this loop uses verlet scheme (VV) to propagate the positions forward one step
		for(i=0;i<GetNumberOfParticles();++i){			
			SetLastXParticlePosition(i,GetXParticlePosition(i));
			SetLastYParticlePosition(i,GetYParticlePosition(i));
			factor=0.5*dt*dt/GetParticleMass(i);
			pxnew = GetXParticlePosition(i) + GetXParticleVelocity(i)*dt + GetXParticleForce(i)*factor;
			pynew = GetYParticlePosition(i) + GetYParticleVelocity(i)*dt + GetYParticleForce(i)*factor;
			//		cout << "x " << pxnew << " y " << pynew << endl;
			if(pxnew > GetParticleRadius(i) && pxnew < (BoxWidth - GetParticleRadius(i))){  
				SetXParticlePosition(i,pxnew);                              // this the standard VV code here
				//      AllPositionsUnchangedFromLastStep = false;  // this is a stability measure, to detect if the simulation has frozen from frame to frame
			}   
			else{  // this is to reflect off the walls; added by DRG in lieu of soft walls to improve real time stability... not part of a standard VV scheme      
				SetXParticlePosition(i,GetLastXParticlePosition(i));
				SetXParticleVelocity(i,-1.0*GetXParticleVelocity(i));     
				SetWasReflectedByWall(i, true); 
				calculateParticleVelocitiesInXWallFrame(i);
				//			cout << "X Wall Reflection, Particle " << i << endl;
			}
			if(pynew > GetParticleRadius(i) && pynew < (BoxHeight - GetParticleRadius(i))){     // this the standard VV code here
				SetYParticlePosition(i,pynew);
			}   
			else{  // this is to reflect off the walls; added by DRG in lieu of soft walls to improve real time stability... not part of a standard VV scheme
				SetYParticlePosition(i,GetLastYParticlePosition(i));
				SetYParticleVelocity(i,-1.0*GetYParticleVelocity(i));
				SetWasReflectedByWall(i, true); 
				calculateParticleVelocitiesInYWallFrame(i);
				//			cout << "Y Wall Reflection, Particle " << i << endl;
			}
		}
		
		//check whether all the positions are changed from the last step
		for(i=0;i<GetNumberOfParticles();++i){
			if(GetYParticlePosition(i)!=GetLastYParticlePosition(i) || GetXParticlePosition(i)!=GetLastXParticlePosition(i)){
				AllPositionsUnchangedFromLastStep=false;
			}
		}
		
		
		if(AllPositionsUnchangedFromLastStep){    // this is a stability measure; if the frame is frozen wrt to the previous frame,
			cout << "Positions unchanged" << endl;
			
			EliminateParticleOverlap(BoxHeight,BoxWidth);    // adjust particle positions to eliminate overlap - this can cause the sim to freeze
			
			for(i=0;i<GetNumberOfParticles();++i){           //  then we zero out the forces and velocities & repropagate the positions
				//      cout << px[i] << " " << py[i] << " " << vx[i] << " " << vy[i] << " " << fx[i] << " " << fy[i] << " " << fxLast[i] << " " << fyLast[i] << endl; 
				SetXParticleForce(i,0.0);
				SetYParticleForce(i,0.0);
				SetLastXParticlePosition(i,GetXParticlePosition(i));
				SetLastYParticlePosition(i,GetYParticlePosition(i));
				SetXParticlePosition(i,GetXParticlePosition(i) + GetXParticleVelocity(i)*dt + (GetXParticleForce(i)/GetParticleMass(i))*dt*dt*0.5);
				SetYParticlePosition(i,GetYParticlePosition(i) + GetYParticleVelocity(i)*dt + (GetYParticleForce(i)/GetParticleMass(i))*dt*dt*0.5);
			}
			
			AllPositionsUnchangedFromLastStep = false;
		}
		
		UpdateInterParticleSeparations();
		
		if(pExternalField != NULL){
			pExternalField->calculateForceField(this);
		}
		
		if((GetForceFieldObject(0)->getForceFieldType())=="HardSphereForceField"){
			GetForceFieldObject(0)->calculateForceField(this);
		}  
		else{	
			
			for(i=0;i<GetNumberOfParticles();++i){	// save the present forces to t-1 vectors
				SetLastXParticleForce(i,GetXParticleForce(i));
				SetLastYParticleForce(i,GetYParticleForce(i));
			}
			
			ZeroXForces();			// zero out the force vectors & potential energy
			ZeroYForces();
			SetPotentialEnergy(0.0);
			
			for(kk=0;kk<GetNumberOfForceFieldObjects();++kk){				// calculate & set the forces at the new positions
				GetForceFieldObject(kk)->calculateForceField(this);		
			}
			
			for(i=0;i<GetNumberOfParticles();++i){                  // use VV scheme to propagate the velocities forward
				factor = dt*0.5/GetParticleMass(i);
				SetXParticleVelocity(i,GetXParticleVelocity(i) + (GetXParticleForce(i) + GetLastXParticleForce(i))*factor);
				SetYParticleVelocity(i,GetYParticleVelocity(i) + (GetYParticleForce(i) + GetLastYParticleForce(i))*factor);
			}
			
			// see whether any collisions occurred
			DetermineIfCollisionsOccurred();
			
		}
	}
  
}


void ParticleEnsemble::CalculateKineticEnergiesAndTemperature(){   // calculate the total, avg, & standard deviation of the kinetic energy
	
	double sum(0.0);
	TotalKineticEnergy = 0.0;
	AverageKineticEnergy = 0.0;
	SDKineticEnergy = 0.0;
	double KE = 0.0;
	
	for(int i=0;i<GetNumberOfParticles();++i){
		double KE = 0.5*GetParticleMass(i)*(pow(GetXParticleVelocity(i),2.0)+pow(GetYParticleVelocity(i),2.0));
		pParticleVector[i]->setKineticEnergy(KE);
		TotalKineticEnergy += KE;
	}
	AverageKineticEnergy = TotalKineticEnergy/NumberOfParticles;
	temperature = TotalKineticEnergy/(NumberOfParticles*kb);
	for(int i=0;i<GetNumberOfParticles();++i){sum += pow((pParticleVector[i]->getKineticEnergy() - AverageKineticEnergy),2.0);} 
	SDKineticEnergy = sqrt(sum/NumberOfParticles);
}

// this function is for the simple Berendsen Thermostat
void ParticleEnsemble::BerendsenVelocityRescaling(){
  
  double scaleFactor;
  int i;
  
  // this is an extra velocity rescaling measure to improve real-time stability... not part of Berendsen!!!!
  //    be sure that no single particle has a KE which differs from the average by 3 standard deviations (sigmas) 
  CalculateKineticEnergiesAndTemperature();
  double sigma(2.0);
  for(i=0;i<GetNumberOfParticles();++i){						
    if((GetParticleKineticEnergy(i) - AverageKineticEnergy) > (sigma * SDKineticEnergy)){
      scaleFactor = (sigma * SDKineticEnergy) / (GetParticleKineticEnergy(i) - AverageKineticEnergy); 
      SetXParticleVelocity(i,scaleFactor * GetXParticleVelocity(i));
      SetYParticleVelocity(i,scaleFactor * GetYParticleVelocity(i));
    }
  }    
  
  // again, a real-time stability measure... not part of Berendsen!!!!
  // re-initialize the system if the temperature gets crazy
  if(temperature > 1.0e8){      
    cout << " T = " << temperature << " scaleFactor " << scaleFactor << endl;
    EliminateParticleOverlap(BoxHeight,BoxWidth);    // adjust particle positions to eliminate overlap - this can cause the sim to freeze
    for(i=0;i<GetNumberOfParticles();++i){           //  then we zero out the forces and velocities 
      SetXParticleForce(i,0.0);
      SetYParticleForce(i,0.0);
      SetLastXParticlePosition(i,GetXParticlePosition(i));
      SetLastYParticlePosition(i,GetYParticlePosition(i));
    }
    CalculateKineticEnergiesAndTemperature();
  }
	
	// this code here is the bona fide Berendsen thermostat !!!!
  scaleFactor=sqrt(Tequilibrium/(BerendsenCoupling*temperature));
  if(scaleFactor != 1.0){
    for(int i=0;i<GetNumberOfParticles();++i){						//rescale the velocities
      SetXParticleVelocity(i,scaleFactor * GetXParticleVelocity(i));
      SetYParticleVelocity(i,scaleFactor * GetYParticleVelocity(i));
    }    
  }
  
}	

// function to calculate the velocity of particles i & j in their center of mass frame
void ParticleEnsemble::calculateParticleVelocitiesInComFrame(int i, int j){
	
	int kk, dimensions(3);
	vector < double > Vi(dimensions,0.0), Qi(dimensions,0.0), Vj(dimensions,0.0), Qj(dimensions,0.0);
	vector < double > Vcom(dimensions,0.0), Vicom(dimensions,0.0), Vjcom(dimensions,0.0), n12(dimensions,0.0);
	vector < double > ViParProj(dimensions,0.0), VjParProj(dimensions,0.0);
	
	Vi[0]=pParticleVector[i]->getvx(); 		// velocity vectors for particle i
	Vi[1]=pParticleVector[i]->getvy();
	Qi[0]=pParticleVector[i]->getpx(); 		// position vectors for particle i
	Qi[1]=pParticleVector[i]->getpy();
	
	Vj[0]=pParticleVector[j]->getvx();		  // velocity vectors for particle j
	Vj[1]=pParticleVector[j]->getvy();
	Qj[0]=pParticleVector[j]->getpx();			// position vectros for particle j
	Qj[1]=pParticleVector[j]->getpy();
	
	double ijSeparation=GetInterParticleSeparation(i,j);   // calculate interparticle separation distance
	
	double MassA = pParticleVector[i]->getMass();
	double MassB = pParticleVector[j]->getMass();
	
	for(kk=0;kk<dimensions;++kk)			//	calculate center of mass (COM) velocity
		Vcom[kk]=(MassA*Vi[kk]+MassB*Vj[kk])/(MassA+MassB);
	
	for(kk=0;kk<dimensions;++kk){			//	calculate velocity of particle i & j in COM frame
		Vicom[kk]=Vi[kk]-Vcom[kk];
		Vjcom[kk]=Vj[kk]-Vcom[kk];
	}
	
	for(kk=0;kk<dimensions;++kk)			//	calculate unit vector, n12, pointing from i to j
		n12[kk]=(Qi[kk]-Qj[kk])/ijSeparation;
	
	for(kk=0;kk<dimensions;++kk){			//	calculate the parallel projection of i & j onto n12
		ViParProj[kk]=n12[kk]*(Vicom[0]*n12[0]+Vicom[1]*n12[1]+Vicom[2]*n12[2]);
		VjParProj[kk]=n12[kk]*(Vjcom[0]*n12[0]+Vjcom[1]*n12[1]+Vjcom[2]*n12[2]);
	}
	
	double ViParMagnitude=sqrt(pow(ViParProj[0],2.0)+pow(ViParProj[1],2.0)+pow(ViParProj[2],2.0));
	double VjParMagnitude=sqrt(pow(VjParProj[0],2.0)+pow(VjParProj[1],2.0)+pow(VjParProj[2],2.0));
	
//debug code
//	cout << " Collision velocity for particle " << i << " " << j << " " << ViParMagnitude << endl; 
	
	pParticleVector[i]->setvInCollisionFrame(ViParMagnitude);
	pParticleVector[j]->setvInCollisionFrame(VjParMagnitude);
	
}

void ParticleEnsemble::calculateParticleVelocitiesInXWallFrame(int i){
	double velx=pParticleVector[i]->getvx(); 		// velocity vectors for particle i in x direction
	pParticleVector[i]->setvInCollisionFrame(velx/2.0);  
//	cout << " Wall Collision velocity for particle " << i << " " << pParticleVector[i]->getvInCollisionFrame() << endl; 
}

void ParticleEnsemble::calculateParticleVelocitiesInYWallFrame(int i){
	double vely=pParticleVector[i]->getvy(); 		// velocity vectors for particle i in x direction
//	cout << " Wall Collision velocity for particle " << i << " " << pParticleVector[i]->getvInCollisionFrame() << endl; 
	pParticleVector[i]->setvInCollisionFrame(vely/2.0);  	
}

void ParticleEnsemble::FFTVelocityAutoCorrelationFunction(){
	
	AverageVelocityAutoCorrelationFunction.assign(1024,0.0);  // before FFT, we need to update the ensemble averaged autocorrelation function 
	double avgfac = 1.0/pParticleVector.size();
	for(int i=0; i<pParticleVector.size(); ++i){			
		for(int jj=0; jj<1024; ++jj){		// for the sake of efficiency, the velocity autocorrelation functions on the particle are PUBLIC data... beware!!!
			AverageVelocityAutoCorrelationFunction[jj] += avgfac*(pParticleVector[i]->VelocityAutoCorrelationFunction[0])*(pParticleVector[i]->VelocityAutoCorrelationFunction[jj]);
			//	cout << jj << " "<< AverageVelocityAutoCorrelationFunction[jj] << endl;
		}
	}
	
	/*
	 cout << "correlation function" << endl;
	 for(int i=0;i<AverageVelocityAutoCorrelationFunction.size();++i){
	 if(AverageVelocityAutoCorrelationFunction[i] != 0.0){
	 cout << i << " " << AverageVelocityAutoCorrelationFunction[i] << endl;
	 }
	 }
	 * /
	
	realft(AverageVelocityAutoCorrelationFunction, 1);		
	/*
	 cout << "FFT" << endl;	
	 for(int i=0;i<AverageVelocityAutoCorrelationFunction.size();++i){
	 cout << i << " " << AverageVelocityAutoCorrelationFunction[i] << endl;
	 }
	 * /	
}

void ParticleEnsemble::realft(std::vector<double> &data, const int isign)
{
	int i,i1,i2,i3,i4;
	double c1=0.5,c2,h1r,h1i,h2r,h2i,wr,wi,wpr,wpi,wtemp,theta;
	
	int n=static_cast<int>(data.size());
	theta=M_PI/double(n>>1);
	if (isign == 1) {
		c2 = -0.5;
		four1(data,1);
	} else {
		c2=0.5;
		theta = -theta;
	}
	wtemp=sin(0.5*theta);
	wpr = -2.0*wtemp*wtemp;
	wpi=sin(theta);
	wr=1.0+wpr;
	wi=wpi;
	for (i=1;i<(n>>2);i++) {
		i2=1+(i1=i+i);
		i4=1+(i3=n-i1);
		h1r=c1*(data[i1]+data[i3]);
		h1i=c1*(data[i2]-data[i4]);
		h2r= -c2*(data[i2]+data[i4]);
		h2i=c2*(data[i1]-data[i3]);
		data[i1]=h1r+wr*h2r-wi*h2i;
		data[i2]=h1i+wr*h2i+wi*h2r;
		data[i3]=h1r-wr*h2r+wi*h2i;
		data[i4]= -h1i+wr*h2i+wi*h2r;
		wr=(wtemp=wr)*wpr-wi*wpi+wr;
		wi=wi*wpr+wtemp*wpi+wi;
	}
	if (isign == 1) {
		data[0] = (h1r=data[0])+data[1];
		data[1] = h1r-data[1];
	} else {
		data[0]=c1*((h1r=data[0])+data[1]);
		data[1]=c1*(h1r-data[1]);
		four1(data,-1);
	}
}

#define SWAP(a,b) tempr=(a);(a)=(b);(b)=tempr

void ParticleEnsemble::four1(std::vector<double> &data, const int isign)
{
	int n,mmax,m,j,istep,i;
	double wtemp,wr,wpr,wpi,wi,theta,tempr,tempi;
	
	int nn=static_cast<int>(data.size()/2);
	n=nn << 1;
	j=1;
	for (i=1;i<n;i+=2) {
		if (j > i) {
			SWAP(data[j-1],data[i-1]);
			SWAP(data[j],data[i]);
		}
		m=nn;
		while (m >= 2 && j > m) {
			j -= m;
			m >>= 1;
		}
		j += m;
	}
	mmax=2;
	while (n > mmax) {
		istep=mmax << 1;
		theta=isign*(2.0 * M_PI/mmax);
		wtemp=sin(0.5*theta);
		wpr = -2.0*wtemp*wtemp;
		wpi=sin(theta);
		wr=1.0;
		wi=0.0;
		for (m=1;m<mmax;m+=2) {
			for (i=m;i<=n;i+=istep) {
				j=i+mmax;
				tempr=wr*data[j-1]-wi*data[j];
				tempi=wr*data[j]+wi*data[j-1];
				data[j-1]=data[i-1]-tempr;
				data[j]=data[i]-tempi;
				data[i-1] += tempr;
				data[i] += tempi;
			}
			wr=(wtemp=wr)*wpr-wi*wpi+wr;
			wi=wi*wpr+wtemp*wpi+wi;
		}
		mmax=istep;
	}
}
#undef SWAP



*/