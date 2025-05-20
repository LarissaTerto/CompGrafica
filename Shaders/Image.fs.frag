#version 330 core
out vec4 FragColor;
in vec2 fragCoord;

uniform vec2 iResolution;

void main()
{
    // Normalize coordinates (0 to 1 range)
    vec2 uv = fragCoord / iResolution;

    // Sky gradient (light blue at top, deeper blue towards bottom)
    vec3 skyColor = mix(vec3(0.6, 0.9, 1.0), vec3(0.2, 0.6, 1.0), uv.y);

    // Grass at the bottom
    vec3 grassColor = vec3(0.1, 0.6, 0.2);
    if (uv.y < 0.2) // Grass takes up the bottom 20% of the screen
        skyColor = grassColor;

    // Output color
    FragColor = vec4(skyColor, 1.0);
}
