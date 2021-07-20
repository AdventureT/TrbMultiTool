#version 400 core

const float alpha_threshold = 0.5;
const float ambient = 0.5;
const float diffuse = 0.4;
const float specularMultiplier = 1;
const float shininess = 32;

struct Material
{
	sampler2D diffuseTexture;
	sampler2D normalTexture;
	sampler2D specularTexture;
	vec3 specular;

	vec4 color;
};

in VertexShaderOutput
{
	vec2 TexCoord;
	vec3 FragPosition;
	vec3 ViewPosition;
	vec3 LightDirection;
} fs_in;


uniform Material material;
uniform bool hasNormals;

out vec4 result;

float saturate(float value)
{
	return clamp(value, 0, 1);
}

float calculateGloss(float p1, float p2)
{
	return saturate(log(exp(saturate(p1 * 0.058824f) * -17) + p2) * -0.058824f);
}

void main()
{
	vec4 diffuseTexture = texture(material.diffuseTexture, fs_in.TexCoord);
	if(diffuseTexture.a < 0.5)
		discard;

	if(hasNormals)
	{
		vec3 ambient = diffuseTexture.rgb * ambient;

		vec3 normal = normalize(texture(material.normalTexture, fs_in.TexCoord).rgb * 2.0 - 1.0);
		vec3 lightDir = normalize(fs_in.LightDirection);

		float diff = max(dot(normal, lightDir), 0.0);
		vec3 diffuse = diffuse * diff * diffuseTexture.rgb;

		vec3 viewDir = normalize(fs_in.ViewPosition - fs_in.FragPosition);
		vec3 reflectDir = reflect(-lightDir, normal);
		float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess); // glossiness // shininess // whateveriness
		vec3 specular = material.specular * spec * texture(material.specularTexture, fs_in.TexCoord).rgb * specularMultiplier;

		result = vec4(ambient + diffuse + (specular * diffuseTexture.a), diffuseTexture.a);
	}
	else
	{
		result = diffuseTexture;
	}
}