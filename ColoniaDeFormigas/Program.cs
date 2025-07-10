using ColoniaDeFormigas;

internal class Program
{
    private static void Main(string[] args)
    {
        LeitorGrafo leitor = new LeitorGrafo(".\\..\\..\\..\\Grafo_100_Cidades.txt");
        Grafo cidades = null;

        leitor.GeraGrafo(ref cidades);

        //Inserir perametrização aqui
        
        double parametroAlpha = 1; // -- Alpha
        double parametroBeta = 5; // -- Beta
        double taxaEvaporacao = 0.5; // -- Sigma
        int qtdFormigasIteracao = cidades.Vertices.Count; // -- m
        double constanteAtualizacao = 100; // -- Q
        double feromonioInicial = 0.00001; // -- T0
        double parametroElitismo = 5; // -- e
        int numeroIteracoes = 1000; 
        

        Colonia colonia = new(taxaEvaporacao, parametroAlpha, parametroBeta, feromonioInicial, constanteAtualizacao, parametroElitismo , numeroIteracoes, qtdFormigasIteracao);
        //colonia.ResolverCaixeiroViajante(cidades);
        colonia.ResolverCaixeiroViajanteParalelo(cidades);
    }
}