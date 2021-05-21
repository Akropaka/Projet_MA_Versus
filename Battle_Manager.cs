using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Battle_Manager : MonoBehaviour
{
    [SerializeField]
    public GameObject ally;
    public GameObject enemy;

    public GameObject spellLayout;
    public GameObject spellDescriptionLayout;

    public GameObject spellButton;
    public List<GameObject> allyPlaces = new List<GameObject>();
    public List<GameObject> enemyPlaces = new List<GameObject>();

    public GameObject panel;
    public Text battleText;

    public GameObject arrowDown;
    public List<GameObject> arrows;

    public enum TurnState {Start, AllyTurn, EnemyTurn, Won, Lost, None};

    [SerializeField]
    private TurnState turnState;

    List<Monster_Manager> allyMonsters = new List<Monster_Manager>();
    List<Monster_Manager> enemyMonsters = new List<Monster_Manager>();

    List<Monster_Manager> everyMonsters = new List<Monster_Manager>();

    List<Monster_Manager> potentialTargets = new List<Monster_Manager>();

    List<Monster_Manager> order = new List<Monster_Manager>();

    Monster_Manager playingMonster;

    Spell_Manager spellSelected;

    List<GameObject> spellButtons = new List<GameObject>();

    public int turn = 0;

    IEnumerator InitiateBattle(List<GameObject> allyTeam, List<GameObject> enemyTeam)
    {
        int i = 0;
        foreach(GameObject go in allyTeam)
        {
            GameObject monster = Instantiate(go, ally.transform);
            Monster_Manager mm = monster.GetComponentInChildren<Monster_Manager>();
            allyMonsters.Add(mm);
            everyMonsters.Add(mm);
            monster.name = mm.name;
            mm.SetBattleManager(this);
            // USE A PROPER VECTOR !
            monster.transform.position = allyPlaces[i].transform.position + new Vector3(0, 25, 0);
            mm.position = i;
            ++i;
        }
        i = 0;
        foreach (GameObject go in enemyTeam)
        {
            GameObject monster = Instantiate(go, enemy.transform);
            Monster_Manager mm = monster.GetComponentInChildren<Monster_Manager>();
            enemyMonsters.Add(mm);
            everyMonsters.Add(mm);
            monster.name = mm.name;
            mm.SetBattleManager(this);
            // USE A PROPER VECTOR !
            monster.transform.position = enemyPlaces[i].transform.position + new Vector3(-10, 25, 0);
            monster.GetComponentInChildren<SpriteRenderer>().flipX = true;
            mm.position = i;
            ++i;
        }

        SetupBattleText();

        yield return new WaitForSeconds(2f);

        turnState = TurnState.Start;
    }

    List<Monster_Manager> setOrder()
    {
        List<Monster_Manager> order = new List<Monster_Manager>(everyMonsters);
        order.Sort((m1, m2) =>m1.getSpeed().CompareTo(m2.getSpeed()));
        return order;
    }

    void giveTurn(List<Monster_Manager> order, int index)
    {
        index %= order.Count;
        // Ally Turn
        if (allyMonsters.Contains(order[index]))
        {
            turnState = TurnState.AllyTurn;
            playingMonster = order[index];
            StartCoroutine(PlayerTurn(order[index]));
        }
        // Enemy Turn
        else if(enemyMonsters.Contains(order[index]))
        {
            turnState = TurnState.EnemyTurn;
            playingMonster = order[index];
            StartCoroutine(EnemyTurn(order[index]));
        }
        // Error !
        else
        {
            turnState = TurnState.None;
            giveTurn(order, ++turn);
            Debug.Log("This monster is dead");
        }
    }

    public List<Spell_Manager> getSpellsManager(Monster_Manager mm)
    {
        List<Spell_Manager> sms = new List<Spell_Manager>();
        foreach (Spell s in mm.getSpells())
        {
            GameObject button = Instantiate(spellButton, spellLayout.transform);
            spellButtons.Add(button);

            // Try to use variable (not GetComponent)

            button.GetComponent<Spell_Manager>().Setup(s);
            button.GetComponent<Spell_Manager>().SetBattleManager(this);
            sms.Add(button.GetComponent<Spell_Manager>());
        }
        return sms;
    }

    public IEnumerator EnemyTurn(Monster_Manager mm)
    {
        mm.ApplyEffects();
        if (mm.internalState == Monster_Manager.InternalState.Sleeping)
        {
            battleText.text = "Le " + mm.name + " ennemi dort !";
            yield return new WaitForSeconds(2f);
            giveTurn(order, ++turn);
        }
        else
        {
            spellLayout.SetActive(false);

            List<Spell_Manager> sms = new List<Spell_Manager>();
            sms = getSpellsManager(mm);
            spellSelected = null;

            System.Tuple<Monster_Manager, Spell_Manager> toUse = playingMonster.ChooseSpellIA(everyMonsters, allyMonsters, enemyMonsters);
            clearSpellButtons();

            foreach (Spell_Manager sm in sms)
            {
                if (toUse.Item2.name == sm.name)
                {
                    StartCoroutine(ApplySpell(sm, toUse.Item1));
                }
            }
        }
    }

    public void clearSpellButtons()
    {
        foreach(GameObject go in spellButtons)
        {
            Destroy(go);
        }
        spellButtons.Clear();
    }

    IEnumerator PlayerTurn(Monster_Manager mm)
    {
        mm.ApplyEffects();
        /*
         * Create a method for it
         */
        if (mm.internalState == Monster_Manager.InternalState.Sleeping)
        {
            battleText.text = "Votre " + mm.name + "dort !";
            yield return new WaitForSeconds(2f);
            giveTurn(order, ++turn);
        }
        else
        {
            foreach (Spell s in mm.getSpells())
            {
                GameObject button = Instantiate(spellButton, spellLayout.transform);
                spellButtons.Add(button);
                button.GetComponent<Spell_Manager>().Setup(s);
                button.GetComponent<Spell_Manager>().SetBattleManager(this);
            }
            battleText.text = "Votre " + mm.name + " attends vos ordres !";

            spellSelected = null;

            spellLayout.SetActive(true);
        }
    }

    void SetupBattleText()
    {
        string enemyInfos = " ";
        if (enemyMonsters.Count == 1)
        {
            enemyInfos = "un " + enemyMonsters[0].name + " isole";
        }
        else
        {
            enemyInfos = "un groupe d'ennemies";
        }
        battleText.text = "Votre equipe rencontre " + enemyInfos;
    }

    public void clearSelectionArrows()
    {
        foreach (GameObject arrow in arrows)
        {
            Destroy(arrow);
        }
        arrows.Clear();
    }

    /*
     * Called in Spell_Manager (Method : onClick)
     */
    public IEnumerator PlayerChooseSpell(Spell_Manager sm)
    {
        clearSelectionArrows();
        spellSelected = sm;

        Target targets = sm.targets;

        if (targets.team == Target.Team.Enemy)
        {
            foreach(int pos in targets.positions)
            {
                if(enemyMonsters.Count >= pos)
                {
                    GameObject place = enemyPlaces[pos - 1];
                    Monster_Manager monster = enemyMonsters[pos - 1];

                    potentialTargets.Add(monster);

                    AddArrows(place);
                }
            }
        }

        if (targets.team == Target.Team.Ally)
        {
            foreach (int pos in targets.positions)
            {
                if (allyMonsters.Count >= pos)
                {
                    GameObject place = allyPlaces[pos - 1];
                    Monster_Manager monster = allyMonsters[pos - 1];

                    potentialTargets.Add(monster);

                    AddArrows(place);
                }
            }
        }

        SwitchToDescription(sm);

        yield return new WaitForSeconds(2f);
    }

    void RemoveSpellSelected()
    {
        potentialTargets.Clear();
        clearSelectionArrows();
        spellSelected = null;
    }

    void SwitchToDescription(Spell_Manager sm)
    {
        spellLayout.SetActive(false);
        spellDescriptionLayout.GetComponent<Text>().text = sm.ReplaceDescriptionVariable(sm.GetDescription());
        spellDescriptionLayout.SetActive(true);
    }

    public void SwitchToSpell()
    {
        RemoveSpellSelected();
        spellLayout.SetActive(true);
        spellDescriptionLayout.SetActive(false);
        
    }

    /*
     * To refont !
     */
    private void AddArrows(GameObject place)
    {
        GameObject go = Instantiate(arrowDown, this.transform);
        go.transform.position = place.transform.position + new Vector3(-60, 0, 0);
        Quaternion qua = Quaternion.Euler(0, 0, 90);
        go.transform.rotation = qua;
        arrows.Add(go);

        go = Instantiate(arrowDown, this.transform);
        go.transform.position = place.transform.position + new Vector3(60, 0, 0);
        qua = Quaternion.Euler(0, 0, -90);
        go.transform.rotation = qua;
        arrows.Add(go);
    }


    /*
     * Called in Monster_Manager (Method : OnMouseDown)
     */
    public void PlayerChooseTarget(Monster_Manager mm)
    {
       if(potentialTargets.Contains(mm))
       {
            StartCoroutine(ApplySpell(spellSelected, mm));
       }
    }

    public IEnumerator ApplySpell(Spell_Manager sm, Monster_Manager mm)
    {
        clearSelectionArrows();
        /*
        * Create a method for it
        */
        if (playingMonster.internalState == Monster_Manager.InternalState.Paralized)
        {
            battleText.text = playingMonster.name + " est paralyze ...";
            if (Effect.AttackWhileParalyzed())
            {
                battleText.text += "réussi son attaque";
                SpellZoneApply(sm, mm);
            }
            else
            {
                battleText.text += "n'arrive pas a bouger";
            }
            yield return new WaitForSeconds(1f);
        }
        else
        {
            SpellZoneApply(sm, mm);
        }

        SwitchToSpell();

        potentialTargets.Clear();
        clearSpellButtons();
        spellSelected = null;
        Debug.Log(turn + " | " + order.Count);

        yield return new WaitForSeconds(2f);

        giveTurn(order, ++turn);
    }

    private void SpellZoneApply(Spell_Manager sm, Monster_Manager mm)
    {
        if (sm.zone.area == Zone.Area.Mono)
        {
            ApplyMonoSpell(sm, mm);
        }
        else if (sm.zone.area == Zone.Area.Backlane)
        {

        }
        else if (sm.zone.area == Zone.Area.All)
        {
            ApplyAllSpell(sm, mm);
        }
        else
        {

        }
    }

    public void LookForDeath(Monster_Manager mm)
    {
        if (mm.state == Monster_Manager.State.Dead)
        {
            everyMonsters.Remove(mm);
            if (enemyMonsters.Contains(mm))
            {
                int place = enemyMonsters.IndexOf(mm);
                for (int i = place + 1; i < enemyMonsters.Count; ++i)
                {
                    enemyMonsters[i].transform.parent.position = enemyPlaces[i - 1].transform.position + new Vector3(-10, 25, 0);
                    enemyMonsters[i].position = i - 1;
                }
                enemyMonsters.Remove(mm);
                //order.Remove(mm);
                // If everyone is dead : WON
            }
            else if (allyMonsters.Contains(mm))
            {
                int place = allyMonsters.IndexOf(mm);
                for (int i = place + 1; i < allyMonsters.Count; ++i)
                {
                    allyMonsters[i].transform.parent.position = allyPlaces[i - 1].transform.position + new Vector3(0, 25, 0);
                    allyMonsters[i].position = i - 1;
                }
                allyMonsters.Remove(mm);
                //order.Remove(mm);
                // If everyone is dead : LOST
            }
            mm.BecomeDead();
            //Destroy(mm.transform.parent.gameObject);
        }
    }

    void ApplyMonoSpell(Spell_Manager sm, Monster_Manager mm)
    {
        battleText.text = playingMonster.name + " utilise " + sm.name + " sur " + mm.name;
        UseSpell(sm, mm);
    }

    void ApplyAllSpell(Spell_Manager sm, Monster_Manager mm)
    {
        battleText.text = mm.name + " lance "+sm.name;
        if (allyMonsters.Contains(mm))
        {
            foreach(Monster_Manager mm_temp in new List<Monster_Manager>(allyMonsters))
            {
                UseSpell(sm, mm_temp);
            }
        }
        else if(enemyMonsters.Contains(mm))
        {
            foreach (Monster_Manager mm_temp in new List<Monster_Manager>(enemyMonsters))
            {
                UseSpell(sm, mm_temp);
            }
        }
    }

    void UseSpell(Spell_Manager sm, Monster_Manager mm)
    {
        if (sm.typesOfSpell.Contains(Spell.TypeOfSpell.Damage))
        {
            int damages = sm.CalculateDamages(playingMonster);
            Monster_Manager.State state = mm.ReceiveDamage(damages);
        }
        if (sm.typesOfSpell.Contains(Spell.TypeOfSpell.Heal))
        {
            int heals = sm.CalculateHealing(playingMonster);
            mm.ReceiveHeal(heals);
        }
        if (sm.typesOfSpell.Contains(Spell.TypeOfSpell.Buff))
        {
            sm.PutEffect(mm);
        }
    }
    
    void Awake()
    {
        turnState = TurnState.None;

        panel = GameObject.Find("Panel");
        spellLayout.SetActive(false);

        allyPlaces.Add(GameObject.Find("Place_1_a"));
        allyPlaces.Add(GameObject.Find("Place_2_a"));
        allyPlaces.Add(GameObject.Find("Place_3_a"));

        enemyPlaces.Add(GameObject.Find("Place_1_e"));
        enemyPlaces.Add(GameObject.Find("Place_2_e"));
        enemyPlaces.Add(GameObject.Find("Place_3_e"));

        ally = GameObject.Find("AllyTeam");
        enemy = GameObject.Find("EnemyTeam");

        List<GameObject> allyTeam = ally.GetComponent<Team_Handler>().team;
        List<GameObject> enemyTeam = enemy.GetComponent<Team_Handler>().team;

        StartCoroutine(InitiateBattle(allyTeam, enemyTeam));
    }

    
    void Update()
    {
        if(turnState == TurnState.Start)
        {
            order = setOrder();
            giveTurn(order,0);
        }
    }
}
