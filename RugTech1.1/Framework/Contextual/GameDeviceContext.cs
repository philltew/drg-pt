using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.DXGI;

namespace RugTech1.Framework.Contextual
{
	public class GameDeviceContext : IDisposable 
	{        
		#region Implementation Detail

		private SlimDX.DXGI.Factory m_Factory;
        private SlimDX.Direct3D11.Device m_Device;

        #endregion

        public SlimDX.DXGI.Factory Factory { get { return m_Factory; } }

        public SlimDX.Direct3D11.Device Device { get { return m_Device; } }

		#region Public Interface

        internal GameDeviceContext(AppContext context) 
		{
			SwapChain defaultSwapChain;

			SlimDX.Direct3D11.Device.CreateWithSwapChain(SlimDX.Direct3D11.DriverType.Hardware, GameConfiguration.CreationFlags, context[0].SwapChainDescription, out m_Device, out defaultSwapChain);
			
			m_Factory = m_Device.Factory;

			int numberOfAdditionalWindows = m_Factory.GetAdapter(GameConfiguration.AdapterOrdinal).GetOutputCount() - 1;

			if (numberOfAdditionalWindows > 0)
			{
				context.CreateAdditionalForms(numberOfAdditionalWindows);
			}

			context[0].SwapChain = defaultSwapChain; 

			m_Factory.SetWindowAssociation(context[0].Form.Handle, WindowAssociationFlags.IgnoreAll | WindowAssociationFlags.IgnoreAltEnter);
			
			for (int i = 1; i < context.Count; i++)
			{				
				context[i].SwapChain = new SlimDX.DXGI.SwapChain(m_Factory, m_Device, context[i].SwapChainDescription);

				m_Device.Factory.SetWindowAssociation(context[i].Form.Handle, WindowAssociationFlags.IgnoreAll | WindowAssociationFlags.IgnoreAltEnter);
			}

			for (int i = 0; i < context.Count; i++)
			{
				context[i].Form.Location = m_Factory.GetAdapter(GameConfiguration.AdapterOrdinal).GetOutput(i).Description.DesktopBounds.Location; 
			}
        }

		~GameDeviceContext()
		{
            Dispose(false);
        }

        public void Dispose() 
		{
            Dispose(true);
        
			GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposeManagedResources) 
		{
            if (disposeManagedResources) 
			{
				try
				{
					m_Device.Dispose();
				}
				catch { }

				try
				{
					m_Factory.Dispose();
				}
				catch { }
            }
        }

        #endregion
	}
}
