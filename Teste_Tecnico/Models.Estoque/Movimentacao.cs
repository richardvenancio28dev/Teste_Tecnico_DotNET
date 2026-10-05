using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teste_Tecnico.Models.Estoque
{
    class Movimentacao
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Identificador único
        public int CodigoProduto { get; set; }
        public TipoMovimentacao Tipo { get; set; }
        public int Quantidade { get; set; }
        public string Descricao { get; set; }
        public DateTime DataHora { get; set; } = DateTime.Now;
    }
}
