using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class PhysicsTest
{
    List<GameObject> objects = new List<GameObject>();
    List<PhysicsState> states = null;

    struct PhysicsState
    {
        public Vector3 position;
        public Quaternion rotation;
    };

    // create dynamic objects we want to track
    void CreateObjects(GameObject parent)
    {
        objects.Clear();        
        for (int n=0; n<10;++n)
        {
            var go = GameObject.CreatePrimitive(n % 2 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere);
            go.AddComponent<Rigidbody>();
            go.transform.parent = parent.transform;
            go.transform.position = new Vector3(n * 0.5f, n * 1.5f, 0.0f);
            objects.Add(go);
        }                
    }

    Scene localScene;
    IEnumerator Run()
    {
        localScene = SceneManager.CreateScene("PhysicsScene", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        SceneManager.SetActiveScene(localScene);

        var prefab = Resources.Load<GameObject>("Prefabs/Environment");
        var env = GameObject.Instantiate(prefab);

        // create (stationary) floor collider
        var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.position = new Vector3(0, -2, 0);

        // add physics stepper since physics scene with LocalPhysicsMode.Physics3D require manual simulate call
        plane.AddComponent<PhysicsStepper>();

        CreateObjects(plane);
        for (int n = 0; n < 100; ++n)
            yield return null;
        states = new List<PhysicsState>();
        foreach (var go in objects)
        {
            var state = new PhysicsState
            {
                position = go.transform.position,
                rotation = go.transform.rotation
            };
            states.Add(state);
        }
        Debug.Log($"First object position {states[0].position.ToString("G17")}");

        yield return UnloadScene();
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Time.captureDeltaTime = 2 * Time.fixedDeltaTime;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Time.captureDeltaTime = 0;
    }

    List<PhysicsState> initialStates;
    [UnitySetUp]
    public IEnumerator Setup()
    {
        yield return Run();
        initialStates = states;
    }

    IEnumerator UnloadScene()
    {
        if (localScene.isLoaded)
        {
            var op = SceneManager.UnloadSceneAsync(localScene);
            while (!op.isDone)
                yield return null;
        }
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        // in case of error during the test, the scene may remain loaded
        yield return UnloadScene();
    }


    [UnityTest]
    public IEnumerator PhysicsTestDeterminism()
    {        
        for (int numRuns=0; numRuns<4; ++numRuns)
        {
            yield return Run();
            for (int obj=0; obj< states.Count; ++obj)
            {
                Assert.AreEqual(initialStates[obj].position, states[obj].position, $"Compare position failed on run {numRuns}, object {obj}");
                Assert.AreEqual(initialStates[obj].rotation, states[obj].rotation, $"Compare rotation failed on run {numRuns}, object {obj}");
            }            
        }                
    }
}
