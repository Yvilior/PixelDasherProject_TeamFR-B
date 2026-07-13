using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    private HashSet<AbilityType> unlocked = new HashSet<AbilityType>();

    public event Action<AbilityType> OnAbilityUnlocked;

    public bool Has(AbilityType ability) => unlocked.Contains(ability);

    public void Unlock(AbilityType ability)
    {
        if (unlocked.Add(ability))
        {
            OnAbilityUnlocked?.Invoke(ability);
            Debug.Log($"Pouvoir débloqué : {ability}");
        }
    }

    public void UnlockAll()
    {
        foreach (AbilityType a in Enum.GetValues(typeof(AbilityType)))
            Unlock(a);
    }
}