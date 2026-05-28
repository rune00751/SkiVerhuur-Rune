namespace SvLib.DataObjects
{
    public class Materiaal
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public int MerkId { get; set; }
        public int TypeMateriaalId { get; set; }
        public string Foto { get; set; }

        public string MerkNaam { get; set; }
        public string TypeMateriaalNaam { get; set; }
    }
}
