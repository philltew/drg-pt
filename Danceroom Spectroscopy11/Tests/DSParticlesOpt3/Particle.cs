using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticlesOpt3
{
	public class Particle
	{
		private double m_Mass = 10;

		public DPVector Position;
		public DPVector PositionLast;
		public DPVector Velocity;
		public DPVector VelocityLast;
		public DPVector Force;
		public DPVector ForceLast;

		//private double m_Radius, m_InitialRadius;
		private double m_KineticEnergy;
		private double m_VInCollisionFrame;
		private bool m_ParticleCollided;
		private bool m_WasReflectedByWall;

		private int m_TypeID = -1; 
		private ParticleInfo m_ParticleType;

		public int TypeID
		{
			get { return m_TypeID; }
			set
			{
				m_TypeID = value;

				if (m_TypeID < 0)
				{
					m_ParticleType = null;
				}
				else
				{
					m_ParticleType = ParticleStaticObjects.AtomPropertiesDefinition.Lookup[m_TypeID]; 
				}
			}
		}
		
		public ParticleInfo ParticleType
		{
			get
			{
				return m_ParticleType; 
			}
		}

		public readonly List<double> VelocityAutoCorrelationFunction;  // this is public for now because of efficiency considerations!!!!! BEWARE!!!!

		#region Properties

		public double Mass 
		{ 
			get 
			{
				/* if (m_ParticleType == null)
				{
					throw ThrowHasNoTypeException();
				}
				else
				{
					return m_ParticleType.Mass; 
				}*/
				
				return m_Mass; 
			} 
		}

		public double Radius 
		{
			get 
			{
				if (m_ParticleType == null)
				{
					throw ThrowHasNoTypeException();
				}
				else
				{
					return m_ParticleType.Radius;
				}			
			}
		}

		public double InitialRadius
		{
			get
			{
				if (m_ParticleType == null)
				{
					throw ThrowHasNoTypeException();
				}
				else
				{
					return m_ParticleType.InitialRadius;
				}
			}
		}
		
		public double KineticEnergy { get { return m_KineticEnergy; } set { m_KineticEnergy = value; } }
		public double VInCollisionFrame { get { return m_VInCollisionFrame; } set { m_VInCollisionFrame = value; } }

		public bool WasReflectedByWall { get { return m_WasReflectedByWall; } set { m_WasReflectedByWall = value; } }
		public bool CollisionOccurred { get { return m_ParticleCollided; } set { m_ParticleCollided = value; } }

		public bool DidParticleCollideWithParticle() { return m_ParticleCollided; }
		public bool DidParticleCollideWithWall() { return m_WasReflectedByWall; }

		#endregion

		public Particle()
		{
			//	cout << "Particle allocated" << endl;
			TypeID = 0;
			m_Mass = 10;
			m_ParticleCollided = false;
			m_KineticEnergy = 0.0;
			m_VInCollisionFrame = 0.0;

			Position = new DPVector(0, 0);
			PositionLast = new DPVector(0, 0);
			Velocity = new DPVector(0, 0);
			VelocityLast = new DPVector(0, 0);
			Force = new DPVector(0, 0);
			ForceLast = new DPVector(0, 0);

			VelocityAutoCorrelationFunction = new List<double>(new double[1024]);  // this is the vector that will hold the particle's velocity autocorrelation function  
		}

		private Exception ThrowHasNoTypeException()
		{
			return new Exception("Particle has not been assigned a ParticleType");
		}

		public void MarkParticleDidNotCollide()
		{
			m_ParticleCollided = false;
			m_WasReflectedByWall = false;
		}

		public void UpdateVelocityAutoCorrelationFunction()
		{

			VelocityAutoCorrelationFunction.RemoveAt(VelocityAutoCorrelationFunction.Count - 1);																													// pop the last element off the vector
			VelocityAutoCorrelationFunction.Insert(0, Math.Sqrt(Velocity.X * Velocity.X + Velocity.Y * Velocity.Y));  // insert the latest modulus of the velocities into the beginning of the vector
			//VelocityAutoCorrelationFunction.insert(VelocityAutoCorrelationFunction.begin(), sqrt(vx * vx + vy * vy));  // insert the latest modulus of the velocities into the beginning of the vector
		}
	}
}

