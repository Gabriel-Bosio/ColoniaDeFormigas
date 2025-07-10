using System.Globalization;

namespace ColoniaDeFormigas
{
    public class LeitorGrafo
    {
        private string _arquivo;

        public string Arquivo
        {
            get { return _arquivo; }
        }

        public LeitorGrafo(string arquivo)
        {
            _arquivo = arquivo;
        }

        public void GeraGrafo(ref Grafo grafo)
        {
            try
            {
                using (StreamReader sr = new StreamReader(Arquivo))
                {
                    //Lê primeira linha e processa dados
                    string linha = sr.ReadLine();
                    if (linha == null) return; // Interrompe caso não tenha lido nada
                    string[] partes = linha.Split(' ');

                    int vertices = int.Parse(partes[0]);
                    int arestas = int.Parse(partes[1]);
                    bool direcionado = partes[2] == "1" ? true : false;
                    bool ponderado = partes[3] == "1" ? true : false;

                    grafo = new Grafo(ponderado, direcionado);

                    //Gera as vértices com base no número lido
                    for (int i = 0; i < vertices; i++)
                    {
                        grafo.InserirVertice("C" + i);
                    }

                    //Lê e insere as arestas n vezes com base no valor lido
                    for (int i = 0; i < arestas; i++)
                    {
                        linha = sr.ReadLine();
                        if (linha == null) break; // Interrome caso não tenha lido nada

                        partes = linha.Split(' ');

                        int origem = int.Parse(partes[0]);
                        int destino = int.Parse(partes[1]);
                        double peso = ponderado ? double.TryParse(partes[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var resultado) ? resultado : 1 : 1;

                        grafo.InserirAresta(origem, destino, peso);


                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Erro ao gerar grafo, verifique a estrutura do arquivo");
                return;
            }
        }
    }
}
