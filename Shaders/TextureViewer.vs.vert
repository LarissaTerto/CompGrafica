#version 330 core
layout(location = 0) in vec2 aPos; // Bird base position
layout(location = 1) in vec2 aWing; // Wing vertices

uniform float iTime; // Animation time
uniform vec2 iResolution;

void main()
{
    // Bird speed settings
    float speed = 0.3; // General speed moving left to right
    float flapSpeed = 5.0; // Wing flapping frequency
    float flapIntensity = 0.05; // How much wings flap
    
    // Create two birds with different timing offsets
    float bird1Offset = 0.0;   // Leading bird
    float bird2Offset = -0.3;  // Following bird (behind banner)

    vec2 birdPosition = aPos;
    
    // Apply horizontal movement
    birdPosition.x += speed * iTime + bird1Offset;
    
    // Apply vertical motion for wing flapping
    birdPosition.y += flapIntensity * sin(iTime * flapSpeed);

    // Apply offset for second bird
    vec2 bird2Position = aPos;
    bird2Position.x += speed * iTime + bird2Offset;
    bird2Position.y += flapIntensity * sin(iTime * flapSpeed + 1.2); // Slightly desynchronized wing flapping

    // Send correct position based on which bird we're rendering
    if (gl_VertexID % 2 == 0)
        gl_Position = vec4