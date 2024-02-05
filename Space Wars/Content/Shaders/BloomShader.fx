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
float Time;
const float pi = 3.1415926f;

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
	float4 color = tex2D(s0, input.TextureCoordinates);
	float colorMagnitude = abs(cos(input.TextureCoordinates.y - Time) * 3.14159);
	color = float4(color.r, color.g, color.b, 1) * color.a * (input.Color * colorMagnitude + float4(1, 1, 1, 1) * (1-colorMagnitude));
	return color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};