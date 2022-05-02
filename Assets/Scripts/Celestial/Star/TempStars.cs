using System.Collections.Generic;
using UnityEngine;

public class TempStars : MonoBehaviour {

    public int seed;
    [Range(0f, 1f)] public Range brightness;
    [Range(220f, 340f)] public Range size;
    [Min(0f)] public float distance = 100000f;
    [Min(1)] public int numStars = 6000;

    // Used to handle 'white dwarf' and 'red giant' stars
    [Range(0f, 1f)] public float whiteDwarfSize = 0.1f;
    [Range(0f, 1f)] public float whiteDwarfChance = 0.33f;
    [Range(0f, 1f)] public float redGiantSize = 0.9f;
    [Range(0f, 1f)] public float redGiantChance = 0.05f;
    [Min(1f)] public float redGiantMultiplier = 5f;
    
    public void Regenerate() {
        Debug.Log("Regenerating Stars");
        Random.InitState(seed);
        var stars = new List<StarData>(numStars);
        for (int i = 0; i < numStars; i++) {
            stars[i] = GenerateRandom();
        }
        
        // We will need to do some boring compute shader stuff
    }

    private StarData GenerateRandom() {
        float val = Random.value;
        
        float brightness = this.brightness.Lerp(val);
        float radius = this.size.Lerp(val);

        // Handle 'white dwarf' and 'red giant' special cases. Big
        // stars have a small chance to become a red giant, and small
        // stars have a medium chance to become a white dwarf. Red giants
        // additionally have their size multiplied since they tend to be
        // MUCH LARGER then average stars. 
        float time = val;
        if (radius < whiteDwarfSize && Random.value < whiteDwarfChance) {
            time = 0.5f; // white
        }
        else if (radius > redGiantSize && Random.value < redGiantChance) {
            radius *= redGiantMultiplier;
            time = 0.0f; // red
        }
        
        return new StarData()
        {
            position = Random.onUnitSphere * distance,
            time = time,
            brightness = brightness,
            radius = radius
        };
    }
    
    private class StarData {
        internal Vector3 position;
        internal float time; // Number between 0 and 1 to determine color
        internal float brightness;
        internal float radius;
    }

}