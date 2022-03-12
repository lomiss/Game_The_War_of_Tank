using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupHealth : Powerup
{
    public int amount = 5;

    public override bool Apply(Player p)
    {
        if (p.health == p.maxHealth) return false;
        int value = p.health;
        value += amount;
        value = Mathf.Clamp(value, value, p.maxHealth);
        p.health = value;
        return true;
    }
    
}
