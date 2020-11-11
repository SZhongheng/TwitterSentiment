using System;
using System.Configuration;
using System.Globalization;
using System.Text.RegularExpressions;
using Tweetinvi;
using Tweetinvi.Events;
using Azure;
using Azure.AI.TextAnalytics;



namespace TwitterSentiment
{
    class Program
    {
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential(ConfigurationManager.AppSettings.Get("AzureKey"));
        private static readonly Uri endpoint = new Uri(ConfigurationManager.AppSettings.Get("endpoint"));
        private static readonly string apiKey  = ConfigurationManager.AppSettings.Get("twitterApiKey");
        private static readonly string apiSecret = ConfigurationManager.AppSettings.Get("twitterApiSecret");
        private static readonly string OauthSecret = ConfigurationManager.AppSettings.Get("OauthSecret");
        private static readonly string OauthKey = ConfigurationManager.AppSettings.Get("OauthKey");
        static void Main(string[] args)
        {

            
            TwitterClient twitter = new TwitterClient(apiKey, apiSecret, OauthKey, OauthSecret);
            var stream = twitter.Streams.CreateFilteredStream();
            //stream.AddTrack("Trump");
            stream.AddTrack("Testing");
            stream.AddLanguageFilter("en");
            stream.MatchingTweetReceived += OnMatchedTweet;
        }

        private static void OnMatchedTweet(object sender, MatchedTweetReceivedEventArgs args)
        {
            var client = new TextAnalyticsClient(endpoint, credentials);
            var sanitized = Sanitize(args.Tweet.FullText);
            Console.WriteLine(args.Tweet);
            DocumentSentiment documentSentiment = client.AnalyzeSentiment(sanitized);
            Console.WriteLine($"Document sentiment: {documentSentiment.Sentiment}\n");
            foreach (var sentence in documentSentiment.Sentences)
            {
                Console.WriteLine($"\tText: \"{sentence.Text}\"");
                Console.WriteLine($"\tSentence sentiment: {sentence.Sentiment}");
                Console.WriteLine($"\tPositive score: {sentence.ConfidenceScores.Positive:0.00}");
                Console.WriteLine($"\tNegative score: {sentence.ConfidenceScores.Negative:0.00}");
                Console.WriteLine($"\tNeutral score: {sentence.ConfidenceScores.Neutral:0.00}\n");
            }


        }

        private static string Sanitize(string raw)
        {
            return Regex.Replace(raw, @"(@[A-Za-z0-9]+)|([^0-9A-Za-z \t])|(\w+:\/\/\S+)", " ").ToString();
        }
    }
}