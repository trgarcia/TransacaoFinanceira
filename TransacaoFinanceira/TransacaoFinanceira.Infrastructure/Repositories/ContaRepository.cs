using System;
using System.Collections.Generic;
using System.Linq;
using TransacaoFinanceira.Domain.Entities;
using TransacaoFinanceira.Domain.Interfaces;

namespace TransacaoFinanceira.Infrastructure.Repositories
{
    public class ContaRepository : IContaRepository
    {
        private readonly List<ContaSaldo> _contas;

        public ContaRepository()
        {
            _contas = new List<ContaSaldo>
            {
                new ContaSaldo("938485762", 180),
                new ContaSaldo("347586970", 1200),
                new ContaSaldo("2147483649", 0),
                new ContaSaldo("675869708", 4900),
                new ContaSaldo("238596054", 478),
                new ContaSaldo("573659065", 787),
                new ContaSaldo("210385733", 10),
                new ContaSaldo("674038564", 400),
                new ContaSaldo("563856300", 1200)
            };
        }

        public ContaSaldo ObterConta(string contaId)
        {
            return _contas.FirstOrDefault(c => c.IdConta == contaId);
        }

        public bool AtualizarConta(ContaSaldo conta)
        {
            var index = _contas.FindIndex(c => c.IdConta == conta.IdConta);
            if (index >= 0)
            {
                _contas[index] = conta;
                return true;
            }
            
            return false;
                
        }

        public bool AdicionarConta(ContaSaldo conta)
        {
            var index = _contas.FindIndex(c => c.IdConta == conta.IdConta);
            if (index < 0)
            {
                _contas.Add(conta);
                return true;
            }

            return false;

        }

    }
}

