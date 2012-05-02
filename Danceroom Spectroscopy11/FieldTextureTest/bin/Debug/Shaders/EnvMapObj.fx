//
// Constant Buffer Variables
//
/* 
vec3 color; 

vec4 normalMap = texture2D(tex2, gl_TexCoord[0].xy); 
vec3 normal = normalize(vec3((normalmap.x-0.5)*2.0, (normalmap.z-0.5)*2.0, normalmap.y-0.5)*-2.0)); 

float NdotL = max(0.0, dot(light_pos, normal)); 
vec3 diffuse_color = vec3(NdotL * 1.0); 

vec3 diffuse_map_vec = obj2world * normal;
diffuse_map_vec.y *= -1.0; 
diffuse_color += textureCube(tex4, diffuse_map_vec).xyz * 0.5; 

vec3 H = normalize(normalize(vertex_pos*-1.0) + normalize(light_pos));
float spec = min(1.0, max(0.0, pow(dot(normal,H),10.0)*1.0 * NdotL)); 
vec3 spec_color = vec3(spec); 

vec3 spec_map_vec = reflect(vertex_pos, normal); 
spec_map_vec = obj2world * spec_map_vec;
spec_map_vec.y *= -1.0; 
spec_color += textureCube(tex3,spec_map_vec).xyz * 0.5; 

vec4 colormap = texture2D(tex,gl_TexCoord[0].xy); 

color = diffuse_color * colorma.xyz + spec_color * colormap.a; 

gl_FragColor = vec4(color,1.0); 
*/ 

Texture2D texColorMap;
SamplerState sampColorMap
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

Texture2D texNormalMap;
SamplerState sampNormalMap
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

TextureCube texDiffuseMap;
SamplerState sampDiffuseMap
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

TextureCube texSpecularMap;
SamplerState sampSpecularMap
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

cbuffer cbConstant
{
    float3 vLightDir = float3(0,1,0); //float3(-0.577,0.577,-0.577);
};

cbuffer cbChangesEveryFrame
{
	matrix Projection;
	matrix View;	
    matrix World;        
	//matrix Obj2World; // inv world 
	float3 Eye;
    float Time;
};

struct VS_INPUT
{
    float3 Pos          : POSITION;         //position
    float3 Norm         : NORMAL;           //normal
    float2 Tex          : TEXCOORD0;        //texture coordinate
};

struct PS_INPUT
{
    float4 Pos : SV_POSITION;
    float3 Norm : TEXCOORD0;
    float2 Tex : TEXCOORD1;
    float3 ViewR : TEXCOORD2;
};

//--------------------------------------------------------------------------------------
// DepthStates
//--------------------------------------------------------------------------------------
DepthStencilState EnableDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
    DepthFunc = LESS_EQUAL;
};

BlendState NoBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
};

RasterizerState cullBack
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

//
// Vertex Shader
//
PS_INPUT VS( VS_INPUT input )
{
    PS_INPUT output = (PS_INPUT)0;
    
    output.Pos = mul(float4(input.Pos, 1), World);    
    output.Pos = mul(output.Pos, View);
    output.Pos = mul(output.Pos, Projection);


    output.Norm = mul(input.Norm, (float3x3)World);
    output.Tex = input.Tex;
    
    // Calculate the reflection vector
    float3 viewNorm = output.Norm; // mul( output.Norm, (float3x3)View );
	//float3 viewNorm = mul( input.Norm, (float3x3)View );
	//viewNorm = mul( viewNorm, (float3x3)InvView );
    output.ViewR = reflect(viewNorm, float3(0, 0, -1.0));
    
    return output;
}


//
// Pixel Shader
//
float4 PS( PS_INPUT input) : SV_Target
{
/* 
    // Calculate lighting assuming light color is <1,1,1,1>
    float fLighting = saturate(dot(input.Norm, vLightDir));
   
    // Load the environment map texture
    float4 cReflect = texDiffuseMap.Sample(sampDiffuseMap, input.ViewR);
    
    // Load the diffuse texture and multiply by the lighting amount
    float4 cDiffuse = texColorMap.Sample(sampColorMap, input.Tex) * fLighting;
    
    // Add diffuse to reflection and go
    float4 cTotal = cDiffuse + cReflect;
	//float4 cTotal = cReflect;
    cTotal.a = 1;
    return cTotal;
	*/
	 
	float3 color; 

	//float4 normalMap = texNormalMap.Sample(sampNormalMap, input.Tex); 
	//float3 normal = normalize(float3((normalMap.x - 0.5) * 2.0, (normalMap.z - 0.5) * 2.0, normalMap.y - 0.5) * -2.0)); 
	float3 normal = input.Norm; 

	float NdotL = saturate(dot(normal, vLightDir)); 
	float3 diffuse_color = float3(NdotL, NdotL, NdotL); 

	//float3 diffuse_map_vec = Obj2World * normal;
	//diffuse_map_vec.y *= -1.0; 
	//diffuse_color += texDiffuseMap.Sample(tex4, diffuse_map_vec).xyz * 0.5; 
	diffuse_color += texDiffuseMap.Sample(sampDiffuseMap, input.ViewR).xyz * 0.5;  

	//float3 H = normalize(normalize(input.Pos * -1.0) + normalize(vLightDir)).xyz;
	float3 H = normalize(normalize(vLightDir) + normalize(Eye - (float3)input.Pos)).xyz;	
	//float3 H = normalize(normalize(vLightDir) + normalize(Eye)).xyz;	

	//float3 vhalf = normalize(vLightDir + Eye);
	//float spec = dot(normal, vhalf);
	//spec = pow(spec, 32);

	//float spec = min(1.0, max(0.0, pow(dot(normal, H), 10.0) * 1 * NdotL)); 
	float spec = 1 * pow(saturate(dot(normal, H)), 0.5);  // min(1.0, max(0.0, pow(dot(normal, H), 10.0) * 1 * NdotL)); 
	float3 spec_color = float3(spec, spec, spec); 

	//float3 spec_map_vec = reflect(output.Pos, normal); 
	//spec_map_vec = obj2world * spec_map_vec;
	//spec_map_vec.y *= -1.0; 
	//spec_color += textureCube(tex3,spec_map_vec).xyz * 0.5; 
	//spec_color += texSpecularMap.Sample(sampSpecularMap, input.ViewR).xyz * 0.5; 

	float4 colormap = texColorMap.Sample(sampColorMap, input.Tex); 

	//color = spec_color + diffuse_color; // diffuse_color; // spec_color; 
	color = diffuse_color * colormap.xyz + (spec_color * colormap.a); 
	//color = diffuse_color * colormap.xyz; // + spec_color * colormap.a; 

	return float4(color,1.0); 
}

//
// Technique
//
technique10 Render
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS() ) );
        
        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
		SetRasterizerState(cullBack);
    }
}
