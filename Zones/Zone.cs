using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Zone_Data", menuName = "Zone")]
public class Zone : ScriptableObject
{
    public enum Area {Mono, Backlane, All};

    public Area area;
}