/* 


class Particle{
  
public:
  
	int TypeID; 
	
  // constructor
  Particle();
  
  // destructor
  ~Particle(){
//		cout << "Particle deallocated" << endl;
	};
  
  // all the set functions
  void setMass(double newMass){mass = newMass;}
  void setpx(double newpx){px = newpx;}
  void setpy(double newpy){py = newpy;}
  void setpxLast(double newpxLast){pxLast = newpxLast;}
  void setpyLast(double newpyLast){pyLast = newpyLast;}
  void setvx(double newvx){
    vxLast = vx;
    vx = newvx;
  }
  void setvy(double newvy){
    vyLast = vy;
    vy = newvy;
  }  
  void setfx(double newfx){fx = newfx;}
  void setfy(double newfy){fy = newfy;}
  void setfxLast(double newfxLast){fxLast = newfxLast;}
	
  void setfyLast(double newfyLast){fyLast = newfyLast;}  
  void setRadius(double newradius){radius = newradius;}
  void setInitialRadius(double newradius){
    initialRadius = newradius;
    radius = newradius;
  }  
  void setKineticEnergy(double newKE){KineticEnergy = newKE;}
  void setWasReflectedByWall(bool value){wasReflectedByWall = value;} 
	void setCollisionOccurred(){ParticleCollided = true;}
	void setParticleDidNotCollide(){
		ParticleCollided = false;
		wasReflectedByWall = false;
	}
	void setvInCollisionFrame(double value){vInCollisionFrame = value;}
	void UpdateVelocityAutoCorrelationFunction();
	
  // all the get functions
	
  double getMass(){return mass;}
  double getpx(){return px;}
  double getpy(){return py;}
  double getpxLast(){return pxLast;}
  double getpyLast(){return pyLast;}
  double getvx(){return vx;}
  double getvy(){return vy;}  
  double getfx(){return fx;}
  double getfy(){return fy;}
  double getfxLast(){return fxLast;}
  double getfyLast(){return fyLast;}  
  double getRadius(){return radius;}
  double getInitialRadius(){return initialRadius;}  
  double getKineticEnergy(){return KineticEnergy;}
	double getvInCollisionFrame(){return vInCollisionFrame;}
	bool   didParticleCollideWithParticle(){return ParticleCollided;}
	bool   didParticleCollideWithWall(){return wasReflectedByWall;}
	
  vector <double> VelocityAutoCorrelationFunction;  // this is public for now because of efficiency considerations!!!!! BEWARE!!!!

	static int getTotalParticleCount(){return TotalParticleCount;}  // static function to return # of particles instantiated
	
private:

  double mass;
  double px, py;
  double pxLast, pyLast;
  double vx, vy;
  double vxLast, vyLast;
  double fx, fy;
  double fxLast, fyLast;
  double radius, initialRadius;
  double KineticEnergy;
	double vInCollisionFrame;
  bool ParticleCollided;
  bool wasReflectedByWall;

	
	static int TotalParticleCount; // static data member to keep track of how many total particles there are

};
*/ 

/* 

// define and initialize static data member
int Particle::TotalParticleCount = 0;

Particle::Particle(){ 
//	cout << "Particle allocated" << endl;
  TypeID = 0; 
  mass = 0.0;
  ParticleCollided = false;
  px = 0.0;
  py = 0.0;
  pxLast = 0.0;
  pyLast = 0.0;
  vx = 0.0;
  vy = 0.0;
  vxLast = 0.0;
  vyLast = 0.0;
  fx = 0.0; 
  fy = 0.0;
  fxLast = 0.0;
  fyLast = 0.0;
  radius = 0.0;
  initialRadius = 0.0;
  KineticEnergy = 0.0;
	vInCollisionFrame = 0.0;
	VelocityAutoCorrelationFunction.assign(1024,0.0);  // this is the vector that will hold the particle's velocity autocorrelation function  
	++TotalParticleCount;  // increment the static counter
}

void Particle::UpdateVelocityAutoCorrelationFunction(){

	VelocityAutoCorrelationFunction.pop_back();																													// pop the last element off the vector
	VelocityAutoCorrelationFunction.insert(VelocityAutoCorrelationFunction.begin(),sqrt(vx*vx+vy*vy));  // insert the latest modulus of the velocities into the beginning of the vector
}
*/