using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticles
{
	public class SoftSpheres : ForceField
	{
		private List<double> LJxforces;
		private List<double> LJyforces;
		private List<double> WallDistance;
		private double epsilon;

		private DPMatrix MinimumDistance;
		private DPMatrix LJenergyTermA;
		private DPMatrix LJenergyTermB;
		private DPMatrix LJgradientTermA;
		private DPMatrix LJgradientTermB;	

		public override string ForceFieldType
		{
			get { return "SoftSphereForceField"; }
		}

		
		// constructor
		public SoftSpheres(ParticleEnsemble pParticleSet)
		{
  
			epsilon=10.0;
	
			// allocate vectors holding positions & forces
			LJxforces = new List<double>(new double[pParticleSet.GetMaxNumberOfParticles()]);
			LJyforces = new List<double>(new double[pParticleSet.GetMaxNumberOfParticles()]);
  
			// allocate vector holding cutoff distance for calculating Wall-Particle interactions
			WallDistance = new List<double>(new double[pParticleSet.GetMaxNumberOfParticles()]);
  		
			// allocate vector holding cutoff distance for calculating particle-particle interaction
			//Mat_DP tmp(0.0,pParticleSet->GetMaxNumberOfParticles(),pParticleSet->GetMaxNumberOfParticles());
			MinimumDistance = new DPMatrix(0, pParticleSet.GetMaxNumberOfParticles(),pParticleSet.GetMaxNumberOfParticles());
	
			// allocate vectors holding particle-particle LJ energy terms
			LJenergyTermA = new DPMatrix(0, pParticleSet.GetMaxNumberOfParticles(),pParticleSet.GetMaxNumberOfParticles()); // =tmp;
			LJenergyTermB = new DPMatrix(0, pParticleSet.GetMaxNumberOfParticles(), pParticleSet.GetMaxNumberOfParticles()); // =tmp;
			LJgradientTermA = new DPMatrix(0, pParticleSet.GetMaxNumberOfParticles(), pParticleSet.GetMaxNumberOfParticles()); // =tmp;
			LJgradientTermB = new DPMatrix(0, pParticleSet.GetMaxNumberOfParticles(), pParticleSet.GetMaxNumberOfParticles()); // =tmp;
  
			CalculateEnergyTerms(pParticleSet);
	
			// calculate initial forcefield
			CalculateForceField(pParticleSet);
  
		}

		public override void CalculateEnergyTerms(ParticleEnsemble ensemble)
		{
			for (int i = 0; i < ensemble.GetMaxNumberOfParticles(); ++i)
			{
				for (int j = (i + 1); j < ensemble.GetMaxNumberOfParticles(); ++j)
				{
					MinimumDistance[i, j] = 2.0 * (ensemble.GetParticleRadius(i) + ensemble.GetParticleRadius(j));
					MinimumDistance[j, i] = MinimumDistance[i, j];
					LJenergyTermA[i, j] = epsilon * Math.Pow(MinimumDistance[i, j], 12.0);
					LJenergyTermA[j, i] = LJenergyTermA[i, j];
					LJenergyTermB[i, j] = -2.0 * epsilon * Math.Pow(MinimumDistance[i, j], 6.0);
					LJenergyTermB[j, i] = LJenergyTermB[i, j];
					LJgradientTermA[i, j] = -12.0 * LJenergyTermA[i, j];
					LJgradientTermA[j, i] = LJgradientTermA[i, j];
					LJgradientTermB[i, j] = -6.0 * LJenergyTermB[i, j];
					LJgradientTermB[j, i] = LJgradientTermB[i, j];
				}
			}
  
		}

		public override void UpdateEnergyTerms(ParticleEnsemble ensemble)
		{
			for (int i = 0; i < ensemble.GetNumberOfParticles(); ++i)
			{
				for (int j = (i + 1); j < ensemble.GetNumberOfParticles(); ++j)
				{
					MinimumDistance[i, j] = 2.0 * (ensemble.GetParticleRadius(i) + ensemble.GetParticleRadius(j));
					MinimumDistance[j, i] = MinimumDistance[i, j];
					LJenergyTermA[i, j] = epsilon * Math.Pow(MinimumDistance[i, j], 12.0);
					LJenergyTermA[j, i] = LJenergyTermA[i, j];
					LJenergyTermB[i, j] = -2.0 * epsilon * Math.Pow(MinimumDistance[i, j], 6.0);
					LJenergyTermB[j, i] = LJenergyTermB[i, j];
					LJgradientTermA[i, j] = -12.0 * LJenergyTermA[i, j];
					LJgradientTermA[j, i] = LJgradientTermA[i, j];
					LJgradientTermB[i, j] = -6.0 * LJenergyTermB[i, j];
					LJgradientTermB[j, i] = LJgradientTermB[i, j];
				}
			}
		}

		public override void CalculateForceField(ParticleEnsemble ensemble)
		{

			//variable declarations
			double posXi, posYi;
			double posXj, posYj;
			double LJxf,  LJyf;
			double ijSeparation = 0.0, LJforce = 0.0, PotentialEnergy = 0.0;
			int j, kk, ctr, dimensions = 2;
  
			//variable initializations

			int BoxHeight = ensemble.GetBoxHeight();
			int BoxWidth = ensemble.GetBoxWidth();
			double MaxForce = ensemble.GetMaxForceThreshold();
			double MinForce = -1.0 * (ensemble.GetMaxForceThreshold());
  
			// initialize vectors holding forces
			for (int i = 0; i < ensemble.GetNumberOfParticles(); ++i)
			{
				LJxforces[i]=0.0;
				LJyforces[i]=0.0;
			}

			for (int i = 0; i < ensemble.GetNumberOfParticles(); ++i)
			{

				for (j = (i + 1); j < ensemble.GetNumberOfParticles(); ++j)
				{
      
					//    get the interparticle separation distances
					ijSeparation = ensemble.GetInterParticleSeparation(i, j);
						
					//		update the radial distribution function
			  //			pParticleSet->UpdateRadialDistributionFunction((int)(ijSeparation));
			
					double cutoffDistance = MinimumDistance[i, j];
			
					if (ijSeparation < cutoffDistance) {
	
						// for each particle, change the appropriate element of the setWithinRangeOfAnotherParticle vector to true

						posXi = ensemble.GetXParticlePosition(i);
						posYi = ensemble.GetYParticlePosition(i);

						posXj = ensemble.GetXParticlePosition(j);
						posYj = ensemble.GetYParticlePosition(j);
												
						//        PotentialEnergy += LJenergyTermA[i][j]/(pow(ijSeparation,12.0))+LJenergyTermB[i][j]/pow(ijSeparation,6.0)+epsilon;
						LJforce = (posXj - posXi) * (LJgradientTermA[i, j] / Math.Pow(ijSeparation, 13.0) + LJgradientTermB[i, j] / Math.Pow(ijSeparation, 7.0)) / ijSeparation;
						if (Math.Abs(LJforce) > MaxForce || Math.Abs(LJforce) < MinForce) { LJforce = 0.0; }    // error check for real-time stability...
						else if(double.IsNaN(LJforce) || double.IsInfinity(LJforce)){LJforce = 0.0;} // error check for real-time stability...
				
						LJxforces[i] += LJforce;
						LJxforces[j] += -1.0*LJforce;
						//        cout << "x "<< i << " " << j << " " << LJforce << endl; 
				//				cout << "i " << i << " LJxforces[i] " << LJxforces[i] << " j " << j <<  " LJxforces[j] " << LJxforces[j] << endl;
						//        cout << "xi:=" << posXi << ";" << "xj:=" << posXj << ";" << "yi:=" << posYi << ";" << "yj:=" << posYj << ";" << LJxforces[i] << endl;
						LJforce = (posYj - posYi) * (LJgradientTermA[i, j] / Math.Pow(ijSeparation, 13.0) + LJgradientTermB[i, j] / Math.Pow(ijSeparation, 7.0)) / ijSeparation;

						if (Math.Abs(LJforce) > MaxForce || Math.Abs(LJforce) < MinForce) { LJforce = 0.0; }    // error check for real-time stability...
						else if (double.IsNaN(LJforce) || double.IsInfinity(LJforce)) { LJforce = 0.0; } // error check for real-time stability...
				
						LJyforces[i] += LJforce;
						LJyforces[j] += -1.0*LJforce;

						ensemble.SetParticlesWithinRange(i, j);
						ensemble.SetParticlesWithinRange(j, i);
				
						//        cout << i << " " << j << endl; 
					}
					else{
						ensemble.SetParticlesNotWithinRange(i, j);
						ensemble.SetParticlesNotWithinRange(j, i);			
					}
				}        
			}
	
			// set the forces in the Particle Ensemble Object
			ensemble.AddXForces(LJxforces);
			ensemble.AddYForces(LJyforces);
			// set the potential energy
			ensemble.AddPotentialEnergy(PotentialEnergy);
		}
	}
}

