using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticlesOpt2
{

	public class ParticlePalette
	{
		public const int MAX_PARTICLE_TYPES = 8; 

		private int m_Count; 
		
		public ParticleInfo[] Lookup; 
		public List<int> Active = new List<int>();

		public ParticlePalette() 
		{ 
			m_Count = 0;
			Lookup = new ParticleInfo[MAX_PARTICLE_TYPES]; 
		} 
	
		public void Clear() { m_Count = 0; }  
	
		public bool addParticleDefinition(double Mass, int Color, int MidiOutput, int Note)
		{
			if (m_Count < MAX_PARTICLE_TYPES) 
			{ 
				ParticleInfo info = new ParticleInfo(); 
		
				//info.Name = Name; 
				info.ID = m_Count; 
				info.Mass = Mass; 
				info.Color = Color; 
				info.MidiOutput = MidiOutput; 
				info.Note = Note; 
				info.Enabled = true; 
				info.AttractiveOrRepulsive = 1.0;   // repulsive is -1.0; attractive is 1.0
				info.soundIsOn = true;              // does what it says

				Lookup[m_Count++] = info; 
		
				info.activeParticle = false;
		
				return true;
			}
			else 
			{ 
				return false; 
			}
		}			

		public void setEnabled(int index, bool enabled) 
		{ 
			Active.Clear(); 
			Lookup[index].Enabled = enabled; 

			for (int i = 0; i < m_Count; i++) 
			{ 
				if (Lookup[i].Enabled == true) 
				{ 
					Active.Add(i); 
				}
			}
		}

		public void toggleEnabled(int index) 
		{ 
			setEnabled(index, !Lookup[index].Enabled); 
		}

		public int getCount() 
		{ 
			return m_Count; 
		}

		public void setAsActiveParticle(int index, bool value)
		{
			Lookup[index].activeParticle = value;
		}

		public void setAttractiveOrRepulsive(int index, double value)
		{
			Lookup[index].AttractiveOrRepulsive = value;
		}

		public void switchAttractiveOrRepulsive(int index)
		{
			Lookup[index].AttractiveOrRepulsive *= -1.0;
		}

		public void setSound(int index, bool value)
		{
			Lookup[index].soundIsOn = value;
		}

		public void toggleSound(int index)
		{
			if(Lookup[index].soundIsOn)
			{
				Lookup[index].soundIsOn = false;
			}		
			else
			{
				Lookup[index].soundIsOn = true;
			}
		}
	}
}
/* 

#define MAX_PARTICLE_TYPES 8

// Additional info shared across a class of particles  
struct ParticleInfo { 

	int ID; 
	//string Name; 
	double Mass; 
	int Color;
	double AttractiveOrRepulsive;
	bool soundIsOn;
	
	int MidiOutput; 	
	int Note; 

	bool Enabled; 
	bool activeParticle;
	
}; 

// holds a lookup of all particle types
class ParticlePalette {
  
private:
	int m_Count; 
	
public:
	ParticlePalette() { m_Count = 0; } 
	
	void Clear(void) { m_Count = 0; }  
	
	bool addParticleDefinition(double Mass, int Color, int MidiOutput, int Note);  
		
	ParticleInfo Lookup[MAX_PARTICLE_TYPES]; 

	vector<int> Active;

	void setEnabled(int index, bool enabled) { 
		Active.clear(); 
		Lookup[index].Enabled = enabled; 

		for (int i = 0; i < m_Count; i++) { 
			if (Lookup[i].Enabled == true) { 
				Active.push_back(i); 
			}
		}
	};

	void toggleEnabled(int index) { 
		setEnabled(index, !Lookup[index].Enabled); 
	}; 
	
	int getCount() { 
		return m_Count; 
	}
	
	void setAsActiveParticle(int index, bool value){
		Lookup[index].activeParticle = value;
	}
	
	void setAttractiveOrRepulsive(int index, double value){
		Lookup[index].AttractiveOrRepulsive = value;
	}

	void switchAttractiveOrRepulsive(int index){
		Lookup[index].AttractiveOrRepulsive *= -1.0;
	}
	
	void setSound(int index, bool value){
		Lookup[index].soundIsOn = value;
	}

	void toggleSound(int index){
		if(Lookup[index].soundIsOn){Lookup[index].soundIsOn = false;}		
		else{Lookup[index].soundIsOn = true;}
	}
};

extern ParticlePalette AtomPropertiesDefinition;

*/

/* 
 
bool ParticlePalette::addParticleDefinition(double Mass, int Color, int MidiOutput, int Note) {  // string Name, 
	if (m_Count < MAX_PARTICLE_TYPES) { 
		ParticleInfo info; 
		
		//info.Name = Name; 
		info.ID = m_Count; 
		info.Mass = Mass; 
		info.Color = Color; 
		info.MidiOutput = MidiOutput; 
		info.Note = Note; 
		info.Enabled = true; 
		info.AttractiveOrRepulsive = 1.0;   // repulsive is -1.0; attractive is 1.0
		info.soundIsOn = true;              // does what it says

		Lookup[m_Count++] = info; 
		
		info.activeParticle = false;
		
		return true;
	}
	else { 
		return false; 
	}
}
*/ 
