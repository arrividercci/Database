using Newtonsoft.Json;
using System.Text;

namespace Database.Common.Services
{
    public static class HtmlValidator
    {
        public static bool IsValid(string html)
        {
            if (!String.IsNullOrEmpty(html))
            {
                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    using (HttpClient client = new HttpClient(handler))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)");
                        var content = new StringContent(html, Encoding.UTF8, "text/html");
                        var response = client.PostAsync("https://validator.w3.org/nu/?out=json", content).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            var result = response.Content.ReadAsStringAsync().Result;
                            var resultModel = JsonConvert.DeserializeObject<HtmlW3CValidatorModel>(result);
                            var errors = resultModel.Messages.Where(x => x.Type == "error").Select(x => x.Message);
                            if (errors.Count() > 0)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
    public class HtmlW3CValidatorModel
    {
        [JsonProperty("messages")]
        public List<HtmlW3CValidatorMMessage> Messages { get; set; }
    }

    public class HtmlW3CValidatorMMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("lastLine")]
        public int LastLine { get; set; }

        [JsonProperty("lastColumn")]
        public int LastColumn { get; set; }
    }
}
