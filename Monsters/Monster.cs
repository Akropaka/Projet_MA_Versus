using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster_Data", menuName = "Monster")]
public class Monster : ScriptableObject
{
    //Identity
        public new string name;

    //Combat
        public int health;

        public int attack_modifier;
        public int healing_modifier;

        public int speed;

    //TO DO:
        public Sprite sprite;

        public List<Spell> spells;
        public List<int> unlock_spells;

        public IA ia;
        //public List<Resistance> resistances;
        //public List<Effect> effects;
}
