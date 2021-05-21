using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "Spell_Data", menuName = "Spell")]
public class Spell : ScriptableObject
{
    //Identity
        public new string name;

    /*
     * Create an ENUM which tells if the spell is {Damage, Heal, Buff} oriented
     * The variable will be a list of these elements
     */

    public string description;

    public TextAsset file;

    public enum TypeOfSpell { Damage, Heal, Buff };

    public List<TypeOfSpell> typesOfSpell;

    public List<Effect> effects;

    //Combat
    public int base_damage;
    public int base_healing;

    public float attack_damage_ratio;
    public float healing_ratio;

    public int self_healing;

    public Target targets;
    public Zone zone;

    public void SetDescription()
    {
        if(Resources.Load<TextAsset>("Spell_Descriptions/" + name + "_description") != null)
        {
            file = Resources.Load<TextAsset>("Spell_Descriptions/" + name + "_description");
            description = file.text;
        }
    }
}
