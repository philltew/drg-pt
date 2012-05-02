using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticlesOpt
{
	public class Particle
	{
		private static int TotalParticleCount = 0; // static data member to keep track of how many total particles there are
		public static int getTotalParticleCount() { return TotalParticleCount; }  // static function to return # of particles instantiated

		private double mass;
		private double px, py;
		private double pxLast, pyLast;
		private double vx, vy;
		private double vxLast, vyLast;
		private double fx, fy;
		private double fxLast, fyLast;
		private double radius, initialRadius;
		private double KineticEnergy;
		private double vInCollisionFrame;
		private bool ParticleCollided;
		private bool wasReflectedByWall;		

		public int TypeID;
		
		public Particle()
		{
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
			VelocityAutoCorrelationFunction = new List<double>(new double[1024]);  // this is the vector that will hold the particle's velocity autocorrelation function  
			++TotalParticleCount;  // increment the static counter
		}

		public void setMass(double newMass) { mass = newMass; }
		public void setpx(double newpx) { px = newpx; }
		public void setpy(double newpy) { py = newpy; }
		public void setpxLast(double newpxLast) { pxLast = newpxLast; }
		public void setpyLast(double newpyLast) { pyLast = newpyLast; }
		public void setvx(double newvx)
		{
			vxLast = vx;
			vx = newvx;
		}
		public void setvy(double newvy)
		{
			vyLast = vy;
			vy = newvy;
		}
		public void setfx(double newfx) { fx = newfx; }
		public void setfy(double newfy) { fy = newfy; }
		public void setfxLast(double newfxLast) { fxLast = newfxLast; }

		public void setfyLast(double newfyLast) { fyLast = newfyLast; }
		public void setRadius(double newradius) { radius = newradius; }
		public void setInitialRadius(double newradius)
		{
			initialRadius = newradius;
			radius = newradius;
		}
		public void setKineticEnergy(double newKE) { KineticEnergy = newKE; }
		public void setWasReflectedByWall(bool value) { wasReflectedByWall = value; }
		public void setCollisionOccurred() { ParticleCollided = true; }
		public void setParticleDidNotCollide()
		{
			ParticleCollided = false;
			wasReflectedByWall = false;
		}
		public void setvInCollisionFrame(double value) { vInCollisionFrame = value; }		

		// all the get functions

		public double getMass() { return mass; }
		public double getpx() { return px; }
		public double getpy() { return py; }
		public double getpxLast() { return pxLast; }
		public double getpyLast() { return pyLast; }
		public double getvx() { return vx; }
		public double getvy() { return vy; }
		public double getfx() { return fx; }
		public double getfy() { return fy; }
		public double getfxLast() { return fxLast; }
		public double getfyLast() { return fyLast; }
		public double getRadius() { return radius; }
		public double getInitialRadius() { return initialRadius; }
		public double getKineticEnergy() { return KineticEnergy; }
		public double getvInCollisionFrame() { return vInCollisionFrame; }
		public bool didParticleCollideWithParticle() { return ParticleCollided; }
		public bool didParticleCollideWithWall() { return wasReflectedByWall; }

		public List<double> VelocityAutoCorrelationFunction;  // this is public for now because of efficiency considerations!!!!! BEWARE!!!!

		public void UpdateVelocityAutoCorrelationFunction(){

			VelocityAutoCorrelationFunction.RemoveAt(VelocityAutoCorrelationFunction.Count - 1);																													// pop the last element off the vector
			VelocityAutoCorrelationFunction.Insert(0, Math.Sqrt(vx*vx+vy*vy));  // insert the latest modulus of the velocities into the beginning of the vector
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