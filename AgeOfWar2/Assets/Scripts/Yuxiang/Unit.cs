using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public interface IUnit
{
    int ownerID { get; }

    GameObject gameObject { get; }
}
