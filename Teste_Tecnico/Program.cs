using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Teste_Tecnico.Models;
using Teste_Tecnico.Models.Estoque;

namespace Teste_Tecnico
{
    class Program
    {
        static void Main(string[] args)
        {
            bool executando = true;

            while (executando)
            {
                Console.Clear();
                Console.WriteLine("=== MENU DE EXERCÍCIOS ===");
                Console.WriteLine("1 - Cálculo de Comissões");
                Console.WriteLine("2 - Controle de Estoque");
                Console.WriteLine("3 - Cálculo de Juros");
                Console.WriteLine("0 - Sair");
                Console.Write("\nEscolha uma opção: ");

                string entrada = Console.ReadLine()?.Trim();

                switch (entrada)
                {
                    case "1":
                        CalculaComissoes();
                        break;
                    case "2":
                        ControleDeEstoque();
                        break;
                    case "3":
                        CalcularJurosDiario();
                        break;
                    case "0":
                    case "exit":
                        executando = false;
                        break;
                    default:
                        Console.WriteLine("\nOpção inválida! Pressione qualquer tecla...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        #region Exercicio 1 : Calcular Comissoes
        private static void CalculaComissoes()
        {
            string json = Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory,
    "Files",
    "vendas.json"
    );
            DadosVendas dados = JsonConvert.DeserializeObject<DadosVendas>(System.IO.File.ReadAllText(json, System.Text.Encoding.UTF8));

            // Agrupo as vendas por vendedor
            var comissoes = dados.vendas.GroupBy(v => v.vendedor)
                .Select(grupo => new
                {
                    Vendedor = grupo.Key,
                    Comissao = grupo.Sum(v =>
                        v.valor < 100
                            ? 0
                            : v.valor < 500
                                ? v.valor * 0.01m
                                : v.valor * 0.05m
                    )
                });

            // Mostro o resultado
            foreach (var item in comissoes)
            {
                Console.WriteLine(
                    $"{item.Vendedor}: R$ {item.Comissao:N2}"
                );
            }

            FecharConsole();
        }
        #endregion
        #region Exercicio 2: Movimentação de Estoque
        public static void ControleDeEstoque()
        {
            string CaminhoArquivo = Path.Combine(
               AppDomain.CurrentDomain.BaseDirectory,
               "Files",
               "estoque.json"
           );

            while (true)
            {
                Console.Clear();
                Console.WriteLine("     SISTEMA DE MOVIMENTAÇÃO DE ESTOQUE     ");
                BaseEstoque baseEstoque = CarregarEstoque(CaminhoArquivo);

                ListarProdutos(baseEstoque.estoque);

                Console.WriteLine("\nOpções:");
                Console.WriteLine("[ 1 ] Lançar Movimentação (Entrada/Saída)");
                Console.WriteLine("[ sair ] Sair do sistema");
                Console.Write("\nEscolha uma opção: ");

                string opcao = Console.ReadLine()?.Trim().ToLower();

                if (opcao == "sair")
                {
                    break;
                }
                else if (opcao == "1")
                {
                    LancarMovimentacao(baseEstoque, CaminhoArquivo);
                }
                else
                {
                    Console.WriteLine("\nOpção inválida! Pressione qualquer tecla para continuar...");
                    Console.ReadKey();
                }
            }
        }


        private static void LancarMovimentacao(BaseEstoque baseEstoque,string CaminhoArquivo)
        {
            Console.WriteLine("\n--- NOVA MOVIMENTAÇÃO ---");

      
            Console.Write("Informe o Código do Produto: ");
            if (!int.TryParse(Console.ReadLine(), out int codigoProduto))
            {
                Console.WriteLine("Código inválido!");
                Pausar();
                return;
            }

            Produto produto = baseEstoque.estoque.FirstOrDefault(p => p.codigoProduto == codigoProduto);
            if (produto == null)
            {
                Console.WriteLine("Produto não encontrado!");
                Pausar();
                return;
            }


            Console.Write("Tipo da Movimentação (1 - Entrada / 2 - Saída): ");
            if (!Enum.TryParse(Console.ReadLine(), out TipoMovimentacao tipo) || !Enum.IsDefined(typeof(TipoMovimentacao), tipo))
            {
                Console.WriteLine("Tipo de movimentação inválido!");
                Pausar();
                return;
            }

 
            Console.Write("Quantidade a movimentar: ");
            if (!int.TryParse(Console.ReadLine(), out int quantidade) || quantidade <= 0)
            {
                Console.WriteLine("Quantidade deve ser um valor inteiro positivo!");
                Pausar();
                return;
            }

    
            if (tipo == TipoMovimentacao.Saida && quantidade > produto.estoque)
            {
                Console.WriteLine($"\n[ERRO] Saldo insuficiente! Estoque atual: {produto.estoque}");
                Pausar();
                return;
            }


            Console.Write("Descrição/Motivo da movimentação: ");
            string descricao = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(descricao))
            {
                Console.WriteLine("A descrição da movimentação é obrigatória!");
                Pausar();
                return;
            }

            Movimentacao mov = new Movimentacao
            {
                CodigoProduto = produto.codigoProduto,
                Tipo = tipo,
                Quantidade = quantidade,
                Descricao = descricao
            };

            if (tipo == TipoMovimentacao.Entrada)
                produto.estoque += quantidade;
            else
                produto.estoque -= quantidade; 
            SalvarEstoque(baseEstoque, CaminhoArquivo);

            Console.WriteLine("      MOVIMENTAÇÃO REALIZADA COM SUCESSO     ");
            Console.WriteLine($"ID Único da Movimentação : {mov.Id}");
            Console.WriteLine($"Produto                 : {produto.codigoProduto} - {produto.descricaoProduto}");
            Console.WriteLine($"Tipo                    : {mov.Tipo}");
            Console.WriteLine($"Quantidade Movimentada  : {mov.Quantidade}");
            Console.WriteLine($"Descrição               : {mov.Descricao}");
            Console.WriteLine($"Data/Hora               : {mov.DataHora:dd/MM/yyyy HH:mm:ss}");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine($" ESTOQUE FINAL : {produto.estoque} unidade(s) ");

            Pausar();
        }

        private static BaseEstoque CarregarEstoque(string CaminhoArquivo)
        {
            string jsonText = File.ReadAllText(CaminhoArquivo);
            return JsonConvert.DeserializeObject<BaseEstoque>(jsonText);
        }

        private static void SalvarEstoque(BaseEstoque baseEstoque,string CaminhoArquivo)
        {
            string jsonFormatado = JsonConvert.SerializeObject(baseEstoque, Formatting.Indented);
            File.WriteAllText(CaminhoArquivo, jsonFormatado);
        }

        private static void ListarProdutos(List<Produto> produtos)
        {
            Console.WriteLine("PRODUTOS DISPONÍVEIS NO DEPÓSITO:");
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine($"{"CÓDIGO",-8} | {"DESCRIÇÃO",-30} | {"ESTOQUE",-10}");
            Console.WriteLine("------------------------------------------------------------------");
            foreach (var p in produtos)
            {
                Console.WriteLine($"{p.codigoProduto,-8} | {p.descricaoProduto,-30} | {p.estoque,-10}");
            }
            Console.WriteLine("------------------------------------------------------------------");
        }

        private static void Pausar()
        {
            Console.WriteLine("\nPressione qualquer tecla para voltar ao menu...");
            Console.ReadKey();
        }
        #endregion
        #region Exercicio 3: Calcular Juros Diario
        private static void CalcularJurosDiario()
        {

            decimal taxaJurosDiaria = 0.025m;

            Console.Write("Digite o valor original (R$): ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal valorOriginal) || valorOriginal <= 0)
            {
                Console.WriteLine("Valor inválido!");
                FecharConsole();
                return;
            }

            Console.Write("Digite a data de vencimento (dd/MM/yyyy): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime dataVencimento))
            {
                Console.WriteLine("Data inválida!");
                FecharConsole();
                return;
            }

            DateTime dataHoje = DateTime.Today;

            dataVencimento = dataVencimento.Date;

            if (dataHoje <= dataVencimento)
            {
                Console.WriteLine("\nO título está em dia. Não há cobrança de juros.");
                Console.WriteLine($"Valor total a pagar: R$ {valorOriginal:N2}");
            }
            else
            {
                int diasAtraso = (dataHoje - dataVencimento).Days;

                // Cálculo de juros simples proporcional aos dias de atraso
                decimal percentualTotalJuros = taxaJurosDiaria * diasAtraso;
                decimal valorJuros = valorOriginal * percentualTotalJuros;
                decimal valorTotal = valorOriginal + valorJuros;

                Console.WriteLine("\n--- Resultado---");
                Console.WriteLine($"Data de Vencimento : {dataVencimento:dd/MM/yyyy}");
                Console.WriteLine($"Data de Hoje       : {dataHoje:dd/MM/yyyy}");
                Console.WriteLine($"Dias em Atraso     : {diasAtraso} dia(s)");
                Console.WriteLine($"Taxa Diária        : 2,5%");
                Console.WriteLine($"Valor dos Juros    : R$ {valorJuros:N2}");
                Console.WriteLine($"Valor Total a Pagar: R$ {valorTotal:N2}");
            }

            FecharConsole();
        }
        #endregion

        //Método generico para voltar ao menu
        private static void FecharConsole()
        {
            while (true)
            {
                Console.WriteLine("\nPressione qualquer tecla para voltar ao Menu:");
                Console.ReadKey();
                break;

            }
        }

    }
}
