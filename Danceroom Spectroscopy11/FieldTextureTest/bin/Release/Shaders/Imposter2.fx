Texture2D imposter;
float brightPassThreshold;	

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

BlendState bsSubtract
{
    BlendEnable[0] = TRUE;
    SrcBlend = ONE; // SRC_COLOR;
    DestBlend = ONE; // DEST_COLOR;
    BlendOp = REV_SUBTRACT;
    SrcBlendAlpha = ONE;
    DestBlendAlpha = ONE;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
};

BlendState bsInvert
{
    BlendEnable[0] = TRUE;
    SrcBlend = ONE;
    DestBlend = ONE;
    BlendOp = SUBTRACT;
    SrcBlendAlpha = ONE;
    DestBlendAlpha = ONE;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
};

BlendState bsAlpha
{
    BlendEnable[0] = TRUE;
    SrcBlend = SRC_ALPHA;
    DestBlend = INV_SRC_ALPHA;
    BlendOp = ADD;
    SrcBlendAlpha = SRC_ALPHA;
    DestBlendAlpha = INV_SRC_ALPHA;
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
	//return imposter.Sample(ImposterSampler, input.texCoord) * float4(brightPassThreshold, brightPassThreshold, brightPassThreshold, 1);
	
	float col = imposter.Sample(ImposterSampler, input.texCoord);

	return float4(col.r, col.r, col.r, col.r) * float4(brightPassThreshold, brightPassThreshold, brightPassThreshold, 1);
}

PS_FINAL_IN Imposter_Invert_VS( VS_FINAL_IN input )
{
	PS_FINAL_IN output = (PS_FINAL_IN)0;
	
	output.pos = float4(input.pos[0], input.pos[1], 0, 1);

	output.texCoord = input.texCoord;
	
	return output;
}

float4 Imposter_Invert_PS( PS_FINAL_IN input ) : SV_Target
{
	float value = imposter.Sample(ImposterSampler, input.texCoord); 

	float4 Color = (1 - float4(value, value, value, value)) * float4(brightPassThreshold, brightPassThreshold, brightPassThreshold, 1);
 
 	Color.a = 1.0f;

	return Color; 
}

float4 Imposter_PS_BrightPass( PS_FINAL_IN input ) : SV_Target
{
	float3 luminanceVector = { 0.2125f, 0.7154f, 0.0721f };
	float4 average = { 0.0f, 0.0f, 0.0f, 0.0f };

	float value = imposter.Sample(ImposterSampler, input.texCoord); 

	float4 value4 = float4(value, value, value, value); 

	// Determine the luminance of this particular pixel
	float luminance = dot(value4.rgb, luminanceVector);

	luminance = max(0.0f, luminance - brightPassThreshold);

	return value4 * luminance;
}


technique10 Imposter
{
	pass NoBlend
	{	
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, Imposter_VS() ) );
		SetPixelShader( CompileShader( ps_4_0, Imposter_PS() ) );

		SetBlendState(bsNoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetDepthStencilState(dssNoTest, 0);
		SetRasterizerState(rsNoCull);
	}

	pass OverlayAdd
	{	
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, Imposter_VS() ) );
		SetPixelShader( CompileShader( ps_4_0, Imposter_PS() ) );

		SetBlendState(bsAdd, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetDepthStencilState(dssNoTest, 0);
		SetRasterizerState(rsNoCull);
	}

	pass OverlaySubtract
	{	
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, Imposter_VS() ) );
		SetPixelShader( CompileShader( ps_4_0, Imposter_PS() ) );

		SetBlendState(bsSubtract, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetDepthStencilState(dssNoTest, 0);
		SetRasterizerState(rsNoCull);
	}

	pass OverlayInvert
	{	
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, Imposter_Invert_VS() ) );
		SetPixelShader( CompileShader( ps_4_0, Imposter_Invert_PS() ) );

		//SetBlendState(bsAdd, float4(0, 0, 0, 0), 0xFFFFFFFF);
		//SetBlendState(bsNoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetBlendState(bsInvert, float4(0, 0, 0, 0), 0xFFFFFFFF);
		
		SetDepthStencilState(dssNoTest, 0);
		SetRasterizerState(rsNoCull);
	}

	pass OverlayAlpha
	{	
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, Imposter_VS() ) );
		SetPixelShader( CompileShader( ps_4_0, Imposter_PS() ) );

		SetBlendState(bsAlpha, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetDepthStencilState(dssNoTest, 0);
		SetRasterizerState(rsNoCull);
	}
}

technique10 Imposter_BrightPass
{
	pass NoBlend
	{	
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, Imposter_VS() ) );
		SetPixelShader( CompileShader( ps_4_0, Imposter_PS_BrightPass() ) );

		SetBlendState(bsNoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetDepthStencilState(dssNoTest, 0);
		SetRasterizerState(rsNoCull);
	}

	pass OverlayAdd
	{	
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, Imposter_VS() ) );
		SetPixelShader( CompileShader( ps_4_0, Imposter_PS_BrightPass() ) );

		SetBlendState(bsAdd, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetDepthStencilState(dssNoTest, 0);
		SetRasterizerState(rsNoCull);
	}

	pass OverlaySubtract
	{	
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, Imposter_VS() ) );
		SetPixelShader( CompileShader( ps_4_0, Imposter_PS_BrightPass() ) );

		SetBlendState(bsSubtract, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetDepthStencilState(dssNoTest, 0);
		SetRasterizerState(rsNoCull);
	}

	pass OverlayAlpha
	{	
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, Imposter_VS() ) );
		SetPixelShader( CompileShader( ps_4_0, Imposter_PS_BrightPass() ) );

		SetBlendState(bsAlpha, float4(0, 0, 0, 0), 0xFFFFFFFF);

		SetDepthStencilState(dssNoTest, 0);
		SetRasterizerState(rsNoCull);
	}
}