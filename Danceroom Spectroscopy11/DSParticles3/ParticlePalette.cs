using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace DSParticles3
{
	public class ParticlePalette
	{
		public const int MAX_PARTICLE_TYPES = 8;

		#region Private Members

		private int m_Count;
		private double m_RadiiScaleFactor = 1.0;

		#endregion

		#region Public Members

		public readonly AtomicInfo[] Lookup;
		public readonly List<int> Active = new List<int>();

		public int Count { get { return m_Count; } }

		public double RadiiScaleFactor { get { return m_RadiiScaleFactor; } }

		#endregion

		#region Constructor

		public ParticlePalette()
		{
			m_Count = 0;
			Lookup = new AtomicInfo[MAX_PARTICLE_TYPES];
		}

		#endregion

		#region Add Particle Definition

		public bool AddParticleDefinition(string Name, double Mass, Color3 Color, int MidiOutput, int Note)
		{
			if (m_Count < MAX_PARTICLE_TYPES)
			{
				AtomicInfo info = new AtomicInfo(m_Count, Name, Mass);

				info.Color = Color;
				info.RenderColor = new Vector4(info.Color.Red, info.Color.Green, info.Color.Blue, (float)info.Radius); 
				info.MidiOutput = MidiOutput;
				info.Note = Note;
				info.Enabled = true;
				info.AttractiveOrRepulsive = -1.0;   // repulsive is -1.0; attractive is 1.0
				info.IsSoundOn = true;              // does what it says

				Lookup[m_Count++] = info;

				info.ActiveParticle = false;

				CalculateEnergyTerms();

				return true;
			}
			else
			{
				return false;
			}
		}

		#endregion

		#region Clear

		public void Clear() { m_Count = 0; }

		#endregion

		#region Enabled

		public void SetEnabled(int index, bool enabled)
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

		public void ToggleEnabled(int index)
		{
			SetEnabled(index, !Lookup[index].Enabled);
		}

		#endregion

		#region Active Particle

		public void SetAsActiveParticle(int index, bool value)
		{
			Lookup[index].ActiveParticle = value;
		}

		#endregion

		#region Attractive Or Repulsive

		public void SetAttractiveOrRepulsive(int index, double value)
		{
			Lookup[index].AttractiveOrRepulsive = value;
		}

		public void SetAttractiveOrRepulsive(int index, bool value)
		{
			bool currentValue = Lookup[index].AttractiveOrRepulsive > 0;

			if (currentValue != value)
			{
				Lookup[index].AttractiveOrRepulsive *= -1.0;
			}
		}

		public void ToggleAttractiveOrRepulsive(int index)
		{
			Lookup[index].AttractiveOrRepulsive *= -1.0;
		}

		#endregion

		#region Sound

		public void SetSound(int index, bool value)
		{
			Lookup[index].IsSoundOn = value;
		}

		public void ToggleSound(int index)
		{
			if (Lookup[index].IsSoundOn)
			{
				Lookup[index].IsSoundOn = false;
			}
			else
			{
				Lookup[index].IsSoundOn = true;
			}
		}

		#endregion

		#region Calculate Energy Terms

		public void CalculateEnergyTerms()
		{
			double epsilon = 10.0;

			for (int i = 0; i < Count; i++)
			{
				AtomicInfo infoi = Lookup[i];

				double radiusi = infoi.Radius;

				for (int j = 0; j < Count; j++)
				{
					AtomicInfo infoj = Lookup[j];

					double radiusj = infoj.Radius;

					double MinDistance = 2.0 * (radiusi + radiusj);

					infoi.MinimumDistance[j] = MinDistance * MinDistance;
					infoi.LJenergyTermA[j] = epsilon * Math.Pow(MinDistance, 12.0);
					infoi.LJenergyTermB[j] = -2.0 * epsilon * Math.Pow(MinDistance, 6.0);
					infoi.LJgradientTermA[j] = -12.0 * infoi.LJenergyTermA[j];
					infoi.LJgradientTermB[j] = -6.0 * infoi.LJenergyTermB[j];
				}
			}
		}

		#endregion

		#region Scale Particle Radii

		/// <summary>
		/// this function allows dynamic scaling of the particle Radii
		/// </summary>
		/// <param name="newScaleFac"></param>
		public void ScaleParticleRadii(double newScaleFac)
		{
			// update the particle radii
			m_RadiiScaleFactor = newScaleFac;

			for (int i = 0; i < Count; ++i)
			{
				AtomicInfo infoi = Lookup[i];

				infoi.Radius = m_RadiiScaleFactor * infoi.InitialRadius;

				infoi.RenderColor = new SlimDX.Vector4(infoi.Color.Red, infoi.Color.Green, infoi.Color.Blue, (float)infoi.Radius);
				infoi.ParticleCollisionColor = new SlimDX.Vector4(1, 1, 1, (float)infoi.Radius);
				infoi.WallCollisionColor = new SlimDX.Vector4(1, 1, 0, (float)infoi.Radius); 
			}

			CalculateEnergyTerms();
		}

		#endregion

		/* 
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
		 */

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
