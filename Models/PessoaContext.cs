using Microsoft.EntityFrameworkCore;

namespace WebApplicationGlobal.Models
{
    //criando o contexto de banco de dados das Pessoas gravado na memória interna (localhost) do dispositivo
    public class PessoaContext : DbContext
    {
        public PessoaContext(DbContextOptions<PessoaContext> options)
            : base(options)
        {
        }

        public DbSet<Pessoa> Pessoas { get; set; }
    }
}
