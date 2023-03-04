using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public interface IUnit
{
    PhotonView PV { get; }

    int ownerID { get; }

    GameObject gameObject { get; }

    int health { get; }

    int damage { get; }

    public void takeDamage(int incomingDamage);

    public void setHealthBar(bool status);

    public int getFullHealth();
}
