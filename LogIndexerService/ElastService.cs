using Elasticsearch.Net;
using Nest;
using SCP.Domain.Entity;
using System.Reflection.Metadata;

namespace LogIndexerService
{
    public class ElastService
    {
        private static IElasticClient _client;

        public ElastService()
        {
            _client = ElasticConfig.GetClient();
        }

        public void TryCreateIndex(string indexName)
        {
            var createIndexResponse = _client.Indices.Create(indexName, c => c.Index(indexName));

            if (!createIndexResponse.IsValid)
            {
                // handle error
                Console.WriteLine(createIndexResponse.OriginalException.Message);
            }
        }

        public void Insert(ActivityLog alog, string indexName)
        {
            _client.Index(alog, i => i.Id(alog.Id).Index(indexName));
        }

        public ActivityLog? GetLogById(string Id, string indexNam)
        {
            var result = _client.Search<ActivityLog>(q => q
                              .Index(indexNam)
                              .Query(qq => qq.Match(m => m.Field(f => f.Id).Query(Id))
                              ).Size(1));

            return result.Documents.FirstOrDefault();
        }

        public List<ActivityLog> Search(string logtext)
        {
            var searchRequest = new SearchRequest<ActivityLog>(Service.GetIndexName())
            {
                Query = new MatchPhraseQuery
                {
                    Field = Infer.Field<ActivityLog>(f => f.LogText),
                    Query = logtext
                }
            };

            var response = _client.Search<ActivityLog>(searchRequest);

            return response.Documents.ToList();
        }





        public static class ElasticConfig
        {

            static ElasticConfig()
            {
                var node1 = new Uri("http://localhost:9200/");
                //var node2 = new Uri("http://152.30.11.192:9200/");
                var nodes = new Uri[]
                {
                node1
                    //,node2
                };
                var connectionPool = new SniffingConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool)
                                    .SniffOnConnectionFault(false)
                                    .SniffOnStartup(false)
                                    .SniffLifeSpan(TimeSpan.FromMinutes(1));
                _client = new ElasticClient(connectionSettings);
            }
            public static IElasticClient GetClient()
            {
                return _client;
            }
        }
    }

    
}

public static class Service
{
    public static string GetIndexName()
    {
        return nameof(ActivityLog).ToLower();
    }
}
