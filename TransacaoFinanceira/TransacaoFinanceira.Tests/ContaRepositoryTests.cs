using System;
using System.Linq;
using TransacaoFinanceira.Domain.Entities;
using TransacaoFinanceira.Infrastructure.Repositories;
using Xunit;

namespace TransacaoFinanceira.Tests.Repositories
{
    public class ContaRepositoryMassasTests
    {
        private readonly ContaRepository _repository;

        public ContaRepositoryMassasTests()
        {
            _repository = new ContaRepository();
        }

        [Fact]
        public void AdicionarVariasContas_DeveInserirTodasCorretamente()
        {
            var contasNovas = new[]
            {
                new ContaSaldo("1001", 500),
                new ContaSaldo("1002", 1500),
                new ContaSaldo("1003", 0)
            };

            foreach (var conta in contasNovas)
            {
                var resultado = _repository.AdicionarConta(conta);
                Assert.True(resultado);
            }

            Assert.Equal(500, _repository.ObterConta("1001").Saldo);
            Assert.Equal(1500, _repository.ObterConta("1002").Saldo);
            Assert.Equal(0, _repository.ObterConta("1003").Saldo);
        }

        [Fact]
        public void AdicionarContasDuplicadas_DeveRetornarFalseESeguirComConsistencia()
        {
            var conta = new ContaSaldo("2001", 700);
            Assert.True(_repository.AdicionarConta(conta));

            // Tentativa de duplicar
            var duplicata = new ContaSaldo("2001", 999);
            Assert.False(_repository.AdicionarConta(duplicata));

            // Continua com o valor original
            var contaSalva = _repository.ObterConta("2001");
            Assert.Equal(700, contaSalva.Saldo);
        }

        [Fact]
        public void AtualizarMultiplasContas_DeveRefletirCorretamente()
        {
            var contasParaTestar = new[]
            {
                new ContaSaldo("3001", 100),
                new ContaSaldo("3002", -50),
                new ContaSaldo("3003", 999999)
            };

            foreach (var conta in contasParaTestar)
                Assert.True(_repository.AdicionarConta(conta));

            contasParaTestar[0].Saldo = 200;
            contasParaTestar[1].Saldo = 0;
            contasParaTestar[2].Saldo = 500000;

            foreach (var conta in contasParaTestar)
                Assert.True(_repository.AtualizarConta(conta));

            Assert.Equal(200, _repository.ObterConta("3001").Saldo);
            Assert.Equal(0, _repository.ObterConta("3002").Saldo);
            Assert.Equal(500000, _repository.ObterConta("3003").Saldo);
        }

        [Fact]
        public void ObterConta_DeveFuncionarComIdsDiversos()
        {
            var contas = new[]
            {
                new ContaSaldo("A1", 10),
                new ContaSaldo("00001", 20),
                new ContaSaldo("XYZ-999", 30)
            };

            foreach (var conta in contas)
                Assert.True(_repository.AdicionarConta(conta));

            Assert.Equal(10, _repository.ObterConta("A1").Saldo);
            Assert.Equal(20, _repository.ObterConta("00001").Saldo);
            Assert.Equal(30, _repository.ObterConta("XYZ-999").Saldo);
        }

        [Fact]
        public void StressTest_DeveAdicionarEManter1000Contas()
        {
            for (int i = 0; i < 1000; i++)
            {
                var conta = new ContaSaldo($"Stress-{i}", i);
                Assert.True(_repository.AdicionarConta(conta));
            }

            // Validar algumas no meio
            Assert.Equal(0, _repository.ObterConta("Stress-0").Saldo);
            Assert.Equal(500, _repository.ObterConta("Stress-500").Saldo);
            Assert.Equal(999, _repository.ObterConta("Stress-999").Saldo);
        }
    }
}
