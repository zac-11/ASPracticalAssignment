<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Registration.aspx.cs" Inherits="PracticalAssignment.Registration" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Registration</title>

    <script type="text/javascript">
        function validatePassword() {
            var password = document.getElementById('<%=tb_password.ClientID%>').value;
            var cfmPassword = document.getElementById('<%=tb_confirmPassword.ClientID%>').value;
            document.getElementById("btn_Register").disabled = true;

            if (password.length < 12) {
                document.getElementById("lbl_passwordChecker").innerHTML = "Password length must be at least 12 characters";
                document.getElementById("lbl_passwordChecker").style.color = "red";
                return ("too_short");
            }
            else if (password.search(/[0-9]/) == -1) {
                document.getElementById("lbl_passwordChecker").innerHTML = "Password requires at least 1 number";
                document.getElementById("lbl_passwordChecker").style.color = "red";
                return ("no_number");
            }
            else if (password.search(/[a-z]/) == -1) {
                document.getElementById("lbl_passwordChecker").innerHTML = "Password requires at least a lowercase character";
                document.getElementById("lbl_passwordChecker").style.color = "Red";
                return ("no_lowercase");

            }
            else if (password.search(/[A-Z]/) == -1) {
                document.getElementById("lbl_passwordChecker").innerHTML = "Password requires at least a uppercase character";
                document.getElementById("lbl_passwordChecker").style.color = "Red";
                return ("no_uppercase");
            }
            else if (password.search(/[^A-Za-z0-9]/) == -1) {
                document.getElementById("lbl_passwordChecker").innerHTML = "Password requires at least a special character";
                document.getElementById("lbl_passwordChecker").style.color = "Red";
                return ("no_specialchar");
            }

            document.getElementById("lbl_passwordChecker").innerHTML = "Excellent!";
            document.getElementById("lbl_passwordChecker").style.color = "green";

            if (cfmPassword != "") {
                if (password != cfmPassword) {
                    document.getElementById("lbl_confirmPasswordChecker").innerHTML = "Confirm password do not match";
                    document.getElementById("lbl_confirmPasswordChecker").style.color = "Red";
                }
            } else {
                document.getElementById("lbl_confirmPasswordChecker").innerHTML = "Confirm password is empty";
                document.getElementById("lbl_confirmPasswordChecker").style.color = "Red";
            }
            if (password == cfmPassword) {
                document.getElementById("lbl_confirmPasswordChecker").innerHTML = "Password matched";
                document.getElementById("lbl_confirmPasswordChecker").style.color = "green";
                document.getElementById("btn_Register").disabled = false;
            }
        }
        
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>
                <br />
                <asp:Label ID="Label1" runat="server" Text="SITConnect Account Registration"></asp:Label>
                <br />
                <br />
            </h2>

            <table class="style1">
            <tr>
                <td class="style3">
                    <asp:Label ID="Label2" runat="server" Text="First Name"></asp:Label>
                </td>
                <td class="style2">
                    <asp:TextBox ID="tb_firstName" runat="server" Height="36px" Width="280px"></asp:TextBox>
                    <asp:Label ID="lbl_firstNameChecker" runat="server" Text=""></asp:Label>
                </td>
            </tr>
                <tr>
                <td class="style3">
                    <asp:Label ID="Label7" runat="server" Text="Last Name"></asp:Label>
                </td>
                <td class="style2">
                    <asp:TextBox ID="tb_lastName" runat="server" Height="36px" Width="280px"></asp:TextBox>
                    <asp:Label ID="lbl_lastNameChecker" runat="server" Text=""></asp:Label>
                </td>
            </tr>
            <tr>
                <td class="style3">
                    <asp:Label ID="Label8" runat="server" Text="Credit Card Info"></asp:Label>
                </td>
                <td class="style2">
                    <asp:TextBox ID="tb_creditCardInfo" runat="server" Height="36px" Width="280px" MaxLength="16"></asp:TextBox>
                    <asp:Label ID="lbl_creditCardChecker" runat="server" Text=""></asp:Label>
                </td>
            </tr>
            <tr>
                <td class="style6">
                    <asp:Label ID="Label5" runat="server" Text="Email"></asp:Label>
                </td>
                <td class="style7">
                    <asp:TextBox ID="tb_email" runat="server" Height="32px" Width="281px" TextMode="Email"></asp:TextBox>
                    <asp:Label ID="lbl_emailChecker" runat="server" Text=""></asp:Label>
                </td>
            </tr>
            <tr>
                <td class="style3">
                    <asp:Label ID="Label3" runat="server" Text="Password"></asp:Label>
                </td>
                <td class="style2">
                    <asp:TextBox ID="tb_password" runat="server" Height="32px" Width="281px" onKeyUp="javascript:validatePassword()" OnTextChanged="onPasswordTextChanged" TextMode="Password"></asp:TextBox>
                    <asp:Label ID="lbl_passwordChecker" runat="server" Text=""></asp:Label>
                    <asp:Label ID="lbl_passwordChecker2" runat="server" Text=""></asp:Label>
                </td>
            </tr>
            <tr>
                <td class="style3">
                    <asp:Label ID="Label4" runat="server" Text="Confirm Password"></asp:Label>
                </td>
                <td class="style2">
                    <asp:TextBox ID="tb_confirmPassword" runat="server" Height="32px" Width="281px" onKeyUp="javascript:validatePassword()" TextMode="Password"></asp:TextBox>
                    <asp:Label ID="lbl_confirmPasswordChecker" runat="server" Text=""></asp:Label>
                </td>
            </tr>
            <tr>
                <td class="style3">
                    <asp:Label ID="Label6" runat="server" Text="Date of Birth"></asp:Label>
                </td>
                <td class="style2">
                    <asp:TextBox ID="tb_dateOfBirth" runat="server" Height="32px" Width="281px" TextMode="Date"></asp:TextBox>
                    <asp:Label ID="lbl_dateOfBirthChecker" runat="server" Text=""></asp:Label>
                </td>
            </tr>
            <tr>
                <td class="style3">
                    <asp:Label ID="Label9" runat="server" Text="Photo"></asp:Label>
                </td>
                <td class="style2">
                    <asp:FileUpload ID="uploadPhoto" runat="server" accept=".jpg,.png"/>
                </td>
            </tr>
            <tr>
                <td class="style4">
                </td>
                <td class="style5">
                    <asp:Button ID="btn_Register" runat="server" Height="48px" onclick="btn_Register_Click" Text="Submit" Width="288px" disabled="true"/>
                </td>
            </tr>
    </table>
        </div>
    </form>
</body>
</html>
