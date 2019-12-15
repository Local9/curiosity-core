using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Discord.Bot.Tools
{
    class HttpTools
    {
        public static async Task<string> GetUrlResultAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage responseMessage = await client.GetAsync(url))
                {
                    using (HttpContent content = responseMessage.Content)
                    {
                        return await content.ReadAsStringAsync();
                    }
                }
            }
        }
    }
}
