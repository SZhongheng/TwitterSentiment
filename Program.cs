using System;
using System.Configuration;
using System.Text.RegularExpressions;
using Tweetinvi;
using Azure;
using Azure.AI.TextAnalytics;
using System.Threading.Tasks;
using Tweetinvi.Models;
using System.Data.SqlClient;
using System.Data;

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
        private static readonly string dbcon = ConfigurationManager.AppSettings.Get("DbConnectionString");
        async static Task Main(string[] args)
        {




            // await createStream();
            //CreateTable();
            ExecuteQuery("DROP TABLE tweet");
            //stream.MatchingTweetReceived += (sender, eventReceived) =>
            //{
            //    var client = new TextAnalyticsClient(endpoint, credentials);
            //    var sanitized = Sanitize(eventReceived.Tweet.Text);
            //    DocumentSentiment documentSentiment = client.AnalyzeSentiment(sanitized);
            //    Console.WriteLine($"Document sentiment: {documentSentiment.Sentiment}\n");
            //    foreach (var sentence in documentSentiment.Sentences)
            //    {   
            //        Console.WriteLine($"\tText: \"{sentence.Text}\"");
            //        Console.WriteLine($"\tSentence sentiment: {sentence.Sentiment}");
            //        Console.WriteLine($"\tPositive score: {sentence.ConfidenceScores.Positive:0.00}");
            //        Console.WriteLine($"\tNegative score: {sentence.ConfidenceScores.Negative:0.00}");
            //        Console.WriteLine($"\tNeutral score: {sentence.ConfidenceScores.Neutral:0.00}\n");
            //    }

            //};



        }



        public static async Task createStream ()
        {

            TwitterClient twitter = new TwitterClient(apiKey, apiSecret, OauthKey, OauthSecret);
            var stream = twitter.Streams.CreateFilteredStream();
            int election = 0;
            stream.AddLanguageFilter(LanguageFilter.English);
            stream.AddTrack("election", tweet =>
            {
                Analyze(tweet.Text);
                election++;
                if (election == 10)
                {
                    stream.Stop();
                    Console.WriteLine("Complete!");
                }
            });
            await stream.StartMatchingAnyConditionAsync();

        }

       private static void Analyze (string x)
        {
            var client = new TextAnalyticsClient(endpoint, credentials);
            var sanitized = Sanitize(x);
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

        //private static void OnMatchedTweet(object sender, MatchedTweetReceivedEventArgs args)
        //{
        //    var client = new TextAnalyticsClient(endpoint, credentials);
        //    var sanitized = Sanitize(args.Tweet.FullText);
        //    Console.WriteLine(args.Tweet);
        //    DocumentSentiment documentSentiment = client.AnalyzeSentiment(sanitized);
        //    Console.WriteLine($"Document sentiment: {documentSentiment.Sentiment}\n");
        //    foreach (var sentence in documentSentiment.Sentences)
        //    {
        //        Console.WriteLine($"\tText: \"{sentence.Text}\"");
        //        Console.WriteLine($"\tSentence sentiment: {sentence.Sentiment}");
        //        Console.WriteLine($"\tPositive score: {sentence.ConfidenceScores.Positive:0.00}");
        //        Console.WriteLine($"\tNegative score: {sentence.ConfidenceScores.Negative:0.00}");
        //        Console.WriteLine($"\tNeutral score: {sentence.ConfidenceScores.Neutral:0.00}\n");
        //    }


        //}

        private static string Sanitize(string raw)
        {
            return Regex.Replace(raw, @"(@[A-Za-z0-9]+)|([^0-9A-Za-z \t])|(\w+:\/\/\S+)", " ").ToString();
        }

        public static void CreateTable()
        {
            SqlConnection con = null;
            try
            {
                // Creating Connection  
                con = new SqlConnection(dbcon);
                // writing sql query  
                SqlCommand cm = new SqlCommand("create table tweet(id int not null, string varchar(100), email varchar(50), join_date date)", con);
                // Opening Connection  
                con.Open();
                // Executing the SQL query  
                cm.ExecuteNonQuery();
                // Displaying a message  
                Console.WriteLine("Table created Successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine("OOPs, something went wrong." + e);
            }
            // Closing the connection  
            finally
            {
                con.Close();
            }
        }

        public static bool ExecuteQuery(String pQuery)
        {
            SqlConnection con = new SqlConnection(dbcon);
            con.Open();

            try
            {
                SqlCommand cmd = new SqlCommand(pQuery, con);

                // if you pass just query text
                cmd.CommandType = CommandType.Text;

                // if you pass stored procedure name
                // cmd.CommandType = CommandType.StoredProcedure;   

                cmd.ExecuteNonQuery();

                con.Close();

                return true;
            }
            catch (Exception exp)
            {
                con.Close();
                Console.WriteLine("error");
            }

            return false;
        }
    }
}