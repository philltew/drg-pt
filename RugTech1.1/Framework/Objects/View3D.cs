using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;

namespace RugTech1.Framework.Objects
{
	public class View3D : View2D
	{
		//public Viewport Viewport;
		
		public Camera Camera;
		public ViewFrustum Frustum; 

		public Matrix View;
		public Matrix ViewCenter;
		public Matrix Projection;
		public Matrix World;


		public View3D(System.Drawing.Rectangle activeRegion, int windowWidth, int windowHeight, float FOV, float Scale) 
			: base(activeRegion, windowWidth, windowHeight)
		{
			Camera = new Camera(FOV, (float)activeRegion.Width / (float)activeRegion.Height, Scale);
			Frustum = new ViewFrustum(); 

			UpdateProjection();			
		}

		public override void Resize(System.Drawing.Rectangle activeRegion, int windowWidth, int windowHeight)
		{
			base.Resize(activeRegion, windowWidth, windowHeight);

			Camera.Setup(Camera.FOV, (float)activeRegion.Width / (float)activeRegion.Height, Camera.Scale); 

			UpdateProjection(); 
		}

		/*
		Quaternion angleBetween(Vector3 v1, Vector3 v2)
		{
			float d = Vector3.Dot(v1, v2);
			Vector3 axis = v1;
			axis = Vector3.Cross(axis, v2);

			float qw = (float)Math.Sqrt(v1.LengthSquared() * v2.LengthSquared()) + d;
			
			if (qw < 0.0001)
			{ 
				// vectors are 180 degrees apart
				Quaternion quat = new Quaternion(-v1.Z, v1.Y, v1.X, 0);

				quat.Normalize();
				
				return quat; 
			}
			Quaternion q = new Quaternion(axis.X, axis.Y, axis.Z, qw);
			q.Normalize();

			return q; 
		} 

		
		void setTarget(Vector3 target)
		{
			Vector3 projectedTarget;

			target = target - Camera.Center;
			projectedTarget = target;

			if (Math.Abs(target[0]) < 0.00001f && Math.Abs(target[2]) < 0.00001f)
			{  
				// YZ plane     
				projectedTarget[0] = 0.0f;
				projectedTarget.Normalize();

				Camera.Right = new Vector3(1.0f, 0.0f, 0.0f);
				Camera.Up = Vector3.Cross(projectedTarget, Camera.Right);
             
				//m_target = target;
				Camera.Forward = target;
				Camera.Forward.Normalize();
				Camera.Right = -Vector3.Cross(Camera.Forward, Camera.Up);
			}         
			else 
			{                                      
				// XZ plane             
				projectedTarget[1] = 0.0f;
				projectedTarget.Normalize();

				Camera.Up = new Vector3(0.0f, 1.0f, 0.0f);
				Camera.Right = -Vector3.Cross(projectedTarget, Camera.Up);

				Camera.Forward = target;
				Camera.Forward.Normalize();
				Camera.Up = Vector3.Cross(Camera.Forward, Camera.Right);
			}

			Camera.Forward.Normalize();
			Camera.Right.Normalize();
			Camera.Up.Normalize();
		}
		*/ 

		/* 
		Quaternion LookAt(Vector3 target, Vector3 current, Vector3 eye, Vector3 up) 
		{ 
			// turn vectors into unit vectors 
			Vector3 n1 = (current - eye); // .norm();
			Vector3 n2 = (target - eye); // .norm();  
			n1.Normalize(); 
			n2.Normalize(); 

			float d = Vector3.Dot(n1, n2); 
			// if no noticable rotation is available return zero rotation
			// this way we avoid Cross product artifacts 
			if(d > 0.9998f) return new Quaternion(0, 1, 0, 0); 
			// in this case there are 2 lines on the same axis 
			if(d < -0.9998){ 
				n1 = n1.Rotx(0.5f); 
				// there are an infinite number of normals 
				// in this case. Anyone of these normals will be 
				// a valid rotation (180 degrees). so rotate the curr axis by 0.5 radians this way we get one of these normals 
			} 
			Vector3 axis = n1;
			axis = Vector3.Cross(axis, n2);
			Quaternion pointToTarget = new Quaternion(axis.X, axis.Y, axis.Z, 1.0f + d); 
			pointToTarget.Normalize();
			// now twist around the target vector, so that the 'up' vector points along the z axis
			Matrix projectionMatrix=new Matrix();
			float a = pointToTarget.X;
			float b = pointToTarget.Y;
			float c = pointToTarget.Z;
			projectionMatrix.M11 = b*b+c*c;
			projectionMatrix.M12 = -a*b;
			projectionMatrix.M13 = -a*c;
			projectionMatrix.M21 = -b*a;
			projectionMatrix.M22 = a*a+c*c;
			projectionMatrix.M23 = -b*c;
			projectionMatrix.M31 = -c*a;
			projectionMatrix.M32 = -c*b;
			projectionMatrix.M33 = a*a+b*b;
			Vector4 upProjected = Vector3.Transform(up,  projectionMatrix);
			Vector4 yaxisProjected = Vector3.Transform(new Vector3(0,1,0), projectionMatrix);
			d = Vector4.Dot(upProjected,yaxisProjected);
			
			// so the axis of twist is n2 and the angle is arcos(d)
			//convert this to quat as follows   
			float s = (float)Math.Sqrt(1.0 - d*d);
			
			Quaternion twist = new Quaternion(n2*s, n2*s, n2*s, d);

			return Quaternion.Multiply(pointToTarget, twist);
		} 
		*/

