#version 330 core
out vec4 FragColor;

struct Light {
	
	vec3 position;
	vec3 ambient;
    vec3 diffuse;
    vec3 specular;
	vec3 colour;

	float constant;
    float linear;
    float quadratic;
};

struct SpotLight {
	vec3 position;
    vec3 direction;
	vec3 colour;
    float cutOff;
    float outerCutOff;

	float constant;
    float linear;
    float quadratic;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular; 
};

in vec3 FragPos;  
in vec3 Normal;  
in vec2 TexCoords;

uniform vec3 viewPos;
uniform Light light;
uniform SpotLight spot;
uniform sampler2D texture_diffuse1;
uniform sampler2D texture_specular1;

vec3 calculatePointLightColour(Light light, vec3 normal, vec3 fragPos, vec3 viewDir);
vec3 calculateSpotLightColour(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir);

void main()
{    
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);

	vec3 result = calculatePointLightColour(light, norm, FragPos, viewDir);
    result += calculateSpotLightColour(spot, norm, FragPos, viewDir);    
    
    FragColor = vec4(result, 1.0);
}

vec3 calculatePointLightColour(Light light, vec3 normal, vec3 fragPos, vec3 viewDir){

	vec3 lightDir = normalize(light.position - fragPos);

    float diff = max(dot(normal, lightDir), 0.0);

    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 64.0f);
 
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    

    vec3 ambient = light.ambient * vec3(texture(texture_diffuse1, TexCoords));
    vec3 diffuse = light.colour * light.diffuse * diff * vec3(texture(texture_diffuse1, TexCoords));
    vec3 specular = light.colour * light.specular * spec * vec3(texture(texture_specular1, TexCoords));
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
    return (ambient + diffuse + specular);
}

vec3 calculateSpotLightColour(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir){
	
	vec3 lightDir = normalize(light.position - fragPos);

    float diff = max(dot(normal, lightDir), 0.0);

    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 64.0f);

    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    

    float theta = dot(lightDir, normalize(-light.direction)); 
    float epsilon = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);

    vec3 ambient = light.ambient * vec3(texture(texture_diffuse1, TexCoords));
    vec3 diffuse = light.colour * light.diffuse * diff * vec3(texture(texture_diffuse1, TexCoords));
    vec3 specular = light.colour * light.specular * spec * vec3(texture(texture_specular1, TexCoords));
    ambient *= attenuation * intensity;
    diffuse *= attenuation * intensity;
    specular *= attenuation * intensity;
    return (ambient + diffuse + specular);
}