/* 

public:
  
  // constructor
  SoftSpheres(ParticleEnsemble*);
  
  // destructor
  virtual ~SoftSpheres(){
//		cout << "SoftSpheres ForceField object deallocated" << endl;
	}
		
  
  virtual std::string getForceFieldType(){return "SoftSphereForceField";}
  
  // function to evaluate the LJ energy terms
  virtual void calculateEnergyTerms(ParticleEnsemble*);

	virtual void updateEnergyTerms(ParticleEnsemble*);
  
  // function to calculate Soft Spheres forcefield
  virtual void calculateForceField(ParticleEnsemble*);
  
private:
  
  vector <double> LJxforces;
  vector <double> LJyforces;
  vector <double> WallDistance;
  double epsilon;

  Mat_DP MinimumDistance;
  Mat_DP LJenergyTermA;
  Mat_DP LJenergyTermB;
  Mat_DP LJgradientTermA;
  Mat_DP LJgradientTermB;	
*/ 

/* 

// constructor
SoftSpheres::SoftSpheres(ParticleEnsemble* pParticleSet){
  
  epsilon=10.0;
	
	// allocate vectors holding positions & forces
  LJxforces.assign(pParticleSet->GetMaxNumberOfParticles(),0.0);
  LJyforces.assign(pParticleSet->GetMaxNumberOfParticles(),0.0);
  
  // allocate vector holding cutoff distance for calculating Wall-Particle interactions
  WallDistance.assign(pParticleSet->GetMaxNumberOfParticles(),0.0);
  		
  // allocate vector holding cutoff distance for calculating particle-particle interaction
  Mat_DP tmp(0.0,pParticleSet->GetMaxNumberOfParticles(),pParticleSet->GetMaxNumberOfParticles());
	MinimumDistance=tmp;
	
  // allocate vectors holding particle-particle LJ energy terms
  LJenergyTermA=tmp;
	LJenergyTermB=tmp;
  LJgradientTermA=tmp;
  LJgradientTermB=tmp;
  
  calculateEnergyTerms(pParticleSet);
	
  // calculate initial forcefield
  calculateForceField(pParticleSet);
  
}

// energy terms calculator
void SoftSpheres::calculateEnergyTerms(ParticleEnsemble* pParticleSet){
    
  for(int i=0;i<pParticleSet->GetMaxNumberOfParticles();++i){
 		for(int j=(i+1);j<pParticleSet->GetMaxNumberOfParticles();++j){
      MinimumDistance[i][j] = 2.0 * (pParticleSet->GetParticleRadius(i)+pParticleSet->GetParticleRadius(j));
      MinimumDistance[j][i] = MinimumDistance[i][j];
      LJenergyTermA[i][j] = epsilon*pow(MinimumDistance[i][j],12.0);
			LJenergyTermA[j][i] = LJenergyTermA[i][j];
      LJenergyTermB[i][j] = -2.0*epsilon*pow(MinimumDistance[i][j],6.0);
			LJenergyTermB[j][i] = LJenergyTermB[i][j];
      LJgradientTermA[i][j] = -12.0*LJenergyTermA[i][j];
			LJgradientTermA[j][i] = LJgradientTermA[i][j];
      LJgradientTermB[i][j] = -6.0*LJenergyTermB[i][j];
			LJgradientTermB[j][i] = LJgradientTermB[i][j];
    }
  }
  
}


// energy terms calculator
void SoftSpheres::updateEnergyTerms(ParticleEnsemble* pParticleSet){
    
	for(int i=0;i<pParticleSet->GetNumberOfParticles();++i){
 		for(int j=(i+1);j<pParticleSet->GetNumberOfParticles();++j){
      MinimumDistance[i][j] = 2.0 * (pParticleSet->GetParticleRadius(i)+pParticleSet->GetParticleRadius(j));
      MinimumDistance[j][i] = MinimumDistance[i][j];
      LJenergyTermA[i][j] = epsilon*pow(MinimumDistance[i][j],12.0);
			LJenergyTermA[j][i] = LJenergyTermA[i][j];
      LJenergyTermB[i][j] = -2.0*epsilon*pow(MinimumDistance[i][j],6.0);
			LJenergyTermB[j][i] = LJenergyTermB[i][j];
      LJgradientTermA[i][j] = -12.0*LJenergyTermA[i][j];
			LJgradientTermA[j][i] = LJgradientTermA[i][j];
      LJgradientTermB[i][j] = -6.0*LJenergyTermB[i][j];
			LJgradientTermB[j][i] = LJgradientTermB[i][j];
    }
  }
  
}


// force field calculator
void SoftSpheres::calculateForceField(ParticleEnsemble* pParticleSet){
  
	//variable declarations
	double posXi, posYi;
	double posXj, posYj;
	double LJxf,  LJyf;
	double ijSeparation(0.0), LJforce(0.0), PotentialEnergy(0.0);
	int j, kk, i, ctr, dimensions(2);
  
	//variable initializations
  
	int BoxHeight = pParticleSet->GetBoxHeight();
	int BoxWidth = pParticleSet->GetBoxWidth();  
  double MaxForce = pParticleSet->GetMaxForceThreshold();
  double MinForce = -1.0*(pParticleSet->GetMaxForceThreshold());
  
  // initialize vectors holding forces
	for(int i=0;i<pParticleSet->GetNumberOfParticles();++i){
    LJxforces[i]=0.0;
		LJyforces[i]=0.0;
	}    
    
	for(i=0;i<pParticleSet->GetNumberOfParticles();++i){
    		
		for(j=(i+1);j<pParticleSet->GetNumberOfParticles();++j){
      
			//    get the interparticle separation distances
			ijSeparation = pParticleSet->GetInterParticleSeparation(i,j);
						
			//		update the radial distribution function
      //			pParticleSet->UpdateRadialDistributionFunction((int)(ijSeparation));
			
			double cutoffDistance = MinimumDistance[i][j];
			
			if (ijSeparation < cutoffDistance) {
	
				// for each particle, change the appropriate element of the setWithinRangeOfAnotherParticle vector to true
				
				posXi = pParticleSet->GetXParticlePosition(i);
				posYi = pParticleSet->GetYParticlePosition(i);
				
				posXj = pParticleSet->GetXParticlePosition(j);
				posYj = pParticleSet->GetYParticlePosition(j);
				
				//        PotentialEnergy += LJenergyTermA[i][j]/(pow(ijSeparation,12.0))+LJenergyTermB[i][j]/pow(ijSeparation,6.0)+epsilon;
				LJforce = (posXj-posXi)*(LJgradientTermA[i][j]/pow(ijSeparation,13.0)+LJgradientTermB[i][j]/pow(ijSeparation,7.0))/ijSeparation;
				if(fabs(LJforce) > MaxForce || fabs(LJforce) < MinForce){LJforce = 0.0;}    // error check for real-time stability...
				else if(isnan(LJforce) || isinf(LJforce)){LJforce = 0.0;} // error check for real-time stability...
				
				LJxforces[i] += LJforce;
				LJxforces[j] += -1.0*LJforce;
				//        cout << "x "<< i << " " << j << " " << LJforce << endl; 
        //				cout << "i " << i << " LJxforces[i] " << LJxforces[i] << " j " << j <<  " LJxforces[j] " << LJxforces[j] << endl;
				//        cout << "xi:=" << posXi << ";" << "xj:=" << posXj << ";" << "yi:=" << posYi << ";" << "yj:=" << posYj << ";" << LJxforces[i] << endl;
				LJforce = (posYj-posYi)*(LJgradientTermA[i][j]/pow(ijSeparation,13.0)+LJgradientTermB[i][j]/pow(ijSeparation,7.0))/ijSeparation;
				
				if(fabs(LJforce) > MaxForce || fabs(LJforce) < MinForce){LJforce = 0.0;}    // error check for real-time stability...
				else if(isnan(LJforce) || isinf(LJforce)){LJforce = 0.0;} // error check for real-time stability...
				
				LJyforces[i] += LJforce;
				LJyforces[j] += -1.0*LJforce;
				
				pParticleSet->SetParticlesWithinRange(i,j);
				pParticleSet->SetParticlesWithinRange(j,i);
				
				//        cout << i << " " << j << endl; 
			}
			else{
				pParticleSet->SetParticlesNotWithinRange(i,j);
				pParticleSet->SetParticlesNotWithinRange(j,i);			
			}
		}        
	}
	
	// set the forces in the Particle Ensemble Object
	pParticleSet->AddXForces(LJxforces);
	pParticleSet->AddYForces(LJyforces);
	// set the potential energy
	pParticleSet->AddPotentialEnergy(PotentialEnergy);
}



*/ 