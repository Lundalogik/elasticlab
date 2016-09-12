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

            searchResponse = client.SearchInMultipleIndices();
            WriteResults(searchResponse, nameof(SearchInMultipleIndices));

            searchResponse = client.SearchForType();
            WriteResults(searchResponse, nameof(SearchForType));

            searchResponse = client.FreetextSearch();
            WriteResults(searchResponse, nameof(FreetextSearch));

            searchResponse = client.TermQuery();
            WriteResults(searchResponse, nameof(TermQuery));

            searchResponse = client.CombineQueies();
            WriteResults(searchResponse, nameof(CombineQueies));
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

        private static ISearchResponse<SearchDocument> FreetextSearch(this IElasticClient client)
        {
            return client.Search<SearchDocument>(s => s
                .Query(q => q.Match(t => t.Field("tag").Query("some")))
                .Index(Indices.Index("pase", "go1"))
                .Type("Organization"));
        }

        private static ISearchResponse<SearchDocument> TermQuery(this IElasticClient client)
        {
            return client.Search<SearchDocument>(s => s
                .Query(q => q.Term(t => t.Field("tag").Value("some tag")))
                .Index(Indices.Index("pase", "go1"))
                .Type("Organization"));
        }

        private static ISearchResponse<SearchDocument> CombineQueies(this IElasticClient client)
        {
            return client.Search<SearchDocument>(s => s
                .Index(Indices.Index("pase", "go1"))
                .Type("Organization")
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            m => m.Term("tag", "some tag"),
                            m => m.Match(match => match.Query("lund").Field("freetext")))
                        .Filter(filter => filter
                            .Bool(fb => fb
                                .Should(
                                    ss => ss.Term("applicationid", 102),
                                    ss => ss.Term("applicationid", 0)))))));
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void WriteResults(ISearchResponse<SearchDocument> response, string resultsFor)
        {
            Console.WriteLine($"Results for: {resultsFor}");
            Console.WriteLine("First 10 hits:");
            Console.WriteLine(JsonConvert.SerializeObject(response.Documents, Formatting.Indented));
            Console.WriteLine($"Total count: {response.Total}");
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