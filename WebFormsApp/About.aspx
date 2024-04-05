<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="WebFormsApp.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">
        <h2 id="title"><%: Title %>.</h2>
        <h3>Your application description page.</h3>
        <p>Use this area to provide additional information.</p>
        <button type="button" onclick="showDialog()">Show Radzen Dialog</button>
    </main>
    <script>
        async function showDialog() {
            var result = await BlazorShared.showDialog("BlazorWebApp.Client.Components.SharedComponents.DataEntry", "From About");
            BlazorShared.showNotification(1, "Response from dialog", result);
        }
    </script>

</asp:Content>

