using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Data;
using RugTech1.Framework.Objects.Simple;
using SlimDX;
using SlimDX.Direct3D11;

namespace RugTech1.Framework.Test
{
	public class TestParticles : ParticleRender
	{
		public TestParticles() :
			base(@"C:\Code\CSharp\RugProjects\Play\ArtAppBase\ArtAppBase\star4.png", 3000)
		{
			this.ColorScale = 4f;
			this.MaxDistance = 400f;
			this.MaxDistance = 100f;
			this.ParticleScale = 0.5f;
			this.ScaleDistance = 1f; 
		}

		public override void Update(Objects.View3D view)
		{
			base.Update(view);

			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext; 

			//DataStream stream = Instances.Map(MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None);

			DataBox box = context.MapSubresource(Instances, MapMode.WriteDiscard, MapFlags.None);
			DataStream stream = box.Data; 

			Random rand = new Random();

			float Dist = 300f;
			float HalfDist = Dist * 0.5f;

			for (int i = 0; i < MaxCount; i++)
			{
				stream.Write(new StarInstanceVertex()
				{
					Color = new Vector4((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()),
					Position = new Vector3((float)(rand.NextDouble() * Dist) - HalfDist,
											(float)(rand.NextDouble() * Dist) - HalfDist,
											(float)(rand.NextDouble() * Dist) - HalfDist)
				});

			}

			this.InstanceCount = MaxCount; 

			//Instances.Unmap(); 
			context.UnmapSubresource(Instances, 0); 
		}
	}
}
