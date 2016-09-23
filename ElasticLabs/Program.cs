using System;
using Nest;
using Newtonsoft.Json;

namespace ElasticLabs
{
    public static class Program
    {
        public static void Main()
        {
            var client = new ElasticClient(new Uri("http://192.168.99.100:9200"));

            var searchResponse = client.YouKnowForSearch();
            WriteResults(searchResponse, nameof(YouKnowForSearch));

            searchResponse = client.SearchInAnIndex();
            WriteResults(searchResponse, nameof(SearchInAnIndex));
        }

        private static ISearchResponse<SearchDocument> YouKnowForSearch(this IElasticClient client)
        {
            return client.Search<SearchDocument>(searchDescriptor => searchDescriptor.MatchAll());

            // Can also be written more verbosely like this:
            /*
            var searchDescriptor = new SearchDescriptor<SearchDocument>();
            searchDescriptor.MatchAll();
            return client.Search<SearchDocument>(searchDescriptor);
            */

            // Or like this
            /*
            var query = Query<SearchDocument>.MatchAll();
            return client.Search<SearchDocument>(s => s.Query(_ => query));
            */
        }

        private static ISearchResponse<SearchDocument> SearchInAnIndex(this IElasticClient client)
        {
            return client.Search<SearchDocument>(
                searchDescriptor => searchDescriptor
                    .MatchAll()
                    .Index("pase"));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void WriteResults(ISearchResponse<SearchDocument> response, string resultsFor)
        {
            Console.WriteLine($"Results for: {resultsFor}");
            Console.WriteLine("First 10 hits:");
            Console.WriteLine(JsonConvert.SerializeObject(response.Documents, Formatting.Indented));
            Console.WriteLine($"Total count: {response.Total}");
            Continue();
        }

        private static void Continue()
        {
            Console.WriteLine("==========================================================================");
            Console.WriteLine("Continue? (type 'q' to quit)");
            Console.WriteLine();
            var key = Console.ReadKey();

            if (key.KeyChar == 'q')
            {
                Environment.Exit(0);
            }
        }
    }

    public class SearchDocument
    {
        [JsonProperty("heading")]
        public string Heading { get; set; }

        [JsonProperty("subheading")]
        public string SubHeading { get; set; }
    }
}