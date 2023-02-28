using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public interface IUnit
{
    PhotonView PV { get; }

    int health { get; }

    int ownerID { get; }

    GameObject gameObject { get; }

    public void takeDamage(int incomingDamage);
}
