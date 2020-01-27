using System;

namespace WebApplicationGlobal.Models
{
    //classe Pessoa para tratamento dos objetos
    public class Pessoa
    {
        public long Id { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string UF { get; set; }
        public DateTime Nascimento  { get; set; }
    }
}
