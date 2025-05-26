using Newtonsoft.Json;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class ApiHandler
{
    private readonly string CreateMatchUrl = "https://localhost:7023/api/Match";
    private readonly string UpdateMatchUrl = "https://localhost:7023/api/Match";
    private readonly string GetMatchUrl = "https://localhost:7023/api/Match";
    private readonly string CreateStatisticsUrl = "https://localhost:7023/api/Statistics";
    private readonly string UpdateStatisticsUrl = "https://localhost:7023/api/Statistics";
    private readonly string GetStatisticsUrl = "https://localhost:7023/api/Statistics";
    bool firstGame = false;
    private readonly JsonSerializerSettings serializerSettings = new();
    Dictionary<string, MatchResult> values = new();
    MatchResult matchScore = new MatchResult();
    public static ApiHandler instance = new();
    string Token = TokenController.Token;

    public Result MatchStats()
    {
        values = MatchController.Instance.gameStats;
        ResultDto matchResultDto = new();
        foreach (var value in values)
        {
            if (PhotonNetwork.LocalPlayer.NickName.Equals(value.Key))
            {
                matchResultDto.MatchResult.Assist = value.Value.Assist;
                matchResultDto.MatchResult.Kill = value.Value.Kill;
                matchResultDto.MatchResult.Deaths = value.Value.Deaths;    
                matchResultDto.MatchResult.Won = value.Value.Won;    
            }       
        }
        matchResultDto.AllKill = AggregatedKills();
        matchResultDto.AvreageELO = AvarageELO();
        
        return ResultMapper(matchResultDto); 
    }

    public Result ResultMapper(ResultDto resultDto)
    {
        Result result = new Result()
        {
            AvarageElo = AvarageELO(),
            AllKill = AggregatedKills(),

            Match = new()
            { 
                Assist = resultDto.MatchResult.Assist,
                Kill = resultDto.MatchResult.Kill,
                Deaths = resultDto.MatchResult.Deaths,
                Won = resultDto.MatchResult.Won,
            }
        };
        return result;
    }

    public int AggregatedKills()
    {
        int kills = 0;

        foreach (var value in values)
        {
            kills= value.Value.Kill;
        }
        return kills;
    }

    public float AvarageELO()
    {
        float elo = 0.0f;
        foreach (var value in values)
        {
            elo = value.Value.OldELO;
        }
        return elo/6;
    }

    public async Task GlobalStatisticsAsync()
    {
        values = MatchController.Instance.gameStats;
        GlobalStatistics globalStatistics = new();
        foreach (var value in values)
        { 
            globalStatistics.AggregatedKills += value.Value.Kill;
            globalStatistics.AggregatedAssits += value.Value.Assist;
        }
        await UpdateGlobalStastisticsAsync(Token, globalStatistics);
    }

    public async Task HandleMatchAPIAsync()
    { 
        int count = await GetMatchesAsync(Token);
        if (count < 4)
        { 
            await CreateMatchAsync(Token);
        }
        await UpdateMatchAsync(Token);
    }

    public async Task<string> CreateMatchAsync(string token)
    {
        var playerStats = MatchStats();
        string httpContent = JsonConvert.SerializeObject(playerStats, serializerSettings);
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(CreateMatchUrl, new StringContent(httpContent,
            Encoding.UTF8, "application/json"));
        if (!httpResponseMessage.IsSuccessStatusCode) throw new System.Exception(httpResponseMessage.StatusCode.ToString());
        string response = await httpResponseMessage.Content.ReadAsStringAsync();
        return response;
    }

    public async Task<string> UpdateMatchAsync(string token)
    {
        var playerStats = MatchStats();
        string httpContent = JsonConvert.SerializeObject(playerStats, serializerSettings);
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage httpResponseMessage = await httpClient.PutAsync(UpdateMatchUrl, new StringContent(httpContent,
            Encoding.UTF8, "application/json"));
        if (!httpResponseMessage.IsSuccessStatusCode) throw new System.Exception(httpResponseMessage.StatusCode.ToString());
        string response = await httpResponseMessage.Content.ReadAsStringAsync();
        return response;
    }

    public async Task<int> GetMatchesAsync(string token)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(GetMatchUrl);
        if (!httpResponseMessage.IsSuccessStatusCode) throw new System.Exception(httpResponseMessage.StatusCode.ToString());
        string response = await httpResponseMessage.Content.ReadAsStringAsync();
        int matchCounter= JsonConvert.DeserializeObject<int>(response);
        return matchCounter;
    }

    public async Task<float> GetUserStatisticsAsync(string token)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(GetStatisticsUrl);
        if (!httpResponseMessage.IsSuccessStatusCode) throw new System.Exception(httpResponseMessage.StatusCode.ToString());
        string response = await httpResponseMessage.Content.ReadAsStringAsync();
        float elo = JsonConvert.DeserializeObject<float>(response);
        return elo;
    }
   
    public async Task<string> UpdateGlobalStastisticsAsync(string token,GlobalStatistics globalStatistics)
    { 
        string httpContent = JsonConvert.SerializeObject(globalStatistics, serializerSettings);
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage httpResponseMessage = await httpClient.PutAsync(UpdateStatisticsUrl, new StringContent(httpContent,
            Encoding.UTF8, "application/json"));
        if (!httpResponseMessage.IsSuccessStatusCode) throw new System.Exception(httpResponseMessage.StatusCode.ToString());
        string response = await httpResponseMessage.Content.ReadAsStringAsync();
        return response;
    }
}
