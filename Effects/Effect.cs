using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Effect_Data", menuName = "Effect")]
public class Effect : ScriptableObject
{
    public string entityName;
    public enum EffectType {DoT, HoT, Sleep, Paralyze}; // Add some more
    public List<EffectType> effectTypes;

    public int turnDuration;

    public int value;
    public float scaling;

    public float procRate;

    private static float wakeUpRate = 0.2f;
    private static float paralyzeAttackRate = 0.3f;

    public Sprite icon;

    public static bool WakeUp()
    {
        float r = Random.Range(0f, 1f);
        if(r < wakeUpRate)
        {
            return true;
        }
        return false;
    }

    public static bool AttackWhileParalyzed()
    {
        float r = Random.Range(0f, 1f);
        if (r < paralyzeAttackRate)
        {
            return true;
        }
        return false;
    }
}
