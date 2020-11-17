#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;
texture Texture;
float4x4 InverseTransposeWorld;
float3 CameraPosition;

float3 AmbientColor; 
float KAmbient;

float KDiffuse;

float3 SpecularColor; 
float KSpecular;
float Shininess;

 

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL;
	float4 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 TextureCoordinate : TEXCOORD0;
	float4 Normal : TEXCOORD1; 
	float4 WorldPosition : TEXCOORD2;
};

sampler2D textureSampler = sampler_state
{
    Texture = (Texture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};


VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);
	output.TextureCoordinate = input.TextureCoordinate;
	output.Normal = mul(input.Normal, InverseTransposeWorld);
	output.WorldPosition = worldPosition;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 lightDirection = normalize(float3(1000,700, 0) + input.WorldPosition.xyz - input.WorldPosition.xyz);
    float3 viewDirection = normalize(CameraPosition - input.WorldPosition.xyz);
    float3 halfVector = normalize(lightDirection + viewDirection);
    
    float3 ambientLight = KAmbient * AmbientColor;
    
    float NdotL = saturate(dot(input.Normal.xyz, lightDirection));
    float3 diffuseLight = KDiffuse * float3(1,1,1) * NdotL;

    float NdotH = dot(input.Normal.xyz, halfVector);
    float3 specularLight = sign(NdotL) * KSpecular * SpecularColor * pow(saturate(NdotH), Shininess);

	//return tex2D(textureSampler, input.TextureCoordinate);
	//return input.Color;
	float4 finalColor = float4(saturate(ambientLight + diffuseLight) * tex2D(textureSampler, input.TextureCoordinate).rgb + specularLight, 0);
    return finalColor;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};