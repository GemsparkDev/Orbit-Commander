#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
sampler s0;
SamplerState TextureSampler
{
    Texture = <SpriteTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 HorizontalBlur(VertexShaderOutput input) : COLOR
{
    float dist = 1 - ((input.TextureCoordinates.x - 0.5) * (input.TextureCoordinates.x - 0.5) + (input.TextureCoordinates.y - 0.5) * (input.TextureCoordinates.y - 0.5)) * 2;
    float gaussian[5][5]  =
	{
        { 0.003, 0.013, 0.022, 0.013, 0.003 },
        { 0.013, 0.062, 0.102, 0.062, 0.013 },
        { 0.022, 0.102, dist, 0.102, 0.022 },
        { 0.013, 0.062, 0.102, 0.062, 0.013 },
        { 0.003, 0.013, 0.022, 0.013, 0.003 },
    };
    float2 screenSize = float2(1920, 1080);
    float2 div = float2(1, 1) / (screenSize);
    float2 tanh2x6 = 5.78416548045;
    float3 col = float3(0, 0, 0);
    for (int x = 0; x < 5; x++)
    {
        for (int y = 0; y < 5; y++)
        {
            float2 texCoord = input.TextureCoordinates + float2((x - 2) * div.x, (y - 2) * div.y);
            float3 s = tex2D(s0, texCoord).rgb;
            col += s * gaussian[x][y];
        }
    }
    float scanline = tanh(2 * sin(screenSize.y * 1.5 * input.TextureCoordinates.y)) / tanh2x6 + 0.8333;
    col = float3(min(1, col.r), min(1, col.g), min(1, col.b)) * scanline;
    //col = float3(min(1, col.r), min(1, col.g), min(1, col.b));
    return float4(col.rgb, 1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL HorizontalBlur();
	}
};