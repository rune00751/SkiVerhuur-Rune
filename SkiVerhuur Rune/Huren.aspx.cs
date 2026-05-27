using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SvLib.DataObjects;

namespace SkiVerhuur_Rune
{
    public partial class Huren : System.Web.UI.Page
    {
        private string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["Skiverhuur"].ConnectionString; }
        }

        private List<WinkelmandItem> Winkelmand
        {
            get
            {
                if (Session["Winkelmand"] == null)
                {
                    Session["Winkelmand"] = new List<WinkelmandItem>();
                }
                return (List<WinkelmandItem>)Session["Winkelmand"];
            }
            set { Session["Winkelmand"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DateTime vandaag = DateTime.Today;
                txtBegindatum.Text = vandaag.ToString("yyyy-MM-dd");
                txtEinddatum.Text = vandaag.AddDays(1).ToString("yyyy-MM-dd");

                string sportNaam = GetSportNaam();
                litTitel.Text = sportNaam.Equals("Langlaufen", StringComparison.OrdinalIgnoreCase)
                    ? "Langlaufski's huren"
                    : "Alpineski's huren";

                BindTypeMateriaal();
                BindMerken();
                BindMaterialen();
                BindMaten();
                ToonMateriaalFoto();
                BerekenBeschikbaar();
            }
        }

        protected void ddlTypeMateriaal_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindMerken();
            BindMaterialen();
            BindMaten();
            ToonMateriaalFoto();
            BerekenBeschikbaar();
        }

        protected void ddlMerk_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindMaterialen();
            BindMaten();
            ToonMateriaalFoto();
            BerekenBeschikbaar();
        }

        protected void ddlMateriaal_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindMaten();
            ToonMateriaalFoto();
            BerekenBeschikbaar();
        }

        protected void ddlMaat_SelectedIndexChanged(object sender, EventArgs e)
        {
            BerekenBeschikbaar();
        }

        protected void Datum_TextChanged(object sender, EventArgs e)
        {
            ControleerDatums();
            BerekenBeschikbaar();
        }

        protected void btnToevoegen_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ControleerDatums()) return;

                int materiaalMaatId = GetMateriaalMaatId();
                if (materiaalMaatId == 0)
                {
                    ToonBoodschap("Kies eerst een materiaal en maat.", "warning");
                    return;
                }

                int beschikbaar = BerekenBeschikbaar();
                int aantal;
                if (!int.TryParse(txtAantal.Text, out aantal)) aantal = 1;

                if (aantal < 1)
                {
                    aantal = 1;
                    txtAantal.Text = "1";
                    ToonBoodschap("Het aantal moet minstens 1 zijn.", "warning");
                    return;
                }

                if (aantal > beschikbaar)
                {
                    txtAantal.Text = beschikbaar.ToString();
                    ToonBoodschap("Je kan niet meer huren dan het aantal dat nog beschikbaar is.", "warning");
                    return;
                }

                if (beschikbaar <= 0)
                {
                    ToonBoodschap("Dit materiaal is niet meer beschikbaar in de gekozen periode.", "warning");
                    return;
                }

                Winkelmand.Add(new WinkelmandItem
                {
                    MateriaalMaatId = materiaalMaatId,
                    MateriaalId = Convert.ToInt32(ddlMateriaal.SelectedValue),
                    MaatId = Convert.ToInt32(ddlMaat.SelectedValue),
                    TypeMateriaal = ddlTypeMateriaal.SelectedItem.Text,
                    Merk = ddlMerk.SelectedItem.Text,
                    Model = ddlMateriaal.SelectedItem.Text,
                    Maat = ddlMaat.SelectedItem.Text,
                    Aantal = aantal,
                    Begindatum = DateTime.Parse(txtBegindatum.Text),
                    Einddatum = DateTime.Parse(txtEinddatum.Text)
                });

                BerekenBeschikbaar();
                ToonBoodschap("Het materiaal werd toegevoegd aan de winkelmand.", "success");
            }
            catch (Exception ex)
            {
                ToonBoodschap("Er ging iets fout bij het toevoegen aan de winkelmand: " + ex.Message, "danger");
            }
        }

        protected void btnToonWinkelmand_Click(object sender, EventArgs e)
        {
            BindWinkelmand(gvWinkelmand);
            ToonModal("winkelmandModal");
        }

        protected void btnHuurBevestigen_Click(object sender, EventArgs e)
        {
            BindWinkelmand(gvBevestigen);
            ToonModal("bevestigModal");
        }

        protected void btnBestellingPlaatsen_Click(object sender, EventArgs e)
        {
            try
            {
                if (Winkelmand.Count == 0)
                {
                    ToonBoodschap("De winkelmand is leeg.", "warning");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtVoornaam.Text) ||
                    string.IsNullOrWhiteSpace(txtAchternaam.Text) ||
                    string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    ToonBoodschap("Vul voornaam, achternaam en e-mail in.", "warning");
                    BindWinkelmand(gvBevestigen);
                    ToonModal("bevestigModal");
                    return;
                }

                using (SqlConnection ObjCn = new SqlConnection(ConnectionString))
                {
                    ObjCn.Open();
                    SqlTransaction tr = ObjCn.BeginTransaction();

                    try
                    {
                        int klantId = GetOrCreateKlant(ObjCn, tr, txtVoornaam.Text.Trim(), txtAchternaam.Text.Trim(), txtEmail.Text.Trim());

                        foreach (WinkelmandItem item in Winkelmand)
                        {
                            int nogBeschikbaar = BerekenBeschikbaarVoorMateriaalMaat(ObjCn, tr, item.MateriaalMaatId, item.Begindatum, item.Einddatum);
                            if (item.Aantal > nogBeschikbaar)
                            {
                                throw new Exception(item.Omschrijving + " is niet meer voldoende beschikbaar.");
                            }

                            int uitleningId = VoegUitleningToe(ObjCn, tr, klantId, item.Begindatum, item.Einddatum);
                            VoegUitleningMateriaalToe(ObjCn, tr, uitleningId, item.MateriaalMaatId, item.Aantal);
                            VerstuurMail(txtEmail.Text);
                        }

                        tr.Commit();
                    }
                    catch
                    {
                        tr.Rollback();
                        throw;
                    }
                }

                Winkelmand = new List<WinkelmandItem>();
                ToonBoodschap("De bestelling werd geplaatst.", "success");
            }
            catch (Exception ex)
            {
                ToonBoodschap("Er ging iets fout bij het plaatsen van de bestelling: " + ex.Message, "danger");
            }
        }

        private void BindTypeMateriaal()
        {
            int typeSportId = GetTypeSportId(GetSportNaam());

            if (typeSportId == 0)
            {
                ddlTypeMateriaal.Items.Clear();
                ToonBoodschap("Het gekozen sporttype werd niet gevonden in de databank.", "warning");
                return;
            }

            DataTable dt = SelectDataTable(@"
                SELECT Id, Naam
                FROM TypeMateriaal
                WHERE TypeSportId = @TypeSportId
                ORDER BY Naam", new SqlParameter("@TypeSportId", typeSportId));

            BindDropDown(ddlTypeMateriaal, dt, "Naam", "Id");
        }

        private void BindMerken()
        {
            if (string.IsNullOrEmpty(ddlTypeMateriaal.SelectedValue))
            {
                ddlMerk.Items.Clear();
                return;
            }

            DataTable dt = SelectDataTable(@"
                SELECT DISTINCT m.Id, m.Naam
                FROM Merk m
                INNER JOIN Materiaal mat ON mat.MerkId = m.Id
                WHERE mat.TypeMateriaalId = @TypeMateriaalId
                ORDER BY m.Naam", new SqlParameter("@TypeMateriaalId", ddlTypeMateriaal.SelectedValue));

            BindDropDown(ddlMerk, dt, "Naam", "Id");
        }

        private void BindMaterialen()
        {
            if (string.IsNullOrEmpty(ddlTypeMateriaal.SelectedValue) || string.IsNullOrEmpty(ddlMerk.SelectedValue))
            {
                ddlMateriaal.Items.Clear();
                return;
            }

            DataTable dt = SelectDataTable(@"
                SELECT Id, Model
                FROM Materiaal
                WHERE TypeMateriaalId = @TypeMateriaalId AND MerkId = @MerkId
                ORDER BY Model",
                new SqlParameter("@TypeMateriaalId", ddlTypeMateriaal.SelectedValue),
                new SqlParameter("@MerkId", ddlMerk.SelectedValue));

            BindDropDown(ddlMateriaal, dt, "Model", "Id");
        }

        private void BindMaten()
        {
            if (string.IsNullOrEmpty(ddlMateriaal.SelectedValue))
            {
                ddlMaat.Items.Clear();
                return;
            }

            DataTable dt = SelectDataTable(@"
                SELECT ma.Id, ma.Naam
                FROM Maat ma
                INNER JOIN MateriaalMaat mm ON mm.MaatId = ma.Id
                WHERE mm.MateriaalId = @MateriaalId
                ORDER BY ma.Naam", new SqlParameter("@MateriaalId", ddlMateriaal.SelectedValue));

            BindDropDown(ddlMaat, dt, "Naam", "Id");
        }

        private void ToonMateriaalFoto()
        {



            if (string.IsNullOrEmpty(ddlMateriaal.SelectedValue)) return;

            object foto = ("SELECT Foto FROM Materiaal WHERE Id = @Id",
                new SqlParameter("@Id", ddlMateriaal.SelectedValue));
            if (foto != null && foto != DBNull.Value && !string.IsNullOrWhiteSpace(foto.ToString()))
            {
                imgMateriaal.ImageUrl = "~/images/products/" + foto;
            }
            else
            {
                imgMateriaal.ImageUrl = "~/images/no-image-available.png";

            }
        }

        private int BerekenBeschikbaar()
        {
            txtBeschikbaar.Text = "0";
            int materiaalMaatId = GetMateriaalMaatId();
            if (materiaalMaatId == 0) return 0;
            if (!ControleerDatums(false)) return 0;

            DateTime begin = DateTime.Parse(txtBegindatum.Text);
            DateTime einde = DateTime.Parse(txtEinddatum.Text);

            int maxMateriaal = Convert.ToInt32(ExecuteScalar(
                "SELECT Aantal FROM MateriaalMaat WHERE Id = @Id",
                new SqlParameter("@Id", materiaalMaatId)) ?? 0);

            int verhuurdMateriaal = Convert.ToInt32(ExecuteScalar(@"
                SELECT ISNULL(SUM(um.Aantal), 0)
                FROM UitleningMateriaal um
                INNER JOIN Uitlening u ON u.Id = um.UitleningId
                WHERE um.MateriaalMaatId = @MateriaalMaatId
                  AND u.DatumUitlening <= @Einddatum
                  AND u.DatumInlevering >= @Begindatum",
                new SqlParameter("@MateriaalMaatId", materiaalMaatId),
                new SqlParameter("@Begindatum", begin),
                new SqlParameter("@Einddatum", einde)) ?? 0);

            int gereserveerdInWinkelmand = 0;
            foreach (WinkelmandItem item in Winkelmand)
            {
                bool overlapt = item.MateriaalMaatId == materiaalMaatId && item.Begindatum <= einde && item.Einddatum >= begin;
                if (overlapt) gereserveerdInWinkelmand += item.Aantal;
            }

            int beschikbaar = Math.Max(0, maxMateriaal - verhuurdMateriaal - gereserveerdInWinkelmand);
            txtBeschikbaar.Text = beschikbaar.ToString();
            return beschikbaar;
        }

        private bool ControleerDatums(bool toonBoodschap = true)
        {
            DateTime begin;
            DateTime einde;

            if (!DateTime.TryParse(txtBegindatum.Text, out begin))
            {
                if (toonBoodschap) ToonBoodschap("Kies een geldige begindatum.", "warning");
                return false;
            }

            if (!DateTime.TryParse(txtEinddatum.Text, out einde))
            {
                if (toonBoodschap) ToonBoodschap("Kies een geldige einddatum.", "warning");
                return false;
            }

            if (begin < DateTime.Today)
            {
                txtBegindatum.Text = DateTime.Today.ToString("yyyy-MM-dd");
                begin = DateTime.Today;
                if (toonBoodschap) ToonBoodschap("De begindatum mag niet in het verleden liggen.", "warning");
            }

            if (begin > einde)
            {
                einde = begin.AddDays(1);
                txtEinddatum.Text = einde.ToString("yyyy-MM-dd");
                if (toonBoodschap) ToonBoodschap("De einddatum werd automatisch aangepast naar de begindatum + 1 dag.", "warning");
            }

            return true;
        }

        private int GetMateriaalMaatId()
        {
            if (string.IsNullOrEmpty(ddlMateriaal.SelectedValue) || string.IsNullOrEmpty(ddlMaat.SelectedValue)) return 0;

            object id = ExecuteScalar(@"
                SELECT Id
                FROM MateriaalMaat
                WHERE MateriaalId = @MateriaalId AND MaatId = @MaatId",
                new SqlParameter("@MateriaalId", ddlMateriaal.SelectedValue),
                new SqlParameter("@MaatId", ddlMaat.SelectedValue));

            return id == null || id == DBNull.Value ? 0 : Convert.ToInt32(id);
        }

        private int GetTypeSportId(string sportNaam)
        {

            string zoekterm = sportNaam.Equals("Langlaufen", StringComparison.OrdinalIgnoreCase) ? "Lang%" : "Alpine";

            object id = ExecuteScalar(
                "SELECT TOP 1 Id FROM TypeSport WHERE Naam LIKE @Naam ORDER BY Naam",
                new SqlParameter("@Naam", zoekterm));

            return id == null || id == DBNull.Value ? 0 : Convert.ToInt32(id);
        }

        private string GetSportNaam()
        {
            string sport = Request.QueryString["sport"] ?? Request.QueryString["TypeSport"];
            if (!string.IsNullOrWhiteSpace(sport) && sport.ToLower().Contains("lang")) return "Langlaufen";
            return "Alpine";
        }

        private void BindDropDown(DropDownList ddl, DataTable dt, string textField, string valueField)
        {
            ddl.DataSource = dt;
            ddl.DataTextField = textField;
            ddl.DataValueField = valueField;
            ddl.DataBind();
        }

        private DataTable SelectDataTable(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection cn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddRange(parameters);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        private object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection cn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddRange(parameters);
                cn.Open();
                return cmd.ExecuteScalar();
            }
        }

        private void BindWinkelmand(GridView grid)
        {
            grid.DataSource = Winkelmand;
            grid.DataBind();
        }

        private void ToonModal(string modalId)
        {
            ClientScript.RegisterStartupScript(GetType(), "toon" + modalId,
                "var myModal = new bootstrap.Modal(document.getElementById('" + modalId + "')); myModal.show();", true);
        }

        private void ToonBoodschap(string boodschap, string type)
        {
            pnlMessage.Visible = true;
            pnlMessage.CssClass = "alert alert-" + type;
            litMessage.Text = Server.HtmlEncode(boodschap);
        }


        private int BerekenBeschikbaarVoorMateriaalMaat(SqlConnection cn, SqlTransaction tr, int materiaalMaatId, DateTime begin, DateTime einde)
        {
            int maxMateriaal;

            using (SqlCommand cmd = new SqlCommand("SELECT Aantal FROM MateriaalMaat WHERE Id = @Id", cn, tr))
            {
                cmd.Parameters.AddWithValue("@Id", materiaalMaatId);
                object result = cmd.ExecuteScalar();
                maxMateriaal = result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }

            int verhuurdMateriaal;
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT ISNULL(SUM(um.Aantal), 0)
                FROM UitleningMateriaal um
                INNER JOIN Uitlening u ON u.Id = um.UitleningId
                WHERE um.MateriaalMaatId = @MateriaalMaatId
                  AND u.DatumUitlening <= @Einddatum
                  AND u.DatumInlevering >= @Begindatum", cn, tr))
            {
                cmd.Parameters.AddWithValue("@MateriaalMaatId", materiaalMaatId);
                cmd.Parameters.AddWithValue("@Begindatum", begin);
                cmd.Parameters.AddWithValue("@Einddatum", einde);
                verhuurdMateriaal = Convert.ToInt32(cmd.ExecuteScalar());
            }

            return Math.Max(0, maxMateriaal - verhuurdMateriaal);
        }

        private int GetOrCreateKlant(SqlConnection cn, SqlTransaction tr, string voornaam, string achternaam, string email)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 Id FROM Klant WHERE Email = @Email", cn, tr))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value) return Convert.ToInt32(result);
            }

            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Klant (Voornaam, Achternaam, Email)
                VALUES (@Voornaam, @Achternaam, @Email);
                SELECT SCOPE_IDENTITY();", cn, tr))
            {
                cmd.Parameters.AddWithValue("@Voornaam", voornaam);
                cmd.Parameters.AddWithValue("@Achternaam", achternaam);
                cmd.Parameters.AddWithValue("@Email", email);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private int VoegUitleningToe(SqlConnection cn, SqlTransaction tr, int klantId, DateTime begin, DateTime einde)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Uitlening (KlantId, DatumUitlening, DatumInlevering)
                VALUES (@KlantId, @DatumUitlening, @DatumInlevering);
                SELECT SCOPE_IDENTITY();", cn, tr))
            {
                cmd.Parameters.AddWithValue("@KlantId", klantId);
                cmd.Parameters.AddWithValue("@DatumUitlening", begin);
                cmd.Parameters.AddWithValue("@DatumInlevering", einde);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void VoegUitleningMateriaalToe(SqlConnection cn, SqlTransaction tr, int uitleningId, int materiaalMaatId, int aantal)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO UitleningMateriaal (UitleningId, MateriaalMaatId, Aantal)
                VALUES (@UitleningId, @MateriaalMaatId, @Aantal)", cn, tr))
            {
                cmd.Parameters.AddWithValue("@UitleningId", uitleningId);
                cmd.Parameters.AddWithValue("@MateriaalMaatId", materiaalMaatId);
                cmd.Parameters.AddWithValue("@Aantal", aantal);
                cmd.ExecuteNonQuery();
            }
        }
        private void VerstuurMail(string naar)
        {
            try
            {
                string inhoud = "Uw bestelling bij Ski Verhuur is bevestigd.\n\n";

                List<WinkelmandItem> winkelmand =
                    (List<WinkelmandItem>)Session["winkelmand"];

                foreach (WinkelmandItem item in winkelmand)
                {
                    inhoud += "Artikel: " + item.TypeMateriaal + "\n";
                    inhoud += "Maat: " + item.Maat + "\n";
                    inhoud += "Aantal: " + item.Aantal + "\n";
                    inhoud += "Periode: " +
                               item.Begindatum.ToShortDateString() +
                               " tot " +
                               item.Einddatum.ToShortDateString() + "\n\n";
                }

                inhoud += "Bedankt voor uw bestelling bij Ski Verhuur.";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("runevanhofstraeten@gmail.com");
                mail.To.Add(naar);

                mail.Subject = "Bestelling skiverhuur bevestigd";
                mail.Body = inhoud;

                SmtpClient smtp = new SmtpClient();
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                litMessage.Text = ex.Message;
                pnlMessage.Visible = true;
            }
        }


    }
}