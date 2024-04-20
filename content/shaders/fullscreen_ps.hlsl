
struct PSInput
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
};

float4 PSMain(PSInput input) : SV_TARGET
{
    float r = input.position.x;
    for (uint i = 0; i < 10000; i++)
    {
        r = sin(r);
    }
    return float4(r, 0, 0, 1);
}