using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public interface IUnit
{
    PhotonView PV { get; }

    int ownerID { get; }

    GameObject gameObject { get; }

    public void takeDamage(int incomingDamage);
}
