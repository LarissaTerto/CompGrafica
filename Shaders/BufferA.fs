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
    vec2 birdSize = vec2(0.2, 0.15);
    float birdX = 1.2 - (iTime - xOffset) / 15.0;
    vec2 birdPos = vec2(birdX, yPos);

    // Add wing flap scaling (oscillate between 85% and 115% height)
    float flap = 1.0 + 0.15 * sin((iTime - xOffset) * 6.0); // Speed & amplitude of flap
    vec2 scaledSize = vec2(birdSize.x, birdSize.y * flap);

    vec2 birdUV = vec2(
        (uv.x - birdPos.x) / scaledSize.x + 0.5,
        -(uv.y - birdPos.y) / scaledSize.y + 0.5
    );

    bool inside = all(greaterThanEqual(birdUV, vec2(0.0))) && all(lessThanEqual(birdUV, vec2(1.0)));
    return inside ? texture(birdTex, birdUV) : vec4(0.0);
}

vec4 birdShadow(vec2 uv, vec2 offset, float xOffset, float yPos, float iTime) {
    vec2 birdSize = vec2(0.2, 0.15);
    float birdX = 1.2 - (iTime - xOffset) / 15.0;
    vec2 birdPos = vec2(birdX, yPos) + offset;  // Add the shadow offset here

    // Optional: match wing flapping
    float flap = 1.0 + 0.15 * sin((iTime - xOffset) * 6.0);
    vec2 scaledSize = vec2(birdSize.x, birdSize.y * flap);

    vec2 birdUV = vec2(
        (uv.x - birdPos.x) / scaledSize.x + 0.5,
        -(uv.y - birdPos.y) / scaledSize.y + 0.5
    );

    bool inside = all(greaterThanEqual(birdUV, vec2(0.0))) && all(lessThanEqual(birdUV, vec2(1.0)));

    // Return dark transparent shadow
    return inside ? vec4(0.0, 0.0, 0.0, 0.3) * texture(iChannel1, birdUV).a : vec4(0.0);
}

vec4 quati(vec2 uv, sampler2D animalTex, float xOffset, float baseY, float iTime) {
    vec2 animalSize = vec2(0.38, 0.32);  // On-screen size of the animal
    int frameCount = 2;                  // Two frames in the sprite sheet
    float frameWidth = 1.0 / float(frameCount);

    // Calculate horizontal movement across the screen
    float animalX = -0.2 + (iTime - xOffset) / 20.0;

    // Hop arc using sine-squared bounce
    float t = mod(iTime - xOffset, 1.0);
    float hop = 0.02 * pow(sin(t * 3.1415), 2.0);
    vec2 animalPos = vec2(animalX, baseY + hop);

    // Map screen UV to local animal UV (flipped vertically)
    vec2 animalUV = vec2(
        (uv.x - animalPos.x) / animalSize.x + 0.5,
        -(uv.y - animalPos.y) / animalSize.y + 0.5
    );

    // Sync animation frame with hop phase
    float legPhase = sin(t * 3.1415);         // Smooth wave: 0 -> 1 -> 0
    float frameIndex = legPhase > 0.5 ? 1.0 : 0.0;

    // Shift into the correct frame on the sprite sheet
    vec2 frameUV = vec2(
        animalUV.x / float(frameCount) + frameIndex * frameWidth,
        animalUV.y
    );

    // Bound checks
    bool inside = all(greaterThanEqual(animalUV, vec2(0.0))) &&
                  all(lessThanEqual(animalUV, vec2(1.0)));

    if (animalX > 1.2 || animalX < -0.3 || !inside) return vec4(0.0);
    return texture(animalTex, frameUV);
}

vec4 birdBanner(vec2 uv, float iTime) {
    float bird1X = 1.2 - (iTime / 15.0);
    float bird2X = 1.2 - ((iTime - 5.0) / 15.0);
    float bannerY = 0.17; // just below their bodies

    vec2 p1 = vec2(bird1X, bannerY);
    vec2 p2 = vec2(bird2X, bannerY);

    // Define rectangular UV space between p1 and p2
    float width = 0.7 * abs(p2.x - p1.x);
    float height = 0.1;

    // Transform screen UV to banner UV
    vec2 bannerCenter = (p1 + p2) * 0.5;
    vec2 bannerUV = (uv - bannerCenter) / vec2(width, height) + 0.5;

    bool inside = all(greaterThanEqual(bannerUV, vec2(0.0))) &&
                  all(lessThanEqual(bannerUV, vec2(1.0)));

    return inside ? texture(iChannel0, bannerUV) : vec4(0.0);
}

vec4 quatiBanner(vec2 uv, float iTime) {
    float q1X = -0.2 + (iTime - 17.0) / 20.0;
    float q2X = -0.2 + (iTime - 21.0) / 20.0;

    float t1 = mod(iTime - 15.0, 1.0);
    float t2 = mod(iTime - 20.0, 1.0);

    float hop1 = 0.02 * pow(sin(t1 * 3.1415), 2.0);
    float hop2 = 0.02 * pow(sin(t2 * 3.1415), 2.0);

    vec2 p1 = vec2(q1X + 0.015, 0.08 + hop1 + 0.82); // top of jumper body
    vec2 p2 = vec2(q2X + 0.015, 0.08 + hop2 + 0.82);

    float width = abs(p2.x - p1.x);
    float height = 0.1;
    vec2 bannerCenter = (p1 + p2) * 0.5;

    vec2 bannerUV = (uv - bannerCenter) / vec2(width, height) + 0.5;

    bool inside = all(greaterThanEqual(bannerUV, vec2(0.0))) &&
                  all(lessThanEqual(bannerUV, vec2(1.0)));

    return inside ? texture(iChannel0, bannerUV) : vec4(0.0);
}

void main() {
    vec2 p = gl_FragCoord.xy / vec2(1024.0, 678.0);
    vec2 uv = vec2(p.x, 1.0 - p.y);

    vec4 background = texture(iChannel0, uv);

    vec4 shadow1 = birdShadow(uv, vec2(+0.02, +0.02), 0.0, 0.15, iTime);
    vec4 shadow2 = birdShadow(uv, vec2(+0.02, +0.02), 5.0, 0.15, iTime);

    // Then draw birds over the shadows
    vec4 bird1 = flyingBird(uv, iChannel1, 0.0, 0.15, iTime);
    vec4 bird2 = flyingBird(uv, iChannel1, 5.0, 0.15, iTime);

    vec4 allBirds = bird1 + bird2;
    vec4 allShadows = shadow1 + shadow2;

    vec4 quati1 = vec4(0.0);
    vec4 quati2 = vec4(0.0);

    if (iTime > 5.0) {
        quati1 = quati(uv, iChannel2, 15.0, 0.85, iTime);
        quati2 = quati(uv, iChannel2, 23.0, 0.85, iTime);
    }

    vec4 birds = bird1 + bird2;
    vec4 shadows = shadow1 + shadow2;
    vec4 quatis = quati1 + quati2;

    vec4 bannerBird = birdBanner(uv, iTime);
    vec4 bannerQuati = (iTime > 20.0) ? quatiBanner(uv, iTime) : vec4(0.0);

    vec4 layered = shadows + birds + quatis+ bannerBird + bannerQuati;
    C = mix(background, layered, clamp(layered.a, 0.0, 1.0));
}
