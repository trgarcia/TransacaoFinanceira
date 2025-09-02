using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using TransacaoFinanceira.Domain.Entities;
using TransacaoFinanceira.Domain.Interfaces;

namespace TransacaoFinanceira.Application.Services
{
	public class TransacaoService
	{
        private readonly IContaRepository _contaRepository;

        public TransacaoService(IContaRepository contaRepository)
        {
            _contaRepository = contaRepository;
        }

        public void ProcessarTransacoesParallel(IEnumerable<Transacao> transacoes,int numeroMaximoThreads = 4)
        {
            var transacoesOrdenadas = OrdenarTransacoesPorData(transacoes);

            var grupos = CriarGruposIndependentes(transacoesOrdenadas);

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = numeroMaximoThreads
            };

            Parallel.ForEach(grupos, grupo =>
            {
                foreach (var t in grupo)
                {
                    try
                    {
                        ExecutarTransacao(t);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao processar transação {t.CorrelationId}: {ex.Message}");
                    }
                }
            });
        }

        private void ExecutarTransacao(Transacao t)
        {
            try
            {
                var contaOrigem = _contaRepository.ObterConta(t.ContaOrigem);
                var contaDestino = _contaRepository.ObterConta(t.ContaDestino);
            
                if (contaOrigem.Saldo < t.Valor)
                {
                    Console.WriteLine($"Transação {t.CorrelationId} cancelada: saldo insuficiente.");
                    return;
                }
                contaOrigem.Saldo -= t.Valor;
                contaDestino.Saldo += t.Valor;

                _contaRepository.AtualizarConta(contaOrigem);
                _contaRepository.AtualizarConta(contaDestino);

                Console.WriteLine($"Transação {t.CorrelationId} efetuada! Origem={contaOrigem.Saldo} | Destino={contaDestino.Saldo}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro interno ao executar transação {t.CorrelationId}: {ex.Message}");
            
            }

        }
        

        private List<List<Transacao>> CriarGruposIndependentes(IEnumerable<Transacao> transacoes)
        {
            var grupos = new List<List<Transacao>>();

            foreach (var transacao in transacoes)
            {
                try { 
                    List<Transacao> grupoEncontrado = null;

                    // Passo 1: procura grupos que ja tem conflito com a transacao
                    foreach (var grupo in grupos)
                    {
                        if (grupo.Any(x => x.ContaOrigem == transacao.ContaOrigem || x.ContaDestino == transacao.ContaOrigem
                                         || x.ContaOrigem == transacao.ContaDestino || x.ContaDestino == transacao.ContaDestino))
                        {
                            grupo.Add(transacao);
                            grupoEncontrado = grupo;
                            break;
                        }
                    }

                    // Passo 2: se nao encontrou grupo, cria novo
                    if (grupoEncontrado == null)
                    {
                        grupoEncontrado = new List<Transacao> { transacao };
                        grupos.Add(grupoEncontrado);
                        continue;
                    }

                    // Passo 3: verifique outros grupos e move transacoes conflitantes
                    for (int i = grupos.Count - 1; i >= 0; i--)
                    {
                        var grupo = grupos[i];
                        if (grupo == grupoEncontrado) continue; // nao verificar o proprio grupo

                        var itensParaMover = grupo.Where(x =>
                               x.ContaOrigem == transacao.ContaOrigem || x.ContaDestino == transacao.ContaOrigem
                                || x.ContaOrigem == transacao.ContaDestino || x.ContaDestino == transacao.ContaDestino
                                ).ToList();

                        foreach (var item in itensParaMover)
                        {
                            grupo.Remove(item);


                            // Encontra a posicao correta no grupoEncontrado
                            int pos = grupoEncontrado.FindIndex(x =>
                            {
                                return item.Datetime < x.Datetime; // insere antes do primeiro item maior
                            });

                            if (pos >= 0)
                                grupoEncontrado.Insert(pos, item);
                            else
                                grupoEncontrado.Add(item);
                        }

                        // Se o grupo ficou vazio, remove
                        if (grupo.Count == 0)
                            grupos.RemoveAt(i);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro interno ao executar transação {transacao.CorrelationId}: {ex.Message}");

                }
            }
            

            return grupos;
        }

        private IEnumerable<Transacao> OrdenarTransacoesPorData(IEnumerable<Transacao> transacoes)
        {
            return transacoes.OrderBy(t => t.Datetime).ToList();
        }
    }
}

