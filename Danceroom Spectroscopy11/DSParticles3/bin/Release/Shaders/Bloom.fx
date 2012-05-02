//----------------------------------------------------------------------------------
// File:   PostFXSeparableGaussian.fx
// Author: Miguel Sainz
// Email:  sdkfeedback@nvidia.com
// 
// Copyright (c) 2007 NVIDIA Corporation. All rights reserved.
//
// TO  THE MAXIMUM  EXTENT PERMITTED  BY APPLICABLE  LAW, THIS SOFTWARE  IS PROVIDED
// *AS IS*  AND NVIDIA AND  ITS SUPPLIERS DISCLAIM  ALL WARRANTIES,  EITHER  EXPRESS
// OR IMPLIED, INCLUDING, BUT NOT LIMITED  TO, IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL  NVIDIA OR ITS SUPPLIERS
// BE  LIABLE  FOR  ANY  SPECIAL,  INCIDENTAL,  INDIRECT,  OR  CONSEQUENTIAL DAMAGES
// WHATSOEVER (INCLUDING, WITHOUT LIMITATION,  DAMAGES FOR LOSS OF BUSINESS PROFITS,
// BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY LOSS)
// ARISING OUT OF THE  USE OF OR INABILITY  TO USE THIS SOFTWARE, EVEN IF NVIDIA HAS
// BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.
//
//
//----------------------------------------------------------------------------------

//#############################################################################
//
// PARAMETERS
//
//#############################################################################

#define MAX_SAMPLES 16

// Contains weights for Gaussian blurring
float g_GWeights[MAX_SAMPLES];

DepthStencilState DisableDepth
{
    DepthEnable    = FALSE;
    DepthWriteMask = ZERO;
};

BlendState DisableBlend
{
    BlendEnable[0] = false;
};

//#############################################################################
//
// SAMPLERS
//
//#############################################################################
Texture2D g_SourceTex : TEXTURE0;

SamplerState BilinearSampler
{
    Filter   = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

//#############################################################################
struct VS_Input
{
    float2 Pos                : POSITION;
    float4 Tex[MAX_SAMPLES/2] : TEXCOORD0;
};

struct VS_Output {
    float4 Pos                : SV_POSITION;
    float4 Tex[MAX_SAMPLES/2] : TEXCOORD0;
};


//#############################################################################
//
// BLUR
//
//#############################################################################
VS_Output VS_blur(in VS_Input IN)
{
    VS_Output OUT;

    OUT = (VS_Output)0;

    OUT.Pos = float4(IN.Pos.x, IN.Pos.y, 0.5f, 1.0f);

    // Copy all coordinates
    [unroll]
    for( int i = 0; i < MAX_SAMPLES/2; i++ )
        OUT.Tex[i] = IN.Tex[i];

    return OUT;
}

float4 PS_blurBilinear(in VS_Output IN) : SV_TARGET0
{

    float4 sample = 0.0f;

    for( int i = 0; i < MAX_SAMPLES/2; i++ )
    {
        sample += g_GWeights[2*i + 0] * g_SourceTex.Sample(BilinearSampler, IN.Tex[i].xy);
        sample += g_GWeights[2*i + 1] * g_SourceTex.Sample(BilinearSampler, IN.Tex[i].zw);
    }


    return sample;
}


//=============================================================
technique10 BlurBilinear
{
    pass Gaussian
    {
        SetVertexShader  ( CompileShader( vs_4_0, VS_blur() ) );
        SetGeometryShader( NULL );
        SetPixelShader   ( CompileShader( ps_4_0, PS_blurBilinear() ) );

		SetBlendState( DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
    }
}
