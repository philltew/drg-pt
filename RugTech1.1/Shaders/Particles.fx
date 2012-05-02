float4x4 worldViewProj; 
Texture2D particle_texture;
float partScale; 
float ampScale; 

float maxDistance; 
float minDistance; 
float scaleDistance; 

SamplerState ParticleTextureSampler {
    Filter = MIN_MAG_MIP_LINEAR; // ANISOTROPIC;
    AddressU = Clamp; // Wrap;
    AddressV = Clamp; // Wrap;
};

struct Part_VS_IN
{
	float4 pos : POSITION;	
	float2 texCoord : TEXCOORD;
	float3 instancePos : INST_POSITION;
	float4 instanceCol : INST_COLOR;
};

struct Part_PS_IN
{
	float4 pos : SV_POSITION;
	float2 texCoord : TEXCOORD;
	float4 col : COLOR;
};

DepthStencilState UIDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ZERO; // ZERO; // ALL; // ZERO; 
	DepthFunc = LESS; // LESS;
	//StencilEnable = FALSE; 
};

BlendState AdditiveBlending
{
	// SRC_ALPHA; // INV_DEST_ALPHA; // SRC_ALPHA; // ONE; // ZERO;

    BlendEnable[0] = TRUE;
	SrcBlend = ONE;
	DestBlend =	ONE;
	BlendOp	= ADD;
	SrcBlendAlpha =	ONE; 
	DestBlendAlpha = ONE; 
    BlendOpAlpha = ADD;

    //RenderTargetWriteMask[0] = 0x0F;
	/*
	BlendEnable[0] = true;  
	SrcBlend = SRC_ALPHA;  
	DestBlend = INV_SRC_ALPHA;  
	BlendOp	= ADD;

	//SeparateAlphaBlendEnable = true;  
	SrcBlendAlpha  = ZERO;  
	DestBlendAlpha = INV_SRC_ALPHA;  
	BlendOpAlpha	= ADD;
	*/
};

RasterizerState rsSolid
{	
	FILLMODE = SOLID;
	CULLMODE = NONE; // BACK;
	FRONTCOUNTERCLOCKWISE = TRUE;
	DEPTHBIAS = 0;
	DEPTHBIASCLAMP = 0;
	SLOPESCALEDDEPTHBIAS = 0;	
	DepthCLIPENABLE = FALSE;
	SCISSORENABLE = FALSE;
	MULTISAMPLEENABLE = FALSE;
	ANTIALIASEDLINEENABLE = FALSE;
};

/*

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
*/ 

Part_PS_IN Part_VS(Part_VS_IN input)
{
	Part_PS_IN output = (Part_PS_IN)0;

	float4 inputPos = float4(input.pos.x * partScale, input.pos.y * partScale, 0, 0);

	output.pos = inputPos + mul(float4(input.instancePos[0], input.instancePos[1], input.instancePos[2], 1.0f), worldViewProj);
	//output.pos = input.pos + mul(float4(input.instancePos[0], input.instancePos[1], input.instancePos[2], 1.0f), worldViewProj);
	output.texCoord = input.texCoord;		 
	output.col = input.instanceCol * ampScale; // (input.instanceCol * ampScale) * saturate((maxDistance - output.pos.z) * (scaleDistance));
	//output.col.a = 1f; 

	//output.col = input.instanceCol * ampScale;

	return output;
}

float4 Part_PS(Part_PS_IN input) : SV_Target
{
	// float4 textColor = particle_texture.Sample(ParticleTextureSampler, input.texCoord);  // * input.col	
	//return float4(textColor.x * input.col.x, textColor.y * input.col.y, textColor.z * input.col.z, textColor.w); 
	float4 text = particle_texture.Sample(ParticleTextureSampler, input.texCoord); 

	float4 result = saturate(text * input.col); 

	result.a = text.a; 

	return result;
}

technique10 RenderParticles
{
	pass Add
	{	
		SetDepthStencilState( UIDepth, 0 );
		SetBlendState(AdditiveBlending, float4(0.0f, 0.0f, 0.0f, 0.0f), 0xFFFFFFFF);
		SetRasterizerState(rsSolid);

		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, Part_VS() ) );
		SetPixelShader( CompileShader( ps_4_0, Part_PS() ) );
	}
}
