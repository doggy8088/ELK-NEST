using System;
using System.Linq;
using System.Threading.Tasks;
using Nest;

namespace c2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // var client = new ElasticClient();

            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .DisableDirectStreaming()
                .DefaultIndex("people");

            var client = new ElasticClient(settings);

            var person = new Person
            {
                Id = 3,
                FirstName = "Will",
                LastName = "Chen"
            };

            var indexResponse = client.IndexDocument(person);
            Console.WriteLine(indexResponse);

            if (!indexResponse.IsValid)
            {
                // If the request isn't valid, we can take action here
            }

            // var asyncIndexResponse = await client.IndexDocumentAsync(person);
            // Console.WriteLine(asyncIndexResponse);

            var searchResponse = await client.SearchAsync<Person>(s =>
                s.From(0).Size(10)
                 .Query(q =>
                    q.Match(m =>
                        m.Field(f => f.FirstName)
                         .Query("Will")
                    )
                )
            );

            var debuginfo = searchResponse.DebugInformation;
            var exception = searchResponse.OriginalException;

            var people = searchResponse.Documents;

            var count = people.Count;

            foreach (var item in people)
            {
                System.Console.WriteLine("----------------------");
                System.Console.WriteLine(item.Id);
                System.Console.WriteLine(item.FirstName);
                System.Console.WriteLine(item.LastName);
                System.Console.WriteLine();
            }

            System.Console.WriteLine("=== Aggregations ===");


            var aggregationSearchResponse = await client.SearchAsync<Person>(s =>
                s.From(0).Size(10)
                 .Query(q =>
                    q.Match(m =>
                        m.Field(f => f.FirstName)
                         .Query("Will")
                    )
                )
                .Aggregations(a => a
                    .Terms("last_names", ta => ta
                        .Field(f => f.LastName.Suffix("keyword"))
                    )
                )
            );

            System.Console.WriteLine(aggregationSearchResponse.DebugInformation);

            var termsAggregation = aggregationSearchResponse.Aggregations.Terms("last_names");

            foreach (var item in termsAggregation.Buckets)
            {
                Console.WriteLine(item.Key + " " + item.DocCount);
            }

        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}