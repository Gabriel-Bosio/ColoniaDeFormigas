using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColoniaDeFormigas
{
    public class Grafo
    {
        #region Atributos, propriedades e construtor

        public bool Ponderado { get; set; }
        public bool Direcionado { get; set; }
        public List<string> Vertices { get; set; }
        public List<List<double>> Arestas { get; set; }

        public Grafo(bool ponderado, bool direcionado)
        {
            Ponderado = ponderado;
            Direcionado = direcionado;
            Vertices = new();
            Arestas = new();
        }

        public Grafo(Grafo grafo)
        {
            Ponderado = grafo.Ponderado;
            Direcionado = grafo.Direcionado;
            Vertices = new();
            grafo.Vertices.ForEach(Vertices.Add);
            Arestas = new();
            grafo.Arestas.ForEach(ar => Arestas.Add(new(ar)));
        }

        public Grafo(Grafo grafo, double pesoInicial)
        {
            Ponderado = grafo.Ponderado;
            Direcionado = grafo.Direcionado;
            Vertices = new();
            grafo.Vertices.ForEach(Vertices.Add);
            Arestas = new();
            int n = Vertices.Count;
            Arestas = new(n);

            for (int i = 0; i < n; i++)
            {
                List<double> linha = new(n);
                for (int j = 0; j < n; j++)
                {
                    double pesoOriginal = grafo.Arestas[i][j];
                    linha.Add(pesoOriginal != 0 ? pesoInicial : 0);
                }
                Arestas.Add(linha);
            }
        }

        #endregion

        #region Controle vértices
        public bool InserirVertice(string label)
        {

            // Não insere caso label já existir
            if (!Vertices.Any(x => x.Equals(label)))
            {
                Vertices.Add(label);
                int index = Vertices.IndexOf(label);

                Arestas.ForEach(x => x.Add(0));
                Arestas.Add(new List<double>());
                for (int i = 0; i <= index; i++) Arestas[index].Add(0);
                return true;
            }
            return false;
        }

        public bool RemoverVertice(int indice)
        {
            //Não remove caso índice inválido
            if (indice < 0 || indice >= Vertices.Count)
                return false;

            Vertices.RemoveAt(indice);

            Arestas.RemoveAt(indice);
            Arestas.ForEach(x => x.RemoveAt(indice));

            return true;
        }

        public string LabelVertice(int index)
        {
            return Vertices[index];
        }

        #endregion

        #region Grafo e controle de arestas
        public void ImprimeGrafo() // Em processo
        {

            // Define o espaçamento entre colunas
            int maxS = Vertices.MaxBy(x => x.Length).Length + 2;

            // Gera primeira linha com labels
            GeraEspaco(maxS);
            for (int i = 0; i < Vertices.Count; i++)
            {
                Console.Write(Vertices[i]);
                GeraEspaco(maxS - Vertices[i].Length);
            }

            Console.Write("\n\n");

            for (int i = 0; i < Vertices.Count; i++)
            {
                Console.Write(Vertices[i]);
                GeraEspaco(maxS - Vertices[i].Length);

                // Imprime coluna
                for (int j = 0; j < Vertices.Count; j++)
                {
                    Console.Write(Arestas[i][j]);
                    GeraEspaco(maxS - Arestas[i][j].ToString().Length);
                }
                Console.Write("\n\n");
            }
        }

        private void GeraEspaco(int size)
        {
            for (int i = 0; i < size; i++)
            {
                Console.Write(" ");
            }

        }

        public bool InserirAresta(int origem, int destino, double peso = 1)
        {
            if (ExisteAresta(origem, destino) || peso <= 0) return false; // Não insere caso já exista

            double val = !Ponderado ? 1 : peso;

            Arestas[origem][destino] = val;

            if (!Direcionado) Arestas[destino][origem] = val;

            return true;
        }

        public bool RemoverAresta(int origem, int destino)
        {
            if (!ExisteAresta(origem, destino)) return false; // Não remove caso não exista

            Arestas[origem][destino] = 0;

            if (!Direcionado) Arestas[destino][origem] = 0;

            return true;
        }

        public bool ExisteAresta(int origem, int destino)
        {
            return Arestas[origem][destino] == 0 ? false : true;
        }

        public double PesoAresta(int origem, int destino)
        {
            return Arestas[origem][destino];
        }

        public List<int> RetornarVizinhos(int vertice)
        {
            List<int> vizinhos = new List<int>();
            for (int i = 0; i < Arestas[vertice].Count; i++)
            {
                if (Arestas[vertice][i] > 0) vizinhos.Add(i);
            }

            return vizinhos;
        }
        #endregion
    }
}
