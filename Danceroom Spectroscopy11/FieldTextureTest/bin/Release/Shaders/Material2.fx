//--------------------------------------------------------------------------------------
// Lighting Effect - 2010 - Bobby Anguelov
//--------------------------------------------------------------------------------------

cbuffer cbChangesEveryFrame
{
    matrix World;
    matrix View;
	matrix InvView;
    matrix Projection;
    float Time;
	float3 Eye;
};

//TEXTURE VARIABLES
//--------------------------------------------------------------------------------------

//color map texture
Texture2D g_txDiffuse;

//texture sampler state
SamplerState samLinear
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
	MaxAnisotropy = 16;
};

TextureCube g_txEnvMap;
SamplerState samLinearClamp
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

//LIGHTING STRUCTURES AND VARIABLES
//--------------------------------------------------------------------------------------
struct DirectionalLight
{
	float4 color;
	float3 dir;
};

struct Material
{
	float Ka, Kd, Ks, A;
};

//lighting vars
DirectionalLight light = {
	float4(1,0.1,0.4,0.5),
	float3(0, -1, 0) // float3(-0.577,0.577,-0.577)
};

Material material = { 
	0.2f,
	0.2f,
	0.9f,
	40
};

float4 ambientLight = float4(1,1,1,1); // float4(1,1,1,1);


//RASTERIZER STATES
//--------------------------------------------------------------------------------------
RasterizerState rsSolid
{
	  FillMode = Solid; // WIREFRAME; //
	  CullMode = NONE;
	  FrontCounterClockwise = false;
};

//VERTEX AND PIXEL SHADER INPUTS
//--------------------------------------------------------------------------------------
struct VS_INPUT
{
	float3 p : POSITION;	 
	float3 n : NORMAL;  
	float2 t : TEXCOORD;
};

//pixel shader inputs
struct PS_INPUT_PV
{
	float4 p : SV_POSITION;   
	float2 t : TEXCOORD;
	float4 i : COLOR;
};

struct PS_INPUT_PP_PHONG
{
	float4 p : SV_POSITION;   
	float4 wp : POSITION;
	float2 t : TEXCOORD;	
	float3 n : TEXCOORD1;		
};

struct PS_INPUT_PP_BLINNPHONG
{
	float4 p : SV_POSITION;  
	float2 t : TEXCOORD;	
	float3 n : TEXCOORD1;
	float3 h : TEXCOORD2;	
};

//--------------------------------------------------------------------------------------
// Phong Lighting Reflection Model
//--------------------------------------------------------------------------------------
float4 calcPhongLighting( Material M, float4 LColor, float3 N, float3 L, float3 V, float3 R )
{	
	float4 Ia = M.Ka * ambientLight;
	float4 Id = M.Kd * saturate( dot(N,L) );
	float4 Is = M.Ks * pow( saturate(dot(R,V)), M.A );
	
	return Ia + (Id + Is) * LColor;
} 
//--------------------------------------------------------------------------------------
// Blinn-Phong Lighting Reflection Model
//--------------------------------------------------------------------------------------
float4 calcBlinnPhongLighting( Material M, float4 LColor, float3 N, float3 L, float3 H )
{	
	float4 Ia = M.Ka * ambientLight;
	float4 Id = M.Kd * saturate( dot(N,L) );
	float4 Is = M.Ks * pow( saturate(dot(N,H)), M.A );
	
	return Ia + (Id + Is) * LColor;
}
//--------------------------------------------------------------------------------------
// PER VERTEX LIGHTING - PHONG
//--------------------------------------------------------------------------------------
PS_INPUT_PV VS_VERTEX_LIGHTING_PHONG( VS_INPUT input )
{
	PS_INPUT_PV output;
	
	//transform position to clip space
	float4 pos = mul( float4(input.p,1), World );
	output.p = mul( pos, View );    
	output.p = mul( output.p, Projection );	
	
	//set texture coords
	output.t = input.t;		
	
	//calculate lighting vectors
	float3 N = normalize( mul( input.n, (float3x3) World) );
	float3 V = normalize( Eye - (float3) pos );	
	//DONOT USE -light.dir since the reflection returns a ray from the surface	
	float3 R = reflect( light.dir, N); 
	
	//calculate per vertex lighting intensity and interpolate it like a color
	output.i = calcPhongLighting( material, light.color, N, -light.dir, V, R);
    
	return output;  
}

