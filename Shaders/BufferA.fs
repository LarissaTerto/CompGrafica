#version 330 core
in vec3 aColor;
in vec4 aPosition;
out vec4 C;
uniform sampler2D iChannel0;
uniform sampler2D iChannel1;
uniform sampler2D iChannel2;
uniform sampler2D iChannel3;
uniform sampler2D iChannel4;
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
    //Tamanho e posição ao longo do tempo
    vec2 birdSize = vec2(0.2, 0.15);
    float birdX = 1.2 - (iTime - xOffset) / 15.0;
    vec2 birdPos = vec2(birdX, yPos);

    //Batida da asa: oscilação de função seno, aplitude 15% e 6 ciclos/segundo
    float flap = 1.0 + 0.15 * sin((iTime - xOffset) * 6.0); 
    vec2 scaledSize = vec2(birdSize.x, birdSize.y * flap);

    //Converte coordenadas da textura (UV) para o espaço da tela
    vec2 birdUV = vec2(
        (uv.x - birdPos.x) / scaledSize.x + 0.5,
        -(uv.y - birdPos.y) / scaledSize.y + 0.5
    );
    
    //Retorna textura no espaço definido
    bool inside = all(greaterThanEqual(birdUV, vec2(0.0))) && all(lessThanEqual(birdUV, vec2(1.0)));
    return inside ? texture(birdTex, birdUV) : vec4(0.0);
}

vec4 birdShadow(vec2 uv, vec2 offset, float xOffset, float yPos, float iTime) {
    //Tamanho e posição ao longo do tempo em relação à posição de pássaro
    vec2 birdSize = vec2(0.2, 0.15);
    float birdX = 1.2 - (iTime - xOffset) / 15.0;
    vec2 birdPos = vec2(birdX, yPos) + offset;  

    //Mesmo movimento do pássaro
    float flap = 1.0 + 0.15 * sin((iTime - xOffset) * 6.0);
    vec2 scaledSize = vec2(birdSize.x, birdSize.y * flap);

    //Aplica sombra (cor acinzentada) dentro do espaço da textura do pássaro
    vec2 birdUV = vec2(
        (uv.x - birdPos.x) / scaledSize.x + 0.5,
        -(uv.y - birdPos.y) / scaledSize.y + 0.5
    );

    bool inside = all(greaterThanEqual(birdUV, vec2(0.0))) && all(lessThanEqual(birdUV, vec2(1.0)));
    return inside ? vec4(0.0, 0.0, 0.0, 0.3) * texture(iChannel1, birdUV).a : vec4(0.0);
}

vec4 quati(vec2 uv, sampler2D animalTex, float xOffset, float baseY, float iTime) {
    //Tamanho na tela e tamanha de cada frame do sprite sheet da textura
    vec2 animalSize = vec2(0.22, 0.22);  
    int frameCount = 3;                 
    float frameWidth = 1.0 / float(frameCount);

    // Movimento horizontal + "pulos" com função seno
    float animalX = -0.2 + (iTime - xOffset) / 16.0;
    float t = mod(iTime - xOffset, 1.0);
    float hop = 0.02 * pow(sin(t * 3.1415), 2.0);
    vec2 animalPos = vec2(animalX, baseY + hop);

    //Converte coordenadas da textura para o espaço da tela
    vec2 animalUV = vec2(
        (uv.x - animalPos.x) / animalSize.x + 0.5,
        -(uv.y - animalPos.y) / animalSize.y + 0.5
    );

    //Calcula qual frame é usada em cada momento
    float frameRate = 6.0;
    float frameIndex = mod(floor((iTime - xOffset) * frameRate), float(frameCount));

    //Converte coordenadas dos frames para a tela
    float frameUVWidth = 168.0 / 500.0;
    float frameStride   = 168.0 / 500.0;

    vec2 frameUV = vec2(
        animalUV.x * frameUVWidth + frameIndex * frameStride,
        animalUV.y
    );

    //Retorna textura no espaço definido
    bool inside = all(greaterThanEqual(animalUV, vec2(0.0))) && all(lessThanEqual(animalUV, vec2(1.0)));

    if (animalX > 1.2 || animalX < -0.3 || !inside) return vec4(0.0);
    return texture(animalTex, frameUV);
}

vec4 quatiShadow(vec2 uv, sampler2D animalTex, float xOffset, float baseY, float iTime) {
    //Tamanho e posição ao longo do tempo em relação à posição do quati
    vec2 animalSize = vec2(0.22, 0.22);  
    float shadowY = baseY + 0.1;        

    float animalX = -0.2 + (iTime - xOffset) / 16.0;
    vec2 shadowPos = vec2(animalX, shadowY);

    vec2 shadowUV = vec2(
        (uv.x - shadowPos.x) / animalSize.x + 0.5,
        (uv.y - shadowPos.y) / (animalSize.y * 0.5) + 0.5  
    );

    //Mesma lógica de frames 
    float frameRate = 6.0;
    int frameCount = 3;
    float frameIndex = mod(floor((iTime - xOffset) * frameRate), float(frameCount));
    float frameStride = 168.0 / 500.0;
    float frameUVWidth = 168.0 / 500.0;

    //Aplica sombra (cor acinzentada) dentro do espaço da textura do quati
    vec2 frameUV = vec2(
        shadowUV.x * frameUVWidth + frameIndex * frameStride,
        1.0 - shadowUV.y  
    );

    bool inside = all(greaterThanEqual(shadowUV, vec2(0.0))) && all(lessThanEqual(shadowUV, vec2(1.0)));
    return inside ? vec4(0.0, 0.0, 0.0, 0.3) * texture(animalTex, frameUV).a : vec4(0.0);
}

