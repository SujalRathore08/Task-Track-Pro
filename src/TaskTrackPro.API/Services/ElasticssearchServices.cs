using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using TaskTrackPro.Core.Models;

namespace TaskTrackPro.API.Services
{
    public class ElasticssearchServices
    {
        private readonly ElasticsearchClient _client;
        private string _indexName;
        public ElasticssearchServices(IConfiguration configuration, ElasticsearchClient client)
        {
            _indexName = configuration["Elasticsearch:DefaultIndex"];
            _client = client;
        }
        public async Task<int> CreateIndexAsync()
        {
            var indexExistsResponse = await _client.Indices.ExistsAsync(_indexName);
            if (!indexExistsResponse.Exists)
            {
                var createIndexResponse = await _client.Indices.CreateAsync<t_task>(index => index.Index(_indexName)
                .Mappings(mappings => mappings.Properties(properties => properties
                .IntegerNumber(x => x.c_tid)
                .IntegerNumber(x => x.c_uid)
                .Keyword(x => x.c_task_title)
                .Keyword(x => x.c_description)
                .Date(x => x.c_start_date)
                .Date(x => x.c_end_date)
                .Keyword(x => x.c_task_status)
                )
                )
                );
                if (!createIndexResponse.IsValidResponse)
                {
                    Console.WriteLine($"Failed to create index: {createIndexResponse.DebugInformation}");
                    return -1;
                }
                Console.WriteLine("Task index created successfully.");
                return 1;
            }
            else
            {
                Console.WriteLine("Task index already exists.");
                return 0;
            }
        }
        public async Task IndexTaskAsync(t_task task){
            var resposnse = await _client.IndexAsync(task, idx => idx.Index(_indexName));
            if (!resposnse.IsValidResponse)
            {
                Console.WriteLine($"Failed to index task: {resposnse.DebugInformation}");
            }
        }

        public async Task<t_task?> SearchContactNameAsync(string name){
            var response = await _client.SearchAsync<t_task>(s => s
            .Query(q => q.Wildcard(w => w
            .Field(f => f.c_task_title.Suffix("keyword")).Value($"*{name}*"))));
            Console.WriteLine(response);
            if(response == null || response.Documents == null){
                Console.WriteLine(" ElasticSearch query returned null or invalid response.");
return null;
            }
            return response.Documents.FirstOrDefault();
        }
    }

}