using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phaser : MonoBehaviour {
    [Header("Prefabs")] public GameObject beamLineRendererPrefab; //Put a prefab with a line renderer onto here.
    public GameObject beamStartPrefab; //This is a prefab that is put at the start of the beam.
    public GameObject beamEndPrefab; //Prefab put at end of beam.

    private GameObject beamStart;
    private GameObject beamEnd;
    private GameObject beam;
    private LineRenderer line;
    public RaycastHit hit;
    
    [Header("Beam Options")] public float distance = 100; //Ingame beam length
    
    
    void Update() {
        if (!beam) 
            return;
        
        
        line.SetPosition(0, transform.position);

        Vector3 end;
        if (Physics.Raycast(transform.position, transform.forward, out hit, this.distance))
            end = hit.point;
        else
            end = transform.position + (transform.forward * this.distance);
        
        line.SetPosition(1, end);

        if (beamStart) {
            beamStart.transform.position = transform.position;
            beamStart.transform.LookAt(end);
        }

        if (beamEnd) {
            beamEnd.transform.position = end;
            beamEnd.transform.LookAt(beamStart.transform.position);
        }

        float distance = Vector3.Distance(transform.position, end);
        line.material.mainTextureScale =
            new Vector2(distance, 1);
    }

    public void SpawnBeam() {
        if (beamLineRendererPrefab) {
            if (beamStartPrefab)
                beamStart = Instantiate(beamStartPrefab);
            if (beamEndPrefab)
                beamEnd = Instantiate(beamEndPrefab);
            beam = Instantiate(beamLineRendererPrefab);
            beam.transform.position = transform.position;
            beam.transform.parent = transform;
            beam.transform.rotation = transform.rotation;
            line = beam.GetComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
        }
        else
            print("Add a hecking prefab with a line renderer to the SciFiBeamStatic script on " + gameObject.name +
                  "! Heck!");
    }

    public void RemoveBeam() //This function removes the prefab with linerenderer
    {
        if (beam)
            Destroy(beam);
        if (beamStart)
            Destroy(beamStart);
        if (beamEnd)
            Destroy(beamEnd);
    }
}