vec4 birdBanner(vec2 uv, float iTime) {
    //Tamanho e posição ao longo do tempo em relação à posição do pássaro
    float bird1X = 1.2 - (iTime / 15.0);
    float bird2X = 1.2 - ((iTime - 7.0) / 15.0);
    float bannerY = 0.17;

    vec2 p1 = vec2(bird1X, bannerY);
    vec2 p2 = vec2(bird2X, bannerY);

    //Converte coordenadas da textura para o espaço da tela
    float width = 0.7 * abs(p2.x - p1.x);
    float height = 0.1;

    vec2 bannerCenter = (p1 + p2) * 0.5;
    vec2 bannerUV = (uv - bannerCenter) / vec2(width, height) + 0.5;
    vec2 correctedUV = vec2(bannerUV.x, 1.0 - bannerUV.y);

    //Movimento suave horizontal com função seno
    float windFreq = 4.0;      //Frequnência
    float windSpeed = 0.5;     //Velocidade
    float windAmp = 0.015;     //Amplitude

    float wave = sin((correctedUV.y + iTime * windSpeed) * windFreq) * windAmp;
    vec2 wavingUV = vec2(correctedUV.x + wave, correctedUV.y);

    //Retorna textura no espaço definido
    bool inside = all(greaterThanEqual(correctedUV, vec2(0.0))) && all(lessThanEqual(correctedUV, vec2(1.0)));

    return inside ? texture(iChannel4, wavingUV) : vec4(0.0);
}

vec4 birdBannerShadow(vec2 uv, float iTime) {
    //Tamanho e posição ao longo do tempo em relação à posição do banner
    float bird1X = 1.2 - (iTime / 15.0);
    float bird2X = 1.2 - ((iTime - 7.0) / 15.0);
    float bannerY = 0.17 + 0.75; 

    vec2 p1 = vec2(bird1X, bannerY);
    vec2 p2 = vec2(bird2X, bannerY);

    //Aplica sombra (cor acinzentada) dentro do espaço da textura do banner
    float width = 0.7 * abs(p2.x - p1.x);
    float height = 0.1 * 0.5; 

    vec2 bannerCenter = (p1 + p2) * 0.5;
    vec2 bannerUV = (uv - bannerCenter) / vec2(width, height) + 0.5;
    vec2 correctedUV = vec2(bannerUV.x, 1.0 - bannerUV.y);

    //Mesmo movimento do banner
    float windFreq = 4.0;
    float windSpeed = 0.5;
    float windAmp = 0.015;
    float wave = sin((correctedUV.y + iTime * windSpeed) * windFreq) * windAmp;
    vec2 wavingUV = vec2(correctedUV.x + wave, correctedUV.y);

    //Aplica sombra (cor acinzentada) dentro do espaço da textura do banner
    bool inside = all(greaterThanEqual(correctedUV, vec2(0.0))) && all(lessThanEqual(correctedUV, vec2(1.0)));
    return inside ? vec4(0.0, 0.0, 0.0, 0.25) * texture(iChannel4, wavingUV).a : vec4(0.0);
}

vec4 quatiBanner(vec2 uv, float iTime) {
    //Tamanho e posição ao longo do tempo em relação à posição do pássaro
    float q1X = -0.2 + (iTime - 17.0) / 16.0;
    float q2X = -0.2 + (iTime - 22.0) / 16.0;

    float t1 = mod(iTime - 15.0, 1.0);
    float t2 = mod(iTime - 20.0, 1.0);

    //Acompanha movimento do quati
    float hop1 = 0.02 * pow(sin(t1 * 3.1415), 2.0);
    float hop2 = 0.02 * pow(sin(t2 * 3.1415), 2.0);

    vec2 p1 = vec2(q1X + 0.015, 0.08 + hop1 + 0.82);
    vec2 p2 = vec2(q2X + 0.015, 0.08 + hop2 + 0.82);

    //Converte coordenadas da textura para o espaço da tela
    float width = abs(p2.x - p1.x);
    float height = 0.1;
    vec2 bannerCenter = (p1 + p2) * 0.5;
    
    vec2 bannerUV = (uv - bannerCenter) / vec2(width, height) + 0.5;
    vec2 correctedUV = vec2(bannerUV.x, 1.0 - bannerUV.y);

    //Movimento suave horizontal com função seno
    float windFreq = 5.0;
    float windSpeed = 0.6;
    float windAmp = 0.015;
    float wave = sin((correctedUV.y + iTime * windSpeed) * windFreq) * windAmp;
    vec2 wavingUV = vec2(correctedUV.x + wave, correctedUV.y);

    //Retorna textura no espaço definido
    bool inside = all(greaterThanEqual(correctedUV, vec2(0.0))) && all(lessThanEqual(correctedUV, vec2(1.0)));
    return inside ? texture(iChannel3, wavingUV) : vec4(0.0);
}

