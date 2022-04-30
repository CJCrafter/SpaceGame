using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GravityObject))]
public class BlackHole : MonoBehaviour {
    
    public float lensingRadius = 200f;
    public float mass = 10;
    [Min(0)] public float step = 5f;
    [Min(0)] public int maxSteps = 1000;
    [Min(0)] public float accretionWidth;
    [UnityEngine.Range(0, 1)] public float accretionMin;
    [UnityEngine.Range(0, 1)] public float accretionMax;
    public float accretionSpin;
    public Texture accretionTexture;
    public Vector3 accretionDiskBrightness;
    public float test;
    public Stars stars;
    
    public bool updateGravity;
    public bool debug;
    [ConditionalHide("debug")]
    public int x;
    [ConditionalHide("debug")] 
    public int y;

    private ComputeBuffer buffer;
    
    // * ----- MATERIAL PROPERTIES ----- * //
    private Material material;
    private static readonly int Center = Shader.PropertyToID("_center");
    private static readonly int SingularityRadius = Shader.PropertyToID("_singularityRadius");
    private static readonly int LensingRadius = Shader.PropertyToID("_lensingRadius");
    private static readonly int Gravity = Shader.PropertyToID("_mass");
    private static readonly int Step = Shader.PropertyToID("_step");
    private static readonly int MaxSteps = Shader.PropertyToID("_maxSteps");
    private static readonly int DiskWidth = Shader.PropertyToID("_diskWidth");
    private static readonly int DiskMin = Shader.PropertyToID("_diskInner");
    private static readonly int DiskMax = Shader.PropertyToID("_diskOuter");
    private static readonly int AccretionDiskBrightness = Shader.PropertyToID("_accretionDiskBrightness");
    private static readonly int DiskSpin = Shader.PropertyToID("_diskSpin");
    private static readonly int DiskTexture = Shader.PropertyToID("_accretionDiskTexture");
    private static readonly int Planets = Shader.PropertyToID("_planets");
    private static readonly int PlanetCount = Shader.PropertyToID("_planetCount");
    private static readonly int _Stars = Shader.PropertyToID("_starTexture");


    private void Start() {
        Stuff();
    }
    
    private void OnValidate() {
        if (updateGravity) {
            
            // Einstein's equation for calculating the change in the angle of
            // a light vector GIVEN mass/distance. Solve for r. 
            // angle = (4mG) / (r * c^2)
            // r = (4mG) / (angle * c^2)
            const float G = Universe.gravitationalConstant;
            const float C = 300_000_000f;
            lensingRadius = 4f * mass * G / (1f * C * C);
            transform.localScale = new Vector3(lensingRadius, lensingRadius, lensingRadius);
        }
        
        Stuff();
    }

    private void Update() {
        Stuff();    
    }

    private void Stuff() {
        UpdateShader();

        if (debug && Application.isPlaying) {
            Camera cam = Camera.main;
            Vector3 center = transform.position;
            
            //Sphere singularitySphere = new Sphere(center, singularityRadius);
            Sphere lensingSphere = new Sphere(center, lensingRadius);
            //Box accretionDisk = new Box(center - this.accretionDisk, center + this.accretionDisk);
            
            Ray ray = new Ray(cam.transform.position, (center + new Vector3(x, y) - cam.transform.position).normalized);

            float lenseDistance = lensingRadius * lensingRadius;
            //float singularityDistance = singularityRadius * singularityRadius;
                
            int i = 0;
            TraceResult result = lensingSphere.Collides(ray);
            bool escaped = false;
            
            if (result.collides) 
                Debug.DrawLine(ray.origin, result.position, Color.magenta);
            
            while (result.collides && i < maxSteps) {
                if (i == 0)
                    ray.origin = result.position;

                Vector3 beforeChanges = ray.origin * 1;
                
                Vector3 between = center - ray.origin;
                float force = Universe.gravitationalConstant * mass / between.sqrMagnitude;
                between = between.normalized * force;

                ray.direction = (ray.direction + between * step).normalized;
                ray.origin += ray.direction * step;

                //if (accretionDisk.Collides(ray, step).collides) {
                //    Debug.DrawLine(beforeChanges, ray.origin, new Color(1f, 0.5f, 0f));
                //    return;
                //}
                
                Debug.DrawLine(beforeChanges, ray.origin, i % 2 == 0 ? Color.cyan : Color.yellow);

                if ((ray.origin - center).sqrMagnitude > lenseDistance) {
                    escaped = true;
                    break;
                } 
                    
                i++;
            }

            const float dst = 500;
            if (escaped) {
                Debug.DrawRay(ray.origin, ray.direction * dst, Color.green);
                return;
            }
                
            Debug.DrawRay(ray.origin, ray.direction * dst, Color.black);
        }
    }

