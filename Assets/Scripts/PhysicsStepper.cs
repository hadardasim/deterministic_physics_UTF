using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsStepper : MonoBehaviour
{        
    void FixedUpdate()
    {
        gameObject.scene.GetPhysicsScene().Simulate(Time.fixedDeltaTime);
    }
}
