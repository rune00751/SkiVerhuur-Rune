using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using SvLib.DataObjects;

namespace SvLib.Managers
{
    public class HurenManager
    {
        private string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["Skiverhuur"].ConnectionString; }
        }

        public int GetTypeSportId(string sportNaam)
        {
            string Welke = sportNaam.Equals("Langlaufen", StringComparison.OrdinalIgnoreCase) ? "Lang%" : "Alpine";
            int id = 0;

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = "SELECT TOP 1 Id FROM TypeSport WHERE Naam LIKE @Naam ORDER BY Naam";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@Naam", Welke);

                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    if (ObjRea.Read())
                    {
                        id = Convert.ToInt32(ObjRea["Id"]);
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
                
                
            }

            return id;
        }

        public List<TypeMateriaal> GetTypeMaterialen(int typeSportId)
        {
            List<TypeMateriaal> typeMaterialen = new List<TypeMateriaal>();

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = @"
                    SELECT Id, Naam, TypeSportId
                    FROM TypeMateriaal
                    WHERE TypeSportId = @TypeSportId
                    ORDER BY Naam";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@TypeSportId", typeSportId);

                    ObjCn.Open();
                    SqlDataReader ObjRea= ObjCmd.ExecuteReader();

                    while (ObjRea.Read())
                    {
                        typeMaterialen.Add(new TypeMateriaal
                        {
                            Id = Convert.ToInt32(ObjRea["Id"]),
                            Naam = ObjRea["Naam"].ToString(),
                            TypeSportId = Convert.ToInt32(ObjRea["TypeSportId"])
                        });
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return typeMaterialen;
        }

        public List<Merk> GetMerkenVoorTypeMateriaal(int typeMateriaalId)
        {
            List<Merk> merken = new List<Merk>();

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = @"
                    SELECT DISTINCT m.Id, m.Naam
                    FROM Merk m
                    INNER JOIN Materiaal mat ON mat.MerkId = m.Id
                    WHERE mat.TypeMateriaalId = @TypeMateriaalId
                    ORDER BY m.Naam";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@TypeMateriaalId", typeMateriaalId);

                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    while (ObjRea.Read())
                    {
                        merken.Add(new Merk
                        {
                            Id = Convert.ToInt32(ObjRea["Id"]),
                            Naam = ObjRea["Naam"].ToString()
                        });
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return merken;
        }

        public List<Materiaal> GetMaterialen(int typeMateriaalId, int merkId)
        {
            List<Materiaal> materialen = new List<Materiaal>();

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = @"
                    SELECT Id, Model, MerkId, TypeMateriaalId, Foto
                    FROM Materiaal
                    WHERE TypeMateriaalId = @TypeMateriaalId AND MerkId = @MerkId
                    ORDER BY Model";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@TypeMateriaalId", typeMateriaalId);
                    ObjCmd.Parameters.AddWithValue("@MerkId", merkId);

                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    while (ObjRea.Read())
                    {
                        materialen.Add(new Materiaal
                        {
                            Id = Convert.ToInt32(ObjRea["Id"]),
                            Model = ObjRea["Model"].ToString(),
                            MerkId = Convert.ToInt32(ObjRea["MerkId"]),
                            TypeMateriaalId = Convert.ToInt32(ObjRea["TypeMateriaalId"]),
                            Foto = ObjRea["Foto"] == DBNull.Value ? string.Empty : ObjRea["Foto"].ToString()
                        });
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return materialen;
        }

        public List<Maat> GetMatenVoorMateriaal(int materiaalId)
        {
            List<Maat> maten = new List<Maat>();

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = @"
                    SELECT ma.Id, ma.Naam
                    FROM Maat ma
                    INNER JOIN MateriaalMaat mm ON mm.MaatId = ma.Id
                    WHERE mm.MateriaalId = @MateriaalId
                    ORDER BY ma.Naam";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@MateriaalId", materiaalId);

                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    while (ObjRea.Read())
                    {
                        maten.Add(new Maat
                        {
                            Id = Convert.ToInt32(ObjRea["Id"]),
                            Naam = ObjRea["Naam"].ToString()
                        });
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return maten;
        }

        public string GetFotoVoorMateriaal(int materiaalId)
        {
            string foto = string.Empty;

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = "SELECT Foto FROM Materiaal WHERE Id = @Id";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@Id", materiaalId);

                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    if (ObjRea.Read() && ObjRea["Foto"] != DBNull.Value)
                    {
                        foto = ObjRea["Foto"].ToString();
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return foto;
        }

        public int GetMateriaalMaatId(int materiaalId, int maatId)
        {
            int id = 0;

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = @"
                    SELECT Id
                    FROM MateriaalMaat
                    WHERE MateriaalId = @MateriaalId AND MaatId = @MaatId";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@MateriaalId", materiaalId);
                    ObjCmd.Parameters.AddWithValue("@MaatId", maatId);

                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    if (ObjRea.Read())
                    {
                        id = Convert.ToInt32(ObjRea["Id"]);
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return id;
        }

        public int GetMaximumAantal(int materiaalMaatId)
        {
            int aantal = 0;

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = "SELECT Aantal FROM MateriaalMaat WHERE Id = @Id";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@Id", materiaalMaatId);

                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    if (ObjRea.Read())
                    {
                        aantal = Convert.ToInt32(ObjRea["Aantal"]);
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return aantal;
        }

        public int GetVerhuurdAantal(int materiaalMaatId, DateTime begin, DateTime einde)
        {
            int aantal = 0;

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = @"
                    SELECT ISNULL(SUM(um.Aantal), 0) AS Aantal
                    FROM UitleningMateriaal um
                    INNER JOIN Uitlening u ON u.Id = um.UitleningId
                    WHERE um.MateriaalMaatId = @MateriaalMaatId
                      AND u.DatumUitlening <= @Einddatum
                      AND u.DatumInlevering >= @Begindatum";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@MateriaalMaatId", materiaalMaatId);
                    ObjCmd.Parameters.AddWithValue("@Begindatum", begin);
                    ObjCmd.Parameters.AddWithValue("@Einddatum", einde);

                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    if (ObjRea.Read())
                    {
                        aantal = Convert.ToInt32(ObjRea["Aantal"]);
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return aantal;
        }

        public int BerekenBeschikbaar(int materiaalMaatId, DateTime begin, DateTime einde)
        {
            return Math.Max(0, GetMaximumAantal(materiaalMaatId) - GetVerhuurdAantal(materiaalMaatId, begin, einde));
        }

        public void BewaarBestelling(string voornaam, string achternaam, string email, List<WinkelmandItem> winkelmand)
        {
            if (winkelmand == null || winkelmand.Count == 0)
            {
                throw new Exception("De winkelmand is leeg.");
            }

            int klantId = GetOrCreateKlant(voornaam, achternaam, email);

            foreach (WinkelmandItem item in winkelmand)
            {
                int beschikbaar = BerekenBeschikbaar(item.MateriaalMaatId, item.Begindatum, item.Einddatum);

                if (item.Aantal > beschikbaar)
                {
                    throw new Exception(item.Omschrijving + " is niet meer voldoende beschikbaar.");
                }

                int uitleningId = VoegUitleningToe(klantId, item.Begindatum, item.Einddatum);
                VoegUitleningMateriaalToe(uitleningId, item.MateriaalMaatId, item.Aantal);
            }
        }

        private int GetOrCreateKlant(string voornaam, string achternaam, string email)
        {
            int klantId = 0;

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = "SELECT TOP 1 Id FROM Klant WHERE Email = @Email";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@Email", email);

                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    if (ObjRea.Read())
                    {
                        klantId = Convert.ToInt32(ObjRea["Id"]);
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            if (klantId != 0)
            {
                return klantId;
            }

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = @"
                    INSERT INTO Klant (Voornaam, Achternaam, Email)
                    OUTPUT INSERTED.Id
                    VALUES (@Voornaam, @Achternaam, @Email)";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@Voornaam", voornaam);
                    ObjCmd.Parameters.AddWithValue("@Achternaam", achternaam);
                    ObjCmd.Parameters.AddWithValue("@Email", email);

                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    if (ObjRea.Read())
                    {
                        klantId = Convert.ToInt32(ObjRea["Id"]);
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return klantId;
        }

        private int VoegUitleningToe(int klantId, DateTime begin, DateTime einde)
        {
            int uitleningId = 0;

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = @"
                    INSERT INTO Uitlening (KlantId, DatumUitlening, DatumInlevering)
                    OUTPUT INSERTED.Id
                    VALUES (@KlantId, @DatumUitlening, @DatumInlevering)";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@KlantId", klantId);
                    ObjCmd.Parameters.AddWithValue("@DatumUitlening", begin);
                    ObjCmd.Parameters.AddWithValue("@DatumInlevering", einde);

                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    if (ObjRea.Read())
                    {
                        uitleningId = Convert.ToInt32(ObjRea["Id"]);
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return uitleningId;
        }

        private void VoegUitleningMateriaalToe(int uitleningId, int materiaalMaatId, int aantal)
        {
            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = @"
                    INSERT INTO UitleningMateriaal (UitleningId, MateriaalMaatId, Aantal)
                    VALUES (@UitleningId, @MateriaalMaatId, @Aantal)";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@UitleningId", uitleningId);
                    ObjCmd.Parameters.AddWithValue("@MateriaalMaatId", materiaalMaatId);
                    ObjCmd.Parameters.AddWithValue("@Aantal", aantal);

                    ObjCn.Open();
                    ObjCmd.ExecuteNonQuery();
                    ObjCn.Close();
                }
            }
        }
    }
}
