FEATURES
{
#include "common/features.hlsl"
}

MODES
{
    Forward();
    Depth();
}

COMMON
{
#include "common/shared.hlsl"
}

struct VertexInput
{
#include "common/vertexinput.hlsl"

    float4 vColorPaintValues : TEXCOORD5 < Semantic(VertexPaintTintColor);
    > ;
};

struct PixelInput
{
#include "common/pixelinput.hlsl"
};

VS
{
#include "common/vertex.hlsl"

    PixelInput MainVs(VertexInput i)
    {
        PixelInput o = ProcessVertex(i);

        ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData(i.nInstanceTransformID);
        o.vVertexColor.rgb = SrgbGammaToLinear(i.vColorPaintValues.rgb) * extraShaderData.vTint.rgb;
        // Add your vertex manipulation functions here
        return FinalizeVertex(o);
    }
}

PS
{
#include "common/pixel.hlsl"

    float4 MainPs(PixelInput i) : SV_Target0
    {
        Material m = Material::Init(i);
        m.Albedo.rgb = i.vVertexColor.rgb;
        /* m.Metalness = 1.0f; // Forces the object to be metalic */
        return ShadingModelStandard::Shade(m);
    }
}
