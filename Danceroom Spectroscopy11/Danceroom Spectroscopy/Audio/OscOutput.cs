using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ventuz.OSC;
using SlimDX;

namespace DS.Audio
{
	class OscOutput
	{
		#region Private Members
		
		private UdpWriter OSCWriter = null;
		private bool m_Connected = false;
		private string m_Message;
		private int m_PacketCount;
		private int m_CollisionPacketCount;
		private OscEventRecorder m_ColisionEventRecorder;
		private OscEventRecorder m_FFTDataRecorder;
		private int m_EventTimeStamp = 0; 

		#endregion

		#region Public Properties and Events
		
		public event EventHandler ConnectionChanged; 
		
		public bool Connected
		{
			get { return m_Connected; }
		}
	
		public string Address
		{
			get { return ArtworkStaticObjects.Options.Osc.Address; }
			set { ArtworkStaticObjects.Options.Osc.Address = value; }
		}

		public int Port
		{
			get { return ArtworkStaticObjects.Options.Osc.Port; }
			set { ArtworkStaticObjects.Options.Osc.Port = value; }
		}

		public string Message
		{
			get { return m_Message; }
		}

		public int PacketCount
		{
			get { return m_PacketCount; }
		}

		public OscEventRecorder ColisionEventRecorder
		{
			get { return m_ColisionEventRecorder; }
			set { m_ColisionEventRecorder = value; }
		}

		public OscEventRecorder FFTDataRecorder
		{
			get { return m_FFTDataRecorder; }
			set { m_FFTDataRecorder = value; }
		} 

		#endregion

		public OscOutput()
		{

		}

		#region Connection Handeling

		private void OnConnectionChanged()
		{
			if (ConnectionChanged != null)
			{
				ConnectionChanged(this, EventArgs.Empty); 
			}
		}

		public void Connect()
		{
			if (m_Connected == false)
			{
				m_Message = "Connecting"; 

				if (String.IsNullOrWhiteSpace(Address))
				{
					m_Message = "Destination IP address has not been supplied";
					return; 
				}
				
				if (Port <= 0)
				{
					m_Message = "Destination Port is invalid";
					return; 
				}

				try 
				{
					OSCWriter = new UdpWriter(Address, Port);

					m_Connected = true; 

					m_Message = "Connected to '" + Address + ":" + Port + "'";
					
					OnConnectionChanged();
				}
				catch (Exception ex)
				{
					m_Message = "Error: " + ex.Message;
				}
			}
		}

		public void Disconnect()
		{
			if (m_Connected == true)
			{
				m_Message = "Disconnecting";

				try
				{
					OSCWriter.Dispose();					

					m_Message = "Disconnected"; 
				}
				catch (Exception ex)
				{
					m_Message = "Error: " + ex.Message;
				}

				OSCWriter = null; 

				m_Connected = false;

				OnConnectionChanged(); 
			}
		}

		public void Reconnect()
		{
			Disconnect(); 
			Connect(); 
		}

		#endregion

		#region Packet Handeling
		
		public void ResetPacketCount()
		{
			m_PacketCount = 0; 
			m_CollisionPacketCount = 0;
		}

		public void SendPacket(float x, float y, float type, float speed)
		{
			// packet filtering to go here

			if (m_CollisionPacketCount > 100)
			{
				return;
			}


			m_PacketCount++;
			m_CollisionPacketCount++;

			if (m_Connected == true)
			{
				OscElement message = new OscElement("/c", x, y, type, speed);

				OSCWriter.Send(message); 
			}

			if (ColisionEventRecorder != null)
			{
				ColisionEventRecorder.WriteEvent(m_EventTimeStamp, x, y, type, speed);
			}
		}

		public void SendPacket(Vector4 data)
		{
			// packet filtering to go here
			if (m_CollisionPacketCount > 100)
			{
				return;
			}

			m_PacketCount++;

			if (m_Connected == true)
			{			
				OscElement message = new OscElement("/c", data.X, data.Y, data.Z, data.W);

				OSCWriter.Send(message);
			}

			if (ColisionEventRecorder != null)
			{
				ColisionEventRecorder.WriteEvent(m_EventTimeStamp, data.X, data.Y, data.Z, data.W);
			}
		}

		object[] temp; 

		public void SendFFTData(float[] peeksFreqAndIntensity)
		{
			if (FFTDataRecorder != null)
			{
				FFTDataRecorder.WriteEvent(m_EventTimeStamp, peeksFreqAndIntensity);
				m_EventTimeStamp++; 
			}

			m_PacketCount++;

			if (m_Connected == true)
			{
				if (temp == null || temp.Length != peeksFreqAndIntensity.Length)
				{
					temp = new object[peeksFreqAndIntensity.Length]; 
				}

				for (int i = 0; i < peeksFreqAndIntensity.Length; i++)
				{
					temp[i] = peeksFreqAndIntensity[i]; 
				}

				OscElement message = new OscElement("/fft", temp);

				OSCWriter.Send(message);
			}
		}
		
		/*
		public void SendPackets(Vector4[] data, int count, Matrix view)
		{
			if (m_Connected == true)
			{
				int portalID = ArtworkStaticObjects.Options.Osc.PortalID;
				float threshold = ArtworkStaticObjects.Options.Osc.SpeedThreshold;
				float maxDist = ArtworkStaticObjects.Options.Osc.DistanceThreshold; 

				for (int i = 0; i < count; i++)
				{
					Vector4 d = data[i]; 

					if (d.W > threshold)
					{
						Vector3 vec = new Vector3(d.X, d.Y, d.Z);

						vec = Vector3.TransformCoordinate(vec, view);

						float dist = vec.Length();

						if (dist < maxDist)
						{
							OSCWriter.Send(new OscElement("/object", portalID, vec.X, vec.Y, vec.Z, d.W));
							m_PacketCount++;
						}
					}
				}
			}
		}
		*/ 

		#endregion
	}
}
