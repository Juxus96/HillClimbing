using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShotData {

    public int variableCount = 2;
    public Ref<float> currentForce;
    public Ref<float> currentAngleY;
    public Ref<float> currentAngleZ;
}
