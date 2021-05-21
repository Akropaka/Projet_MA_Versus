using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Spell_Manager : MonoBehaviour
{

    public new string name;

    //Combat
    public int base_damage;
    public int base_healing;

    public float attack_damage_ratio;
    public float healing_ratio;

    public int self_healing;

    public Target targets;
    public Zone zone;

    private string description;

    //Reference
    Battle_Manager bm;

    public List<Spell.TypeOfSpell> typesOfSpell;
    public List<Effect> effects;

    public Spell spell;


    private Dictionary<string, string> replacer;

    public void Setup(Spell spell)
    {
        this.spell = spell;

        this.name = spell.name;
        this.base_damage = spell.base_damage;
        this.base_healing = spell.base_healing;
        this.attack_damage_ratio = spell.attack_damage_ratio;
        this.healing_ratio = spell.healing_ratio;
        this.self_healing = spell.self_healing;
        this.targets = spell.targets;
        this.zone = spell.zone;
        this.typesOfSpell = spell.typesOfSpell;

        foreach (Effect effet in spell.effects)
        {
            effects.Add(Instantiate(effet));
        }

        this.gameObject.GetComponentInChildren<Text>().text = name;

        this.spell.SetDescription();
        this.description = spell.description;

        SetDictonnary();
    }

    public void SetBattleManager(Battle_Manager bm)
    {
        this.bm = bm;
    }

    public int CalculateDamages(Monster_Manager mm)
    {
        return Mathf.FloorToInt(base_damage + mm.attack_modifier * attack_damage_ratio);
    }

    public int CalculateHealing(Monster_Manager mm)
    {
        return Mathf.FloorToInt(base_healing + mm.healing_modifier * healing_ratio);
    }

    public void onClick()
    {
        StartCoroutine(bm.PlayerChooseSpell(this));
    }

    public void PutEffect(Monster_Manager mm)
    {
        foreach(Effect effect in new List<Effect>(effects))
        {
            if (!mm.effects.Exists(x => effect.entityName == x.entityName))
            {
                float r = Random.Range(0f, 1f);
                Debug.Log(r);
                if (r < effect.procRate)
                {
                    if(effect.effectTypes.Contains(Effect.EffectType.Sleep))
                    {
                        if (ApplySleep(mm))
                        {
                            mm.effects.Add(effect);
                            mm.AddIcon(effect);
                        }
                    }
                    else if (effect.effectTypes.Contains(Effect.EffectType.Paralyze))
                    {
                        if (ApplyParalize(mm))
                        {
                            mm.effects.Add(effect);
                            mm.AddIcon(effect);
                        }
                    }
                    else
                    {
                        mm.effects.Add(effect);
                        mm.AddIcon(effect);
                    }
                }
            }
            else
            {
                /*
                 * This re-apply sleeping, this could be strong (maybe just delete it)
                 */
                mm.effects.Remove(mm.effects.Find(x => effect.entityName == x.entityName));
                mm.effects.Add(effect);
            }
        }
    }

    public bool ApplySleep(Monster_Manager mm)
    {
        if (mm.internalState == Monster_Manager.InternalState.None)
        {
            mm.internalState = Monster_Manager.InternalState.Sleeping;
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void UnApplySleep(Monster_Manager mm)
    {
        mm.internalState = Monster_Manager.InternalState.None;
    }

    public bool ApplyParalize(Monster_Manager mm)
    {
        if (mm.internalState == Monster_Manager.InternalState.None)
        {
            mm.internalState = Monster_Manager.InternalState.Paralized;
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void UnApplyParalize(Monster_Manager mm)
    {
        mm.internalState = Monster_Manager.InternalState.None;
    }

    public string GetDescription()
    {
        return description;
    }

    public string ReplaceDescriptionVariable(string s)
    {
        string[] cutted = Regex.Split(s, @" ");
        List<string> toReplace = new List<string>();
        foreach(string s_temp in cutted)
        {
            string find = Regex.Match(s_temp, @"\{(.*?)\}").Groups[1].Value;
            if (find != "")
            {
                toReplace.Add(find);
                Debug.Log(find);
            }
        }
        foreach(string s_temp in toReplace)
        {
            s = s.Replace(s_temp, replacer[s_temp]);
        }
        s = s.Replace("{", "");
        s = s.Replace("}", "");
        return s;
    }

    /*
     * To complet & DOCUMENT
     */
    public void SetDictonnary()
    {
        replacer = new Dictionary<string, string>();
        for (int i = 0; i < this.effects.Count; ++i)
        {
            AddDictonnary("EFFECT_DAMAGE_" + i.ToString(), "<color=orange>" + this.effects[i].value.ToString() + "</color>");
            AddDictonnary("EFFECT_DURATION_" + i.ToString(), this.effects[i].turnDuration.ToString());
        }
        Debug.Log(base_damage.ToString());
        AddDictonnary("DAMAGE","<color=orange>" + base_damage.ToString() + "</color>");
    }



    private void AddDictonnary(string from, string to)
    {
        replacer.Add(from, to);
    }
}
