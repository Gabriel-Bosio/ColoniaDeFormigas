using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColoniaDeFormigas
{
    public class Formiga
    {
        double ParametroAlpha { get; set; }
        double ParametroBeta { get; set; }
        public int Inicio {  get; set; }
        public List<int> Caminho { get; set; }
        public double Distancia { get; set; }

        public Formiga(int inicio, double parametroAlpha, double parametroBeta)
        {
            ParametroAlpha = parametroAlpha;
            ParametroBeta = parametroBeta;
            Inicio = inicio;
            Caminho = new List<int>();
            Distancia = 0;
        }

        public void PercorrerCaminho(Grafo mapaRotas, Grafo mapaFeromonio)
        {
            if(Caminho.Count > 0)
            {
                Console.WriteLine(" -- Esta formiga já percorreu o grafo!!");
                return;
            }

            int posicaoAtual = Inicio;
            Caminho.Add(posicaoAtual);

            while (Caminho.Count < mapaRotas.Vertices.Count)
            {
                List<int> vizinhos = mapaRotas.RetornarVizinhos(posicaoAtual).Where(v => !Caminho.Contains(v)).ToList();
                if(vizinhos.Count > 1)
                {
                    List<double> pesoVizinhos = new();
                    double pesoTotal = 0;
                    foreach (var vizinho in vizinhos)
                    {
                        // [Txy(t)]^Alpha * [Nxy(t)]^Beta
                        double peso = Math.Pow(mapaFeromonio.PesoAresta(posicaoAtual, vizinho), ParametroAlpha) * Math.Pow(1 / mapaRotas.PesoAresta(posicaoAtual, vizinho), ParametroBeta);
                        pesoVizinhos.Add(peso);
                        pesoTotal += peso;
                    }

                    Random random = new();
                    double aleatorio = random.NextDouble();

                    double valorAcumuldado = 0;

                    for (int i = 0; i < vizinhos.Count; i++)
                    {
                        // Pkxy = {[Txy(t)]^Alpha * [Nxy(t)]^Beta}/ SumNxk{[Txy(t)]^Alpha * [Nxy(t)]^Beta}
                        valorAcumuldado += pesoVizinhos[i] / pesoTotal; //Acumula probabilidade do vizinho atual 
                        if (aleatorio <= valorAcumuldado) // Se true, valor aleatório escolheu vizinho atual
                        {
                            Distancia += mapaRotas.PesoAresta(posicaoAtual, vizinhos[i]);
                            posicaoAtual = vizinhos[i];
                            Caminho.Add(posicaoAtual);
                            break;
                        }
                    }
                }
                else if(vizinhos.Count > 0) //Alternativa mais leve caso haja apenas um vizinho sobrando
                {
                    Distancia += mapaRotas.PesoAresta(posicaoAtual, vizinhos[0]);
                    posicaoAtual = vizinhos[0];
                    Caminho.Add(posicaoAtual);
                }
                else 
                {
                    Console.WriteLine(" -- Formiga falhou em percorrer em todos os vétices ou a retornar à vértice de origem");
                    Distancia = 0;
                    Caminho = new();
                    return;
                } 
            }

            Distancia += mapaRotas.PesoAresta(posicaoAtual, Inicio);
            posicaoAtual = Inicio;
            Caminho.Add(posicaoAtual);
        }
    }
}
