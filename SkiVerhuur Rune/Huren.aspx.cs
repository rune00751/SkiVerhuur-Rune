using System;
using System.CodeDom;
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
using SvLib.Managers;

namespace SkiVerhuur_Rune
{
    public partial class Huren : System.Web.UI.Page
    {
        private HurenManager hurenmanager = new HurenManager();

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

                int materiaalMaatId = hurenmanager.GetMateriaalMaatId(Convert.ToInt32(ddlTypeMateriaal.SelectedValue), Convert.ToInt32(ddlMerk.SelectedValue));
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

                hurenmanager.BewaarBestelling(txtVoornaam.Text.Trim(), txtAchternaam.Text.Trim(), txtEmail.Text.Trim(), Winkelmand);

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
            int typeSportId = hurenmanager.GetTypeSportId(GetSportNaam());

            if (typeSportId == 0)
            {
                ddlTypeMateriaal.Items.Clear();
                ToonBoodschap("Het gekozen sporttype werd niet gevonden in de databank.", "warning");
                return;
            }

            List<TypeMateriaal> typeMaterialen = hurenmanager.GetTypeMaterialen(typeSportId);

            BindDropDown(ddlTypeMateriaal, typeMaterialen, "Naam", "Id");
        }

        private void BindMerken()
        {
            if (string.IsNullOrEmpty(ddlTypeMateriaal.SelectedValue))
            {
                ddlMerk.Items.Clear();
                return;
            }

            List<Merk> merken = hurenmanager.GetMerkenVoorTypeMateriaal(Convert.ToInt32(ddlTypeMateriaal.SelectedValue));

            BindDropDown(ddlMerk, merken, "Naam", "Id");
        }

        private void BindMaterialen()
        {
            if (string.IsNullOrEmpty(ddlTypeMateriaal.SelectedValue) || string.IsNullOrEmpty(ddlMerk.SelectedValue))
            {
                ddlMateriaal.Items.Clear();
                return;
            }

            List<Materiaal> materialen = hurenmanager.GetMaterialen(Convert.ToInt32(ddlTypeMateriaal.SelectedValue), Convert.ToInt32(ddlMerk.SelectedValue));

            BindDropDown(ddlMateriaal, materialen, "Model", "Id");
        }

        private void BindMaten()
        {
            if (string.IsNullOrEmpty(ddlMateriaal.SelectedValue))
            {
                ddlMaat.Items.Clear();
                return;
            }

            List<Maat> maten = hurenmanager.GetMatenVoorMateriaal(Convert.ToInt32(ddlMateriaal.SelectedValue));

            BindDropDown(ddlMaat, maten, "Naam", "Id");
        }

        private void ToonMateriaalFoto()
        {



            if (string.IsNullOrEmpty(ddlMateriaal.SelectedValue)) return;

            string foto = hurenmanager.GetFotoVoorMateriaal(Convert.ToInt32(ddlMateriaal.SelectedValue));
            if (!string.IsNullOrWhiteSpace(foto))
            {
                imgMateriaal.ImageUrl = "~/" + foto;
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

            int maxMateriaal = hurenmanager.GetMaximumAantal(materiaalMaatId);
            int verhuurdMateriaal = hurenmanager.GetVerhuurdAantal(materiaalMaatId, begin, einde);

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

            return hurenmanager.GetMateriaalMaatId(Convert.ToInt32(ddlMateriaal.SelectedValue), Convert.ToInt32(ddlMaat.SelectedValue));
        }

        private int GetTypeSportId(string sportNaam)
        {
            return hurenmanager.GetTypeSportId(sportNaam);
        }

        private string GetSportNaam()
        {
            string sport = Request.QueryString["sport"] ?? Request.QueryString["TypeSport"];
            if (!string.IsNullOrWhiteSpace(sport) && sport.ToLower().Contains("lang")) return "Langlaufen";
            return "Alpine";
        }

        private void BindDropDown(DropDownList ddl, object gegevens, string textField, string valueField)
        {
            ddl.DataSource = gegevens;
            ddl.DataTextField = textField;
            ddl.DataValueField = valueField;
            ddl.DataBind();
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


       
        }


    }
