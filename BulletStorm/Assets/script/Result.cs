using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Result 
{
    [JsonProperty("resulClient")]
    public Match Match { get; set; }

    [JsonProperty("avarageElo")]
    public float AvarageElo { get; set; }

    [JsonProperty("aggregatedKills")]
    public int AllKill { get; set; }
}
