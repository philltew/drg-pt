float4x4 view;
float4x4 proj;
float4x4 world;

float3 locationColor;

Texture2D rayStart_texture;
Texture2D rayDir_texture;
Texture2D imposter; 
Texture3D volume_texture;

struct VS_IN
{
	float3 pos : POSITION;	
	float3 col : COLOR;
};

struct PS_IN
{
	float4 pos : SV_POSITION;	
	float3 col : COLOR;
};

RasterizerState rsBackCull
{	
	FILLMODE = SOLID;
	CULLMODE = BACK; 
	FRONTCOUNTERCLOCKWISE = TRUE;
	DEPTHBIAS = 0;
	DEPTHBIASCLAMP = 0;
	SLOPESCALEDDEPTHBIAS = 0;
	DepthCLIPENABLE = FALSE;
	SCISSORENABLE = FALSE;
	MULTISAMPLEENABLE = FALSE;
	ANTIALIASEDLINEENABLE = FALSE;
};

RasterizerState rsFrontCull
{	
	FILLMODE = SOLID;
	CULLMODE = FRONT; 
	FRONTCOUNTERCLOCKWISE = TRUE;
	DEPTHBIAS = 0;
	DEPTHBIASCLAMP = 0;
	SLOPESCALEDDEPTHBIAS = 0;
	DepthCLIPENABLE = FALSE;
	SCISSORENABLE = FALSE;
	MULTISAMPLEENABLE = FALSE;
	ANTIALIASEDLINEENABLE = FALSE;
};

RasterizerState rsNoCull
{	
	FILLMODE = SOLID;
	CULLMODE = NONE; 
	FRONTCOUNTERCLOCKWISE = TRUE;
	DEPTHBIAS = 0;
	DEPTHBIASCLAMP = 0;
	SLOPESCALEDDEPTHBIAS = 0;
	DepthCLIPENABLE = FALSE;
	SCISSORENABLE = FALSE;
	MULTISAMPLEENABLE = FALSE;
	ANTIALIASEDLINEENABLE = FALSE;
};

BlendState bsNoBlend
{
	AlphaToCoverageEnable= FALSE;
	BlendEnable[0] = FALSE;
	RenderTargetWriteMask[0] = 0x0F;
};

BlendState bsDirection
{
	AlphaToCoverageEnable = FALSE;
	BlendEnable[0] = TRUE;
	BlendOP = SUBTRACT;
	SrcBlend = ONE;
	DestBlend = ONE;
	RenderTargetWriteMask[0] = 0x0F;
};

DepthStencilState dssNoTest
{
	DepthEnable = FALSE;
	DepthFunc = LESS;
	StencilEnable = FALSE;
	DepthWriteMask = ZERO;
};

PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	float4x4 worldViewProj = mul(mul(world, view), proj);
	
	output.pos = mul(float4(input.pos[0], input.pos[1], input.pos[2], 1), worldViewProj);
	output.col = input.col;
	
	return output;
}

PS_IN Inside_VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
		
	output.pos = float4(input.pos[0], input.pos[1], 0, 1);
	output.col = input.col;
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	return float4(input.col[0], input.col[1], input.col[2], 1);
}


technique10 RayStart
{
	pass Outside
	{	
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );

		SetBlendState(bsNoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetDepthStencilState(dssNoTest, 0);
		SetRasterizerState(rsBackCull);
	}

	pass Inside
	{	
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, Inside_VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );

		SetBlendState(bsNoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetDepthStencilState(dssNoTest, 0);
		SetRasterizerState(rsNoCull);
	}
}


technique10 RayDirection
{
	/*pass FrontFacePass
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );

		SetBlendState(bsNoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetDepthStencilState(dssNoTest, 0);
		SetRasterizerState(rsBackCull);
	}*/ 

	pass BackFacePass
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );

		SetBlendState(bsNoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetDepthStencilState(dssNoTest, 0);
		SetRasterizerState(rsFrontCull);
	}
}


SamplerState FinalTextureSampler {
    Filter = ANISOTROPIC; // MIN_MAG_MIP_LINEAR; // ANISOTROPIC;
    AddressU = Clamp; // Wrap;
    AddressV = Clamp; // Wrap;	
	AddressW = Clamp;
	BorderColor = float4(0, 0, 0, 0);
	MaxAnisotropy = 16;
	MaxLOD = 0; 
	MinLOD = 0; 
	MipLODBias = 0;
};

SamplerState RayTextureSampler {
    Filter = MIN_MAG_MIP_POINT; // ANISOTROPIC;
    AddressU = Clamp; // Wrap;
    AddressV = Clamp; // Wrap;	
	AddressW = Clamp;
	BorderColor = float4(0, 0, 0, 0);
	MaxAnisotropy = 0;
	MaxLOD = 0; 
	MinLOD = 0; 
	MipLODBias = 0;
};


struct VS_FINAL_IN
{
	float2 pos : POSITION;	
	float2 texCoord : TEXCOORD;
};

