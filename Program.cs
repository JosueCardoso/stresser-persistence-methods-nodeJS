using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.Linq;
using System.IO;

namespace stresser
{
    class Program
    {
        private static List<long> _requestsTimes = new List<long>();
        private static Dictionary<TypeOfTest, ConfigVO> _config = new Dictionary<TypeOfTest, ConfigVO>();


        static void Main(string[] args)
        {
            CreateConfigs();

            foreach(var typeOfTest in Enum.GetValues(typeof(TypeOfTest)).Cast<TypeOfTest>())                         
                ExecuteRequests(_config[typeOfTest]);
        }

        private static void ExecuteRequests(ConfigVO config)
        {
            for(int i = config.Attempt; i <= 3; i++)
            {
                _requestsTimes = new List<long>();
                List<Task> taskArray = new List<Task>();

                for (int j = 0; j < 5; j++) 
                    taskArray.Add(Task.Run(() => RequestApplicationAsync(config.StressRequestURL)));  

                Task.WaitAll(taskArray.ToArray());  
                WriteDataFile(config);
                RequestClearData(config.DeleteRequestURL);
                config.Attempt++;
            }            
        }

        private static void RequestClearData(string deleteRequestURL)
        {
            HttpClient client = new HttpClient();
            var stringContent = new StringContent(JsonSerializer.Serialize(new List<KeyValuePair<string, string>>()), Encoding.UTF8, "application/json");   

            var response = client.PostAsync(deleteRequestURL, stringContent);
            _ = response.Result.StatusCode;
        }

        private static async Task RequestApplicationAsync(string stressRequestURL)
        {
            HttpClient client = new HttpClient();
            
            for (int i = 0; i < 2000; i++)
            {               
                var stringContent = new StringContent(JsonSerializer.Serialize(new List<KeyValuePair<string, string>>()), Encoding.UTF8, "application/json");   
                var responseBody = String.Empty;
                var sw = new Stopwatch();
                
                sw.Start();             
                var response = await client.PostAsync(stressRequestURL, stringContent);

                if (response.IsSuccessStatusCode)                    
                    responseBody = await response.Content.ReadAsStringAsync();                

                sw.Stop();
                _requestsTimes.Add(sw.ElapsedMilliseconds);
                Console.WriteLine($"{sw.ElapsedMilliseconds} ms");
            }
        }

        private static void CreateConfigs()
        {
            var configSql = new ConfigVO{
                Attempt = 1,
                TypeOfTest = TypeOfTest.Sql,
                FileName = "./data/sql-attempt-{0}.csv",
                StressRequestURL = "http://localhost:9090/createAccountSql",
                DeleteRequestURL = "http://localhost:9090/deleteAllAccountSql"
            };

            var configNoSql = new ConfigVO{
                Attempt = 1,
                TypeOfTest = TypeOfTest.NoSql,
                FileName = "./data/no-sql-attempt-{0}.csv",
                StressRequestURL = "http://localhost:9090/createAccountNoSql",
                DeleteRequestURL = "http://localhost:9090/deleteAllAccountNoSql"
            };

            var configCacheNoSql = new ConfigVO{
                Attempt = 1,
                TypeOfTest = TypeOfTest.CacheNoSql,
                FileName = "./data/cache-sql-attempt-{0}.csv",
                StressRequestURL = "http://localhost:9090/createAccountCacheNoSql",
                DeleteRequestURL = "http://localhost:9090/deleteAllAccountCacheNoSql"
            };

            var configCacheSql = new ConfigVO{
                Attempt = 1,
                TypeOfTest = TypeOfTest.CacheSql,
                FileName = "./data/cache-sql-attempt-{0}.csv",
                StressRequestURL = "http://localhost:9090/createAccountCacheSql",
                DeleteRequestURL = "http://localhost:9090/deleteAllAccountCacheSql"
            };

            _config = new Dictionary<TypeOfTest, ConfigVO>
            {
                { TypeOfTest.Sql, configSql },
                { TypeOfTest.NoSql, configNoSql },
                { TypeOfTest.CacheSql, configCacheSql },
                { TypeOfTest.CacheNoSql, configCacheNoSql },
            };
        }

        private static void WriteDataFile(ConfigVO config)
        {
            StringBuilder csvContent = new StringBuilder();

            for(int i = 0; i < _requestsTimes.Count(); i++)
                csvContent.AppendLine($"{i},{_requestsTimes[i].ToString()}");            

            File.WriteAllText(string.Format(config.FileName, config.Attempt), csvContent.ToString());
        }
    }
}