float4 PS_VERTEX_LIGHTING_PHONG( PS_INPUT_PV input ) : SV_Target
{    
	//with texturing
	//return input.i * g_txDiffuse.Sample(samLinear, input.t);
	
	//no texturing pure lighting
	return input.i;      
}
//--------------------------------------------------------------------------------------
// PER VERTEX LIGHTING - BLINN-PHONG
//--------------------------------------------------------------------------------------
PS_INPUT_PV VS_VERTEX_LIGHTING_BLINNPHONG( VS_INPUT input )
{
	PS_INPUT_PV output;
	
	//transform position to clip space
	float4 pos = mul( float4(input.p,1), World );
    output.p = mul( pos, View );    
    output.p = mul( output.p, Projection );	
	
	//set texture coords
	output.t = input.t;		
	
	//calculate lighting
	float3 N = normalize( mul( input.n, (float3x3) World) );
	float3 V = normalize( Eye - (float3) pos );
	float3 H = normalize( -light.dir + V );
	
	//calculate per vertex lighting intensity and interpolate it like a color
	output.i = calcBlinnPhongLighting( material, light.color, N, -light.dir, H);
    
	return output;  
}

float4 PS_VERTEX_LIGHTING_BLINNPHONG( PS_INPUT_PV input ) : SV_Target
{    
	//with texturing
	//return input.i * g_txDiffuse.Sample(samLinear, input.t);
	
	//no texturing pure lighting
	return input.i;      
}
//--------------------------------------------------------------------------------------
// PER PIXEL LIGHTING 
//--------------------------------------------------------------------------------------
PS_INPUT_PP_PHONG VS_PIXEL_LIGHTING_PHONG( VS_INPUT input )
{
	PS_INPUT_PP_PHONG output;
	
	//transform position to clip space - keep worldspace position
	output.wp = mul( float4(input.p,1), World );
	output.p = mul( output.wp, View );    
	output.p = mul( output.p, Projection );	
	
	//set texture coords
	output.t = input.t;			
	
	//set required lighting vectors for interpolation
	output.n = normalize( mul(input.n, (float3x3)World) );
   
    return output;  
}

float4 PS_PIXEL_LIGHTING_PHONG( PS_INPUT_PP_PHONG input ) : SV_Target
{     	
	//calculate lighting vectors - renormalize vectors
	input.n = normalize( input.n );		
	float3 V = normalize( Eye - (float3) input.wp );
	//DONOT USE -light.dir since the reflection returns a ray from the surface
	float3 R = reflect( light.dir, input.n);
	
	//calculate lighting		
	float4 I = calcPhongLighting( material, light.color, input.n, -light.dir, V, R );
    
	//with texturing
	//return I * g_txDiffuse.Sample(samLinear, input.t);
	
	//no texturing pure lighting
	return I;    
}
//--------------------------------------------------------------------------------------
// PER PIXEL LIGHTING 
//--------------------------------------------------------------------------------------
PS_INPUT_PP_BLINNPHONG VS_PIXEL_LIGHTING_BLINNPHONG( VS_INPUT input )
{
	PS_INPUT_PP_BLINNPHONG output;
	
	//set position into clip space
	float4 pos = mul( float4(input.p,1), World );
	output.p = mul( pos, View );    
	output.p = mul( output.p, Projection );	
	
	//set texture coords
	output.t = input.t;			
	
	//set required lighting vectors for interpolation
	float3 V = normalize( Eye - (float3) pos );
	output.n = normalize( mul(input.n, (float3x3)World) );	
	output.h = normalize( -light.dir + V );		  
    
	return output;  
}

float4 PS_PIXEL_LIGHTING_BLINNPHONG( PS_INPUT_PP_BLINNPHONG input ) : SV_Target
{     	
	//renormalize interpolated vectors
	input.n = normalize( input.n );		
	input.h = normalize( input.h );
	
	//calculate lighting	
	float4 I = calcBlinnPhongLighting( material, light.color, input.n, -light.dir, input.h );
	
	//with texturing
	//return I * g_txDiffuse.Sample(samLinear, input.t);
	
	//no texturing pure lighting
	return I;    
}
//--------------------------------------------------------------------------------------
// Techniques
//--------------------------------------------------------------------------------------

technique10 Render
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS_PIXEL_LIGHTING_PHONG() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS_PIXEL_LIGHTING_PHONG() ) );
        SetRasterizerState( rsSolid );
    }    
}

technique10 RENDER_VL_PHONG
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS_VERTEX_LIGHTING_PHONG() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS_VERTEX_LIGHTING_PHONG() ) );
        SetRasterizerState( rsSolid );
    }    
}

technique10 RENDER_VL_BLINNPHONG
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS_VERTEX_LIGHTING_BLINNPHONG() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS_VERTEX_LIGHTING_BLINNPHONG() ) );
        SetRasterizerState( rsSolid );
    }    
}

technique10 RENDER_PL_PHONG
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS_PIXEL_LIGHTING_PHONG() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS_PIXEL_LIGHTING_PHONG() ) );
        SetRasterizerState( rsSolid );
    }    
}

technique10 RENDER_PL_BLINNPHONG
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS_PIXEL_LIGHTING_BLINNPHONG() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS_PIXEL_LIGHTING_BLINNPHONG() ) );
        SetRasterizerState( rsSolid );
    }    
}
