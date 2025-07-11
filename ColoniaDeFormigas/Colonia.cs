using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColoniaDeFormigas
{
    public class Colonia
    {
        private double TaxaEvaporacao {  get; set; }
        private double ParametroAlpha { get; set; }
        private double ParametroBeta { get; set; }
        private double FeromonioInicial { get; set; }
        private double ConstanteAtualizacao { get; set; }
        private double ParametroElitismo { get; set; }
        private int NumeroIteracoes { get; set; }
        private int QtdFormigasIteracao { get; set; }


        public Colonia(double taxaEvaporacao, double parametroAlpha, double parametroBeta, double feromonioInicial, double constanteAtualizacao, double parametroElitismo, int numeroIteracoes, int qtdFormigasIteracao)
        {
            TaxaEvaporacao = taxaEvaporacao;
            ParametroAlpha = parametroAlpha;
            ParametroBeta = parametroBeta;
            FeromonioInicial = feromonioInicial;
            ConstanteAtualizacao = constanteAtualizacao;
            ParametroElitismo = parametroElitismo;
            NumeroIteracoes = numeroIteracoes;
            QtdFormigasIteracao = qtdFormigasIteracao;
        }

        public void ResolverCaixeiroViajante(Grafo mapaRotas)
        {
            Stopwatch sw = Stopwatch.StartNew();

            Grafo mapaFeromonios = new(mapaRotas, FeromonioInicial);
            double distanciaPrimeiraIteracao = int.MaxValue;
            double menorDistancia = int.MaxValue;
            List<int> menorCaminho = new();
            int ultimaIteracaoMelhorada = 0;
            double feromonioMelhorFormiga = 0;

            for (int iteracao = 0; iteracao < NumeroIteracoes; iteracao++)
            {
                Random rand = new();
                List<Formiga> formigas = new();

                //Seção de percorrimento do grafo
                for (int i = 0; i < QtdFormigasIteracao; i++)
                {
                    int inicio = rand.Next(mapaRotas.Vertices.Count);
                    Formiga formiga = new(inicio, ParametroAlpha, ParametroBeta);
                    formiga.PercorrerCaminho(mapaRotas, mapaFeromonios);
                    formigas.Add(formiga);
                }


                //Seção de calculo do feromonio lançado pelas formigas e verificação de distância
                Grafo feromonioIteracao = new(mapaFeromonios, 0);

                foreach (Formiga formiga in formigas)
                {
                    if (formiga.Distancia < menorDistancia)
                    {
                        menorDistancia = formiga.Distancia;
                        menorCaminho = formiga.Caminho;
                        ultimaIteracaoMelhorada = iteracao + 1;
                        Console.WriteLine($" -- Iteração {iteracao + 1} -- Formiga encontrou um melhor caminho!! Distância: {menorDistancia}");
                    }

                    double feromonioFormiga = ConstanteAtualizacao / formiga.Distancia; //DeltaTkxy = Q/dk 

                    if (formiga.Distancia < menorDistancia)
                    {
                        feromonioMelhorFormiga = feromonioFormiga;
                    }

                    for (int i = 0; i < formiga.Caminho.Count - 1; i++)
                    {
                        int origem = formiga.Caminho[i];
                        int destino = formiga.Caminho[i + 1];

                        double feromonioAresta = feromonioIteracao.PesoAresta(origem, destino) + feromonioFormiga;
                        feromonioIteracao.RemoverAresta(origem, destino);
                        feromonioIteracao.InserirAresta(origem, destino, feromonioAresta);
                    }
                }

                if (iteracao == 0)
                {
                    distanciaPrimeiraIteracao = menorDistancia;
                }

                //Seção de atualização de feromônio
                for (int i = 0; i < mapaFeromonios.Vertices.Count; i++)
                {
                    for (int j = 0; j < mapaFeromonios.Vertices.Count; j++)
                    {
                        if (!mapaFeromonios.Direcionado && j > i || i != j) //Não repete rotas caso não direcionado e evita insersão própria
                        {
                            double feromonioAnterior = mapaFeromonios.PesoAresta(i, j);

                            double bestD = 0;
                            for (int v = 0; v < menorCaminho.Count - 1; v++)
                            {
                                if (i == menorCaminho[v] && j == menorCaminho[v + 1] || i == menorCaminho[v + 1] && j == menorCaminho[v])
                                {
                                    bestD = feromonioMelhorFormiga;
                                    break;
                                }
                            }
                            //Tkxy(t) = (1 - Sigma) * Txy(t-1) + Sum(DeltaTkxy(t)) + e * Best(DeltaTkxy(t))
                            double feromonioAtualizado = (1 - TaxaEvaporacao) * feromonioAnterior + feromonioIteracao.PesoAresta(i, j) + ParametroElitismo * Math.Pow(bestD, ParametroElitismo);
                            mapaFeromonios.RemoverAresta(i, j);
                            mapaFeromonios.InserirAresta(i, j, feromonioAtualizado);
                        }
                    }
                }
            }
            sw.Stop();
            ApresentaResultadoCaixeiroViajante(mapaRotas, sw.Elapsed, distanciaPrimeiraIteracao, menorDistancia, menorCaminho, ultimaIteracaoMelhorada);
        }

        public void ResolverCaixeiroViajanteParalelo(Grafo mapaRotas)
        {
            Stopwatch sw = Stopwatch.StartNew();

            Grafo mapaFeromonios = new(mapaRotas, FeromonioInicial);
            double distanciaPrimeiraIteracao = double.MaxValue;
            double menorDistancia = double.MaxValue;
            List<int> menorCaminho = new();
            int ultimaIteracaoMelhorada = 0;
            double feromonioMelhorFormiga = 0;

            for (int iteracao = 0; iteracao < NumeroIteracoes; iteracao++)
            {
                Random rand = new();
                List<Formiga> formigas = new();

                //Seção de percorrimento do grafo
                ConcurrentBag<Formiga> formigasParalelas = new();

                Parallel.For(0, QtdFormigasIteracao, i =>
                {
                    int inicio = Random.Shared.Next(mapaRotas.Vertices.Count); 
                    Formiga formiga = new(inicio, ParametroAlpha, ParametroBeta);
                    formiga.PercorrerCaminho(mapaRotas, mapaFeromonios);
                    lock (formigas)
                    {
                        formigas.Add(formiga);
                    }
                    
                });

                //Seção de calculo do feromonio lançado pelas formigas e verificação de distância
                Grafo feromonioIteracao = new(mapaFeromonios, 0);

                foreach (Formiga formiga in formigas)
                {
                    if (formiga.Distancia < menorDistancia)
                    {
                        menorDistancia = formiga.Distancia;
                        menorCaminho = formiga.Caminho;
                        ultimaIteracaoMelhorada = iteracao + 1;
                        Console.WriteLine($" -- Iteração {iteracao + 1} -- Formiga encontrou um melhor caminho!! Distância: {menorDistancia}");
                    }

                    double feromonioFormiga = ConstanteAtualizacao / formiga.Distancia; //DeltaTkxy = Q/dk 

                    if (formiga.Distancia < menorDistancia)
                    {
                        feromonioMelhorFormiga = feromonioFormiga;
                    }

                    for (int i = 0; i < formiga.Caminho.Count - 1; i++)
                    {
                        int origem = formiga.Caminho[i];
                        int destino = formiga.Caminho[i + 1];

                        double feromonioAresta = feromonioIteracao.PesoAresta(origem, destino) + feromonioFormiga;
                        feromonioIteracao.RemoverAresta(origem, destino);
                        feromonioIteracao.InserirAresta(origem, destino, feromonioAresta);
                    }
                }

                if (iteracao == 0)
                {
                    distanciaPrimeiraIteracao = menorDistancia;
                }

                //Seção de atualização de feromônio
                int n = mapaFeromonios.Vertices.Count;
                Parallel.For(0, n * n, index =>
                {
                    int i = index / n;
                    int j = index % n;

                    if ((!mapaFeromonios.Direcionado && j > i) || i != j) //Não repete rotas caso não direcionado e evita insersão própria
                    {
                        double feromonioAnterior = mapaFeromonios.PesoAresta(i, j);

                        double bestD = 0;
                        for(int v = 0; v < menorCaminho.Count - 1; v++)
                        {
                            if(i == menorCaminho[v] && j == menorCaminho[v+ 1] || i == menorCaminho[v + 1] && j == menorCaminho[v])
                            {
                                bestD = feromonioMelhorFormiga;
                                break;
                            }
                        }

                        //Tkxy(t) = (1 - Sigma) * Txy(t-1) + Sum(DeltaTkxy(t)) + e * Best(DeltaTkxy(t))
                        double feromonioAtualizado = (1 - TaxaEvaporacao) * feromonioAnterior + feromonioIteracao.PesoAresta(i, j) + ParametroElitismo * Math.Pow(bestD, ParametroElitismo);

                        lock (mapaFeromonios)
                        {
                            mapaFeromonios.RemoverAresta(i, j);
                            mapaFeromonios.InserirAresta(i, j, feromonioAtualizado);
                        }
                    }
                });
            }
            sw.Stop();
            ApresentaResultadoCaixeiroViajante(mapaRotas, sw.Elapsed, distanciaPrimeiraIteracao, menorDistancia, menorCaminho, ultimaIteracaoMelhorada);
        }

        public void ApresentaResultadoCaixeiroViajante(Grafo mapaRotas, TimeSpan tempo, double distanciaPrimeiraIteracao, double menorDistancia, List<int> menorCaminho, int ultimaIteracaoMelhorada)
        {
            Console.WriteLine("\n\nResultado do caixeiro com colônia de formigas:");
            Console.WriteLine($"\n -- Tempo de execução: {tempo.ToString("mm\\:ss\\.fff")}");
            Console.WriteLine($"\n -- Distância encontrada na primeira iteracao: {distanciaPrimeiraIteracao}");
            Console.WriteLine($"\n -- Menor distância encontrada: {menorDistancia}");
            Console.WriteLine($"\n -- Última iteração que apresentou melhoria: {ultimaIteracaoMelhorada}");
            Console.WriteLine($"\n -- Total de iterações realizadas: {NumeroIteracoes}");
            Console.Write($"\nCaminho percorrido: ");
            for(int i = 0; i <  menorCaminho.Count; i++)
            {
                Console.Write($"{mapaRotas.LabelVertice(menorCaminho[i])} ");
                if(i != menorCaminho.Count - 1) Console.Write("-> ");
            }

            Console.WriteLine("\n\n\n\n");

        }
    }
}
