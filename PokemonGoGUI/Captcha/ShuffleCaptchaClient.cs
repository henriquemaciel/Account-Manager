using PokemonGoGUI.Enums;
using PokemonGoGUI.GoManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.Captcha
{
    public class ShuffleCaptchaClient
    {
        private string APIKey { get; set; }

        public ShuffleCaptchaClient(string apiKey)
        {
            apiKey = apiKey;
        }

        public async Task<string> SolveRecaptchaV2(Client client, string googleKey, string pageUrl)
        {
            string requestUrl = String.Format("https://api.shuffletanker.com/api/v2/Captcha/In?API={0}&WebsiteUrl={1}&WebsiteKey={2}", APIKey, pageUrl, googleKey);

            try
            {
                WebRequest req = WebRequest.Create(requestUrl);
                int test = 0;
                using (WebResponse resp = req.GetResponse())
                using (StreamReader read = new StreamReader(resp.GetResponseStream()))
                {
                    string response = read.ReadToEnd();
                    if (int.TryParse(response, out test))
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            WebRequest getAnswer = WebRequest.Create(String.Format("https://api.shuffletanker.com/api/v2/Captcha/Out?API={0}&captchaId={1}", APIKey, response));

                            using (WebResponse answerResp = getAnswer.GetResponse())
                            using (StreamReader answerStream = new StreamReader(answerResp.GetResponseStream()))
                            {
                                string answerResponse = answerStream.ReadToEnd();

                                if (answerResponse == "CAPCHA_NOT_READY")
                                {
                                    continue;
                                }
                                else if (answerResponse.Length > 50)
                                {
                                    return answerResponse;
                                }
                                else
                                {
                                    client.ClientManager.LogCaller(new LoggerEventArgs($"Shuffle Captcha Error " + response, LoggerTypes.Exception));
                                    return string.Empty;
                                }
                            }
                        }
                    }
                    else
                    {
                        client.ClientManager.LogCaller(new LoggerEventArgs($"Shuffle Captcha Error " + response, LoggerTypes.Exception));
                    }
                }

            }
            catch (Exception ex)
            {
                client.ClientManager.LogCaller(new LoggerEventArgs($"Shuffle Captcha Error", LoggerTypes.Exception, ex));
            }
            return string.Empty;
        }
    }
}
