using System;
using Nest;
using Newtonsoft.Json;

namespace ElasticLabs
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ElasticClient(new Uri("http://192.168.99.100:9200"));
            var response = client.Search<SearchDocument>(s => s.MatchAll());

            foreach (var hit in response.Hits)
            {
                Console.WriteLine(JsonConvert.SerializeObject(hit.Source));
            }

            Console.Read();
        }
    }

    internal class SearchDocument 
    {
        [JsonProperty("heading")]
        public string Heading { get; set; }

        [JsonProperty("subheading")]
        public string SubHeading { get; set; }

    }
}
