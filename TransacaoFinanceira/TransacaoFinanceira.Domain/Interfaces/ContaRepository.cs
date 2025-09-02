using TransacaoFinanceira.Domain.Entities;

namespace TransacaoFinanceira.Domain.Interfaces
{
    public interface IContaRepository
    {
        ContaSaldo ObterConta(string contaId);
        bool AtualizarConta(ContaSaldo conta);
        bool AdicionarConta(ContaSaldo conta);
    }
}

