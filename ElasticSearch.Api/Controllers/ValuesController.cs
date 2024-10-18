using Elasticsearch.Net;
using ElasticSearch.Api.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices.JavaScript;

namespace ElasticSearch.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        AppDbContext context = new();
        [HttpGet("[action]")]
        public async Task<IActionResult> CreateData(CancellationToken cancellationToken)//apiye isteğimiz gittikten sonra uygulamayı kapatsak dahi api isteği aldığı için işlemine devam edecektir. Veya isteği iptal edemediğimiz için mecburen bekleyeceğiz. İşte bu noktada Cancellation Token bizim bize büyük kolaylık sağlıyor.


        {
            IList<Travel> travels = new List<Travel>();
            var random = new Random();
            for (int i = 0; i < 50000; i++)
            {
                var title = new string(Enumerable.Repeat("abcçdefgğhıijklmnoöprsştuüvyz", 5).Select(s => s[random.Next(s.Length)]).ToArray());
                var words = new List<string>();
                for (int j = 0; j < 500; j++)
                {
                    words.Add(new string(Enumerable.Repeat("abcçdefgğhıijklmnoöprsştuüvyz", 5).Select(s => s[random.Next(s.Length)]).ToArray()));

                }
                var desc = string.Join(" ", words);
                var travel = new Travel()
                {
                    Title = title,
                    Description = desc
                };
                travels.Add(travel);
            }
            await context.Set<Travel>().AddRangeAsync(travels,cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return Ok();
        }
        [HttpGet("[action]/{description}")]
        public async Task<IActionResult> GetDataListWithEf(string description)
        {
            IList<Travel> travels = await context.Set<Travel>().Where(d => d.Description.Contains(description)).ToListAsync();
            return Ok(travels.Take(10));
        }
        [HttpGet("[action]/{description}")]
        public async Task<IActionResult> GetDataListWithEfAndAsNoTracking(string description)
        {
            IList<Travel> travels = await context.Set<Travel>().Where(d => d.Description.Contains(description)).AsNoTracking().ToListAsync();//Tek farkı .AsNoTracking() olması bir iki saniye fark ettiğini göstermek için
            return Ok(travels.Take(10));
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> SyncToElastic()
        {
            var settings = new ConnectionConfiguration(new Uri("http://localhost:9200"));
            var client = new ElasticLowLevelClient(settings);
            List<Travel> travels = await context.Travels.ToListAsync();
            var tasks = new List<Task>();
            foreach (var item in travels)
            {
                tasks.Add(client.IndexAsync<StringResponse>("travels", item.Id.ToString(), PostData.Serializable(new
                {
                    item.Id,
                    item.Title,
                    item.Description
                })));
            }
            await Task.WhenAll(tasks);
            return Ok();
        }
        [HttpGet("[action]/{description}")]
        public async Task<IActionResult> GetDataListWithElasticSearch(string description)
        {
            var settings = new ConnectionConfiguration(new Uri("http://localhost:9200"));
            var client = new ElasticLowLevelClient(settings);
            var response = await client.SearchAsync<StringResponse>("travels", PostData.Serializable(new
            {
                query=new
                {
                    wildcard = new
                    {
                        Description=new {value=$"*{description}*"}
                    }
                }
            }));
            var results = JObject.Parse(response.Body);
            var hits = results["hits"]["hits"].ToObject<List<JObject>>();
            List<Travel> travels = new List<Travel>();
            foreach (var hit in hits)
            {
                travels.Add(hit["_source"].ToObject<Travel>());
            }
            return Ok(travels.Take(20));
        }



        #region Veri Tabanı Indexleme Info

        /*
         Veri tabanımızda seçili dbdeki tablolara indexleme ekledik bunun için tableın altındaki açılan klasörlerde index add diyip işlem yaptık
        Yapılan işlem ;
            1-) Indexex -> New Index -> Non Clustered Index
            2-) Index key columns -> Add -> Id
            3-) Index Incloude columns -> Title,Description, ...
         */

        #endregion
    }
}
