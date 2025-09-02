using System.Collections.Generic;
using TransacaoFinanceira.Application.Services;
using TransacaoFinanceira.Domain.Entities;

using TransacaoFinanceira.Infrastructure.Repositories;

namespace TransacaoFinanceira
{
    class Program
    {
        static void Main(string[] args)
        {
            var transacoes = new List<Transacao>
            {
                new Transacao("1", "09/09/2023 14:15:00", "938485762", "2147483649", 150),
                new Transacao("2", "09/09/2023 14:15:05", "2147483649", "210385733", 149),
                new Transacao("3", "09/09/2023 14:15:29", "347586970", "238596054", 1100),
                new Transacao("4", "09/09/2023 14:17:00", "675869708", "210385733", 5300),
                new Transacao("5", "09/09/2023 14:18:00", "238596054", "674038564", 1489),
                new Transacao("6", "09/09/2023 14:18:20", "573659065", "563856300", 49),
                new Transacao("7", "09/09/2023 14:19:00", "938485762", "2147483649", 44),
                new Transacao("8", "09/09/2023 14:19:01", "573659065", "675869708", 150)
            };

            var contasRepository = new ContaRepository();
            var transacaoService = new TransacaoService(contasRepository);

            transacaoService.ProcessarTransacoesParallel(transacoes,3);

        }
       
    }
}
