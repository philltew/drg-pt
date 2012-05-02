using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace RugTech1.Framework.Objects
{
	public enum FrustumPlanes { Near = 0, Far = 1, Top = 2, Bottom = 3, Left = 4, Right = 5 }

	public class ViewFrustum
	{
		public Vector3 CameraPosition;
		public Vector3 CameraDirection;
		public Vector3 Up;
		public Vector3 Right;

		public float NearDistance;
		public float FarDistance;

		public Vector2 NearSize;
		public Vector2 FarSize;

		public Vector3 FTL;
		public Vector3 FTR;

		public Vector3 FBL;
		public Vector3 FBR;

		public Vector3 NTL;
		public Vector3 NTR;

		public Vector3 NBL;
		public Vector3 NBR;

		public Plane[] Planes = new Plane[6];

		public void Setup(Camera camera)
		{
			CameraPosition = camera.Center;
			CameraDirection = camera.Forward;
			Up = camera.Up;
			Right = camera.Right;

			NearDistance = 0.1f;
			FarDistance = 80f * camera.Scale;

			float nearHeight = 2f * (float)Math.Tan(camera.FOV / 2f) * NearDistance;
			float nearWidth = nearHeight * camera.AspectRatio;

			NearSize = new Vector2(nearWidth, nearHeight);

			float farHeight = 2f * (float)Math.Tan(camera.FOV / 2f) * FarDistance;
			float farWidth = farHeight * camera.AspectRatio;

			FarSize = new Vector2(farWidth, farHeight);

			Vector3 farCenter = CameraPosition + (CameraDirection * FarDistance);

			// ftl = fc + (up * Hfar/2) - (right * Wfar/2)
			FTL = farCenter + (Up * (farHeight / 2f)) - (Right * (farWidth / 2f));

			// ftr = fc + (up * Hfar/2) + (right * Wfar/2)
			FTR = farCenter + (Up * (farHeight / 2f)) + (Right * (farWidth / 2f));

			// fbl = fc - (up * Hfar/2) - (right * Wfar/2)
			FBL = farCenter - (Up * (farHeight / 2f)) - (Right * (farWidth / 2f));

			// fbr = fc - (up * Hfar/2) + (right * Wfar/2)
			FBR = farCenter - (Up * (farHeight / 2f)) + (Right * (farWidth / 2f));


			Vector3 nearCenter = CameraPosition + (CameraDirection * NearDistance);

			// ftl = fc + (up * Hfar/2) - (right * Wfar/2)
			NTL = nearCenter + (Up * (nearHeight / 2f)) - (Right * (nearWidth / 2f));

			// ftr = fc + (up * Hfar/2) + (right * Wfar/2)
			NTR = nearCenter + (Up * (nearHeight / 2f)) + (Right * (nearWidth / 2f));

			// fbl = fc - (up * Hfar/2) - (right * Wfar/2)
			NBL = nearCenter - (Up * (nearHeight / 2f)) - (Right * (nearWidth / 2f));

			// fbr = fc - (up * Hfar/2) + (right * Wfar/2)
			NBR = nearCenter - (Up * (nearHeight / 2f)) + (Right * (nearWidth / 2f));

			// compute the six planes
			// the function set3Points assumes that the points
			// are given in counter clockwise order
			Planes[(int)FrustumPlanes.Top] = new Plane(NTR, NTL, FTL);
			Planes[(int)FrustumPlanes.Bottom] = new Plane(NBL, NBR, FBR);
			Planes[(int)FrustumPlanes.Left] = new Plane(NTL, NBL, FBL);
			Planes[(int)FrustumPlanes.Right] = new Plane(NBR, NTR, FBR);
			Planes[(int)FrustumPlanes.Near] = new Plane(NTL, NTR, NBR);
			Planes[(int)FrustumPlanes.Far] = new Plane(FTR, FTL, FBL);
		}

		public bool CheckBounds(BoundingBox boundingBox)
		{
			foreach (Plane plane in Planes)
			{
				PlaneIntersectionType intersect = Plane.Intersects(plane, boundingBox);

				if (intersect == PlaneIntersectionType.Front)
				{
					return false;
				}
			}

			return true;
		}
	}
}
