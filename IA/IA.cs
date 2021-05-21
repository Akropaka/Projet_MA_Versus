using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IA_Data", menuName = "IA")]
public class IA : ScriptableObject
{
    public float damageCoefficient;
    public float healingCoefficient;
    public float buffingCoefficient;
    public float allCoefficient;
    public float executeCoefficient;
}
