
public partial class Edge
{
    public double Intensity { get; set; } // Peso da aresta - provavelmente vai definir também a largura da aresta no grafo

    public Edge(double Intensity)
    {
        this.Intensity = Intensity;
    }
}