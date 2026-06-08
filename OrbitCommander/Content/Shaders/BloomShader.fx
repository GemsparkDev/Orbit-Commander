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
    //One up          , Two up
    //One up one left , Two up one left
    //One up two left , Two up Two left
    float gaussian[3][2]  =
    {
        { 0.33975863, 0.10331259 },
        { 0.22847118, 0.0694727 },
        { 0.0694727, 0.021125011 },
    };
    //Screensize = 1920, 1080
    float2 div = float2(0.000520833333333, 0.000925925925926); //One over the screensize
    float2 tanh2x6 = 5.78416548045;
    float3 col = tex2D(s0, input.TextureCoordinates).rgb * 0.5;
    for (int x = 0; x < 3; x++)
    {
        for (int y = 0; y < 2; y++)
        {
	    float g = gaussian[x][y] / 3;
            col += g * (
		tex2D(s0, input.TextureCoordinates + float2(x * div.x, (y+1) * div.y)).rgb +
		tex2D(s0, input.TextureCoordinates + float2((y+1) * div.x, -x * div.y)).rgb +
		tex2D(s0, input.TextureCoordinates + float2(-x * div.x, (-y-1) * div.y)).rgb +
		tex2D(s0, input.TextureCoordinates + float2((-y-1) * div.x, x * div.y)).rgb);
        }
    }
    float scanline = tanh(2 * sin(1620 * input.TextureCoordinates.y)) / tanh2x6 + 0.8333;
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