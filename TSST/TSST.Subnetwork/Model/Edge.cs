namespace TSST.Subnetwork.Model
{
    public class Edge
    {
        public int Id { get; set; }
        public Node Node1 { get; set; }
        public Node Node2 { get; set; }
        public int Length { get; set; }
        public bool State { get; set; }
    }
}
