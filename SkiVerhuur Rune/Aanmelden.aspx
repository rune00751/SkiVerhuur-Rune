<%@ Page Title="" Language="C#" MasterPageFile="~/Public.master" AutoEventWireup="true" CodeBehind="Aanmelden.aspx.cs" Inherits="SkiVerhuur_Rune.Aanmelden" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">
        <div class="Aanmelden">
    <h1>Aanmelden</h1>


   
  <div class="form-group toppadding">
      <label for="txtUsername">Gebruikersnaam</label>
      <asp:TextBox ID="txtGebruikersnaam" runat="server" class="form-control" placeholder="Je gebruikersnaam" required></asp:TextBox>
  </div>
  <div class="form-group">
      <label for="txtPassword">Wachtwoord:</label>
      <asp:TextBox ID="txtWachtwoord" runat="server" TextMode="Password" class="form-control" placeholder="Je wachtwoord" required></asp:TextBox>
  </div>
  <div class="form-group">
      <asp:Button ID="btnAanmelden" runat="server" Text="Aanmelden" class="btn btn-primary btn-block" OnClick="btnAanmelden_Click" />
  </div>
         </div>

</asp:Content>

