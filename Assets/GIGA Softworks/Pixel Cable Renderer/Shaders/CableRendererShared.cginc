/*-------------------------------------------------------
* shared data for the cable renderer shader
*
*---------------------------------------------------------------------------------------------------*/

// defines
#define RAD_90 1.5708
#define SCALE_COMPRESSION 100.0f
#define PI 3.14159265359

// shared properties
sampler2D _CableTexture; 
int _TimeEnabled; 
fixed4 _Color1; 
fixed4 _Color2;
int _ColorMode;
fixed4 _IndexedColors[16];
float _InverseCanvasSize;
int _PixelSize;


struct appdata_t
{
    float4 vertex   : POSITION;
    float4 color    : COLOR;
    float2 texcoord : TEXCOORD0;
};

struct v2f
{
    float4 vertex   : SV_POSITION;
    fixed4 color    : COLOR;
    half2 texcoord  : TEXCOORD0;
};

v2f vert(appdata_t IN)
{
	v2f OUT;

	OUT.vertex = UnityObjectToClipPos( IN.vertex);
	OUT.texcoord = IN.texcoord;
	OUT.color = IN.color;
	return OUT;
}