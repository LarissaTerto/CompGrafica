uniform sampler2D iChannel0;
uniform sampler2D iChannel1;
uniform sampler2D iChannel2;
uniform sampler2D iChannel3;
uniform float iTime;
uniform vec4 iMouse;
uniform vec2 iResolution;
uniform int iFrame;

#define PI 3.1459

out vec4 FragColor;
in vec2 fragCoord;

void main()
{
    // Debug colors to identify objects
    if (fragCoord.y > 0.8) // Simulate bird height
        FragColor = vec4(1.0, 0.0, 0.0, 1.0); // Red (birds)
    else if (fragCoord.x > 0.3 && fragCoord.x < 0.7) // Simulate banner in center
        FragColor = vec4(0.0, 0.0, 1.0, 1.0); // Blue (banner)
    else
        FragColor = vec4(0.1, 0.6, 0.2, 1.0); // Green (grass)
}


