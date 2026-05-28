using System;
using System.Configuration;
using System.Data.SqlClient;
using SvLib.DataObjects;

namespace SvLib.Managers
{
    public class GebruikerManager
    {
        private string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["Skiverhuur"].ConnectionString; }
        }

        public bool ControleerLogin(string gebruikersnaam, string wachtwoord)
        {
            bool loginCorrect = false;

            using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
            {
                string sql = @"
                    SELECT Id
                    FROM Gebruiker
                    WHERE Gebruikersnaam = @Gebruikersnaam
                      AND Wachtwoord = @Wachtwoord";

                using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
                {
                    ObjCmd.Parameters.AddWithValue("@Gebruikersnaam", gebruikersnaam);
                    ObjCmd.Parameters.AddWithValue("@Wachtwoord", wachtwoord);

                    ObjCn.Open();
                    SqlDataReader ObjRea = ObjCmd.ExecuteReader();

                    if (ObjRea.Read())
                    {
                        loginCorrect = true;
                    }

                    ObjRea.Close();
                    ObjCn.Close();
                }
            }

            return loginCorrect;
        }
    }
}
