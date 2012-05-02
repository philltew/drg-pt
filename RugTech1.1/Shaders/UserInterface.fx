Texture2D UiElementsTexture; 

struct VS_IN
{
	float3 pos : POSITION;
	float2 tex : TEXCOORD;
	float4 col : COLOR;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};

struct PS_IN_Tex
{
	float4 pos : SV_POSITION;
	float2 tex : TEXCOORD;
	float4 col : COLOR;
};

SamplerState TextTextureSampler {
    Filter = MIN_MAG_MIP_POINT; // MIN_MAG_MIP_LINEAR; // ANISOTROPIC;
    AddressU = Clamp; // Wrap;
    AddressV = Clamp; // Wrap;
};

DepthStencilState UIDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
	DepthFunc = LESS; // LESS;
	//StencilEnable = FALSE; 
};

RasterizerState UIRasterizer
{	
	FILLMODE = SOLID;
	CULLMODE = NONE; 
	FRONTCOUNTERCLOCKWISE = TRUE;
	DEPTHBIAS = 0;
	DEPTHBIASCLAMP = 0;
	SLOPESCALEDDEPTHBIAS = 0;
	DepthCLIPENABLE = TRUE;
	SCISSORENABLE = FALSE;
	MULTISAMPLEENABLE = FALSE;
	ANTIALIASEDLINEENABLE = FALSE;
};

BlendState AlphaBlending
{
	BlendEnable[0] = TRUE;
	SrcBlend = SRC_ALPHA;
	DestBlend =	INV_SRC_ALPHA;
	BlendOp	= ADD;
	SrcBlendAlpha =	SRC_ALPHA;
	DestBlendAlpha = INV_SRC_ALPHA;
	BlendOpAlpha = ADD;
	RenderTargetWriteMask[0] = 0x0F;
};

PS_IN VS( VS_IN	input )
{
	PS_IN output = (PS_IN)0;

	output.pos = float4(input.pos[0], input.pos[1],	input.pos[2], 1);
	output.col = input.col;

	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	return input.col;
}

PS_IN_Tex VS_Tex( VS_IN	input )
{
	PS_IN_Tex output = (PS_IN_Tex)0;

	output.pos = float4(input.pos[0], input.pos[1],	input.pos[2], 1);
	output.tex = input.tex; 
	output.col = input.col;

	return output;
}

float4 PS_Tex( PS_IN_Tex input ) : SV_Target
{
	return UiElementsTexture.Sample(TextTextureSampler, input.tex) * input.col;
}

technique10	LinesAndBoxes
{
	pass BoxesAndText
	{
		SetDepthStencilState( UIDepth, 0 );
		SetBlendState(AlphaBlending, float4(0.0f, 0.0f,	0.0f, 0.0f), 0xFFFFFFFF);
		SetRasterizerState(UIRasterizer);

		SetGeometryShader( 0 );
		SetVertexShader( CompileShader(	vs_4_0,	VS_Tex() ) );
		SetPixelShader(	CompileShader( ps_4_0, PS_Tex()	) );
	}

	pass Lines
	{
		SetDepthStencilState( UIDepth, 0 );
		SetBlendState(AlphaBlending, float4(0.0f, 0.0f,	0.0f, 0.0f), 0xFFFFFFFF);
		SetRasterizerState(UIRasterizer);

		SetGeometryShader( 0 );
		SetVertexShader( CompileShader(	vs_4_0,	VS() ) );
		SetPixelShader(	CompileShader( ps_4_0, PS()	) );
	}
}