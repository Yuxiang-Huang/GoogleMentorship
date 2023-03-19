using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOE : Spell
{
    public override void effect()
    {
        foreach (Tile neighbor in tile.neighbors)
        {
            neighbor.setDark(false);
        }

        foreach (Tile neighbor in tile.neighbors2)
        {
            neighbor.setDark(false);
        }

        kill();
    }
}
