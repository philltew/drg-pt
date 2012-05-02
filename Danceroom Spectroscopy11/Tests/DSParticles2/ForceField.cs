using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticles2
{
	public abstract class ForceField
	{
		public abstract string ForceFieldType { get; }
		public abstract void CalculateEnergyTerms(ParticleEnsemble ensemble);
		public abstract void UpdateEnergyTerms(ParticleEnsemble ensemble);
		public abstract void CalculateForceField(ParticleEnsemble ensemble);
	}
}

/* 
 * 
 * class ParticleEnsemble;

class ForceField {
  
public:
	
	ForceField(){
//		cout << "ForceField object allocated" << endl;
	}
	
	virtual ~ForceField(){
//		cout << "ForceField object deallocated" << endl;		
	}
	
  virtual std::string getForceFieldType() = 0;
  
  virtual void calculateEnergyTerms(ParticleEnsemble *pParticleEnsemble){}
  
	virtual void updateEnergyTerms(ParticleEnsemble *pParticleEnsemble) {}

  // virtual function to calculate forcefield
  virtual void calculateForceField(ParticleEnsemble *pParticleEnsemble) = 0;
  
};

#endif
*/ 
