<%@ Page Title="" Language="C#" MasterPageFile="~/Public.master" AutoEventWireup="true" CodeBehind="Aanmelden.aspx.cs" Inherits="SkiVerhuur_Rune.Aanmelden" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">
      <div class="container my-4">
     <h1 class="h2 border-bottom pb-2">Aanmelden</h1>

     <asp:Panel ID="pnlMelding" runat="server" Visible="false" CssClass="alert alert-warning">
         <asp:Label ID="lblMelding" runat="server"></asp:Label>
     </asp:Panel>

     <div class="mb-3">
         <label for="txtGebruikersnaam" class="form-label">Gebruikersnaam</label>
         <asp:TextBox ID="txtGebruikersnaam" runat="server" CssClass="form-control"></asp:TextBox>
     </div>

     <div class="mb-3">
         <label for="txtWachtwoord" class="form-label">Wachtwoord</label>
         <asp:TextBox ID="txtWachtwoord" runat="server" TextMode="Password" CssClass="form-control"></asp:TextBox>
     </div>

     <asp:Button ID="btnAanmelden" runat="server" Text="Aanmelden" CssClass="btn btn-primary w-100" OnClick="btnAanmelden_Click" />
 </div>
</asp:Content>

