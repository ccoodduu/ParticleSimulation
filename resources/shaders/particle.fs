#version 330

in vec2 localCoord;

uniform vec4 colDiffuse;

out vec4 finalColor;

void main()
{
    float distanceSquared = dot(localCoord, localCoord);
    if (distanceSquared > 1.0) discard;

    vec3 normal = vec3(localCoord, sqrt(1.0 - distanceSquared));
    vec3 lightDirection = normalize(vec3(0.4, 0.6, 0.7));
    float diffuse = max(dot(normal, lightDirection), 0.0);

    float ambient = 0.2;
    finalColor = vec4(colDiffuse.rgb * (ambient + (1.0 - ambient) * diffuse), 1.0);
}