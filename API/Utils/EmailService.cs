using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace API.Utils
{
    public class EmailService
    {

        private static readonly string API_URL = "https://mail-sender-service.vercel.app/send-email";

        public static async Task<bool> SendMailAsync(string to, string subject, string text)
        {

            var emailData = new
            {
                to = to,
                subject = subject,
                text = text
            };

            string jsonData = System.Text.Json.JsonSerializer.Serialize(emailData);

            using HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(API_URL, content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
