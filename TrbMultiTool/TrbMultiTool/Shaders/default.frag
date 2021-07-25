#version 330

const float ambient = 0.5;
const float diffuse = 0.7;

in vec3 Normal;

out vec4 result;

void main()
{
    vec4 diffuseTexture = vec4(1, 1, 1, 1);
    
    vec3 ambient = diffuseTexture.rgb * ambient; // 0.5 is the ambient light value

    vec3 lightDir = normalize(vec3(0, 0, 1)); // mess with this until you get what you want

    float diff = max(dot(Normal, lightDir), 0.0);
    vec3 diffuse = diffuse * diff * diffuseTexture.rgb;

    result = vec4(ambient + diffuse, 1);
}