using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITroop
{
    public void findPath(Tile target);

    public void move();
}
