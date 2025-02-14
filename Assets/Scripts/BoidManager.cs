﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BoidManager : MonoBehaviour {


    public static int score;
    [HideInInspector]
    TextMeshProUGUI text;


    const int threadGroupSize = 1024;

    public BoidSettings settings;
    public ComputeShader compute;
    List<Boid> boids;

    public Transform target;

    void Start () {
        boids = new List<Boid>(FindObjectsOfType<Boid> ());
        foreach (Boid b in boids) {
            b.Initialize (settings, target);
            b.DeathEvent += BoidGet;
        }
        score = 0;
        text = transform.parent.FindDeepChild("Score").GetComponent<TextMeshProUGUI>();
    }

    void BoidGet(Boid boid)
    {
        boids.Remove(boid);
        score++;
        if (text)
            text.text = $"Score:{score}";
    }

    void Update () {
        if (boids != null) {

            int numBoids = boids.Count;
            var boidData = new BoidData[numBoids];

            for (int i = boids.Count-1; i >= 0; i--) {
                if (!boids[i])
                {
                    boids.RemoveAt(i);
                    continue;
                }
                boidData[i].position = boids[i].position;
                boidData[i].direction = boids[i].forward;
            }

            var boidBuffer = new ComputeBuffer (numBoids, BoidData.Size);
            boidBuffer.SetData (boidData);

            compute.SetBuffer (0, "boids", boidBuffer);
            compute.SetInt ("numBoids", boids.Count);
            compute.SetFloat ("viewRadius", settings.perceptionRadius);
            compute.SetFloat ("avoidRadius", settings.avoidanceRadius);

            int threadGroups = Mathf.CeilToInt (numBoids / (float) threadGroupSize);
            compute.Dispatch (0, threadGroups, 1, 1);

            boidBuffer.GetData (boidData);

            for (int i = 0; i < boids.Count; i++) {
                boids[i].avgFlockHeading = boidData[i].flockHeading;
                boids[i].centreOfFlockmates = boidData[i].flockCentre;
                boids[i].avgAvoidanceHeading = boidData[i].avoidanceHeading;
                boids[i].numPerceivedFlockmates = boidData[i].numFlockmates;

                boids[i].UpdateBoid ();
            }

            boidBuffer.Release ();
        }
    }



    public struct BoidData {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 flockHeading;
        public Vector3 flockCentre;
        public Vector3 avoidanceHeading;
        public int numFlockmates;

        public static int Size {
            get {
                return sizeof (float) * 3 * 5 + sizeof (int);
            }
        }
    }
}