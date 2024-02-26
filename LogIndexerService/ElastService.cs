using Elasticsearch.Net;
using Nest;

namespace LogIndexerService
{
    public class ElastService
    {
        private static IElasticClient _client;

        public ElastService()
        {
            _client = ElasticConfig.GetClient();
        }

        public void CreateIndex(string indexName)
        {
            var createIndexResponse = _client.Indices.Create(indexName, c => c.Index(indexName));

            if (!createIndexResponse.IsValid)
            {
                // handle error
                throw new Exception(createIndexResponse.OriginalException.Message);
            }
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
