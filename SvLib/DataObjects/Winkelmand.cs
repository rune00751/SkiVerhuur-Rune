using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvLib.DataObjects
{
    public class Winkelmand
    {
        public int MateriaalMaatId { get; set; }
        public int MateriaalId { get; set; }
        public int MaatId { get; set; }
        public string TypeMateriaal { get; set; }
        public string Merk { get; set; }
        public string Model { get; set; }
        public string Maat { get; set; }
        public int Aantal { get; set; }
        public DateTime Begindatum { get; set; }
        public DateTime Einddatum { get; set; }

        public string Omschrijving
        {
            get { return TypeMateriaal + " - " + Merk + " " + Model + " - maat " + Maat; }
        }

        public string Periode
        {
            get
            {
                return "Periode: " + Begindatum.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture) + " tot " + Einddatum.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
            }
        }
    }
}