    private void UpdateShader() {
        if (true || material == null) {
            material = new Material(Shader.Find("Unlit/BlackHoleShader"));
            GetComponent<MeshRenderer>().material = material;
        }

        material.SetVector(Center, transform.position);
        material.SetFloat(SingularityRadius, 0.1f);
        material.SetFloat(LensingRadius, lensingRadius);
        material.SetFloat(Gravity, mass);
        material.SetFloat(Step, step);
        material.SetInt(MaxSteps, maxSteps);
        material.SetFloat(DiskWidth, accretionWidth);
        material.SetFloat(DiskMin, accretionMin);
        material.SetFloat(DiskMax, accretionMax);
        material.SetFloat(DiskSpin, accretionSpin);
        material.SetTexture(DiskTexture, accretionTexture);
        material.SetVector(AccretionDiskBrightness, accretionDiskBrightness);
        material.SetFloat("_test", test);
        material.SetTexture(_Stars, stars.target);


        List<PlanetStruct> planets = new List<PlanetStruct>();
        foreach (Planet planet in FindObjectsOfType<Planet>()) {
            PlanetStruct temp = new PlanetStruct();
            temp.origin = planet.transform.position;
            temp.radius = planet.radius;
            planets.Add(temp);
        }

        if (buffer == null || buffer.count != planets.Count) {
            OnDisable();
            buffer = new ComputeBuffer(planets.Count, System.Runtime.InteropServices.Marshal.SizeOf(typeof(PlanetStruct)), ComputeBufferType.Default);
        }
        
        buffer.SetData(planets);
        material.SetInt(PlanetCount, planets.Count);
        material.SetBuffer(Planets, buffer);
    }

    private void OnDisable() {
        buffer?.Release();
    }

    private struct PlanetStruct {
        internal Vector3 origin;
        internal float radius;
    }

    private struct Ray {
        internal Vector3 origin;
        internal Vector3 direction;

        public Ray(Vector3 origin, Vector3 direction) {
            this.origin = origin;
            this.direction = direction;
        }
    }

    private struct TraceResult {
        internal static TraceResult EMPTY = new TraceResult();

        internal bool collides;
        internal Vector3 position;
        internal Vector3 normal;
        internal float distance;
    }
    
    private struct Sphere {
        private Vector3 position;
        private float radius;

        public Sphere(Vector3 position, float radius) {
            this.position = position;
            this.radius = radius;
        }

        public TraceResult Collides(Ray ray) {
            Vector3 d = ray.origin - position;
            float p1 = -Vector3.Dot(ray.direction, d);
            float p2sqr = p1 * p1 - Vector3.Dot(d, d) + radius * radius;
            if (p2sqr < 0)
                return TraceResult.EMPTY;

            float p2 = Mathf.Sqrt(p2sqr);
            float t = p1 - p2 > 0 ? p1 - p2 : p1 + p2;
            
            TraceResult result;
            result.collides = true;
            result.distance = t;
            result.position = ray.origin + t * ray.direction;
            result.normal = (result.position - position).normalized;
            return result;
        }
    }

    private struct Box {
        internal Vector3 min;
        internal Vector3 max;

        public Box(Vector3 min, Vector3 max) {
            this.min = min;
            this.max = max;
        }

        public TraceResult Collides(Ray ray, float distance) {
            float tmin = float.MinValue;
            float tmax = float.MaxValue;
            
            if (ray.direction.x != 0.0) {
                float tx1 = (min.x - ray.origin.x) / ray.direction.x;
                float tx2 = (max.x - ray.origin.x) / ray.direction.x;

                tmin = Mathf.Max(tmin, Mathf.Min(tx1, tx2));
                tmax = Mathf.Min(tmax, Mathf.Max(tx1, tx2));
            }

            if (ray.direction.y != 0.0) {
                float ty1 = (min.y - ray.origin.y) / ray.direction.y;
                float ty2 = (max.y - ray.origin.y) / ray.direction.y;

                tmin = Mathf.Max(tmin, Mathf.Min(ty1, ty2));
                tmax = Mathf.Min(tmax, Mathf.Max(ty1, ty2));
            }

            if (ray.direction.z != 0.0) {
                float tz1 = (min.z - ray.origin.z) / ray.direction.z;
                float tz2 = (max.z - ray.origin.z) / ray.direction.z;

                tmin = Mathf.Max(tmin, Mathf.Min(tz1, tz2));
                tmax = Mathf.Min(tmax, Mathf.Max(tz1, tz2));
            }


            float t = tmin > 0 ? tmin : tmax;
            if (t > distance)
                return TraceResult.EMPTY;
    
            TraceResult result;
            result.collides = tmax > tmin;
            result.distance = t;
            result.position = ray.origin + t * ray.direction;
            result.normal = Vector3.zero; // TODO
            return result;
        }
    }
}
