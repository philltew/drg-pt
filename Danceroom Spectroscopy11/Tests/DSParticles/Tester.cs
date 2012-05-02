using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticles
{
	public class Tester
	{
		public ParticleEnsemble Ensemble1;
		ExternalField Field; 
		int m_SizeX, m_SizeY;
		int m_NumberOfPixels; 
		double[] m_Data;

		public Tester(int sizeX, int sizeY, double[] data)
		{
			m_SizeX = sizeX;
			m_SizeY = sizeY;
			m_NumberOfPixels = m_SizeX * m_SizeY; 

			if (data.Length != m_NumberOfPixels)
			{
				throw new Exception("Test field data is not the correct size"); 
			}

			m_Data = new double[m_NumberOfPixels];
			data.CopyTo(m_Data, 0);			
		}		

		private void SetupForTesting(int Nparticles)
		{
			double gradScaleFactor = 3000.0;
			double MinRad = 40.0;
			double MaxRad = 50.0;

			List<string> FFtype = new List<string>(new string[] { "SoftSpheres", "ExternalField" }); 
			
			ParticleStaticObjects.AtomPropertiesDefinition.Clear(); 
			ParticleStaticObjects.AtomPropertiesDefinition.addParticleDefinition(2, unchecked((int)0xc00A32FF), 0, 50); // neon blue
			ParticleStaticObjects.AtomPropertiesDefinition.addParticleDefinition(4, unchecked((int)0xc0FF0A32), 1, 49); // red
			ParticleStaticObjects.AtomPropertiesDefinition.addParticleDefinition(6, unchecked((int)0xc0320AFF), 2, 45); // purple
			ParticleStaticObjects.AtomPropertiesDefinition.addParticleDefinition(9.2, unchecked((int)0xc026D8FF), 3, 43); // sky blue // 0xc032FF0A, 3,  43); // green 
			ParticleStaticObjects.AtomPropertiesDefinition.addParticleDefinition(12.6, unchecked((int)0xc0FF320A), 4, 38); // orange

			ParticleStaticObjects.ReSeedRandom(42); 

			Ensemble1 = new ParticleEnsemble(Nparticles, MinRad, MaxRad, FFtype, m_SizeY, m_SizeX, gradScaleFactor);
			Field = new ExternalField(Ensemble1);

			double[] background = new double[m_NumberOfPixels]; 

			for (int i = 0; i < m_NumberOfPixels; i++)
			{
				background[i] = 512; 
			}

			Field.IncrementGrabberCalls();                         // increment the number of grabber calls			
			Field.BackgroundCalibration(m_NumberOfPixels, background);  // in order to simply set the background to zero 
			Field.SetCalibrating(false);
		}

		public long RunTest(int Nparticles, int iterations, int drawFrequency)
		{			
			SetupForTesting(Nparticles);

			long ticksAtStart = DateTime.Now.Ticks; 

			for (int i = 0; i < iterations; i++)
			{
				Field.SetPixelDiff(m_NumberOfPixels, m_Data); 

				Ensemble1.ResetParticleCollisions();

				for (int j = 0; j < drawFrequency; j++)
				{
					Ensemble1.VelocityVerletPropagation(m_SizeY, m_SizeX, Field);
				}
			}

			long ticksAtEnd = DateTime.Now.Ticks;

			return ticksAtEnd - ticksAtStart; 
		}

		public void SetupSingleFrame(int Nparticles)
		{
			SetupForTesting(Nparticles);
		}

		public void RunSingleFrame(int drawFrequency)
		{
			Field.SetPixelDiff(m_NumberOfPixels, m_Data);

			Ensemble1.ResetParticleCollisions();

			for (int j = 0; j < drawFrequency; j++)
			{
				Ensemble1.VelocityVerletPropagation(m_SizeY, m_SizeX, Field);
			}
		}
	}
}
