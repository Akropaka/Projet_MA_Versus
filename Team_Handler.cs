using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team_Handler : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> team = new List<GameObject>();

    void addMonster(GameObject monster)
    {
        team.Add(monster);
    }

    void clearMonsters()
    {
        team.Clear();
    }
}
