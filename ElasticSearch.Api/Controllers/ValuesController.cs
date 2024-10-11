using ElasticSearch.Api.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> GetDataList(string description)
        {
            IList<Travel> travels = await context.Set<Travel>().Where(d => d.Description.Contains(description)).ToListAsync();
            return Ok(travels.Take(10));
        }
        [HttpGet("[action]/{description}")]
        public async Task<IActionResult> GetDataListAsNoTracking(string description)
        {
            IList<Travel> travels = await context.Set<Travel>().Where(d => d.Description.Contains(description)).AsNoTracking().ToListAsync();//Tek farkı .AsNoTracking() olması bir iki saniye fark ettiğini göstermek için
            return Ok(travels.Take(10));
        }
    }
}
