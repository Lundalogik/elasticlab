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

            searchResponse = client.SearchForType();
            WriteResults(searchResponse, nameof(SearchForType));

            searchResponse = client.SearchInMultipleIndices();
            WriteResults(searchResponse, nameof(SearchInMultipleIndices));

            searchResponse = client.TermQuery();
            WriteResults(searchResponse, nameof(TermQuery));

            searchResponse = client.FreetextSearch();
            WriteResults(searchResponse, nameof(FreetextSearch));

            searchResponse = client.FreetextSearchNGram();
            WriteResults(searchResponse, nameof(FreetextSearchNGram));

            searchResponse = client.CombineQueies();
            WriteResults(searchResponse, nameof(CombineQueies));

            client.Aggregations();
            client.FilteredAggregations();
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

        private static ISearchResponse<SearchDocument> SearchForType(this IElasticClient client)
        {
            return client.Search<SearchDocument>(s => s
                .MatchAll()
                .Index(Indices.Index("pase"))
                .Type("Organization"));
        }

        private static ISearchResponse<SearchDocument> SearchInMultipleIndices(this IElasticClient client)
        {
            return client.Search<SearchDocument>(
                searchDescriptor => searchDescriptor
                    .MatchAll()
                    .Type("Organization")
                    .Index(Indices.Index("pase", "go1")));
        }

        private static ISearchResponse<SearchDocument> FreetextSearch(this IElasticClient client)
        {
            return client.Search<SearchDocument>(s => s
                .Query(q => q.Match(t => t.Field("freetext").Query("lundalogik")))
                .Index(Indices.Index("pase", "go1"))
                .Type("Organization"));
        }

        private static ISearchResponse<SearchDocument> FreetextSearchNGram(this IElasticClient client)
        {
            return client.Search<SearchDocument>(s => s
                .Query(q => q.Match(t => t.Field("freetext.freetext_edgengram").Query("lunda")))
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
                            m => m.Match(match => match.Query("lundalogik").Field("freetext")))
                        .Filter(filter => filter
                            .Bool(fb => fb
                                .Should(
                                    ss => ss.Term("applicationid", 102),
                                    ss => ss.Term("applicationid", 0)))))));
        }
        /*
        POST /pase,go1/Organization/_search
        {
           "aggs": {
              "myagg": {
                 "terms": {
                    "field": "tag"
                 }
              }
           }
        }
        */

        private static void Aggregations(this IElasticClient client)
        {
            var searchResult = client.Search<SearchDocument>(s => s
                .Index(Indices.Index("pase", "go1"))
                .Type("Organization")
                .Aggregations(agg => agg.Terms("myagg", terms => terms.Field("tag"))));

            var aggs = searchResult.Aggs.Terms("myagg");
            var buckets = aggs.Buckets;

            Console.WriteLine("Terms aggregation results");

            var index = 0;
            Console.WriteLine("Bucket no.\tTerm\t\tCount");
            foreach (var keyedBucket in buckets)
            {
                index++;
                Console.WriteLine($"{index}\t\t{keyedBucket.Key}\t\t{keyedBucket.DocCount}");
                if (index < buckets.Count)
                {
                    Console.WriteLine("-------------------------------------------------");
                }
            }
            Continue();
        }

        /*
        POST /pase,go1/Organization/_search
        {
           "aggs": {
              "applicationid_filter": {
                 "filter": {
                    "bool": {
                       "should": [
                          {
                             "term": {
                                "applicationid": {
                                   "value": 102
                                }
                             }
                          },
                          {
                             "term": {
                                "applicationid": {
                                   "value": 0
                                }
                             }
                          }
                       ]
                    }
                 },
                 "aggs": {
                    "myagg": {
                       "terms": {
                          "field": "tag"
                       }
                    }
                 }
              }
           }
        }
        */

        private static void FilteredAggregations(this IElasticClient client)
        {
            var searchResult = client.Search<SearchDocument>(s => s
                .Index(Indices.Index("pase", "go1"))
                .Type("Organization")
                .Aggregations(agg => agg
                    .Filter("applicationid_filter",
                        filterAgg => filterAgg
                            .Filter(appIdFilter => appIdFilter
                            .Bool(fb => fb
                                .Should(
                                    ss => ss.Term("applicationid", 102),
                                    ss => ss.Term("applicationid", 0))))
                            .Aggregations(termsagg => termsagg.Terms("myagg", terms => terms.Field("tag"))))));

            var aggs = searchResult.Aggs.Filter("applicationid_filter").Terms("myagg");
            var buckets = aggs.Buckets;

            Console.WriteLine("Terms aggregation results");

            var index = 0;
            Console.WriteLine("Bucket no.\tTerm\t\tCount");
            foreach (var keyedBucket in buckets)
            {
                index++;
                Console.WriteLine($"{index}\t\t{keyedBucket.Key}\t\t{keyedBucket.DocCount}");
                if (index < buckets.Count)
                {
                    Console.WriteLine("-------------------------------------------------");
                }
            }
            Continue();
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