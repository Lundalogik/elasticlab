using System;
using System.Collections;
using System.Collections.Generic;
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
            WriteResults(searchResponse.Documents);

            searchResponse = client.SearchInAnIndex();
            WriteResults(searchResponse.Documents);

            searchResponse = client.SearchInMultipleIndices();
            WriteResults(searchResponse.Documents);

            searchResponse = client.SearchForType();
            WriteResults(searchResponse.Documents);

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
        }

        private static ISearchResponse<SearchDocument> SearchInAnIndex(this IElasticClient client)
        {
            return client.Search<SearchDocument>(
                searchDescriptor => searchDescriptor
                    .MatchAll()
                    .Index("pase"));
        }
        
        private static ISearchResponse<SearchDocument> SearchInMultipleIndices(this IElasticClient client)
        {
            return client.Search<SearchDocument>(
                searchDescriptor => searchDescriptor
                    .MatchAll()
                    .Index(Indices.Index("pase", "go1")));
        }

        private static ISearchResponse<SearchDocument> SearchForType(this IElasticClient client)
        {
            return client.Search<SearchDocument>(s => s
                .MatchAll()
                .Index(Indices.Index("pase", "go1"))
                .Type("Organization"));
        }



        private static void WriteResults(IEnumerable<SearchDocument> documents)
        {
            Console.WriteLine(JsonConvert.SerializeObject(documents, Formatting.Indented));
            Console.WriteLine("==========================================================================");
            Console.WriteLine("Continue? (type 'q' to quit)");
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
