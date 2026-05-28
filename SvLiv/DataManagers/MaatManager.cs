using SvLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Runtime.Remoting;

namespace SvLib.Managers
{
    public class MaatManager
    {
        private string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["Skiverhuur"].ConnectionString; }
        }

        public List<Maat> GetAlleMaten()
        {
            List<Maat> maten = new List<Maat>();

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = "SELECT Id, Naam FROM Maat ORDER BY Naam";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    while (ObjRea.Read())
                    {
                        maten.Add(MaakMaat(ObjRea));
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return maten;
        }

        public Maat GetMaat(int id)
        {
            Maat maat = null;

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = "SELECT Id, Naam FROM Maat WHERE Id = @Id";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@Id", id);

                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    if (ObjRea.Read())
                    {
                        maat = MaakMaat(ObjRea);
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return maat;
        }

        public int BewaarMaat(Maat maat)
        {
            if (maat == null) throw new ArgumentNullException("maat");
            if (string.IsNullOrWhiteSpace(maat.Naam)) throw new Exception("Maat is verplicht.");

            if (maat.Id == 0)
            {
                int nieuwId = 0;

                using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
                {
                    string sql = @"
                        INSERT INTO Maat (Naam)
                        OUTPUT INSERTED.Id
                        VALUES (@Naam)";

                    using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                    {
                        ObjCmd.Parameters.AddWithValue("@Naam", maat.Naam.Trim());

                        ObjCn.Open();
                        SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                        if (ObjRea.Read())
                        {
                            nieuwId = Convert.ToInt32(ObjRea["Id"]);
                        }

                        ObjRea.Close();
                        ObjCn.Close();
                    }
                }

                return nieuwId;
            }
            else
            {
                using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
                {
                    string sql = "UPDATE Maat SET Naam = @Naam WHERE Id = @Id";

                    using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                    {
                        ObjCmd.Parameters.AddWithValue("@Naam", maat.Naam.Trim());
                        ObjCmd.Parameters.AddWithValue("@Id", maat.Id);

                        ObjCn.Open();
                        ObjCmd.ExecuteNonQuery();
                        ObjCn.Close();
                    }
                }

                return maat.Id;
            }
        }

        public void VerwijderMaat(int id)
        {
            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = "DELETE FROM Maat WHERE Id = @Id";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@Id", id);

                    ObjCn.Open();
                    ObjCmd.ExecuteNonQuery();
                    ObjCn.Close();
                }
            }
        }

        public List<MateriaalMaat> GetMatenVoorMateriaal(int materiaalId)
        {
            List<MateriaalMaat> materiaalMaten = new List<MateriaalMaat>();

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = @"
                    SELECT mm.Id AS MateriaalMaatId, mm.MateriaalId, mm.MaatId, m.Naam, mm.Aantal
                    FROM MateriaalMaat mm
                    INNER JOIN Maat m ON m.Id = mm.MaatId
                    WHERE mm.MateriaalId = @MateriaalId
                    ORDER BY m.Naam";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@MateriaalId", materiaalId);

                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    while (ObjRea.Read())
                    {
                        materiaalMaten.Add(new MateriaalMaat
                        {
                            Id = Convert.ToInt32(ObjRea["MateriaalMaatId"]),
                            MateriaalId = Convert.ToInt32(ObjRea["MateriaalId"]),
                            MaatId = Convert.ToInt32(ObjRea["MaatId"]),
                            Aantal = Convert.ToInt32(ObjRea["Aantal"]),
                            MaatNaam = ObjRea["Naam"].ToString()
                        });
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return materiaalMaten;
        }

        public MateriaalMaat GetMateriaalMaat(int materiaalId, int maatId)
        {
            MateriaalMaat materiaalMaat = null;

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = @"
                    SELECT TOP 1 mm.Id, mm.MateriaalId, mm.MaatId, mm.Aantal,
                           mat.Model AS MateriaalModel, m.Naam AS MaatNaam
                    FROM MateriaalMaat mm
                    INNER JOIN Materiaal mat ON mat.Id = mm.MateriaalId
                    INNER JOIN Maat m ON m.Id = mm.MaatId
                    WHERE mm.MateriaalId = @MateriaalId AND mm.MaatId = @MaatId";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@MateriaalId", materiaalId);
                    ObjCmd.Parameters.AddWithValue("@MaatId", maatId);

                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    if (ObjRea.Read())
                    {
                        materiaalMaat = MaakMateriaalMaat(ObjRea);
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return materiaalMaat;
        }

        public void VoegMaatToeAanMateriaal(int materiaalId, int maatId, int aantal)
        {
            if (aantal <= 0) throw new Exception("Aantal moet groter zijn dan 0.");
            if (GetMateriaalMaat(materiaalId, maatId) != null) throw new Exception("Deze maat is al gekoppeld aan dit materiaal.");

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = @"
                    INSERT INTO MateriaalMaat (MateriaalId, MaatId, Aantal)
                    VALUES (@MateriaalId, @MaatId, @Aantal)";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@MateriaalId", materiaalId);
                    ObjCmd.Parameters.AddWithValue("@MaatId", maatId);
                    ObjCmd.Parameters.AddWithValue("@Aantal", aantal);

                    ObjCn.Open();
                    ObjCmd.ExecuteNonQuery();
                    ObjCn.Close();
                }
            }
        }

        public void UpdateAantalVoorMateriaalMaat(int materiaalId, int maatId, int aantal)
        {
            if (aantal <= 0) throw new Exception("Aantal moet groter zijn dan 0.");

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = @"
                    UPDATE MateriaalMaat
                    SET Aantal = @Aantal
                    WHERE MateriaalId = @MateriaalId AND MaatId = @MaatId";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@Aantal", aantal);
                    ObjCmd.Parameters.AddWithValue("@MateriaalId", materiaalId);
                    ObjCmd.Parameters.AddWithValue("@MaatId", maatId);

                    ObjCn.Open();
                    ObjCmd.ExecuteNonQuery();
                    ObjCn.Close();
                }
            }
        }

        public void VerwijderMaatVanMateriaal(int materiaalId, int maatId)
        {
            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = @"
                    DELETE FROM MateriaalMaat
                    WHERE MateriaalId = @MateriaalId AND MaatId = @MaatId";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@MateriaalId", materiaalId);
                    ObjCmd.Parameters.AddWithValue("@MaatId", maatId);

                    ObjCn.Open();
                    ObjCmd.ExecuteNonQuery();
                    ObjCn.Close();
                }
            }
        }

        private Maat MaakMaat(SqlDataReader ObjRea)
        {
            return new Maat
            {
                Id = Convert.ToInt32(ObjRea["Id"]),
                Naam = ObjRea["Naam"].ToString()
            };
        }

        private MateriaalMaat MaakMateriaalMaat(SqlDataReader ObjRea)
        {
            return new MateriaalMaat
            {
                Id = Convert.ToInt32(ObjRea["Id"]),
                MateriaalId = Convert.ToInt32(ObjRea["MateriaalId"]),
                MaatId = Convert.ToInt32(ObjRea["MaatId"]),
                Aantal = Convert.ToInt32(ObjRea["Aantal"]),
                MateriaalModel = ObjRea["MateriaalModel"].ToString(),
                MaatNaam = ObjRea["MaatNaam"].ToString()
            };
        }
    }
}
