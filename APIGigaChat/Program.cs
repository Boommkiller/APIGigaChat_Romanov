using System.Text;
using APIGigaChat.Models;
using APIGigaChat.Models.Response;
using Newtonsoft.Json;

namespace APIGigaChat;

class Program
{
    static string ClientId = "***";
    static string AuthorizationKey = "***";

    static async Task Main(string[] args)
    {
        string Token = await GetToken(ClientId, AuthorizationKey);
        Console.WriteLine(Token);
    }

    public static async Task<string> GetToken(string rqUID, string bearer)
    {
        string ReturnToken = null;
        string Url = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";

        using (HttpClientHandler Handler = new HttpClientHandler())
        {
            Handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;

            using (HttpClient Clien = new HttpClient(Handler))
            {
                HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Post, Url);

                Request.Headers.Add("Accept", "application/json");
                Request.Headers.Add("RqUID", rqUID);
                Request.Headers.Add("Authorization", $"Basic {bearer}");

                var Data = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("scope", "GIGACHAT_API_PERS")
                };

                Request.Content = new FormUrlEncodedContent(Data);

                HttpResponseMessage Response = await Clien.SendAsync(Request);

                if (Response.IsSuccessStatusCode)
                {
                    string ResponseContent = await Response.Content.ReadAsStringAsync();

                    ResponseToken Token = JsonConvert.DeserializeObject<ResponseToken>(ResponseContent);

                    ReturnToken = Token.access_token;
                }
            }
        }

        return ReturnToken;
    }

    public static async Task<ResponseMessage> GetAnswer(string token, string message)
    {
        ResponseMessage responseMessage = null;

        string Url = "https://gigachat.devices.sberbank.ru/api/v1/chat/completions";

        using (HttpClientHandler Handler = new HttpClientHandler())
        {
            Handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;

            using (HttpClient Client = new HttpClient(Handler))
            {
                HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Post, Url);

                Request.Headers.Add("Accept", "application/json");
                Request.Headers.Add("Authorization", $"Bearer {token}");

                Request DataRequest = new Request()
                {
                    model = "GigaChat",
                    stream = false,
                    repetition_penalty = 1,
                    messages = new List<Request.Message>() {
                        new Request.Message() {
                            role = "user",
                            content = message
                        }
                    }
                };

                string JsonContent = JsonConvert.SerializeObject(DataRequest);

                Request.Content = new StringContent(JsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage Response = await Client.SendAsync(Request);

                if (Response.IsSuccessStatusCode)
                {
                    string ResponseContent = await Response.Content.ReadAsStringAsync();

                    responseMessage = JsonConvert.DeserializeObject<ResponseMessage>(ResponseContent);
                }
            }
        }

        return responseMessage;
    }
}
