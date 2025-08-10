using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tweetinvi;
using Tweetinvi.Models;

namespace XWeather
{
    class Program
    {
        // 🔑 Twitter API v2 credentials
        private static readonly string consumerKey = "LzpK6cfaswdvTTC5oBbpO8W2f";
        private static readonly string consumerSecret = "wq5xxnK7oXBs2ZJlFsZA3JqJcy8SP3Q4TzcaIDdNkgmD5yKehT";
        private static readonly string accessToken = "1922932714275209218-pX4jAyRxuV25iyDwwYL6QJg4KUKY4n";
        private static readonly string accessTokenSecret = "Q8HPgAC9DOIWSRZnbnXtQBGYwg0gpMwIuIAjlDkzM5ckE";

        // 🌦 Weather API key
        private static readonly string weatherApiKey = "2797b946af5bc2e2c2168252a08ebdc9";

        static async Task Main(string[] args)
        {
            Timer timer = new Timer(async _ => await PostWeatherUpdate(), null, TimeSpan.Zero, TimeSpan.FromHours(1));
            Console.WriteLine("🟢 XWeather bot is running... successfully Press Enter to exit.");
            Console.ReadLine();
        }

        private static async Task PostWeatherUpdate()
        {
            try
            {
                var weatherData = await GetWeatherDataAsync("Bengaluru");
                if (weatherData == null || weatherData.Main == null || weatherData.Weather == null || weatherData.Weather.Length == 0)
                {
                    Console.WriteLine("❌ Weather data is incomplete.");
                    return;
                }

                string temp = weatherData.Main.Temp.ToString("0.0");
                string desc = weatherData.Weather[0].Description;
                string time = DateTime.Now.ToString("HH:mm");
                string tweetContent = $"🌦 Bengaluru Weather: {temp}°C, {desc}. 🕒 {time} #XWeatherBot";

                Console.WriteLine("📝 Posting tweet: " + tweetContent);

                var userClient = new TwitterClient(consumerKey, consumerSecret, accessToken, accessTokenSecret);
                var tweet = await userClient.Tweets.PublishTweetAsync(tweetContent);

                if (tweet != null)
                    Console.WriteLine($"✅ Tweet sent: {tweet.FullText}");
                else
                    Console.WriteLine("❌ Tweet failed to send.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        static async Task<WeatherResponse?> GetWeatherDataAsync(string city)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={weatherApiKey}&units=metric";
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<WeatherResponse>(json);
                }
                else
                {
                    Console.WriteLine($"❌ Weather API call failed with status: {response.StatusCode}");
                    return null;
                }
            }
        }
    }

    public class WeatherResponse
    {
        public Main Main { get; set; }
        public Weather[] Weather { get; set; }
    }

    public class Main
    {
        public float Temp { get; set; }
    }

    public class Weather
    {
        public string Description { get; set; }
    }
}
