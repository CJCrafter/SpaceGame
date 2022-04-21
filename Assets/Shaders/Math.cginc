// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'

static const float PI = 3.14159265359;
static const float TAU = PI * 2;
static const float maxFloat = 3.402823466e+38;
static const float G = 6.6743e-11;

float lengthSquared(float3 v) {
    return v.x * v.x + v.y * v.y + v.z * v.z;
}

float lengthSquared(float3 a, float3 b) {
    const float x = a.x - b.x;
    const float y = a.y - b.y;
    const float z = a.z - b.z;
    return x * x + y * y + z * z;
}

// Remap a value from one range to another
float remap(float v, float minOld, float maxOld, float minNew, float maxNew) {
    return saturate(minNew + (v - minOld) * (maxNew - minNew) / (maxOld - minOld));
}

// Remap the components of a vector from one range to another
float4 remap(float4 v, float minOld, float maxOld, float minNew, float maxNew) {
    return saturate(minNew + (v - minOld) * (maxNew - minNew) / (maxOld - minOld)); //
}

// Remap a float value (with a known mininum and maximum) to a value between 0 and 1
float remap01(float v, float minOld, float maxOld) {
    return saturate((v - minOld) / (maxOld - minOld));
}

// Remap a float2 value (with a known mininum and maximum) to a value between 0 and 1
float2 remap01(float2 v, float2 minOld, float2 maxOld) {
    return saturate((v - minOld) / (maxOld - minOld));
}

// Smooth minimum of two values, controlled by smoothing factor k
// When k = 0, this behaves identically to min(a, b)
float smoothMin(float a, float b, float k) {
    k = max(0, k);
    // https://www.iquilezles.org/www/articles/smin/smin.htm
    float h = max(0, min(1, (b - a + k) / (2 * k)));
    return a * h + b * (1 - h) - k * h * (1 - h);
}

// Smooth maximum of two values, controlled by smoothing factor k
// When k = 0, this behaves identically to max(a, b)
float smoothMax(float a, float b, float k) {
    k = min(0, -k);
    float h = max(0, min(1, (b - a + k) / (2 * k)));
    return a * h + b * (1 - h) - k * h * (1 - h);
}

float Blend(float startHeight, float blendDst, float height) {
    return smoothstep(startHeight - blendDst / 2, startHeight + blendDst / 2, height);
}


// Returns vector (dstToSphere, dstThroughSphere)
// If ray origin is inside sphere, dstToSphere = 0
// If ray misses sphere, dstToSphere = maxValue; dstThroughSphere = 0
float2 raySphere(float3 sphereCentre, float sphereRadius, float3 rayOrigin, float3 rayDir) {
    float3 offset = rayOrigin - sphereCentre;
    float a = 1; // Set to dot(rayDir, rayDir) if rayDir might not be normalized
    float b = 2 * dot(offset, rayDir);
    float c = dot(offset, offset) - sphereRadius * sphereRadius;
    float d = b * b - 4 * a * c; // Discriminant from quadratic formula

    // Number of intersections: 0 when d < 0; 1 when d = 0; 2 when d > 0
    if (d > 0) {
        float s = sqrt(d);
        float dstToSphereNear = max(0, (-b - s) / (2 * a));
        float dstToSphereFar = (-b + s) / (2 * a);

        // Ignore intersections that occur behind the ray
        if (dstToSphereFar >= 0) {
            return float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
        }
    }
    // Ray did not intersect sphere
    return float2(maxFloat, 0);
}

// * ---- RAY TRACING STUFF ----- * //

struct Ray {
    float3 origin;
    float3 direction;
    float3 energy;
};

struct Material {
    float3 specular;
    float3 albedo; 
};

struct RayHit {
    bool skipExtras;
    float3 position;
    float distance;
    float3 normal;
    Material material;
};

struct TraceResult {
    bool collides;
    float3 position;
    float distance;
    float3 normal;
};

