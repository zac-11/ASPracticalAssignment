<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HomePage.aspx.cs" Inherits="PracticalAssignment.HomePage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <fieldset>
            <legend>HomePage</legend>
                <br />
                <asp:Label ID="lblWelcomeMessage" runat="server" EnableViewState="false" />
                <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />
                <br />
                <br />
                <asp:Button ID="btnLogout" runat="server" Text="Logout" OnClick="LogoutMe" Visible="false" />
                <asp:Button ID="btnChangePassword" runat="server" Text="Change Password" OnClick="ChangePassword" Visible="false" />
                <p/>
            </fieldset>
        </div>
    </form>
</body>
</html>
