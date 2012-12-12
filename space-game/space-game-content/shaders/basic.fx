float4x4 g_WorldViewProj;

texture g_ColorMap:TEXUNIT0;

float4 g_Color;
float2 g_PixelSize;

sampler ColorSampler = 
sampler_state
{
    Texture = <g_ColorMap>;
    MipFilter = NONE;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VS_OUTPUT
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
};

VS_OUTPUT MainVS( 
    float4 Pos      : POSITION, 
    float2 TexCoord : TEXCOORD0)
{
    VS_OUTPUT Output;

    Output.Position = mul( Pos, g_WorldViewProj );
    Output.TexCoord = TexCoord;
    
    return Output;
}

float4 ColorPS() : COLOR
{
    return g_Color;
}

float4 ColorTexturePS(float2 TexCoord : TEXCOORD0) : COLOR
{
    return g_Color * tex2D(ColorSampler,TexCoord + g_PixelSize);
}

technique Color
{
    pass P0
    {          
        VertexShader = compile vs_2_0 MainVS( );
        PixelShader  = compile ps_2_0 ColorPS( ); 
    }
}

technique ColorTexture
{
    pass P0
    {          
        VertexShader = compile vs_2_0 MainVS( );
        PixelShader  = compile ps_2_0 ColorTexturePS( ); 
    }
}
