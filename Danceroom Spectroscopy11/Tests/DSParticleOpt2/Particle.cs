using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticlesOpt2
{
	public class Particle
	{
		private double m_Mass;

		private DPVector m_Position;
		private DPVector m_PositionLast;
		private DPVector m_Velocity;
		private DPVector m_VelocityLast;
		private DPVector m_Force;
		private DPVector m_ForceLast;		

		private double m_Radius, m_InitialRadius;
		private double m_KineticEnergy;
		private double m_VInCollisionFrame;
		private bool m_ParticleCollided;
		private bool m_WasReflectedByWall;		

		public int TypeID;
		public readonly List<double> VelocityAutoCorrelationFunction;  // this is public for now because of efficiency considerations!!!!! BEWARE!!!!

		#region Properties
		
		public double Mass { get { return m_Mass; } set { m_Mass = value; } }

		public DPVector Position { get { return m_Position; } set { m_Position = value; } }
		public DPVector PositionLast { get { return m_PositionLast; } set { m_PositionLast = value; } }
		public DPVector Velocity { get { return m_Velocity; } set { m_Velocity = value; } }
		public DPVector VelocityLast { get { return m_VelocityLast; } set { m_VelocityLast = value; } }
		public DPVector Force { get { return m_Force; } set { m_Force = value; } }
		public DPVector ForceLast { get { return m_ForceLast; } set { m_ForceLast = value; } }

		public double Radius { get { return m_Radius; } set { m_Radius = value; } }
		public double InitialRadius { get { return m_InitialRadius; } set { m_InitialRadius = value; m_Radius = value; } }
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
			m_Mass = 0.0;
			m_ParticleCollided = false;
			m_Radius = 0.0;
			m_InitialRadius = 0.0;
			m_KineticEnergy = 0.0;
			m_VInCollisionFrame = 0.0;

			m_Position = new DPVector(0, 0);
			m_PositionLast = new DPVector(0, 0);
			m_Velocity = new DPVector(0, 0);
			m_VelocityLast = new DPVector(0, 0);
			m_Force = new DPVector(0, 0);
			m_ForceLast = new DPVector(0, 0);

			VelocityAutoCorrelationFunction = new List<double>(new double[1024]);  // this is the vector that will hold the particle's velocity autocorrelation function  
		}

		public void MarkParticleDidNotCollide()
		{
			m_ParticleCollided = false;
			m_WasReflectedByWall = false;
		}
		
		public void UpdateVelocityAutoCorrelationFunction(){

			VelocityAutoCorrelationFunction.RemoveAt(VelocityAutoCorrelationFunction.Count - 1);																													// pop the last element off the vector
			VelocityAutoCorrelationFunction.Insert(0, Math.Sqrt(m_Velocity.X * m_Velocity.X + m_Velocity.Y * m_Velocity.Y));  // insert the latest modulus of the velocities into the beginning of the vector
			//VelocityAutoCorrelationFunction.insert(VelocityAutoCorrelationFunction.begin(), sqrt(vx * vx + vy * vy));  // insert the latest modulus of the velocities into the beginning of the vector
		}
	}
}