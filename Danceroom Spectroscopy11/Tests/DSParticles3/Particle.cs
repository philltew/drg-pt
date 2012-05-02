using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticles3
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

		public GridSector GridSector; 

		private double m_Radius, m_InitialRadius;
		private double m_KineticEnergy;
		private double m_VInCollisionFrame;
		private bool m_ParticleCollided;
		private bool m_WasReflectedByWall;

		private int m_TypeID = -1; 
		private AtomicInfo m_ParticleType;

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
		
		public AtomicInfo ParticleType
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
			set { m_Mass = value; }
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
				
				// return m_Radius;
			}
			//set { m_Radius = value; }
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
				//return m_InitialRadius;
			}
			//set { m_InitialRadius = value; }
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
			GridSector = new GridSector(); 

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
