using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using TaskTrackPro.Core.Models;
using Microsoft.Extensions.Configuration;

namespace TaskTrackPro.API.Services
{
    public class ElasticsearchServices
    {
        private readonly ElasticsearchClient _client;
        private readonly string _indexName;

        public ElasticsearchServices(IConfiguration configuration, ElasticsearchClient client)
        {
            _indexName = configuration["Elasticsearch:DefaultIndex"] ?? "task_index";
            _client = client;
        }

        /// <summary>
        /// Creates the index with the correct mapping for wildcard search.
        /// </summary>
        public async Task<int> CreateIndexAsync()
        {
            var indexExistsResponse = await _client.Indices.ExistsAsync(_indexName);
            if (!indexExistsResponse.Exists)
            {
                var createIndexResponse = await _client.Indices.CreateAsync<t_task>(index => index.Index(_indexName)
                    .Mappings(mappings => mappings.Properties(properties => properties
                        .IntegerNumber(x => x.c_tid)
                        .IntegerNumber(x => x.c_uid)
                        .Text(x => x.c_task_title, t => t.Fielddata(true)) // Enable fielddata for text field
                        .Text(x => x.c_description)
                        .Date(x => x.c_start_date)
                        .Date(x => x.c_end_date)
                        .Keyword(x => x.c_task_status)
                    ))
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

        /// <summary>
        /// Indexes a new task document into Elasticsearch.
        /// </summary>
        public async Task IndexTaskAsync(t_task task)
        {
            var response = await _client.IndexAsync(task, idx => idx.Index(_indexName));
            if (!response.IsValidResponse)
            {
                Console.WriteLine($"Failed to index task: {response.DebugInformation}");
            }
            else
            {
                Console.WriteLine("Task indexed successfully.");
            }
        }

        /// <summary>
        /// Searches for tasks based on a wildcard match in the task title.
        /// </summary>
        public async Task<List<t_task>> SearchTaskByTitleAsync(string name)
        {
            var response = await _client.SearchAsync<t_task>(s => s
                .Index(_indexName)
                .Query(q => q.Wildcard(w => w
                    .Field(f => f.c_task_title) // Use the text field for wildcard search
                    .Value($"{name}")
                    .CaseInsensitive(true) // Ensure case insensitivity
                ))
            );

            if (response == null || response.Documents == null)
            {
                Console.WriteLine("Elasticsearch query returned null or invalid response.");
                return new List<t_task>();
            }

            Console.WriteLine($"Search completed. Found {response.Documents.Count} tasks.");
            return response.Documents.ToList();
        }
    }
}