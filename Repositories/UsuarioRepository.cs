using System.Collections.Generic;
using System.Linq;
using WebApplicationGlobal.Models;

//por simplificações já é definido por hardcode o usuário e senha para autenticação
namespace WebApplicationGlobal.Repositories
{
    public class UsuarioRepository
    {
        public static Usuario Get(string username, string password)
        {
            var users = new List<Usuario>
            {
                new Usuario { Id = 1, Username = "Iuri", Password = "batman", Role = "desafio" }
            };
            return users.Where(x => x.Username.ToLower() == username.ToLower() && x.Password == x.Password).FirstOrDefault();
        }
    }
}
