using System;
using System.Collections.Generic;
using TransacaoFinanceira.Application.Services;
using TransacaoFinanceira.Domain.Entities;
using TransacaoFinanceira.Domain.Interfaces;
using Xunit;


namespace TransacaoFinanceira.Tests
{
    public class TransacaoServiceTestsMassas
    {
        [Fact]
        public void MultiplasTransacoesValidas_DevemAtualizarSaldos()
        {
            var repo = new ContaRepositoryFake();
            var service = new TransacaoService(repo);

            var transacoes = new List<Transacao>
        {
            new Transacao("1", DateTime.Now.ToString(), "123", "456", 100),
            new Transacao("2", DateTime.Now.AddSeconds(1).ToString(), "123", "456", 50)
        };

            service.ProcessarTransacoesParallel(transacoes);

            var contaOrigem = repo.ObterConta("123");
            var contaDestino = repo.ObterConta("456");

            Assert.Equal(350, contaOrigem.Saldo); // 500 - (100+50)
            Assert.Equal(250, contaDestino.Saldo); // 100 + (100+50)
        }

        [Fact]
        public void MultiplasTransacoesComUmaInvalida_ApenasValidasDevemExecutar()
        {
            var repo = new ContaRepositoryFake();
            var service = new TransacaoService(repo);

            var transacoes = new List<Transacao>
        {
            new Transacao("3", DateTime.Now.ToString(), "123", "456", 200),
            new Transacao("4", DateTime.Now.AddSeconds(1).ToString(), "123", "456", 2000) // inválida
        };

            service.ProcessarTransacoesParallel(transacoes);

            var contaOrigem = repo.ObterConta("123");
            var contaDestino = repo.ObterConta("456");

            Assert.Equal(300, contaOrigem.Saldo); // 500 - 200
            Assert.Equal(300, contaDestino.Saldo); // 100 + 200
        }

        [Fact]
        public void TransacoesSemConflitoDevemRodarEmParalelo()
        {
            var repo = new ContaRepositoryFake();
            repo.AdicionarConta(new ContaSaldo("789", 1000));
            repo.AdicionarConta(new ContaSaldo("999", 200));

            var service = new TransacaoService(repo);

            var transacoes = new List<Transacao>
        {
            new Transacao("5", DateTime.Now.ToString(), "123", "456", 100),
            new Transacao("6", DateTime.Now.ToString(), "789", "999", 300)
        };

            service.ProcessarTransacoesParallel(transacoes);

            Assert.Equal(400, repo.ObterConta("123").Saldo); // 500 - 100
            Assert.Equal(200, repo.ObterConta("456").Saldo); // 100 + 100
            Assert.Equal(700, repo.ObterConta("789").Saldo); // 1000 - 300
            Assert.Equal(500, repo.ObterConta("999").Saldo); // 200 + 300
        }

        [Fact]
        public void TransacoesComConflitoDevemManterOrdem()
        {
            var repo = new ContaRepositoryFake();
            var service = new TransacaoService(repo);

            var transacoes = new List<Transacao>
        {
            new Transacao("7", DateTime.Now.ToString(), "123", "456", 100),
            new Transacao("8", DateTime.Now.AddSeconds(1).ToString(), "456", "123", 50)
        };

            service.ProcessarTransacoesParallel(transacoes);

            Assert.Equal(450, repo.ObterConta("123").Saldo); // 500 - 100 + 50
            Assert.Equal(150, repo.ObterConta("456").Saldo); // 100 + 100 - 50
        }

        [Fact]
        public void StressTest_1000TransacoesDeveProcessarCorretamente()
        {
            var repo = new ContaRepositoryFake();
            repo.AdicionarConta(new ContaSaldo("777", 5000));
            repo.AdicionarConta(new ContaSaldo("888", 0));

            var service = new TransacaoService(repo);
            var transacoes = new List<Transacao>();

            for (int i = 0; i < 1000; i++)
            {
                transacoes.Add(new Transacao(i.ToString(), DateTime.Now.AddMilliseconds(i).ToString(), "777", "888", 1));
            }

            service.ProcessarTransacoesParallel(transacoes);

            Assert.Equal(4000, repo.ObterConta("777").Saldo); // 5000 - 1000
            Assert.Equal(1000, repo.ObterConta("888").Saldo); // 0 + 1000
        }
    }
}


// Fake repository só para testes
public class ContaRepositoryFake : IContaRepository
{
    private readonly Dictionary<string, ContaSaldo> _contas = new()
    {
        { "123", new ContaSaldo("123", 500) },
        { "456", new ContaSaldo("456", 100) }
    };

    public ContaSaldo ObterConta(string id) => _contas[id];

    public bool AtualizarConta(ContaSaldo conta)
    {
        _contas[conta.IdConta] = conta;
        return true;
    }

    public bool AdicionarConta(ContaSaldo conta)
    {
        if (_contas.ContainsKey(conta.IdConta))
            return false;

        _contas[conta.IdConta] = conta;
        return true;
    }
}

