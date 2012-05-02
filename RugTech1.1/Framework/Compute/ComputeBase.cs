using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rug.Cmd;
using RugTech1.Framework.Objects;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;

namespace RugTech1.Framework.Compute
{
	public abstract class ComputeBase : IResourceManager
	{
		private SlimDX.D3DCompiler.ShaderBytecode m_Bytecode;

		public abstract string ShaderLocation { get; }

		public abstract string EntryPoint { get; }

		public SlimDX.D3DCompiler.ShaderBytecode Bytecode
		{
			get
			{
				if (m_Bytecode == null)
				{
					string compilationErrors;
					//m_Bytecode = SlimDX.D3DCompiler.ShaderBytecode.CompileFromFile(Helper.ResolvePath(ShaderLocation), "fx_4_0", SlimDX.D3DCompiler.ShaderFlags.EnableStrictness, SlimDX.D3DCompiler.EffectFlags.None);
					//m_Bytecode = SlimDX.D3DCompiler.ShaderBytecode.CompileFromFile(Helper.ResolvePath(ShaderLocation), "cs_4_0", SlimDX.D3DCompiler.ShaderFlags.None, SlimDX.D3DCompiler.EffectFlags.None, null, null, out compilationErrors);
					m_Bytecode = ShaderBytecode.CompileFromFile(Helper.ResolvePath(ShaderLocation), EntryPoint, "cs_4_0", ShaderFlags.None, EffectFlags.None, null, null, out compilationErrors);

					if (Helper.IsNotNullOrEmpty(compilationErrors) == true)
					{
						RC.WriteLine(ConsoleVerbosity.Debug, ConsoleThemeColor.ErrorColor1, "Errors compiling shader: " + ShaderLocation);
						RC.WriteLine(ConsoleVerbosity.Debug, ConsoleThemeColor.ErrorColor2, compilationErrors);
					}
				}

				return m_Bytecode;
			}
		}


		#region IResourceManager Members

		public abstract bool Disposed { get; }

		public abstract void LoadResources();

		public abstract void UnloadResources();

		#endregion

		#region IDisposable Members

		public virtual void Dispose()
		{
			if (m_Bytecode != null && m_Bytecode.Disposed == false)
			{
				m_Bytecode.Dispose();
				m_Bytecode = null;
			}
		}

		#endregion

		/* 
		const int elementCount = 16;
        const int bufferSizeInBytes = elementCount * sizeof(float);

        D3D.Device device = new D3D.Device(D3D.DriverType.Hardware, D3D.DeviceCreationFlags.Debug);

        // The input to the computation will be a constant buffer containing
        // integers (in floating point representation) from 1 to numberOfElements,
        // inclusive. The compute shader itself will double these values and write
        // them to the output buffer.
        D3D.BufferDescription inputBufferDescription = new D3D.BufferDescription
        {
            BindFlags = D3D.BindFlags.ConstantBuffer,
            CpuAccessFlags = D3D.CpuAccessFlags.Write,
            OptionFlags = D3D.ResourceOptionFlags.None,
            SizeInBytes = bufferSizeInBytes,
            StructureByteStride = sizeof(float),
            Usage = D3D.ResourceUsage.Dynamic,
        };
        D3D.Buffer inputBuffer = new D3D.Buffer(device, inputBufferDescription);
        DataBox input = device.ImmediateContext.MapSubresource(inputBuffer, D3D.MapMode.WriteDiscard, D3D.MapFlags.None);
		Console.Write("Input:  ");
		for (int value = 1; value <= elementCount; ++value)
		{
			float v = 32 - (float)value;
			input.Data.Write(v);
			Console.Write(" {0}", v);
		}

		Console.WriteLine();

        device.ImmediateContext.UnmapSubresource(inputBuffer, 0);

        // A staging buffer is used to copy data between the CPU and GPU; the output
        // buffer (which gets the computation results) cannot be mapped directly.
        D3D.BufferDescription stagingBufferDescription = new D3D.BufferDescription
        {
            BindFlags = D3D.BindFlags.None,
            CpuAccessFlags = D3D.CpuAccessFlags.Read,
            OptionFlags = D3D.ResourceOptionFlags.StructuredBuffer,
            SizeInBytes = bufferSizeInBytes,
            StructureByteStride = sizeof(float),
            Usage = D3D.ResourceUsage.Staging,
        };
        D3D.Buffer stagingBuffer = new D3D.Buffer(device, stagingBufferDescription);

        // The output buffer itself, and the view required to bind it to the pipeline.
        D3D.BufferDescription outputBufferDescription = new D3D.BufferDescription
        {
            BindFlags = D3D.BindFlags.UnorderedAccess | D3D.BindFlags.ShaderResource,
            OptionFlags = D3D.ResourceOptionFlags.StructuredBuffer,
            SizeInBytes = bufferSizeInBytes,
            StructureByteStride = sizeof(float),
            Usage = D3D.ResourceUsage.Default,
        };
        D3D.Buffer outputBuffer = new D3D.Buffer(device, outputBufferDescription);
        D3D.UnorderedAccessViewDescription outputViewDescription = new D3D.UnorderedAccessViewDescription
        {
            ElementCount = elementCount,
            Format = DXGI.Format.Unknown,
            Dimension = D3D.UnorderedAccessViewDimension.Buffer
        };
        D3D.UnorderedAccessView outputView = new D3D.UnorderedAccessView(device, outputBuffer, outputViewDescription);

        // Compile the shader.
        ShaderBytecode computeShaderCode = ShaderBytecode.CompileFromFile("BasicComputeShader.hlsl", "main", "cs_4_0", ShaderFlags.None, EffectFlags.None);
        D3D.ComputeShader computeShader = new D3D.ComputeShader(device, computeShaderCode);

        device.ImmediateContext.ComputeShader.Set(computeShader);
        device.ImmediateContext.ComputeShader.SetUnorderedAccessView(outputView, 0);
        device.ImmediateContext.ComputeShader.SetConstantBuffer(inputBuffer, 0);

        // Compute shaders execute on multiple threads at the same time. Those execution
        // threads are grouped; Dispatch() indicates how many groups in the X, Y and Z
        // dimension will be utilized. The shader itself specified how many threads per
        // group (also in X, Y and Z dimensions) to use via the [numthreads] attribute.
        // In this sample, one thread group will be used with 16 threads, each thread
        // will process one element of the input data.
        device.ImmediateContext.Dispatch(1, 1, 1);

        device.ImmediateContext.CopyResource(outputBuffer, stagingBuffer);
        DataBox output = device.ImmediateContext.MapSubresource(stagingBuffer, D3D.MapMode.Read, D3D.MapFlags.None);

        Console.Write("Results:");
        for (int index = 0; index < elementCount; ++index)
            Console.Write(" {0}", output.Data.Read<float>());
        device.ImmediateContext.UnmapSubresource(outputBuffer, 0);
        Console.WriteLine();
		Console.ReadKey(true); 

        computeShader.Dispose();
        outputView.Dispose();
        outputBuffer.Dispose();
        stagingBuffer.Dispose();
        inputBuffer.Dispose();
        device.Dispose();
		*/
		
	}
}
