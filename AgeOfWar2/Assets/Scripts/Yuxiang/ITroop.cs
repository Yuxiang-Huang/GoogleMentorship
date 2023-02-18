using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITroop
{
    Tile tile { get; set; }

    public void findPath(Tile target);

    public void move();

    public void highlight(bool status);
}
