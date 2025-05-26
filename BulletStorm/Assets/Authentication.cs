using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System;
using Newtonsoft.Json;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;

public class Authentication : MonoBehaviour
{
    [SerializeField] GameObject LoginPage;
    [SerializeField] GameObject LoggedInPage;
    public TMP_InputField loginName;
    public TMP_InputField loginPassword;
    public TMP_InputField userName;
    public TMP_InputField password;
    public TMP_InputField passwordConfimation;
    public TMP_InputField email;
    public TMP_Text message;
    public Button RegisterButton;
    public Button LoginButton;
    public Button PlayButton;
    private string RegisterURL= "https://localhost:7023/api/User/registration";
    private string LoginURL = "https://localhost:7023/api/Auth/login";
    private string jwtToken;
    [SerializeField] TMP_Text token;
    [SerializeField] TMP_Text nameforToken;
    [SerializeField] TMP_Text ELO;
    [SerializeField] TMP_Text emailForToken;
    private readonly JsonSerializerSettings serializerSettings = new();
   
    public async void Login()
    {
        var data = new LoginDto
        { 
            Name = loginName.text,
            Password = loginPassword.text,
        };
        await LoginAsync(data);
    }

    public async Task<string> LoginAsync(LoginDto loginDto)
    {
        string httpContent = JsonConvert.SerializeObject(loginDto);

        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post, 
            RequestUri = new Uri(LoginURL),
            Content = new StringContent(httpContent, Encoding.UTF8, "application/json")
        };

        var httpClient = new HttpClient();

        HttpResponseMessage responseMessage = await httpClient.SendAsync(httpRequestMessage);
        string responseJSON = await responseMessage.Content.ReadAsStringAsync();

        if (!responseMessage.IsSuccessStatusCode)
        {
            Debug.LogError($"Login error: {responseMessage.StatusCode} - {responseJSON}");
            throw new Exception("Login failed");
        }

        var tokenObj = JsonConvert.DeserializeObject<TokenObj>(responseJSON);
        TokenController.Token = tokenObj.Token;
        if (tokenObj is not null)
        {
            DecodeJWT(tokenObj.Token);
            LoginPage.gameObject.SetActive(false);
            LoggedInPage.gameObject.SetActive(true);
        }
        return tokenObj.Token;
    }

    private void DecodeJWT(string token)
    {
        string[] splitToken = token.Split('.');

        if (splitToken.Length == 3)
        { 
            string payload = splitToken[1];
            int remainder = payload.Length % 4;
            if (remainder > 0)
            { 
                int missingChar = 4 - remainder;
                payload += new string('=', missingChar);
            }
            byte[] bytes = Convert.FromBase64String(payload); 
            string data = Encoding.UTF8.GetString(bytes);;
            var payloadObj = JObject.Parse(data);

            string userName = payloadObj["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]?.ToString();
            string email = payloadObj["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"]?.ToString();
            string id = payloadObj["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]?.ToString();
            UserDto userDto = new()
            {
                Email = email,
                Name = userName,
                Id = int.Parse(id)
            };
            UserInformations.Name = userDto.Name;
            nameforToken.text = userDto.Name;
            emailForToken.text = userDto.Email;
         
            UserInformations.Email = userDto.Email;
            UserInformations.UserId = userDto.Id;
        }
    }

    public async Task<RegisterDtO> RegisterAsync(RegisterDtO registerDtO)
    {
        string httpContent = JsonConvert.SerializeObject(registerDtO, serializerSettings);
        var httpClient = new HttpClient();
        HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(RegisterURL,new StringContent(httpContent,
            Encoding.UTF8,"application/json"));
        if (!httpResponseMessage.IsSuccessStatusCode) throw new System.Exception(httpResponseMessage.StatusCode.ToString());
        string response = await httpResponseMessage.Content.ReadAsStringAsync();
        RegisterDtO registerResponse = JsonConvert.DeserializeObject<RegisterDtO>(response);
        return registerResponse;

    }
    

    public async void ComparePassword()
    {
        if (password.text == passwordConfimation.text)
        {
            var user = new RegisterDtO
            {
                Email = email.text,
                Name = userName.text,
                Password = password.text,
            };
           await RegisterAsync(user);
        }
        else
        {
            message.text = "A jelszavak nem egyeznek!";
        }
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
