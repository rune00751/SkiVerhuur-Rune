<%@ Page Title="" Language="C#" MasterPageFile="~/Public.master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SkiVerhuur_Rune.Default" %>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">
     <div id="homeCarousel" class="carousel slide home-carousel" data-bs-ride="carousel">
        <div class="carousel-indicators">
            <button type="button" data-bs-target="#homeCarousel" data-bs-slide-to="0" class="active" aria-current="true" aria-label="Slide 1"></button>
            <button type="button" data-bs-target="#homeCarousel" data-bs-slide-to="1" aria-label="Slide 2"></button>
        </div>

        <div class="carousel-inner">
            <div class="carousel-item active">
                <img class="d-block w-100" src="images/xc.jpg"  />
                <div class="carousel-caption">
                    <h2>Langlaufen</h2>
                    <a class="btn btn-primary  carousel-cta" id="base" href="Huren.aspx">Klik hier om je langlaufmateriaal te huren.</a>
                </div>
            </div>
            <div class="carousel-item">
                <img class="d-block w-100" src="images/ski.jpg"  />
                <div class="carousel-caption">
                    <h2>Alpineskiën</h2>
                     <a class="btn btn-primary  carousel-cta" href="Huren.aspx">Klik hier om je Skimateriaal te huren.</a>
</div>
            </div>
        </div>

        <button class="carousel-control-prev" type="button" data-bs-target="#homeCarousel" data-bs-slide="prev">
            <span class="carousel-control-prev-icon" aria-hidden="true"></span>
            <span class="visually-hidden">Previous</span>
        </button>
        <button class="carousel-control-next" type="button" data-bs-target="#homeCarousel" data-bs-slide="next">
            <span class="carousel-control-next-icon" aria-hidden="true"></span>
            <span class="visually-hidden">Next</span>
        </button>
    </div>

</asp:Content>
