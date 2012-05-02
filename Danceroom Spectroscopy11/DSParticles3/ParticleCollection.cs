using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticles3
{
	public class ParticleCollection : IEnumerable<Particle>, System.Collections.IEnumerable // : ICollection<Particle>
	{
		private int m_Count;
		private int m_Maximum;
		private ParticleEnsemble m_Owner;
		private Particle[] m_Particles; 

		public ParticleEnsemble Owner { get { return m_Owner; } } 
		public int Count { get { return m_Count; } } 
		public int Maximum { get { return m_Maximum; } }
		public bool IsReadOnly { get { return false; } }

		public Particle this[int index]
		{
			get 
			{
				if (index >= m_Maximum)
				{
					throw new IndexOutOfRangeException(string.Format("The index exceded the maximum possible for this particle ensemble {{0}}", m_Maximum));
				}
				else if (index >= m_Count)
				{
					throw new IndexOutOfRangeException(string.Format("The index exceded the number of valid particles for this particle ensemble {{0}}", m_Count));
				}

				return m_Particles[index];
			}			
		}

		public ParticleCollection(ParticleEnsemble owner, int maximum)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("owner", "Owner must be supplied");
			}

			if (maximum <= 0)
			{
				throw new ArgumentException("Maximum must be 1 or greater", "maximum"); 
			}

			m_Count = 0;
			m_Maximum = maximum;
			m_Owner = owner;
			m_Particles = new Particle[maximum];
		}		

		public void Add(Particle item)
		{
			if (m_Count >= m_Maximum)
			{
				throw new Exception("Particle collection allready has the maximum allowable particles");
			}

			m_Particles[m_Count++] = item; 
		}

		public Particle Remove(int index)
		{
			if (index < 0)
			{
				throw new IndexOutOfRangeException(string.Format("Index must be 0 or greater", m_Maximum));
			}
			else if (index >= m_Maximum)
			{
				throw new IndexOutOfRangeException(string.Format("The index exceded the maximum possible for this particle ensemble {{0}}", m_Maximum));
			}
			else if (index >= m_Count)
			{
				throw new IndexOutOfRangeException(string.Format("The index exceded the number of valid particles for this particle ensemble {{0}}", m_Count));
			}

			Particle particle = m_Particles[index];

			m_Particles[index] = null;

			if (index < m_Count - 1)
			{				
				for (int i = index + 1, ie = m_Count; i < ie; i++)
				{
					m_Particles[i - 1] = m_Particles[i];
				}
			}

			m_Count--; 

			return particle; 
		}

		public Particle Pop()
		{
			Particle particle = m_Particles[m_Count - 1];

			m_Particles[m_Count - 1] = null;

			m_Count--;

			return particle; 
		}

		public void Clear()
		{
			m_Count = 0; 
		}

		public bool Contains(Particle item)
		{
			foreach (Particle part in m_Particles)
			{
				if (part == item)
				{
					return true; 
				}
			}

			return false; 
		}

		#region IEnumerable<Particle> Members

		public IEnumerator<Particle> GetEnumerator()
		{
			return (m_Particles as IEnumerable<Particle>).GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return m_Particles.GetEnumerator();
		}

		#endregion
	}
}
