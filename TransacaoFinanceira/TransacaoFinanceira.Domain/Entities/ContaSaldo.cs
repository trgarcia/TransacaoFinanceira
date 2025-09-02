namespace TransacaoFinanceira.Domain.Entities
{
	public class ContaSaldo
	{
        public ContaSaldo(string IdConta, decimal Saldo)
        {
            this.IdConta = IdConta;
            this.Saldo = Saldo;
        }
        public string IdConta { get; set; }
        public decimal Saldo { get; set; }
    }
}

