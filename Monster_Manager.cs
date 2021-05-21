using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Monster_Manager : MonoBehaviour
{
    public Monster monster;

    public enum State {Alive, Dead};
    public enum InternalState {None, Sleeping, Paralized};

    public State state;

    public InternalState internalState = InternalState.None;

    public string team;
    public int position;

    public new string name;
    public int level;

    public int health;
    public int currentHealth;

    public int attack_modifier;
    public int healing_modifier;

    public int speed;

    public Sprite sprite;

    public List<Spell> spells;

    [SerializeField] public List<Effect> effects;

    private Battle_Manager bm;

    private IA ia;

    [SerializeField] Image healthBar;
    [SerializeField] Text healthText;

    public GameObject iconLayer;

    List<GameObject> listIcons;

    public GameObject icon;

    public int getSpeed()
    {
        return speed;
    }

    public int getCurrentHP()
    {
        return currentHealth;
    }

    public List<Spell> getSpells()
    {
        return spells;
    }

    void DisplayName()
    {
        Text tm = transform.parent.GetComponentInChildren<Text>();
        tm.text = "Lv." + level + " " + name;
    }

    //TO DO : UI
    public void ShowEffects()
    {

    }

    public void BecomeDead()
    {
        GameObject display = transform.parent.gameObject.transform.GetChild(0).gameObject;
        display.SetActive(false);
    }

    void DisplaySprite()
    {
        SpriteRenderer sr = transform.parent.GetComponentInChildren<SpriteRenderer>();
        sr.sprite = sprite;
    }

    public void SetBattleManager(Battle_Manager bm)
    {
        this.bm = bm;
    }

    public void OnMouseDown()
    {
        bm.PlayerChooseTarget(this);
    }

    public State ReceiveDamage(int damage)
    {
        //TO DO : Include Resistances
        currentHealth -= damage;

        RefreshHPBar();

        if(currentHealth <= 0)
        {
            this.state = State.Dead;
            bm.LookForDeath(this);
            return State.Dead;
        }
        else
        {
            return State.Alive;
        }
    }

    public void ReceiveHeal(int heal)
    {
        currentHealth = Mathf.Min(health, currentHealth + heal);
        RefreshHPBar();
    }

    public void RefreshHPBar()
    {
        healthBar.fillAmount = (float)currentHealth / (float)health;
        healthText.text = currentHealth + "/" + health;
    }

    // THIS IS A BEHAVIOUR METHODS
    public int SelectRandomSpell()
    {
        return Mathf.FloorToInt(Random.Range(0,spells.Count));
    }

    public float FindMaxFloat(List<float> list)
    {
        if (list.Count == 0)
        {
            throw new System.InvalidOperationException("Empty list");
        }
        float max = float.MinValue;
        foreach (float element in list)
        {
            if (element > max)
            {
                max = element;
            }
        }
        return max;
    }

    /* "TRY TO SEE WHAT HAPPEN WITH EVERYSPELL" 
     * -> DETERMINE A COEFFICIENT OF EFFICIENCIE
     * -> THE BIGGEST COEFFICIENT IS THE SPELL THAT WILL BE USED
     * -> EVERYSPELL NOT USABLE WILL NOT BE ADDED IN THE LIST -> Make a function for it
     * 
     * DONE
     * 
     * NOW :
     * -> Try to Focus (depending on HP Left / Missing)
     */

    public System.Tuple<Monster_Manager, Spell_Manager> ChooseSpellIA(List<Monster_Manager> everyMonster, List<Monster_Manager> allyMonster, List<Monster_Manager> enemyMonster)
    {

        // Every index of these lists correspond to a situation
        List<Spell_Manager> sms = new List<Spell_Manager>();
        List<Monster_Manager> mms = new List<Monster_Manager>();
        List<float> efficiencities = new List<float>();

        List<Monster_Manager> targetList = new List<Monster_Manager>();

        List<Spell_Manager> sms_import = new List<Spell_Manager>(bm.getSpellsManager(this));

        float efficiency;

        foreach (Spell_Manager sm in sms_import)
        {

            if(sm.targets.team == Target.Team.Ally)
            {
                targetList = enemyMonster;
            }
            else if(sm.targets.team == Target.Team.Enemy)
            {
                targetList = allyMonster;
            }


            if(sm.zone.area == Zone.Area.Mono)
            {
                foreach(Monster_Manager mm in targetList)
                {
                    efficiency = TryToUseMono(sm, mm);
                    if(efficiency>0)
                    {
                        sms.Add(sm);
                        mms.Add(mm);
                        efficiencities.Add(efficiency);
                    }
                }
            }
            if(sm.zone.area == Zone.Area.Backlane)
            {

            }
            if(sm.zone.area == Zone.Area.All)
            {
                efficiency = TryToUseAll(sm, targetList);
                if (efficiency > 0)
                {
                    sms.Add(sm);
                    mms.Add(targetList.FindLast(x => x.state == State.Alive));
                    efficiencities.Add(efficiency);
                }
            }
        }
        int index = efficiencities.IndexOf(FindMaxFloat(efficiencities));
        return new System.Tuple<Monster_Manager, Spell_Manager>(mms[index], sms[index]);
    }

    private float TryToUseMono(Spell_Manager sm, Monster_Manager mm)
    {
        if(sm.targets.positions.Contains(mm.position+1))
        {
            if (sm.typesOfSpell.Contains(Spell.TypeOfSpell.Damage))
            {
                return (sm.CalculateDamages(this) - Mathf.Min(mm.currentHealth - sm.CalculateDamages(this),0) + Mathf.Min(mm.currentHealth - sm.CalculateDamages(this), 0)*ia.executeCoefficient) * ia.damageCoefficient;
            }
            if (sm.typesOfSpell.Contains(Spell.TypeOfSpell.Heal))
            {
                return (sm.CalculateHealing(this) - Mathf.Max(mm.currentHealth + sm.CalculateHealing(this), mm.health) + mm.health) * ia.healingCoefficient;
            }
            if (sm.typesOfSpell.Contains(Spell.TypeOfSpell.Buff))
            {
                return 0;
            }
        }
        else
        {
            return 0;
        }
        return 0;
    }

    private float TryToUseAll(Spell_Manager sm, List<Monster_Manager> mms)
    {
        float sum = 0;
        foreach(Monster_Manager mm in mms)
        {
            sum += TryToUseMono(sm, mm);
        }
        return sum*ia.allCoefficient;
    }

    public void ApplyEffects()
    {
        foreach (Effect effect in new List<Effect>(this.effects))
        {
            Debug.Log(effect.turnDuration);
            ApplySleep(effect);
            ApplyParalize(effect);
            ApplyDoT(effect);
            ApplyHoT(effect);

            if (effect.turnDuration == 0)
            {
                this.RemoveIcon(effect);
                this.effects.Remove(effect);
            }
            effect.turnDuration -= 1;
            //this.RefreshEffectDuration()
        }
    }

    void ApplySleep(Effect effect)
    {
        if (effect.effectTypes.Contains(Effect.EffectType.Sleep))
        {
            //Spell_Manager.ApplySleep(this);
            if (Effect.WakeUp())
            {
                effect.turnDuration = 0;
            }
            if (effect.turnDuration == 0)
            {
                Spell_Manager.UnApplySleep(this);
            }
        }
    }
    void ApplyParalize(Effect effect)
    {
        if (effect.effectTypes.Contains(Effect.EffectType.Paralyze))
        {
            if (effect.turnDuration == 0)
            {
                Spell_Manager.UnApplyParalize(this);
            }
        }
    }

    void ApplyDoT(Effect effect)
    {
        if (effect.effectTypes.Contains(Effect.EffectType.DoT))
        {
            this.ReceiveDamage(effect.value + Mathf.FloorToInt(effect.value * effect.scaling));
        }
    }

    void ApplyHoT(Effect effect)
    {
        if (effect.effectTypes.Contains(Effect.EffectType.HoT))
        {
            this.ReceiveHeal(effect.value + Mathf.FloorToInt(effect.value * effect.scaling));
        }
    }

    public void AddIcon(Effect effect)
    {
        GameObject go = Instantiate(icon, iconLayer.transform);
        go.name = effect.icon.name;
        go.GetComponent<SpriteRenderer>().sprite = effect.icon;
        listIcons.Add(go);
    }

    public void RemoveIcon(Effect effect)
    {
        foreach(GameObject go in new List<GameObject>(listIcons))
        {
            if(go.name == effect.icon.name)
            {
                Destroy(go);
                //Maybe delete it from the list too
            }
        }
    }

    void Awake()
    {
        this.name = monster.name;
        this.health = monster.health;
        this.currentHealth = health;
        this.attack_modifier = monster.attack_modifier;
        this.healing_modifier = monster.healing_modifier;

        this.effects = new List<Effect>();

        this.sprite = monster.sprite;

        this.ia = monster.ia;

        state = State.Alive;

        int i = 0;
        foreach(Spell s in monster.spells)
        {
            if (monster.unlock_spells[i] <= level)
            {
                this.spells.Add(s);
            }
            ++i;
        }

        listIcons = new List<GameObject>();

        DisplayName();
        DisplaySprite();

        RefreshHPBar();
    }
}
