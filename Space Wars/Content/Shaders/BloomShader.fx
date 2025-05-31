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

float4 MainPS(VertexShaderOutput input) : COLOR
{
	
    float gaussian[7][7]  =
	{
        { 0.011, 0.039, 0.082, 0.105, 0.082, 0.039, 0.011 },
        { 0.039, 0.135, 0.286, 0.368, 0.286, 0.135, 0.039 },
        { 0.082, 0.286, 0.607, 0.779, 0.607, 0.286, 0.082 },
        { 0.105, 0.368, 0.779,     1, 0.779, 0.368, 0.105 },
        { 0.082, 0.286, 0.607, 0.779, 0.607, 0.286, 0.082 },
        { 0.039, 0.135, 0.286, 0.368, 0.286, 0.135, 0.039 },
        { 0.011, 0.039, 0.082, 0.105, 0.082, 0.039, 0.011 }
    };
	
    float2 screenSize = float2(1920, 1080);
    float2 div = float2(1, 1) / (screenSize * 2);
    float3 comp = tex2D(s0, input.TextureCoordinates);
    float3 col = float3(0, 0, 0);
    for (int x = 0; x < 7; x++)
    {
        for (int y = 0; y < 7; y++)
        {
            if (x == 3 && y == 3)
            {
                y++;
            }
            float2 texCoord = input.TextureCoordinates + float2((x - 3) * div.x, (y - 3) * div.y);
            float3 s = tex2D(s0, texCoord).rgb;
            float3 ds = s - comp;
            float colorDiff = sqrt(ds.r * ds.r + ds.g * ds.g + ds.b * ds.b);
            col += s * gaussian[x][y] * colorDiff;
        }
    }
    col += comp;
    col = float3(min(1, col.r), min(1, col.g), min(1, col.b)) * (sin(screenSize.y * 1.5 * input.TextureCoordinates.y) / 2 + 0.5);
    return float4(col.rgb, 1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};