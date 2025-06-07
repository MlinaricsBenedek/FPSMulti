using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match 
{
    [JsonProperty("kill")]
    public int Kill { get; set; }

    [JsonProperty("assist")]
    public int Assist { get; set; }

    [JsonProperty("deaths")]
    public int Deaths { get; set; }

    [JsonProperty("won")]
    public bool Won { get; set; }
}
