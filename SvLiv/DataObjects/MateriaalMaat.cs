namespace SvLib.DataObjects
{
    public class MateriaalMaat
    {
        public int Id { get; set; }
        public int MateriaalId { get; set; }
        public int MaatId { get; set; }
        public int Aantal { get; set; }

        public string MateriaalModel { get; set; }
        public string MaatNaam { get; set; }
    }
}
