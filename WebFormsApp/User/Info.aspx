<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Info.aspx.cs" Inherits="WebFormsApp.UserInfo" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1>User Info Page</h1>
    <p>The link below is relative to this page and it will not work if base href is set to "/", what is required for blazor custom element to work.</p>
    <p>Because the base is set to "/" the link will try to open /Settings.aspx resulting in 404</p>
    <p>Instead of using base href blazor web assembly source is modified to use global variable instead of base meta tag</p>
    <a href="Settings.aspx">User settings page</a>
</asp:Content>
