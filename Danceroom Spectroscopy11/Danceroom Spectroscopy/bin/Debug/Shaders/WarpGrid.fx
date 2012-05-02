Texture2D imposter;
float globalAlpha; 

RasterizerState rsNoCull
{	
	FILLMODE = SOLID; // WIREFRAME;
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

BlendState bsAdd
{
    BlendEnable[0] = TRUE;
    SrcBlend = ONE;
    DestBlend = ONE;
    BlendOp = ADD;
    SrcBlendAlpha = ONE;
    DestBlendAlpha = ONE;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
};

DepthStencilState dssNoTest
{
	DepthEnable = FALSE;
	DepthFunc = LESS;
	StencilEnable = FALSE;
	DepthWriteMask = ZERO;
};

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

PS_FINAL_IN Imposter_VS( VS_FINAL_IN input )
{
	PS_FINAL_IN output = (PS_FINAL_IN)0;
	
	output.pos = float4(input.pos[0], input.pos[1], 0, 1);
	output.texCoord = input.texCoord;
	
	return output;
}

float4 Imposter_PS( PS_FINAL_IN input ) : SV_Target
{
	float4 col = imposter.Sample(ImposterSampler, input.texCoord);

	//return float4(1, 1, 1, 1); //  col * float4(globalAlpha, globalAlpha, globalAlpha, 1);
	return col * float4(globalAlpha, globalAlpha, globalAlpha, 1);
}

technique10 Imposter
{	
	pass Pass0
	{	
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, Imposter_VS() ) );
		SetPixelShader( CompileShader( ps_4_0, Imposter_PS() ) );

		SetBlendState(bsAdd, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetDepthStencilState(dssNoTest, 0);
		SetRasterizerState(rsNoCull);
	}
}