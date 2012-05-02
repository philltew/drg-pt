using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Effects;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace RugTech1.Framework.Objects.Simple
{
	public class BloomChain : IResourceManager
	{
		protected static ImposterEffect Effect;

		private bool m_Disposed = true; 
		private BloomImposter[] m_Imposters;
		private BloomLayer[] m_Layers;
		private Format m_Format; 

		private float m_BloomScale = 1f;
		private float m_BloomAmmout = 4f;
		private int m_HighQualityLayerCount = 0;
		private bool m_UseLowQualityLayers = true;
		private float m_FallOff = 2f; 

		private RenderTargetBackupState m_State = new RenderTargetBackupState();

		private ImposterOverlayType m_OverlayType = ImposterOverlayType.Add;

		public ImposterOverlayType OverlayType { get { return m_OverlayType; } set { m_OverlayType = value; } }

		public float BloomAmmout 
		{ 
			get 
			{ 
				return m_BloomAmmout; 
			} 
			set 
			{ 
				m_BloomAmmout = value;

				m_BloomScale = m_BloomAmmout; //  m_BloomAmmout / (float)m_Layers.Length;
			} 
		}

		public float FallOff 
		{
			get { return m_FallOff; }
			set 
			{
				if (m_FallOff != value)
				{
					m_FallOff = value;

					CaluculateGaussianSlope();
				}
			}
		}
		
		public int HighQualityLayerCount { get { return m_HighQualityLayerCount; } set { m_HighQualityLayerCount = value; } }

		public bool UseLowQualityLayers { get { return m_UseLowQualityLayers; } set { m_UseLowQualityLayers = value; } }

		public BloomChain(int width, int height, Format format, int highQualityLayerCount, bool useLowQualityLayers)
		{
			if (Effect == null)
			{
				Effect = SharedEffects.Effects["Imposter"] as ImposterEffect;
			}

			m_HighQualityLayerCount = highQualityLayerCount;
			m_UseLowQualityLayers = useLowQualityLayers; 

			m_Format = format; 

			List<BloomImposter> imposters = new List<BloomImposter>();
			List<BloomLayer> layers = new List<BloomLayer>(); 

			int w = width, h = height;

			for (int i = 0; i < m_HighQualityLayerCount; i++)
			{
				imposters.Add(new BloomImposter(w, h, format));
				layers.Add(new BloomLayer(w, h, format));
			}

			if (m_UseLowQualityLayers == true)
			{
				while (true)
				{
					imposters.Add(new BloomImposter(w, h, format));
					layers.Add(new BloomLayer(w, h, format));

					w /= 2;
					h /= 2;

					if (w == 0 || h == 0)
					{
						break;
					}
				}
			}

			m_Imposters = imposters.ToArray();
			m_Layers = layers.ToArray();

			m_BloomScale = m_BloomAmmout;// / (float)m_Layers.Length;

			CaluculateGaussianSlope();
		}

		public void Resize(int width, int height)
		{
			UnloadResources(); 

			List<BloomImposter> imposters = new List<BloomImposter>();
			List<BloomLayer> layers = new List<BloomLayer>();

			int w = width, h = height;

			for (int i = 0; i < m_HighQualityLayerCount; i++)
			{
				imposters.Add(new BloomImposter(w, h, m_Format));
				layers.Add(new BloomLayer(w, h, m_Format));
			}

			if (m_UseLowQualityLayers == true)
			{
				while (true)
				{
					imposters.Add(new BloomImposter(w, h, m_Format));
					layers.Add(new BloomLayer(w, h, m_Format));

					w /= 2;
					h /= 2;

					if (w == 0 || h == 0)
					{
						break;
					}
				}
			}

			m_Imposters = imposters.ToArray();
			m_Layers = layers.ToArray();

			m_BloomScale = m_BloomAmmout;// / (float)m_Layers.Length;

			CaluculateGaussianSlope();
		}

		private void CaluculateGaussianSlope()
		{
			double point = (double)0;
			double pointCount = (double)m_Layers.Length;

			foreach (BloomLayer layer in m_Layers)
			{
				// ((e^((x / 10)^2) / 10 * Sqrt(2 * pi)))) * (e / 2) 
				// layer.Intensity = (float)((Math.Exp(Math.Pow(point / pointCount, 2)) / (pointCount * Math.Sqrt(2 * Math.PI))) * Math.E / 2);
				// ((e^((-x + 4.5) / 4.5)) / e) 
				layer.Intensity = (float)(Math.Exp((-point + (pointCount / 2)) / (pointCount / 2)) / Math.E);

				//point -= 1; 
				point += m_FallOff;
			}
		}

		public void Render(RenderTargetView destination, ShaderResourceView source)
		{
			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext;

			try
			{				

				m_State.RenderTargets = context.OutputMerger.GetRenderTargets(1);
				m_State.Viewports = context.Rasterizer.GetViewports();

				RenderTargetView currentDestination = m_Layers[0].View;
				ShaderResourceView currentSource = source;

				for (int i = 0; i < m_Imposters.Length - 1; i++)
				{
					m_Imposters[i].Render(currentDestination, currentSource);
					currentSource = m_Layers[i].ResourceView;
					currentDestination = m_Layers[i + 1].View;
				}

				m_Imposters[m_Imposters.Length - 1].Render(currentDestination, currentSource);
			}
			finally
			{
				context.OutputMerger.SetTargets(m_State.RenderTargets);
				context.Rasterizer.SetViewports(m_State.Viewports);
			}

			foreach (BloomLayer layer in m_Layers)
			{
				Effect.RenderImposter(m_OverlayType, layer.ResourceView, m_BloomScale * layer.Intensity);
			}
		}

		#region IResourceManager Members

		public bool Disposed
		{
			get { return m_Disposed; }
		}

		public void LoadResources()
		{
			if (m_Disposed == true)
			{
				foreach (BloomImposter imposter in m_Imposters)
				{
					imposter.LoadResources(); 
				}

				foreach (BloomLayer layer in m_Layers)
				{
					layer.LoadResources(); 
				}

				m_Disposed = false;
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				foreach (BloomImposter imposter in m_Imposters)
				{
					imposter.UnloadResources();
				}

				foreach (BloomLayer layer in m_Layers)
				{
					layer.UnloadResources();
				}

				m_Disposed = true;
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			UnloadResources(); 
		}

		#endregion				
	}
}