struct PS_FINAL_IN
{
	float4 pos : SV_POSITION;
	float2 texCoord : TEXCOORD;
};

PS_FINAL_IN Final_VS( VS_FINAL_IN input )
{
	PS_FINAL_IN output = (PS_FINAL_IN)0;
	
	output.pos = float4(input.pos[0], input.pos[1], 0, 1);
	output.texCoord = input.texCoord;
	
	return output;
}

float4 Final_PS( PS_FINAL_IN input ) : SV_Target
{
	//return rayDir_texture.Sample(FinalTextureSampler, input.texCoord) * 0.785;	

	float4 sample1 = rayStart_texture.Sample(RayTextureSampler, input.texCoord); 

	float3 FrontPoint = float3(sample1[0], sample1[1], sample1[2]); 
	
	float4 sample2 = rayDir_texture.Sample(RayTextureSampler, input.texCoord) - sample1;

	float3 DirectionVector = float3(sample2[0], sample2[1], sample2[2]);
	float3 RayStep = DirectionVector / 48; // SamplingStep;
	
	float4 current = float4(0, 0, 0, 0); 
 
	//if (RayStep[0] != 0 || RayStep[1] != 0 || RayStep[2] != 0) 
	//{
		for(int i = 0; i < 48; i++) // Many H/W do not support variable boundary	
		{
		
			float3 SamplePoint = FrontPoint + (RayStep * i);
			//float4 nDensity = volume_texture.Sample(FinalTextureSampler, SamplePoint) * 0.1;
			float4 nDensity = volume_texture.SampleLevel(FinalTextureSampler, SamplePoint, 0);

			if (nDensity[3] > 0) 
			{
				// current += nDensity[1] * 3; // float4(nDensity[1], nDensity[1], nDensity[1], nDensity[1]); // nDensity[1]; 
				current += (nDensity) * (1 - current[3]); // nDensity[1]; 
				
				//float invAlpha = (1 - current[3]); 				
				//current += float4(nDensity[0] * invAlpha, nDensity[1]  * invAlpha, nDensity[2]  * invAlpha, nDensity[3]); // nDensity[1]; 

				if (current[3] > 1) 
				{
					i = 3000; 
				}
			}
		}
	//}
	  
	//current += volume_texture.SampleLevel(FinalTextureSampler, float3(input.texCoord[0], input.texCoord[1], 0.5), 0);
	// (sample1 * 0.2) + 
	//return sample1 * 0.2; // + (float4(current, current * 0.95, current * 0.8, current) * 0.00055); // * 0.00075);// * 0.003; // float4(current[0], current[1], current[2], 1);
	//return (float4(current, current * 0.95, current * 0.8, current) / 32) * 0.025; // * 0.00075);// * 0.003; // float4(current[0], current[1], current[2], 1);
	//return ((current / 48) * 0.15) * current[3];  // * 0.025; // * 0.00075);// * 0.003; // float4(current[0], current[1], current[2], 1);
	//return ((current / 48) * 0.095) * current[3];  // * 0.025; // * 0.00075);// * 0.003; // float4(current[0], current[1], current[2], 1);
	//return ((current / 48) * 0.075) * current[3]; 
	return ((current / 48) * 0.035) * current[3]; 
	//return (current / 32) * 0.075; // * 0.00075);// * 0.003; // float4(current[0], current[1], current[2], 1);

}

technique10 Final
{
	pass P0
	{	
		// SetBlendState(AdditiveBlending, float4(0.0f, 0.0f, 0.0f, 0.0f), 0xFFFFFFFF);

		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, Final_VS() ) );
		SetPixelShader( CompileShader( ps_4_0, Final_PS() ) );

		SetBlendState(bsNoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetDepthStencilState(dssNoTest, 0);
		SetRasterizerState(rsFrontCull);
	}
}


SamplerState ImposterSampler {
    Filter = MIN_MAG_MIP_LINEAR; // ANISOTROPIC;
    AddressU = Clamp; // Wrap;
    AddressV = Clamp; // Wrap;	
	BorderColor = float4(0, 0, 0, 0);
	MaxAnisotropy = 0;
	MaxLOD = 0; 
	MinLOD = 0; 
	MipLODBias = 0;
};


PS_FINAL_IN Imposter_VS( VS_FINAL_IN input )
{
	PS_FINAL_IN output = (PS_FINAL_IN)0;
	
	output.pos = float4(input.pos[0], input.pos[1], 0, 1);
	output.texCoord = input.texCoord;
	
	return output;
}

float4 Imposter_PS( PS_FINAL_IN input ) : SV_Target
{
	return imposter.Sample(ImposterSampler, input.texCoord);
}


technique10 Imposter
{
	pass P0
	{	
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, Imposter_VS() ) );
		SetPixelShader( CompileShader( ps_4_0, Imposter_PS() ) );

		SetBlendState(bsNoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetDepthStencilState(dssNoTest, 0);
		SetRasterizerState(rsNoCull);
	}
}