<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="PracticalAssignment.ChangePassword" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Registration</title>

    <script type="text/javascript">
        function validatePassword() {
            var newPassword = document.getElementById('<%=tb_newPassword.ClientID%>').value;
            var cfmNewPassword = document.getElementById('<%=tb_confirmNewPassword.ClientID%>').value;
            document.getElementById("btn_ChangePassword").disabled = true;

            if (newPassword.length < 12) {
                document.getElementById("lbl_passwordChecker").innerHTML = "Password length must be at least 12 characters.";
                document.getElementById("lbl_passwordChecker").style.color = "red";
                return ("too_short");
            }
            else if (newPassword.search(/[0-9]/) == -1) {
                document.getElementById("lbl_passwordChecker").innerHTML = "Password requires at least 1 number.";
                document.getElementById("lbl_passwordChecker").style.color = "red";
                return ("no_number");
            }
            else if (newPassword.search(/[a-z]/) == -1) {
                document.getElementById("lbl_passwordChecker").innerHTML = "Password requires at least a lowercase character.";
                document.getElementById("lbl_passwordChecker").style.color = "Red";
                return ("no_lowercase");

            }
            else if (newPassword.search(/[A-Z]/) == -1) {
                document.getElementById("lbl_passwordChecker").innerHTML = "Password requires at least a uppercase character.";
                document.getElementById("lbl_passwordChecker").style.color = "Red";
                return ("no_uppercase");
            }
            else if (newPassword.search(/[^A-Za-z0-9]/) == -1) {
                document.getElementById("lbl_passwordChecker").innerHTML = "Password requires at least a special character.";
                document.getElementById("lbl_passwordChecker").style.color = "Red";
                return ("no_specialchar");
            }

            document.getElementById("lbl_passwordChecker").innerHTML = "Excellent!";
            document.getElementById("lbl_passwordChecker").style.color = "green";

            if (cfmNewPassword != "") {
                if (newPassword != cfmNewPassword) {
                    document.getElementById("lbl_confirmPasswordChecker").innerHTML = "Confirm password do not match.";
                    document.getElementById("lbl_confirmPasswordChecker").style.color = "Red";
                }
            } else {
                document.getElementById("lbl_confirmPasswordChecker").innerHTML = "Confirm password is empty.";
                document.getElementById("lbl_confirmPasswordChecker").style.color = "Red";
            }
            if (newPassword == cfmNewPassword) {
                document.getElementById("lbl_confirmPasswordChecker").innerHTML = "Password matched.";
                document.getElementById("lbl_confirmPasswordChecker").style.color = "green";
                document.getElementById("btn_ChangePassword").disabled = false;
            }
        }
        
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>
                <br />
                <asp:Label ID="Label1" runat="server" Text="Change Account Password"></asp:Label>
                <br />
                <br />
            </h2>
            <asp:Label ID="lbl_message" runat="server" EnableViewState="false" Visible="false">Message</asp:Label>
            <table class="style1">
            <tr>
                <td class="style3">
                    <asp:Label ID="Label2" runat="server" Text="Current Password"></asp:Label>
                </td>
                <td class="style2">
                    <asp:TextBox ID="tb_currentPassword" runat="server" Height="32px" Width="281px" TextMode="Password"></asp:TextBox>
                    <asp:Label ID="lbl_currentPassword" runat="server" Text=""></asp:Label>
                </td>
            </tr>
            <tr>
                <td class="style3">
                    <asp:Label ID="Label3" runat="server" Text="Password"></asp:Label>
                </td>
                <td class="style2">
                    <asp:TextBox ID="tb_newPassword" runat="server" Height="32px" Width="281px" onKeyUp="javascript:validatePassword()" OnTextChanged="onPasswordTextChanged" TextMode="Password"></asp:TextBox>
                    <asp:Label ID="lbl_passwordChecker" runat="server" Text=""></asp:Label>
                    <asp:Label ID="lbl_passwordChecker2" runat="server" Text=""></asp:Label>
                </td>
            </tr>
            <tr>
                <td class="style3">
                    <asp:Label ID="Label4" runat="server" Text="Confirm Password"></asp:Label>
                </td>
                <td class="style2">
                    <asp:TextBox ID="tb_confirmNewPassword" runat="server" Height="32px" Width="281px" onKeyUp="javascript:validatePassword()" TextMode="Password"></asp:TextBox>
                    <asp:Label ID="lbl_confirmPasswordChecker" runat="server" Text=""></asp:Label>
                </td>
            </tr>
            <tr>
                <td class="style4">
                </td>
                <td class="style5">
                    <asp:Button ID="btn_ChangePassword" runat="server" Height="48px" Text="Submit" Width="288px" disabled="true" OnClick="btn_ChangePassword_Click"/>
                </td>
            </tr>
    </table>
        </div>
    </form>
</body>
</html>
