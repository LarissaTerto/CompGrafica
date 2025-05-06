uniform sampler2D iChannel0;
uniform sampler2D iChannel1;
uniform sampler2D iChannel2;
uniform sampler2D iChannel3;
uniform float iTime;
uniform vec4 iMouse;
uniform vec2 iResolution;
uniform int iFrame;
out vec4 C;

#define PI 3.1459

/*  Install  Istructions

sudo apt-get install g++ cmake git
 sudo apt-get install libsoil-dev libglm-dev libassimp-dev libglew-dev libglfw3-dev libxinerama-dev libxcursor-dev
libxi-dev libfreetype-dev libgl1-mesa-dev xorg-dev

git clone https://github.com/JoeyDeVries/LearnOpenGL.git*/


float dist(vec2 p, float r)
{
    float d= length(p)-r;
    return smoothstep(r,r+0.001,d);
}


void main ()
{
    vec2 p = gl_FragCoord.xy/iResolution.xy;
     vec3 col =mix(vec3(0.9,0.3,0.1),vec3(0.7,0.5,0.3),p.y);
     float ra =iResolution.x/iResolution.y;
     p.x*=ra;
     float r =0.06;
     vec2 q = p -vec2(0.5,0.7);
     r+=0.027*cos(atan(q.x,q.y)*15-20*q.x+ 2*sin(iTime*1.2));
     r+=0.01*sin(atan(q.x,q.y)*90.0);
      col*=dist(q,r);

      r=0.02;
      r+= 0.005*cos(q.y*130);

      r+=exp(-40*p.y);
     col*=1.0-(1.0-smoothstep(r,r+0.001,abs(q.x-0.3*sin(q.y*2.2))))*(1.0-smoothstep(0.0,0.1,q.y));

    vec2 s = vec2(1.0, 0.4);
    vec2 t = p - s;
    col = mix(
        vec3(1.0, 0.8, 0.0),
        col,
        dist(t, 0.1+sin(atan(t.x, t.y)*180.0)*0.005)
    );

C = vec4(col,1.0);

}