		public void LookAt(Vector3 target)
		{
			Vector3 dir = Camera.Center-target;
			dir.Normalize();
			Vector3 right = new Vector3(dir.Z, 0, -dir.X);
			right.Normalize();
			Vector3 up = Vector3.Cross(dir, right);
			// m_camera->setOrientation(Ogre::Quaternion(right,up,dir));
			//Quaternion newRot = Quaternion.
			
			//Camera.SetRotation(Quaternion.Slerp(Camera.Rotation, newRot, 1f * GameEnvironment.FrameDelta));

			/*
			// Make sure the view matrix is up to date
			//Update();

			// Create vector between camera an target
			Vector3 targetLookVector = target - Camera.Center;
			targetLookVector.Normalize();

			// Create a vector between camera and current look vector
			// TODO: Matrix indices may need swapping (M31 -> M13)
			Vector3 currentLookVector = -Camera.Forward;

			currentLookVector.Normalize();

			// Calculate the dot product
			float dot = Vector3.Dot(currentLookVector, targetLookVector);

			// If the dot is almost at 1 do nothing
			if (dot > 0.9998) return;

			// Calculate the angle
			float angle = (float)Math.Acos(dot);

			// Calculate the cross product
			Vector3 cross = Vector3.Cross(currentLookVector, targetLookVector);

			// Rotate about the cross
			//m_orientation.RotateAxis(cross, angle);

			Quaternion newRot = Quaternion.RotationAxis(cross, angle); 
			
			Camera.SetRotation(Quaternion.Slerp(Camera.Rotation, newRot, 1f * GameEnvironment.FrameDelta));

			// Flag for update
			//m_bNeedsUpdate = true;
			 */ 
		}