struct Box {
    float3 min;
    float3 max;
    Material material;
};

struct Sphere {
    float3 pos;
    float radius;
    Material material;
};


// No constructors in hlsl
Ray CreateRay(float3 origin, float3 direction) {
    Ray ray;
    ray.origin = origin;
    ray.direction = direction;
    ray.energy = float3(1, 1, 1);
    return ray;
}

Material CreateMaterial(float3 albedo, float3 specular) {
    Material material;
    material.albedo = albedo;
    material.specular = specular;
    return material;
}
static const Material defaultMaterial = CreateMaterial(float3(0.8f, 0.8f, 0.8f), float3(0.04, 0.04, 0.04));

RayHit CreateRayHit() {
    RayHit hit;
    hit.skipExtras = false;
    hit.position = float3(0, 0, 0);
    hit.distance = maxFloat;
    hit.normal = float3(0, 0, 0);
    hit.material = defaultMaterial;
    return hit;
}

TraceResult EmptyCollision() {
    TraceResult result;
    result.collides = false;
    result.distance = 0;
    result.normal = 0;
    result.position = 0;
    return result;
}

Box CreateBox(float3 min, float3 max, Material material) {
    Box box;
    box.min = min;
    box.max = max;
    box.material = material;
    return box;
}

Sphere CreateSphere(float3 pos, float radius, Material material) {
    Sphere sphere;
    sphere.pos = pos;
    sphere.radius = radius;
    sphere.material = material;
    return sphere;
}

TraceResult IntersectSphere(Ray ray, Sphere sphere) {

    // Calculate distance along the ray where the sphere is intersected
    float3 d = ray.origin - sphere.pos.xyz;
    float p1 = -dot(ray.direction, d);
    float test = dot(d, d);
    float p2sqr = p1 * p1 - test + sphere.radius * sphere.radius;
    if (p2sqr < 0)
        return EmptyCollision();
    
    float p2 = sqrt(p2sqr);
    float t = p1 - p2 > 0 ? p1 - p2 : p1 + p2;
    if (t > 0) {
        TraceResult result;
        result.collides = true;
        result.distance = t;
        result.position = ray.origin + t * ray.direction;
        result.normal = normalize(result.position - sphere.pos);
        return result;
    }

    return EmptyCollision();
}

TraceResult IntersectSphere(Ray ray, float4 sphere) {
    return IntersectSphere(ray, CreateSphere(sphere.xyz, sphere.w, defaultMaterial));
}

TraceResult IntersectBox(Ray ray, Box box, float distance) {
    float tmin = -maxFloat;
    float tmax = maxFloat;
    
    if (ray.direction.x != 0.0) {
        float tx1 = (box.min.x - ray.origin.x) / ray.direction.x;
        float tx2 = (box.max.x - ray.origin.x) / ray.direction.x;

        tmin = max(tmin, min(tx1, tx2));
        tmax = min(tmax, max(tx1, tx2));
    }

    if (ray.direction.y != 0.0) {
        float ty1 = (box.min.y - ray.origin.y) / ray.direction.y;
        float ty2 = (box.max.y - ray.origin.y) / ray.direction.y;

        tmin = max(tmin, min(ty1, ty2));
        tmax = min(tmax, max(ty1, ty2));
    }

    if (ray.direction.z != 0.0) {
        float tz1 = (box.min.z - ray.origin.z) / ray.direction.z;
        float tz2 = (box.max.z - ray.origin.z) / ray.direction.z;

        tmin = max(tmin, min(tz1, tz2));
        tmax = min(tmax, max(tz1, tz2));
    }

    float t = tmin > 0 ? tmin : tmax;
    if (t > distance)
        return EmptyCollision();
    
    TraceResult result;
    result.collides = tmax > tmin;
    result.distance = t;
    result.position = ray.origin + t * ray.direction;
    result.normal = 0; // TODO
    return result;
}

TraceResult IntersectPlane() {
    
}

TraceResult IntersectDisc() {
    
}