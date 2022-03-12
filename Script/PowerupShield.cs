using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupShield : Powerup
{
    public int amount = 3;

    public override bool Apply(Player p)
    {
        if (p.shield == amount) return false;
        p.shield = amount;
        return true;
    }

}
