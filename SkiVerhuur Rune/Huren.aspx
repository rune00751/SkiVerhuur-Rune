<%@ Page Title="" Language="C#" MasterPageFile="~/Public.master" AutoEventWireup="true" CodeBehind="Huren.aspx.cs" Inherits="SkiVerhuur_Rune.Huren" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">
     <div class="container-fluid py-3 huren-page">
     <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert al" role="alert">
         <asp:Literal ID="litMessage" runat="server" />
     </asp:Panel>

     <h2 class="text-center mb-3"><asp:Literal ID="litTitel" runat="server" /></h2>

     <div class="row g-3">
         <div class="col-12 col-md-8">
             <div class="mb-2">
                 <label for="txtBegindatum" class="form-label">Begindatum huren</label>
                 <asp:TextBox ID="txtBegindatum" runat="server" CssClass="form-control" TextMode="Date" AutoPostBack="true" OnTextChanged="Datum_TextChanged" />
             </div>

             <div class="mb-2">
                 <label for="txtEinddatum" class="form-label">Einddatum huren</label>
                 <asp:TextBox ID="txtEinddatum" runat="server" CssClass="form-control" TextMode="Date" AutoPostBack="true" OnTextChanged="Datum_TextChanged" />
             </div>

             <div class="mb-2">
                 <label for="ddlTypeMateriaal" class="form-label">Type materiaal</label>
                 <asp:DropDownList ID="ddlTypeMateriaal" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlTypeMateriaal_SelectedIndexChanged" />
             </div>

             <div class="mb-2">
                 <label for="ddlMerk" class="form-label">Merk</label>
                 <asp:DropDownList ID="ddlMerk" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlMerk_SelectedIndexChanged" />
             </div>

             <div class="mb-2">
                 <label for="ddlMateriaal" class="form-label">Materiaal</label>
                 <asp:DropDownList ID="ddlMateriaal" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlMateriaal_SelectedIndexChanged" />
             </div>

             <div class="mb-2">
                 <label for="ddlMaat" class="form-label">Maten</label>
                 <asp:DropDownList ID="ddlMaat" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlMaat_SelectedIndexChanged" />
             </div>

             <div class="mb-2">
                 <label for="txtBeschikbaar" class="form-label">Nog beschikbaar</label>
                 <asp:TextBox ID="txtBeschikbaar" runat="server" CssClass="form-control" ReadOnly="true" />
             </div>

             <div class="mb-3">
                 <label for="txtAantal" class="form-label">Aantal huren</label>
                 <asp:TextBox ID="txtAantal" runat="server" CssClass="form-control" TextMode="Number" Text="1" />
             </div>
         </div>

         <div class="col-12 col-md-4 text-center">
             <asp:Image ID="imgMateriaal" runat="server" CssClass="img-fluid bg-white p-3 product-image" AlternateText="Materiaal" />
         </div>
     </div>

     <div class="row g-2 mt-2">
         <div class="col-12 col-md-4">
             <asp:Button ID="btnToevoegen" runat="server" Text="Toevoegen aan winkelmand" CssClass="btn btn-primary w-100" OnClick="btnToevoegen_Click" />
         </div>
         <div class="col-12 col-md-4">
             <asp:Button ID="btnToonWinkelmand" runat="server" Text="Toon winkelmand" CssClass="btn btn-primary w-100" OnClick="btnToonWinkelmand_Click" />
         </div>
         <div class="col-12 col-md-4">
             <asp:Button ID="btnHuurBevestigen" runat="server" Text="Huur bevestigen" CssClass="btn btn-primary w-100" OnClick="btnHuurBevestigen_Click" />
         </div>
     </div>
 </div>

 <div class="modal fade" id="winkelmandModal" tabindex="-1" aria-labelledby="winkelmandModalLabel" aria-hidden="true">
     <div class="modal-dialog modal-xl modal-dialog-centered">
         <div class="modal-content">
             <div class="modal-header">
                 <h5 class="modal-title" id="winkelmandModalLabel">Winkelmand</h5>
                 <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Sluiten"></button>
             </div>
             <div class="modal-body">
                 <asp:GridView ID="gvWinkelmand" runat="server" CssClass="table table-sm table-striped" AutoGenerateColumns="false" EmptyDataText="De winkelmand is leeg.">
                     <Columns>
                         <asp:BoundField DataField="Omschrijving" HeaderText="Materiaal" />
                         <asp:BoundField DataField="Aantal" HeaderText="Aantal" />
                         <asp:BoundField DataField="Periode" HeaderText="Periode" />
                     </Columns>
                 </asp:GridView>
             </div>
             <div class="modal-footer">
                 <button type="button" class="btn btn-primary w-100" data-bs-dismiss="modal">Sluiten</button>
             </div>
         </div>
     </div>
 </div>

 <div class="modal fade" id="bevestigModal" tabindex="-1" aria-labelledby="bevestigModalLabel" aria-hidden="true">
     <div class="modal-dialog modal-xl modal-dialog-centered">
         <div class="modal-content">
             <div class="modal-header">
                 <h5 class="modal-title" id="bevestigModalLabel">Bevestig huur</h5>
                 <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Sluiten"></button>
             </div>
             <div class="modal-body">
                 <asp:GridView ID="gvBevestigen" runat="server" CssClass="table table-sm table-striped" AutoGenerateColumns="false" EmptyDataText="De winkelmand is leeg.">
                     <Columns>
                         <asp:BoundField DataField="Omschrijving" HeaderText="Materiaal" />
                         <asp:BoundField DataField="Aantal" HeaderText="Aantal" />
                         <asp:BoundField DataField="Periode" HeaderText="Periode" />
                     </Columns>
                 </asp:GridView>

                 <div class="mb-2">
                     <label for="txtVoornaam" class="form-label">Voornaam</label>
                     <asp:TextBox ID="txtVoornaam" runat="server" CssClass="form-control" />
                 </div>
                 <div class="mb-2">
                     <label for="txtAchternaam" class="form-label">Achternaam</label>
                     <asp:TextBox ID="txtAchternaam" runat="server" CssClass="form-control" />
                 </div>
                 <div class="mb-2">
                     <label for="txtEmail" class="form-label">E-mail</label>
                     <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" />
                 </div>
             </div>
             <div class="modal-footer">
                 <asp:Button ID="btnBestellingPlaatsen" runat="server" Text="Bestelling plaatsen" CssClass="btn btn-primary w-100" OnClick="btnBestellingPlaatsen_Click" />
             </div>
         </div>
     </div>
 </div>
</asp:Content>
