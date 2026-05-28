using SvLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Runtime.Remoting;

namespace SvLib.Managers
{
    public class MerkManager
    {
        private string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["Skiverhuur"].ConnectionString; }
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
                        merken.Add(MaakMerk(ObjRea));
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return merken;
        }

        public Merk GetMerk(int id)
        {
            Merk merk = null;

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = "SELECT Id, Naam FROM Merk WHERE Id = @Id";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@Id", id);

                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    if (ObjRea.Read())
                    {
                        merk = MaakMerk(ObjRea);
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return merk;
        }

        public int BewaarMerk(Merk merk)
        {
            if (merk == null) throw new ArgumentNullException("merk");
            if (string.IsNullOrWhiteSpace(merk.Naam)) throw new Exception("Naam is verplicht.");

            if (merk.Id == 0)
            {
                int nieuwId = 0;

                using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
                {
                    string sql = @"
                        INSERT INTO Merk (Naam)
                        OUTPUT INSERTED.Id
                        VALUES (@Naam)";

                    using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                    {
                        ObjCmd.Parameters.AddWithValue("@Naam", merk.Naam.Trim());

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
                    string sql = "UPDATE Merk SET Naam = @Naam WHERE Id = @Id";

                    using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                    {
                        ObjCmd.Parameters.AddWithValue("@Naam", merk.Naam.Trim());
                        ObjCmd.Parameters.AddWithValue("@Id", merk.Id);

                        ObjCn.Open();
                        ObjCmd.ExecuteNonQuery();
                        ObjCn.Close();
                    }
                }

                return merk.Id;
            }
        }

        public void VerwijderMerk(int id)
        {
            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = "DELETE FROM Merk WHERE Id = @Id";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@Id", id);

                    ObjCn.Open();
                    ObjCmd.ExecuteNonQuery();
                    ObjCn.Close();
                }
            }
        }

        private Merk MaakMerk(SqlDataReader ObjRea)
        {
            return new Merk
            {
                Id = Convert.ToInt32(ObjRea["Id"]),
                Naam = ObjRea["Naam"].ToString()
            };
        }
    }
}
