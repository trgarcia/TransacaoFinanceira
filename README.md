# TransacaoFinanceira
Case para refatoração

Passos a implementar:
1. Corrija o que for necessario para resolver os erros de compilação.
2. Execute o programa para avaliar a saida, identifique e corrija o motivo de algumas transacoes estarem sendo canceladas mesmo com saldo positivo e outras sem saldo sendo efetivadas.
3. Aplique o code review e refatore conforme as melhores praticas(SOLID,Patterns,etc).
4. Implemente os testes unitários que julgar efetivo.
5. Crie um git hub e compartilhe o link respondendo o ultimo e-mail.

Obs: Voce é livre para implementar na linguagem de sua preferência desde que respeite as funcionalidades e saídas existentes, além de aplicar os conceitos solicitados.


# Refatoracao

## 1. Correções de Compilação  

Durante a análise inicial, foram identificados erros relacionados ao tipo de dado utilizado para representar números de contas.  

🔧 **Decisão:** utilizar o tipo `string` em vez de tipos numéricos, pelos seguintes motivos:  
- Nenhuma operação matemática é realizada sobre esse dado, logo não há necessidade de um tipo numérico.  
- O uso de `string` proporciona maior flexibilidade para futuras alterações (exemplo: CNPJs alfanuméricos).  
- Evita problemas com zeros à esquerda.  
- O impacto no consumo de memória é irrelevante frente à capacidade dos sistemas atuais.  

---

## 2. Correção da Regra de Negócio  

Ao executar o programa, foram observados comportamentos incorretos:  
- Algumas transações eram canceladas mesmo com saldo suficiente.  
- Outras eram aprovadas mesmo sem saldo disponível.  

🔍 **Causas identificadas:**  
- Erro de parametrização em um log, que gerava exceção.  
- Falha na regra de negócio devido à execução concorrente (threads) de transações interdependentes.  

✅ **Solução implementada:**  
- Manter a execução paralela para ganho de desempenho.  
- Criar um mecanismo de **agrupamento de transações interdependentes**, garantindo que:  
  - Transações dependentes sejam executadas de forma ordenada.  
  - Transações independentes possam ser executadas em paralelo.  

O método principal para isso foi **`CriarGruposIndependentes`**.  

⚠️ **Ponto de atenção:**  
Foi adicionada a possibilidade de limitar o número máximo de threads no método **`ProcessarTransacoesParallel`**, prevenindo problemas futuros em cenários com banco de dados e alto volume de operações.  

---

## 3. Refatoração e Arquitetura  

Após a correção funcional, foi aplicado **Code Review** com foco em boas práticas de desenvolvimento:  

- Aplicação de princípios **SOLID**.  
- Estruturação baseada em **Clean Architecture**.  
- Separação em **camadas** para facilitar a manutenção e escalabilidade:  
  - **Application:** regras de negócio.  
  - **Domain:** entidades e interfaces de domínio.  
  - **Infrastructure:** repositórios e interações externas.  
  - **Console:** ponto de entrada da aplicação.  

Essa divisão em **projetos/dlls** permite maior flexibilidade e adequação ao crescimento do sistema.  

---

## 4. Testes Unitários  

Foram implementados testes unitários para garantir a confiabilidade do código.  

- Diversos cenários e massas de dados foram considerados.  
- Testes cobrem todas as classes principais, **exceto**:  
  - A camada de domínio (por conter apenas entidades e contratos).  
  - O console (ponto de entrada da aplicação).  

O uso de repositórios **fake** permitiu isolar a lógica de negócio durante os testes.  

---

## ✅ Resultado Final  

Após a refatoração, o projeto:  
- Executa corretamente todas as transações.  
- Garante consistência mesmo em execução paralela.  
- Está organizado de forma escalável e aderente a boas práticas de arquitetura.  
- Possui cobertura de testes unitários para cenários críticos.  