		public virtual void UpdateProjection()
		{
			//Projection = Matrix.PerspectiveFovLH(Camera.FOV, Camera.AspectRatio, 0.00001f, 50000.0f);

			Projection = Matrix.PerspectiveFovLH(Camera.FOV, Camera.AspectRatio, 0.1f, 500.0f);

			if (Camera.IsFreeCamera)
			{
				Matrix matRot = Matrix.RotationQuaternion(Quaternion.Conjugate(Camera.Rotation));

				ViewCenter = (Matrix.Translation(Camera.Center) * matRot);

				Vector3 fwd = Vector3.TransformCoordinate(new Vector3(0, 0, 1), matRot);
				//Vector3 right = Vector3.TransformCoordinate(new Vector3(1, 0, 0), matRot);
 
				//View = (Matrix.RotationZ(Camera.ViewElevation) * Matrix.RotationY(Camera.ViewRotation) * Matrix.Translation(Camera.Forward * Camera.Offset)) * ViewCenter;
				//View = Matrix.RotationX(Camera.ViewElevation) * (Matrix.Translation(Camera.Forward * Camera.Offset)) * ViewCenter;
				//Vector3 projectFwd = Vector3.TransformCoordinate(Camera.Forward, Matrix.RotationX(Camera.ViewElevation) * Matrix.Invert(matRot));
				//View = (Matrix.Translation(fwd * Camera.Offset)) * (Matrix.Translation(Camera.Center) * ((matRot) * Matrix.RotationAxis(right, Camera.ViewElevation)));
				
				
				//View = Matrix.Translation(Camera.Forward * Camera.Offset) * (Matrix.Translation(Camera.Center) * matRot);
				// THIS IS THE REAL ONE! 
				//View = Matrix.Translation(Camera.Forward * Camera.Offset) * (Matrix.Translation(Camera.Center) * Matrix.RotationQuaternion(Quaternion.Conjugate(Camera.Rotation)));
				float offset = Camera.Offset; 
				if (Camera.ViewRotation != 0)
				{
					offset *= -1f; 
				}

				View = Matrix.Translation(Camera.Forward * offset) * (Matrix.Translation(Camera.Center) * (Matrix.RotationQuaternion(Quaternion.Conjugate(Camera.Rotation)) * Matrix.RotationY(Camera.ViewRotation) * Matrix.RotationX(Camera.ViewElevation)));
			}
			else
			{
				/*Vector3 zaxis = Camera.LookAtPoint - Camera.Center; 
				zaxis.Normalize(); 
				Vector3 xaxis = Vector3.Cross(new Vector3(0, 1, 0), zaxis);
				xaxis.Normalize();
				Vector3 yaxis = Vector3.Cross(zaxis, xaxis);

				Matrix mat = new Matrix() { 
					M11 = xaxis.X, 
					M12 = yaxis.X,
					M13 = zaxis.X, 
					M14 = 0, 
					M21 = xaxis.Y, 
					M22 = yaxis.Y,
					M23 = zaxis.Y, 
					M24 = 0, 
					M31 = xaxis.Z, 
					M32 = yaxis.Z,
					M33 = zaxis.Z, 
					M34 = 0, 
					M41 = 0, 
					M42 = 0,
					M43 = 0, 
					M44 = 1,
				}; 

				//Quaternion.RotationMatrix(mat)

				Camera.SetRotation(Quaternion.Slerp(Camera.Rotation, Quaternion.RotationMatrix(mat), 0.4f * GameEnvironment.FrameDelta));
				*/
				//Vector3 zaxis = Camera.LookAtPoint - Camera.Center; 
				//zaxis.Normalize(); 

				//float angle = (float)Math.Acos((double)Vector3.Dot(zaxis, -Camera.Forward));

				//Vector3 axis = Vector3.Cross(zaxis, Camera.Forward);
				//axis.Normalize();

				//angleBetween(Camera.LookAtPoint - Camera.Center, -Camera.Forward);
				//Vector3 fwd = new Vector3(-Camera.Forward.X,  Camera.Forward.Y,  Camera.Forward.Z * -1f);

				//Camera.SetRotation(Quaternion.Slerp(Camera.Rotation, angleBetween(Camera.LookAtPoint - Camera.Center, fwd), 0.4f * GameEnvironment.FrameDelta));
				//Matrix InverseRot = Matrix.RotationQuaternion(Camera.Rotation); 
				//InverseRot.Invert();
				//Vector3 lookVector = Camera.LookAtPoint - Camera.Center; 
				//lookVector.Normalize();
				//Vector4 diff = Vector3.Transform(lookVector, InverseRot); 
				//Vector3 diff2 = new Vector3(diff.X, diff.Y, diff.Z);
				
				//Camera.SetRotation(Quaternion.Slerp(Camera.Rotation, Camera.Rotation - angleBetween(diff2, new Vector3(0, 0, -1)), 0.4f * GameEnvironment.FrameDelta));

				LookAt(Camera.LookAtPoint);

				View = Matrix.Translation(Camera.Center) * Matrix.RotationQuaternion(Quaternion.Conjugate(Camera.Rotation)); // Matrix.LookAtRH(Camera.Center, Camera.LookAtPoint, Camera.Up);				
				//View = Matrix.LookAtLH(Camera.Center, Camera.LookAtPoint, new Vector3(0, 1, 0));

				//setTarget(Camera.LookAtPoint); 

				//Camera.Forward = Camera.Center - Camera.LookAtPoint;
				//Camera.Forward.Normalize();
				//Camera.Right = -Vector3.Cross(Camera.Forward, new Vector3(0, 1, 0));
				//Camera.Right.Normalize(); 
			}

			World = Matrix.Identity; 

			Frustum.Setup(Camera); 
		}

		public Vector2 TransformMouseCoords(System.Windows.Forms.Form form, System.Windows.Forms.MouseEventArgs e)
		{
			if ((form == null) || 
				(form.ClientSize.Width == Viewport.Width && form.ClientSize.Height == Viewport.Height))
			{
				return new Vector2(e.X, e.Y);
			}

			return new Vector2(e.X, e.Y);

			/* 
			else
			{
				float xScale = (1f / base.Viewport.Width) * (float)form.ClientSize.Width;
				float yScale = (1f / base.Viewport.Height) * (float)form.ClientSize.Height;

				return new Vector2((float)e.X * xScale, (float)e.Y * yScale);
			}
			*/
		}
	}
}
