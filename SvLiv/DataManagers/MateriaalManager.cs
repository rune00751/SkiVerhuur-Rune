using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using SvLib.DataObjects;

namespace SvLib.Managers
{
    public class MateriaalManager
    {
        private string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["Skiverhuur"].ConnectionString; }
        }

        public List<TypeMateriaal> GetAlleTypeMaterialen()
        {
            List<TypeMateriaal> typeMaterialen = new List<TypeMateriaal>();

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = "SELECT Id, Naam, TypeSportId FROM TypeMateriaal ORDER BY Naam";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    while (ObjRea.Read())
                    {
                        typeMaterialen.Add(new TypeMateriaal
                        {
                            Id = Convert.ToInt32(ObjRea["Id"]),
                            Naam = ObjRea["Naam"].ToString(),
                            TypeSportId = Convert.ToInt32(ObjRea["TypeSportId"])
                        });
                    }

                    ObjRea  .Close();
                    ObjCn.Close();
                }
            }

            return typeMaterialen;
        }

        public List<Merk> GetAlleMerken()
        {
            List<Merk> merken = new List<Merk>();

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = "SELECT Id, Naam FROM Merk ORDER BY Naam";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
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
                        materialen.Add(MaakMateriaal(ObjRea, false));
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return materialen;
        }

        public Materiaal GetMateriaal(int id)
        {
            Materiaal materiaal = null;

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = @"
                    SELECT mat.Id, mat.Model, mat.MerkId, mat.TypeMateriaalId, mat.Foto,
                           merk.Naam AS MerkNaam, tm.Naam AS TypeMateriaalNaam
                    FROM Materiaal mat
                    INNER JOIN Merk merk ON merk.Id = mat.MerkId
                    INNER JOIN TypeMateriaal tm ON tm.Id = mat.TypeMateriaalId
                    WHERE mat.Id = @Id";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@Id", id);

                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    if (ObjRea.Read())
                    {
                        materiaal = MaakMateriaal(ObjRea, true);
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return materiaal;
        }

        public int BewaarMateriaal(Materiaal materiaal)
        {
            if (materiaal == null) throw new ArgumentNullException("materiaal");
            if (string.IsNullOrWhiteSpace(materiaal.Model)) throw new Exception("Model is verplicht.");
            if (materiaal.MerkId <= 0) throw new Exception("Merk is verplicht.");
            if (materiaal.TypeMateriaalId <= 0) throw new Exception("Materiaaltype is verplicht.");

            if (materiaal.Id == 0)
            {
                int nieuwId = 0;

                using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
                {
                    string sql = @"
                        INSERT INTO Materiaal (Model, MerkId, TypeMateriaalId, Foto)
                        OUTPUT INSERTED.Id
                        VALUES (@Model, @MerkId, @TypeMateriaalId, @Foto)";

                    using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                    {
                        ObjCmd.Parameters.AddWithValue("@Model", materiaal.Model.Trim());
                        ObjCmd.Parameters.AddWithValue("@MerkId", materiaal.MerkId);
                        ObjCmd.Parameters.AddWithValue("@TypeMateriaalId", materiaal.TypeMateriaalId);
                        ObjCmd.Parameters.AddWithValue("@Foto", string.IsNullOrWhiteSpace(materiaal.Foto) ? (object)DBNull.Value : materiaal.Foto);

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
                    string sql = @"
                        UPDATE Materiaal
                        SET Model = @Model, MerkId = @MerkId, TypeMateriaalId = @TypeMateriaalId, Foto = @Foto
                        WHERE Id = @Id";

                    using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                    {
                        ObjCmd.Parameters.AddWithValue("@Model", materiaal.Model.Trim());
                        ObjCmd.Parameters.AddWithValue("@MerkId", materiaal.MerkId);
                        ObjCmd.Parameters.AddWithValue("@TypeMateriaalId", materiaal.TypeMateriaalId);
                        ObjCmd.Parameters.AddWithValue("@Foto", string.IsNullOrWhiteSpace(materiaal.Foto) ? (object)DBNull.Value : materiaal.Foto);
                        ObjCmd.Parameters.AddWithValue("@Id", materiaal.Id);

                        ObjCn.Open();
                        ObjCmd.ExecuteNonQuery();
                        ObjCn.Close();
                    }
                }

                return materiaal.Id;
            }
        }

        public void UpdateFoto(int materiaalId, string foto)
        {
            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = "UPDATE Materiaal SET Foto = @Foto WHERE Id = @Id";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@Foto", string.IsNullOrWhiteSpace(foto) ? (object)DBNull.Value : foto);
                    ObjCmd.Parameters.AddWithValue("@Id", materiaalId);

                    ObjCn.Open();
                    ObjCmd.ExecuteNonQuery();
                    ObjCn.Close();
                }
            }
        }

        public void VerwijderMateriaal(int id)
        {
            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = "DELETE FROM Materiaal WHERE Id = @Id";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@Id", id);

                    ObjCn.Open();
                    ObjCmd.ExecuteNonQuery();
                    ObjCn.Close();
                }
            }
        }

        private Materiaal MaakMateriaal(SqlDataReader ObjRea, bool metExtraNamen)
        {
            Materiaal materiaal = new Materiaal
            {
                Id = Convert.ToInt32(ObjRea["Id"]),
                Model = ObjRea["Model"].ToString(),
                MerkId = Convert.ToInt32(ObjRea["MerkId"]),
                TypeMateriaalId = Convert.ToInt32(ObjRea["TypeMateriaalId"]),
                Foto = ObjRea["Foto"] == DBNull.Value ? string.Empty : ObjRea["Foto"].ToString()
            };

            if (metExtraNamen)
            {
                materiaal.MerkNaam = ObjRea["MerkNaam"].ToString();
                materiaal.TypeMateriaalNaam = ObjRea["TypeMateriaalNaam"].ToString();
            }

            return materiaal;
        }
    }
}
