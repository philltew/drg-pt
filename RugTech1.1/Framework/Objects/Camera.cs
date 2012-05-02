using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace RugTech1.Framework.Objects
{
	public class Camera
	{
		#region Fields
		
		public float Scale;
		public float AspectRatio;
		public float FOV;

		public Vector3 Forward;
		public Vector3 Up;
		public Vector3 Right;

		public float Offset = 4f;
		public float ViewRotation = 0f;
		public float ViewElevation = 0f;

		public Vector3 Center;
		public Vector3 LookAtPoint = new Vector3(0, 0, -0.5f);
		public Quaternion Rotation;
		public Quaternion DestinationRotation;

		public bool SmoothCamera = true;

		public float SmoothSpeed = 0.12f; // 0.5f; // 

		public bool IsFreeCamera = true; //  true;

		public bool HasChanged;


		#endregion

		public Camera(float FOV, float AspectRatio, float scale) // Game game)
		{
			Center = new Vector3(0, 0, 0);
			Rotation = Quaternion.Identity;
			DestinationRotation = Quaternion.Identity; 

			this.Scale = scale; 
			this.FOV = FOV;
			this.AspectRatio = AspectRatio;			

			Rotate(0f, 0f, 0f);
		}

		public void Setup(float FOV, float AspectRatio, float Scale)
		{
			this.Scale = Scale; 
			this.FOV = FOV;
			this.AspectRatio = AspectRatio;						
		}

		#region State

		public CameraState State
		{
			get 
			{
				return new CameraState(this); 
			}

			set
			{
				SetFromState(value); 
			}
		}

		public void SetFromState(CameraState value)
		{
			Scale = value.Scale;
			AspectRatio = value.AspectRatio;
			FOV = value.FOV;
			Forward = value.Forward;
			Up = value.Up;
			Right = value.Right;
			Center = value.Center;
			Rotation = value.Rotation;

			HasChanged = true; 
		}

		public void CopyToState(ref CameraState value)
		{
			value.Scale = Scale;
			value.AspectRatio = AspectRatio;
			value.FOV = FOV;
			value.Forward = Forward;
			value.Up = Up;
			value.Right = Right;
			value.Center = Center;
			value.Rotation = Rotation;
		}

		#endregion

		#region	Move

		public virtual void Translate(Vector3 translate)
		{
			Center += Vector3.TransformCoordinate(translate, Matrix.RotationQuaternion(Rotation));

			//if (IsFreeCamera == false)
			//{
				//Rotation = Quaternion.RotationMatrix(Matrix.LookAtLH(Center, LookAtPoint, new Vector3(0, 1, 0)));

				//SetVector3(ref Right, Vector3.Transform(new Vector3(1, 0, 0), Rotation));
				//SetVector3(ref Up, Vector3.Transform(new Vector3(0, 1, 0), Rotation));
				//SetVector3(ref Forward, Vector3.Transform(new Vector3(0, 0, 1), Rotation));
			//}
		}

		public virtual void Rotate(float yaw, float pitch, float roll)
		{
			//if (IsFreeCamera == false)
			//{
			//	Rotation = Quaternion.RotationMatrix(Matrix.LookAtLH(Center, LookAtPoint, new Vector3(0, 1, 0)));
			//}

			if (SmoothCamera == true)
			{
				if (yaw != 0) DestinationRotation = Quaternion.Multiply(DestinationRotation, Quaternion.RotationAxis(Up, yaw));
				if (pitch != 0) DestinationRotation = Quaternion.Multiply(DestinationRotation, Quaternion.RotationAxis(Right, pitch));
				if (roll != 0) DestinationRotation = Quaternion.Multiply(DestinationRotation, Quaternion.RotationAxis(Forward, roll));

				DestinationRotation.Normalize();
			}
			else
			{ 
				if (yaw != 0) Rotation = Quaternion.Multiply(Rotation, Quaternion.RotationAxis(Up, yaw));
				if (pitch != 0) Rotation = Quaternion.Multiply(Rotation, Quaternion.RotationAxis(Right, pitch));
				if (roll != 0) Rotation = Quaternion.Multiply(Rotation, Quaternion.RotationAxis(Forward, roll));
			
				Rotation.Normalize();
			}
			/*
			if (IsFreeCamera == false)
			{
				SetVector3(ref Up, Vector3.Transform(new Vector3(0, 1, 0), Rotation));

				Rotation = Quaternion.RotationMatrix(Matrix.LookAtLH(Center, LookAtPoint, new Vector3(0, 1, 0)));

				SetVector3(ref Right, Vector3.Transform(new Vector3(1, 0, 0), Rotation));
				SetVector3(ref Up, Vector3.Transform(new Vector3(0, 1, 0), Rotation));
				SetVector3(ref Forward, Vector3.Transform(new Vector3(0, 0, 1), Rotation));
			}
			else
			{*/
			SetRightUpForwardVectors(); 
			//}
		}

		/*
		public Quaternion QuaternionFromMatrix33(Matrix src) 
		{ 
			float trace = 1.0f + src.M11 + src.M22 + src.M33; 

			if (trace > 0.00001f) 
			{ 
				float s = (float)Math.Sqrt(trace) * 2f; 
				return new Quaternion((src.M32 - src.M23) / s, (src.M13 - src.M31) / s, (src.M21 - src.M12) / s, s / 4); 
			} 
			else if (src.M11 > src.M22 && src.M11 > src.M33) 
			{ 
				float s = sqrt(1.0f + src->m00 - src->m11 - src->m22) * 2; 
				return Quaternion(s / 4, (src->m10 + src->m01) / s, (src->m02 + src->m20) / s, (src->m21 - src->m12) / s); 
			} 
			else if (src->m11 > src->m22) 
			{ 
				float s = sqrt(1.0f + src->m11 - src->m00 - src->m22) * 2; 
				return Quaternion((src->m10 + src->m01) / s, s / 4, (src->m21 + src->m12) / s, (src->m02 - src->m20) / s); 
			} 
			else 
			{ 
				const float s = sqrt(1.0f + src->m22 - src->m00 - src->m11) * 2; 
				return Quaternion((src->m02 + src->m20) / s, (src->m21 + src->m12) / s, s / 4, (src->m10 - src->m01) / s); 
			} 
		}
		 * D3DXVECTOR3 direction = targetPos - currentPos;D3DXQUATERNION q;D3DXQuaternionLookRotation( &q, &direction, &D3DXVECTOR3( 0, 1, 0 ) );//// Use this rotation for the displayed actor's world matrix.//D3DXMatrixRotationQuaternion( &pActors->worldMatrix, &q );//// Calculate desired rotation around y-axis//float idealFacing = (float)atan2f(direction.x, direction.z);//// Check this against the actual rotation around y-axis given by// D3DXQuaternionLookRotation().... HOW TO DO THIS???//???
		 * 
		*/
		public virtual void SetRotation(Quaternion newRotation)
		{
			if (SmoothCamera == true)
			{
				DestinationRotation = newRotation;
				DestinationRotation.Normalize();
			}
			else
			{
				Rotation = newRotation;
				Rotation.Normalize();
			}

			/*if (IsFreeCamera == false)
			{
				SetVector3(ref Up, Vector3.Transform(new Vector3(0, 1, 0), Rotation));
				
				Rotation = Quaternion.RotationMatrix(Matrix.LookAtLH(Center, LookAtPoint, new Vector3(0, 1, 0)));

				SetVector3(ref Right, Vector3.Transform(new Vector3(1, 0, 0), Rotation));
				SetVector3(ref Up, Vector3.Transform(new Vector3(0, 1, 0), Rotation));
				SetVector3(ref Forward, Vector3.Transform(new Vector3(0, 0, 1), Rotation));
			}
			else 
			{*/
			SetRightUpForwardVectors(); 
			//}
		}

		public void SetRightUpForwardVectors()
		{
			SetVector3(ref Right, Vector3.Transform(new Vector3(1, 0, 0), Rotation));
			SetVector3(ref Up, Vector3.Transform(new Vector3(0, 1, 0), Rotation));
			SetVector3(ref Forward, Vector3.Transform(new Vector3(0, 0, 1), Rotation));
		}

		private void SetVector3(ref Vector3 vector3, Vector4 vector4)
		{
			vector3.X = vector4.X;
			vector3.Y = vector4.Y;
			vector3.Z = vector4.Z; 
		}

		#endregion

		public virtual void Update()
		{
			if (SmoothCamera == true)
			{
				Rotation = Quaternion.Slerp(Rotation, DestinationRotation, SmoothSpeed * GameEnvironment.FrameDelta);
				//Rotation = Quaternion.Lerp(Rotation, DestinationRotation, SmoothSpeed * GameEnvironment.FrameDelta);
				//Rotation = DestinationRotation;
				//Rotation = SlerpNoInvert(Rotation, DestinationRotation, SmoothSpeed * GameEnvironment.FrameDelta); 
				Rotation.Normalize();
				SetRightUpForwardVectors(); 
			}
		}

		// This version of slerp, used by squad, does not check for theta > 90.
		static Quaternion SlerpNoInvert(Quaternion q1, Quaternion q2, float t) 
		{
			float dot = Quaternion.Dot(q1, q2);

			if (dot > -0.95f && dot < 0.95f)
			{
				float angle = (float)Math.Acos(dot);
				return (q1 * (float)Math.Sin(angle * (1 - t)) + q2 * (float)Math.Sin(angle * t)) / (float)Math.Sin(angle);
			}
			else
			{
				// if the angle is small, use linear interpolation								
				return Quaternion.Lerp(q1, q2, t);
			}
		}
	}

	public struct CameraState : IEquatable<CameraState>, IEquatable<Camera>
	{
		public float Scale;
		public float AspectRatio;
		public float FOV;

		public Vector3 Forward;
		public Vector3 Up;
		public Vector3 Right;

		public Vector3 Center;
		public Quaternion Rotation;

		public CameraState(Camera camera)
		{
			Scale = camera.Scale;
			AspectRatio = camera.AspectRatio;
			FOV = camera.FOV;
			Forward = camera.Forward;
			Up = camera.Up;
			Right = camera.Right;
			Center = camera.Center;
			Rotation = camera.Rotation; 			
		}

		public override bool Equals(object obj)
		{
			if (obj is CameraState)
			{
				return Equals((CameraState)obj); 
			}
			else if (obj is Camera)
			{
				return Equals(obj as Camera);
			}
			else
			{
				return false;
			}
		}

		public bool Equals(CameraState state)
		{
			if (Scale == state.Scale &&
				AspectRatio == state.AspectRatio &&
				FOV == state.FOV &&
				Forward == state.Forward &&
				Up == state.Up &&
				Right == state.Right &&
				Center == state.Center &&
				Rotation == state.Rotation)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool Equals(Camera camera)
		{
			if (Scale == camera.Scale &&
				AspectRatio == camera.AspectRatio &&
				FOV == camera.FOV &&
				Forward == camera.Forward &&
				Up == camera.Up &&
				Right == camera.Right &&
				Center == camera.Center &&
				Rotation == camera.Rotation)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
