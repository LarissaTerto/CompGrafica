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


float minDist = 0.01;
float maxDist = 100.;
int maxIt = 100;

float sphereDist(vec3 p)
{
    vec4 s =vec4(0.0,1.0,5.0,1.0);
    return length(s.xyz-p)-s.w;
}

float planeDist(vec3 p)
{
    vec4 plane = vec4(0.0,1.0,0.0,0.0);
    return dot(p,plane.xyz)-plane.w;
}

float getSceneDist(vec3 p)
{
    float sd = sphereDist(p);
    float pd = planeDist(p);
    return min(sd,pd);
}

float rayMarching(vec3 ro,vec3 rd)
{
    int i=0;
    float da=0.0;
    vec3 p = ro+da*rd;
    float d_o =getSceneDist(p);
    while ((da<maxDist)&&(d_o>minDist)&&(i<maxIt))
    {
        da+=d_o;
        p =ro+da*rd;
        d_o =getSceneDist(p);
        i++;
    }
    if((i<maxIt)&&(da<maxDist))
        return da;
    else
        return maxDist;
}

vec3 estimateNormal(vec3 p)
{
    float ep = 0.01;
    float d = getSceneDist(p);
    vec3 n =vec3 (d-getSceneDist(vec3(p.x-ep,p.y,p.z)),d-getSceneDist(vec3(p.x,p.y-ep,p.z)),d-getSceneDist(vec3(p.x,p.y,p.z-ep)));
    return normalize(n);
}


float getLight(vec3 p)
{
    vec3 lp = vec3 (0.0,6.0,5.0);
    lp.xz+=vec2(sin(iTime),cos(iTime))*3.0;
    vec3 ld = normalize(lp-p);
    vec3 n = estimateNormal(p);
    float r =clamp(dot(ld,n),0.0,1.0);
    float s =rayMarching(p+1.0*minDist*n,ld);
    if(s<length(p-lp))
        r*=0.2;
    return r;

}



void main ()
{
    vec2 uv = (gl_FragCoord.xy-0.5*iResolution.xy)/iResolution.xy;
    float ra =iResolution.x/iResolution.y;
    uv.x*=ra;

    vec3 ro = vec3(0.0,1.0,0.0);
    vec3 rd =vec3(uv.x,uv.y,1.0);
     float d = rayMarching(ro,rd);
    vec3 p = ro+d*rd;
    float l = getLight(p);
   vec3 col = vec3(l);
C = vec4(col,1.0);

}





