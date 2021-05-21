using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Target_Data", menuName = "Target")]
public class Target : ScriptableObject
{
    public enum Team {Ally, Enemy};
    public List<int> positions;
    public Team team;
}
