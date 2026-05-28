using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SkiVerhuur_Rune
{
    public partial class Aanmelden : System.Web.UI.Page
    {
      
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                pnlMelding.Visible = false;
            }
        }

        protected void btnAanmelden_Click(object sender, EventArgs e)
        {
            pnlMelding.Visible = false;
            string gebruikersnaam = txtGebruikersnaam.Text.Trim();
            string wachtwoord = txtWachtwoord.Text;
            if(string.IsNullOrEmpty(gebruikersnaam) || string.IsNullOrEmpty(wachtwoord))
            {
                lblMelding.Text = "Vul zowel gebruikersnaam als wachtwoord in.";
                pnlMelding.Visible = true;
                return;
            }
            string connectionString = ConfigurationManager.ConnectionStrings["Skiverhuur"].ConnectionString;
            
            string sql = @"
                SELECT COUNT(*)
                FROM Gebruiker
                WHERE Gebruikersnaam = @Gebruikersnaam
                  AND Wachtwoord = @Wachtwoord";

            using (SqlConnection ObjCn = new SqlConnection(connectionString))
            using (SqlCommand ObjCmd = new SqlCommand(sql, ObjCn))
            {
                ObjCmd.Parameters.AddWithValue("@Gebruikersnaam", gebruikersnaam);
                ObjCmd.Parameters.AddWithValue("@Wachtwoord", wachtwoord);

                ObjCn.Open();
               SqlDataReader objRea = ObjCmd.ExecuteReader();
                
                if (objRea.Read())
                {
                    Session["Aangemeld"] = true;   
                    Session["Gebruikersnaam"] = gebruikersnaam;
                    Response.Redirect("DefaultAangemeld.aspx");
                }
                else
                {
                    lblMelding.Text = "Ongeldige gebruikersnaam of wachtwoord.";
                    pnlMelding.Visible = true;
                }
            }
        }
    }
}