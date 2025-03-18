using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using TaskTrackPro.Core.Models;


namespace TaskTrackPro.API.Services
{
    public class ElasticsearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly string _indexName;

        public ElasticsearchService(ElasticsearchClient client,IConfiguration configuration)
        {
            _client = client;
            _indexName = configuration["Elasticsearch:DefaultIndex"] ?? "task_index";
        }

        public async Task<int> CreateIndexAsync()
        {
            var indexExistsResponse = await _client.Indices.ExistsAsync(_indexName);
            if (!indexExistsResponse.Exists)
            {
                var createIndexResponse = await _client.Indices.CreateAsync<t_User>(index => index.Index(_indexName)
                    .Mappings(mappings => mappings.Properties(properties => properties
                        .IntegerNumber(x => x.c_uid)
                        .Text(x => x.c_uname, t => t.Fielddata(true))
                        .Text(x => x.c_email)
                        .Text(x => x.c_gender)
                        .Text(x => x.c_profilepicture)
                        .Boolean(x => x.c_user_status)
                    ))
                );
                if (!createIndexResponse.IsValidResponse)
                {
                    Console.WriteLine($"Failed to create index: {createIndexResponse.DebugInformation}");
                    return -1;
                }
                Console.WriteLine("User index created successfully.");
                return 1;
            }
            else
            {
                Console.WriteLine("User index already exists.");
                return 0;
            }
        }

        public async Task IndexUserAsync(t_User user){
            var response = await _client.IndexAsync(user, idx => idx.Index(_indexName));
            if(!response.IsValidResponse){
                Console.WriteLine($"Failed to index user: {response.DebugInformation}");
            }else{
                Console.WriteLine($"User {user.c_uname} indexed successfully in ElasticSearch.");
            }
        }

        public async Task<List<t_User?>> SearchUserAsync(string name){
            var searchResponse = await _client.SearchAsync<t_User>(s => s
                .Index(_indexName)
                .Query(q => q
                    .Wildcard(w => w
                        .Field(f => f.c_uname)
                        .Value($"*{name}*") 
                        .CaseInsensitive(true)
                    )
                )
            );
            if(!searchResponse.IsValidResponse){
                Console.WriteLine($"Failed to search user: {searchResponse.DebugInformation}");
                return new List<t_User>();
            }
            return searchResponse.Documents.ToList();
        }
    }
}