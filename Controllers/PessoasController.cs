using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationGlobal.Models;
using WebApplicationGlobal.Repositories;
using WebApplicationGlobal.Services;

namespace WebApplicationGlobal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PessoasController : ControllerBase
    {
        private readonly PessoaContext _context;

        public object UserRepository { get; private set; }

        //contexto de informações passadas na urls e nos pacotes json
        public PessoasController(PessoaContext context)
        {
            _context = context;
        }

        // GET: api/Pessoas, retorna a lista de pessoas
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Pessoa>>> GetPessoas()
        {
            return await _context.Pessoas.ToListAsync();
        }

        // GET: api/Pessoas/GO, retorna a lista de pessoas por UF
        [HttpGet("{uf}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Pessoa>>> GetPessoas(string uf)
        {
            List<Pessoa> lista = await _context.Pessoas.ToListAsync();
            List<Pessoa> filtrada = new List<Pessoa> { };
            //implementar foreach para filtragem da lista geral
            foreach (var item in lista)
            {
                if (item.UF.ToLower() == uf.ToLower())
                {
                    filtrada.Add(item);
                }
            }
            return filtrada;
        }

        // GET: api/Pessoas/5, retorna pessoa por id
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<Pessoa>> GetPessoa(long id)
        {
            var pessoa = await _context.Pessoas.FindAsync(id);

            if (pessoa == null)
            {
                return NotFound();
            }

            return pessoa;
        }

        // PUT: api/Pessoas/5, para atualização de dados passa na url o id, e nas informações json
        // as informações de id (obrigatório) e os dados que queiram mudar
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<Pessoa>> PutPessoa(long id, Pessoa pessoa)
        {
            if (id != pessoa.Id)
            {
                return BadRequest();
            }

            _context.Entry(pessoa).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PessoaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return pessoa;
        }

        //Post de login: api/Pessoas/login
        //primeira url que o usuário tem que passar
        //json: {"username":"" ,"password":""}
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult<dynamic>> Authenticate([FromBody]Usuario model)
        {
            //chama o repositório definido
            var user = UsuarioRepository.Get(model.Username, model.Password);

            //verifica se é válido
            if (user == null)
                return NotFound(new { message = "Usuário ou senha inválidos" });

            //cria o token para esse usuário
            var token = TokenService.GenerateToken(user);
            //oculta o password do pacote json
            user.Password = "";
            //e retorna o usuário e o token pra ser usado nas outras requisiçoes com
            // a definição [Authorize]
            return new
            {
                user, token
            };
        }

        // POST: api/Pessoas, adiciona pessoas ao banco
        //json: {"id":"" ,"nome":"", "cpf":"", "uf":"", "nascimento":""}
        //exemplo de dado de nascimento: 2019-09-17T00:00:00
        //o horário n precisa vai zerado por default mas se quiser pode
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Pessoa>> PostPessoa(Pessoa pessoa)
        {
            //cpf digitado sem traços e barras
            //informações obrigatórias: nascimento, nome, uf, cpf(11 dígitos)
            if (pessoa.Nascimento == null && pessoa.UF == null && pessoa.Nome == null)
            {
                return BadRequest();
            }
            else if(pessoa.Cpf.Length != 11)
            {
                return BadRequest();
            }
            else if (!VerificaCpf(pessoa.Cpf))
            {
                return BadRequest();
            }
            _context.Pessoas.Add(pessoa);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPessoa), new { id = pessoa.Id }, pessoa);
        }

        // DELETE: api/Pessoas/5, deleta por id
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<Pessoa>> DeletePessoa(long id)
        {
            var pessoa = await _context.Pessoas.FindAsync(id);
            if (pessoa == null)
            {
                return NotFound();
            }

            _context.Pessoas.Remove(pessoa);
            await _context.SaveChangesAsync();

            return pessoa;
        }

        private bool PessoaExists(long id)
        {
            return _context.Pessoas.Any(e => e.Id == id);
        }

        public bool VerificaCpf(string cpf)
        {
            switch (cpf)
            {
                case "11111111111":
                    return false;
                case "00000000000":
                    return false;
                case "2222222222":
                    return false;
                case "33333333333":
                    return false;
                case "44444444444":
                    return false;
                case "55555555555":
                    return false;
                case "66666666666":
                    return false;
                case "77777777777":
                    return false;
                case "88888888888":
                    return false;
                case "99999999999":
                    return false;
            }
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;

            cpf = cpf.Trim(); //retira o indicador de fim da string \0

            tempCpf = cpf.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = resto.ToString();

            tempCpf += digito;

            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito += resto.ToString();

            //todo esse cálculo acima para achar o dígito verificador que se for igual
            // ao que está definido no cpf, é válido se verdade
            return cpf.EndsWith(digito);
        }
    }
}
