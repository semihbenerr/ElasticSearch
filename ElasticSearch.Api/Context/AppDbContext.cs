using Microsoft.EntityFrameworkCore;

namespace ElasticSearch.Api.Context
{
    public sealed class AppDbContext : DbContext
    //Sealed (mühürlü), sınıfların kalıtım işlemini engellemek için kullanılan bir anahtar kelimedir. Sealed olarak tanımlanan bir sınıftan kalıtım almaya çalışırsak hata verecektir. Sealed anahtar kelimesi bir sınıf için uygulanacak ise kalıtımı, bir üye için uygulanacak ise üyenin override edilmesini engellemektedir. 
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\local;Database=ElasticSearch;Trusted_Connection=True;Connect Timeout=30;MultipleActiveResultSets=True;");
        }
        public DbSet<Travel> Travels { get; set; }
    }
}
