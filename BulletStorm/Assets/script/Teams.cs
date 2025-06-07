using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Teams 
{
    public static HashSet<Player> RedTeams { get; } = new HashSet<Player>();

    public static HashSet<Player> BlueTeams { get; } = new HashSet<Player>();
}
