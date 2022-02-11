<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="PracticalAssignment.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login</title>
    <script src="https://www.google.com/recaptcha/api.js?render="></script>
    <!-- removed secret key -->

    <style type="text/css">
        .auto-style1 { width: 100px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>
                <br />
                <asp:Label ID="Label1" runat="server" Text="SITConnect Login"></asp:Label>
                <br />
            </h2>
            <asp:Label ID="lbl_errorMsg" runat="server" EnableViewState="false" Visible="false">Error message</asp:Label>
            <table class="style1">
            <tr>
                <td class="auto-style1">
                    <asp:Label ID="Label2" runat="server" Text="Email"></asp:Label>
                </td>
                <td class="style2">
                    <asp:TextBox ID="tb_email" runat="server" Height="16px" Width="280px"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="auto-style1">
                    <asp:Label ID="Label3" runat="server" Text="Password"></asp:Label>
                </td>
                <td class="style2">
                    <asp:TextBox ID="tb_password" runat="server" Height="16px" Width="281px" TextMode="Password"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="auto-style1">
                </td>
                <td class="style2">
                    <asp:Button ID="btn_Login" runat="server" Height="48px" onclick="btn_Login_Click" Text="Login" Width="288px" />
                </td>
            </tr>
                
            </table>
            <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response"/>
            <asp:Label ID="lbl_captchaMsg" runat="server" EnableViewState="false"></asp:Label>
        </div>
    </form>
    <script>
        grecaptcha.ready(function () {
            // removed secret key
            grecaptcha.execute("", { action: "Login" }).then(function (token) {
                document.getElementById("g-recaptcha-response").value = token;
            });
        });
    </script>
</body>
</html>
