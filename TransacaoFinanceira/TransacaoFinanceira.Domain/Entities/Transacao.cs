using System;
namespace TransacaoFinanceira.Domain.Entities
{
	public class Transacao
	{
		
        public Transacao(string CorrelationId, string Datetime, string ContaOrigem, string ContaDestino, decimal Valor)
        {
            this.CorrelationId = CorrelationId;
            this.Datetime = DateTime.ParseExact(Datetime, "dd/MM/yyyy HH:mm:ss", null);
            this.ContaOrigem = ContaOrigem;
            this.ContaDestino = ContaDestino;
            this.Valor = Valor;
        }

        public string CorrelationId { get; set; }
        public DateTime Datetime { get; set; }
        public string ContaOrigem { get; set; }
        public string ContaDestino { get; set; }
        public decimal Valor { get; set; }

    }
}

