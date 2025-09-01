using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TransacaoFinanceira
{
    class Program
    {
        static void Main(string[] args)
        {
            var TRANSACOES = new[]
            {
                new {correlation_id= 1, datetime="09/09/2023 14:15:00", conta_origem= "938485762", conta_destino= "2147483649", VALOR= 150m},
                new {correlation_id= 2, datetime="09/09/2023 14:15:05", conta_origem= "2147483649", conta_destino= "210385733", VALOR= 149m},
                new {correlation_id= 3, datetime="09/09/2023 14:15:29", conta_origem= "347586970", conta_destino= "238596054", VALOR= 1100m},
                new {correlation_id= 4, datetime="09/09/2023 14:17:00", conta_origem= "675869708", conta_destino= "210385733", VALOR= 5300m},
                new {correlation_id= 5, datetime="09/09/2023 14:18:00", conta_origem= "238596054", conta_destino= "674038564", VALOR= 1489m},
                new {correlation_id= 6, datetime="09/09/2023 14:18:20", conta_origem= "573659065", conta_destino= "563856300", VALOR= 49m},
                new {correlation_id= 7, datetime="09/09/2023 14:19:00", conta_origem= "938485762", conta_destino= "2147483649", VALOR= 44m},
                new {correlation_id= 8, datetime="09/09/2023 14:19:01", conta_origem= "573659065", conta_destino= "675869708", VALOR= 150m},
            };

            var executor = new executarTransacaoFinanceira();

            // Cria grupos de transacoes independentes (sem contas em comum)
            var grupos = CriarGruposIndependentes(TRANSACOES);

            Parallel.ForEach(grupos, grupo =>
            {
                foreach (var t in grupo)
                {
                    executor.transferir(t.correlation_id, t.conta_origem, t.conta_destino, t.VALOR);
                }
            });
        }

        static List<List<dynamic>> CriarGruposIndependentes(dynamic[] transacoes)
        {
            var grupos = new List<List<dynamic>>();

            foreach (var transacao in transacoes)
            {
                List<dynamic> grupoEncontrado = null;

                // Passo 1: procura grupos que ja tem conflito com a transacao
                foreach (var grupo in grupos)
                {
                    if (grupo.Any(x => x.conta_origem == transacao.conta_origem || x.conta_destino == transacao.conta_origem
                                     || x.conta_origem == transacao.conta_destino || x.conta_destino == transacao.conta_destino))
                    {
                        grupo.Add(transacao);
                        grupoEncontrado = grupo;
                        break;
                    }
                }

                // Passo 2: se nao encontrou grupo, cria novo
                if (grupoEncontrado == null)
                {
                    grupoEncontrado = new List<dynamic> { transacao };
                    grupos.Add(grupoEncontrado);
                    continue;
                }

                // Passo 3: verifique outros grupos e move transacoes conflitantes
                for (int i = grupos.Count - 1; i >= 0; i--)
                {
                    var grupo = grupos[i];
                    if (grupo == grupoEncontrado) continue; // nao verificar o proprio grupo

                    var itensParaMover = grupo.Where(x =>
                           x.conta_origem == transacao.conta_origem || x.conta_destino == transacao.conta_origem
                            || x.conta_origem == transacao.conta_destino || x.conta_destino == transacao.conta_destino
                            ).ToList();

                    foreach (var item in itensParaMover)
                    {
                        grupo.Remove(item);
                        
                        DateTime dataItem = DateTime.ParseExact(item.datetime, "dd/MM/yyyy HH:mm:ss", null);

                        // Encontra a posicao correta no grupoEncontrado
                        int pos = grupoEncontrado.FindIndex(x =>
                        {
                            DateTime dataX = DateTime.ParseExact(x.datetime, "dd/MM/yyyy HH:mm:ss", null);
                            return dataItem < dataX; // insere antes do primeiro item maior
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

            return grupos;
        }


        class executarTransacaoFinanceira : acessoDados
        {
            public void transferir(int correlation_id, string conta_origem, string conta_destino, decimal valor)
            {
                
                var conta_saldo_origem = getSaldo<contas_saldo>(conta_origem);
                if (conta_saldo_origem.saldo < valor)
                {
                    Console.WriteLine("Transacao {0} cancelada por falta de saldo", correlation_id);
                }
                else
                {
                    var conta_saldo_destino = getSaldo<contas_saldo>(conta_destino);
                    conta_saldo_origem.saldo -= valor;
                    conta_saldo_destino.saldo += valor;

                    Console.WriteLine("Transacao {0} efetuada! Saldos: Origem={1} | Destino={2}",
                        correlation_id, conta_saldo_origem.saldo, conta_saldo_destino.saldo);
                }
            }
        }

        class contas_saldo
        {
            public contas_saldo(string conta, decimal valor)
            {
                this.conta = conta;
                this.saldo = valor;
            }
            public string conta { get; set; }
            public decimal saldo { get; set; }
        }

        class acessoDados
        {
            protected List<contas_saldo> TABELA_SALDOS;

            public acessoDados()
            {
                TABELA_SALDOS = new List<contas_saldo>
            {
                new contas_saldo("938485762", 180),
                new contas_saldo("347586970", 1200),
                new contas_saldo("2147483649", 0),
                new contas_saldo("675869708", 4900),
                new contas_saldo("238596054", 478),
                new contas_saldo("573659065", 787),
                new contas_saldo("210385733", 10),
                new contas_saldo("674038564", 400),
                new contas_saldo("563856300", 1200)
            };
            }

            public T getSaldo<T>(string id)
            {
                return (T)Convert.ChangeType(TABELA_SALDOS.Find(x => x.conta == id), typeof(T));
            }

            public bool atualizar<T>(T dado)
            {
                try
                {
                    contas_saldo item = (dado as contas_saldo);
                    TABELA_SALDOS.RemoveAll(x => x.conta == item.conta);
                    TABELA_SALDOS.Add(dado as contas_saldo);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
        }
    }
}
