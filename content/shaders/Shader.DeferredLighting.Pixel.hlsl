#include "Shader.DeferredLighting.hlsli"


float3 CalculateLighting(in LightInstanceData light, float3 worldPosition, float3 normal, float3 albedo)
{
    float3 lightColor = light.Color *light.IntensityOrRadius;
    
    // Lambertian Diffuse Lighting
    float3 lightDir = normalize(-light.Direction);
    float NdotL = max(dot(normal, lightDir), 0.0f);
    
    float3 ambientLight = GetAmbientLight();
    return albedo * lightColor * NdotL * ambientLight;
}

float3 CalculateBlinnPhongLighting(in LightInstanceData light, float3 position, float3 normal, float3 albedo, float specularStrength)
{
    float3 lightDirection = normalize(light.Position - position);
     lightDirection = normalize(-light.Direction);
    float3 cameraPosition = GetCameraPosition();
    // cameraPosition = float3(0,0,0);
    float3 ambientLight = GetAmbientLight();
    float3 ambient = ambientLight * albedo;
    
    float diff = max(dot(normal, lightDirection), 0.0);
    float3 diffuse = diff * light.Color * albedo;

    float3 viewDirection = normalize(cameraPosition - position);
    float3 halfwayDirection = normalize(lightDirection + viewDirection);
    float spec = pow(max(dot(normal, halfwayDirection), 0.0), 16.0);
    float3 specular = spec * light.Color * specularStrength;
    
    return ambient + diffuse + specular;
    // return ambient + diffuse + specular;

}
float3 ApplyLighting(LightInstanceData light, float3 normal, float3 albedoColor)
{
    // Normalize the normal
    normal = normalize(normal);

    // Compute directional lighting (diffuse)
    float ndotl = max(dot(normal, light.Direction), 0.0f);
    float3 directLight = light.Color * (light.IntensityOrRadius * ndotl);

    // Add ambient lighting (always affects the scene)
    // float3 ambientLight = GetAmbientLight();
    float3 ambientLight = GetAmbientLight();

    // Final color
    return albedoColor * (directLight + ambientLight);
}

float4 main(LightsVertexOutput input): SV_TARGET0
{
    Texture2D gPosition = GetInputTexture(PositionIndex);
    Texture2D gAlbedo = GetInputTexture(AlbedoIndex);
    Texture2D gNormal = GetInputTexture(NormalIndex);
    Texture2D gSpecular = GetInputTexture(SpecularIndex);

    float3 specular = gSpecular.Sample(PointSampler, input.Texture).xyz;
    float3 position = gPosition.Sample(PointSampler, input.Texture).xyz;
    float3 normal = gNormal.Sample(PointSampler, input.Texture).xyz;
    float4 albedo  = gAlbedo.Sample(PointSampler, input.Texture);
    float3 albedoColor = albedo.rgb;
    

    LightInstanceData light = GetLight(input.InstanceId);

    float3 lighting;
    switch(light.type) {
        case 0:
            lighting = CalculateBlinnPhongLighting(light, position, normal, albedoColor, specular.r);
            break;
        case 1:
            lighting = CalculateLighting(light, position, normal, albedoColor);
            break;
        case 2:
            lighting = ApplyLighting(light, normal, albedoColor);
            break;
    }
    
    lighting = pow(lighting, 1.0 / 2.2); // Gamma correction TODO: should we have this?

    return float4(lighting, 1.0);
}