vec4 quatiBannerShadow(vec2 uv, float iTime) {
     //Tamanho e posição ao longo do tempo em relação à posição do banner
    float q1X = -0.2 + (iTime - 17.0) / 16.0;
    float q2X = -0.2 + (iTime - 22.0) / 16.0;

    float t1 = mod(iTime - 15.0, 1.0);
    float t2 = mod(iTime - 20.0, 1.0);

    //Acompanha movimento do quati
    float hop1 = 0.02 * pow(sin(t1 * 3.1415), 2.0);
    float hop2 = 0.02 * pow(sin(t2 * 3.1415), 2.0);

    vec2 p1 = vec2(q1X + 0.015, 0.08 + hop1 + 0.82 + 0.1); 
    vec2 p2 = vec2(q2X + 0.015, 0.08 + hop2 + 0.82 + 0.1);

    //Aplica sombra (cor acinzentada) dentro do espaço da textura do banner
    float width = abs(p2.x - p1.x);
    float height = 0.1 * 0.5; 

    vec2 bannerCenter = (p1 + p2) * 0.5;
    vec2 bannerUV = (uv - bannerCenter) / vec2(width, height) + 0.5;
    vec2 correctedUV = vec2(bannerUV.x, 1.0 - bannerUV.y);

    //Mesmo movimento do banner
    float windFreq = 5.0;
    float windSpeed = 0.6;
    float windAmp = 0.015;
    float wave = sin((correctedUV.y + iTime * windSpeed) * windFreq) * windAmp;
    vec2 wavingUV = vec2(correctedUV.x + wave, correctedUV.y);

    //Aplica sombra (cor acinzentada) dentro do espaço da textura do banner
    bool inside = all(greaterThanEqual(correctedUV, vec2(0.0))) && all(lessThanEqual(correctedUV, vec2(1.0)));
    return inside ? vec4(0.0, 0.0, 0.0, 0.25) * texture(iChannel3, wavingUV).a : vec4(0.0);
}

void main() {
    //Normaliza coordenadas dos pixels entre 0 e 1, no espaço da imagem de plano de fundo
    vec2 p = gl_FragCoord.xy / vec2(1024.0, 678.0);
    
    //APlica textura de fundo e gira imagem para ficar na posição correta
    vec2 uv = vec2(p.x, 1.0 - p.y);
    vec4 background = texture(iChannel0, uv);

    //Adiciona elementos de sombra
    vec4 shadow1 = birdShadow(uv, vec2(+0.02, +0.75), 0.0, 0.15, iTime);
    vec4 shadow2 = birdShadow(uv, vec2(+0.02, +0.75), 6.6, 0.15, iTime);
    vec4 quatiShadow1 = quatiShadow(uv, iChannel2, 15.0, 0.85, iTime);
    vec4 quatiShadow2 = quatiShadow(uv, iChannel2, 23.5, 0.85, iTime);
    vec4 bannerShadow = birdBannerShadow(uv, iTime);
    vec4 shadowBannerQuati = (iTime > 20.0) ? quatiBannerShadow(uv, iTime) : vec4(0.0);

    //Adiciona pássaros, um pós o outro
    vec4 bird1 = flyingBird(uv, iChannel1, 0.0, 0.15, iTime);
    vec4 bird2 = flyingBird(uv, iChannel1, 6.6, 0.15, iTime);

    //Adiciona quatis para aparecerem depois dos pássaros, um após o outro
    vec4 quati1 = vec4(0.0);
    vec4 quati2 = vec4(0.0);

    if (iTime > 5.0) {
        quati1 = quati(uv, iChannel2, 15.0, 0.85, iTime);
        quati2 = quati(uv, iChannel2, 23.5, 0.85, iTime);
    }

    vec4 birds = bird1 + bird2;
    vec4 quatis = quati1 + quati2;
    vec4 shadows = shadow1 + shadow2 + quatiShadow1 + quatiShadow2 + bannerShadow + shadowBannerQuati;
  
    //Adiciona banners entre os pássaros e quatis
    vec4 bannerBird = birdBanner(uv, iTime);
    vec4 bannerQuati = (iTime > 20.0) ? quatiBanner(uv, iTime) : vec4(0.0);

    //Composição da cena
    vec4 layered = shadows + birds + quatis+ bannerBird + bannerQuati;
   
    //Insere gradiente de luz ambiente, mais claro no céu e mais escuro embaixo
    float ambient = mix(1.0, 0.7, uv.y); // chão = 70%, céu = 100%

    //Aplica cores do canal RGB
    vec3 finalColor = mix(background.rgb, layered.rgb, clamp(layered.a, 0.0, 1.0));
    finalColor *= ambient;

    //Output final
    C = vec4(finalColor, 1.0);
}
