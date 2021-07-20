#version 400 core

in vec3 Position;
in vec2 TexCoord;
in vec3 Normal;
in vec3 Tangent;
//in vec4 Weights;
//in ivec4 BoneIndices;

out VertexShaderOutput
{
	vec2 TexCoord;
	vec3 FragPosition;
	vec3 ViewPosition;
	vec3 LightDirection;
} vs_out;

uniform vec3 viewPosition;
uniform vec3 lightDirection;
uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main()
{
	vec4 position = vec4(Position, 1.0) * modelMatrix;
	gl_Position = position * viewMatrix * projectionMatrix;
	vec3 tangent = normalize((modelMatrix * vec4(Tangent, 0.0)).xyz);
	vec3 normal = normalize((modelMatrix * vec4(Normal, 0.0)).xyz);
	vec3 bitangent = normalize(cross(normal, tangent));

	mat3 TBN = transpose(mat3(tangent, bitangent, normal));
	vs_out.LightDirection = TBN * lightDirection;
	vs_out.ViewPosition = TBN * viewPosition;
	vs_out.FragPosition = TBN * position.xyz;
	vs_out.TexCoord = TexCoord;
}