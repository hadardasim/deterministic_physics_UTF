using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PhysicsTest
{
    List<GameObject> objects = new List<GameObject>();
    List<PhysicsState> states = null;

    struct PhysicsState
    {
        public Vector3 position;
        public Quaternion rotation;
    };
    void CreateObjects()
    {
        objects.Clear();
        for (int n=0; n<10;++n)
        {
            var go = GameObject.CreatePrimitive(n % 2 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere);
            go.AddComponent<Rigidbody>();
            go.transform.position = new Vector3(n * 0.5f, n * 1.5f, 0.0f);
            objects.Add(go);
        }
    }

    IEnumerator Run()
    {
        CreateObjects();
        for (int n = 0; n < 100; ++n)
            yield return null;
        states = new List<PhysicsState>();
        foreach (var go in objects)
        {
            GameObject.Destroy(go);
            var state = new PhysicsState
            {
                position = go.transform.position,
                rotation = go.transform.rotation
            };
            states.Add(state);
        }

    }

    List<PhysicsState> initialStates;
    [UnitySetUp]
    public IEnumerator Setup()
    {
        yield return Run();
        initialStates = states;
    }


    [UnityTest]
    public IEnumerator PhysicsTestDeterminism()
    {        
        yield return Run();
    }
}
