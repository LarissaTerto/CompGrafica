#version 330 core
in vec3 aColor;
in vec4 aPosition;
out vec4 C;
uniform sampler2D iChannel0;
uniform sampler2D iChannel1;
uniform sampler2D iChannel2;
uniform sampler2D iChannel3;
uniform vec2 iResolution;
uniform vec4 iMouse;
uniform float iTime;
uniform int iFrame;

#define MAX_STEPS 100
#define MAX_DIST 200.
#define SURF_DIST .01
#define EPSILON .01
#define PI 3.14159265359
float dot2( in vec2 v ) { return dot(v,v); }
float dot2( in vec3 v ) { return dot(v,v); }
float ndot( in vec2 a, in vec2 b ) { return a.x*b.x - a.y*b.y; }

vec4 flyingBird(vec2 uv, sampler2D birdTex, float xOffset, float yPos, float iTime) {
    vec2 birdSize = vec2(0.2, 0.15); // you can pass this as another parameter if needed

    float birdX = 1.2 - (iTime - xOffset) / 15.0; // xOffset controls delay
    vec2 birdPos = vec2(birdX, yPos);

    vec2 birdUV = vec2(
        (uv.x - birdPos.x) / birdSize.x + 0.5,
        -(uv.y - birdPos.y) / birdSize.y + 0.5
    );

    bool inside = all(greaterThanEqual(birdUV, vec2(0.0))) &&
                  all(lessThanEqual(birdUV, vec2(1.0)));

    return inside ? texture(birdTex, birdUV) : vec4(0.0);
}

void main() {
    vec2 p = gl_FragCoord.xy / vec2(1024.0, 678.0);
    vec2 uv = vec2(p.x, 1.0 - p.y);

    vec4 background = texture(iChannel0, uv);

    vec4 bird1 = flyingBird(uv, iChannel1, 0.0, 0.15, iTime);    // Appears at t=0
    vec4 bird2 = flyingBird(uv, iChannel1, 5.0, 0.15, iTime);    // Appears at t=5

    vec4 birds = bird1 + bird2;
    C = mix(background, birds, clamp(birds.a, 0.0, 1.0));
}
