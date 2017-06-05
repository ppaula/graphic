sampler2D input : register(s0);
float brightness : register(c0);
float alpha : register(c1);

float4 main(float2 uv : TEXCOORD) : COLOR
{
	float4 pixelColor = tex2D(input, uv);

	/*moje*/

	pixelColor.rgb /= pixelColor.a;

	pixelColor.rgb += brightness;

	pixelColor.a = alpha;

	pixelColor.rgb *= pixelColor.a;

	return pixelColor;
}