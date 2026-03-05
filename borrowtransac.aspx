<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="borrowtransac.aspx.cs" Inherits="prjLibrarySystem.Loans" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Borrowing Transaction - Library System</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css?v=2.0" rel="stylesheet">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css?v=2.0" rel="stylesheet">
    <style>
        .sidebar {
            min-height: 100vh;
            background: linear-gradient(135deg, #8b0000 0%, #b11226 100%);
        }
        .sidebar .nav-link {
            color: white;
            padding: 15px 20px;
            border-radius: 0;
        }
        .sidebar .nav-link:hover {
            background-color: rgba(255, 255, 255, 0.1);
            color: white;
        }
        .sidebar .nav-link.active {
            background: rgba(255, 255, 255, 0.2);
            border-left: 4px solid white;
        }
        .main-content {
            padding: 20px;
        }
        .action-buttons .btn {
            margin-right: 5px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container-fluid">
            <div class="row">
                <!-- Sidebar -->
                <nav class="col-md-3 col-lg-2 d-md-block sidebar collapse">
                    <div class="position-sticky pt-3">
                        <h4 class="text-white text-center mb-4">Library System</h4>
                        <ul class="nav flex-column">
                            <li class="nav-item">
                                <a class="nav-link" href="WebForm1.aspx">
                                    <i class="fas fa-tachometer-alt me-2"></i> Dashboard
                                </a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link" href="Books.aspx">
                                    <i class="fas fa-book me-2"></i> Books
                                </a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link" href="Members.aspx">
                                    <i class="fas fa-users me-2"></i> Members
                                </a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link active" href="borrowtransac.aspx">
                                    <i class="fas fa-hand-holding me-2"></i> Borrow Transaction
                                </a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link" href="reports.aspx">
                                    <i class="fas fa-chart-bar me-2"></i> Reports
                                </a>
                            </li>
                        </ul>
                    </div>
                </nav>

                <!-- Main Content -->
                <main class="col-md-9 ms-sm-auto col-lg-10 px-md-4 main-content">
                    <div class="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
                        <h1 class="h2">Borrow Transaction</h1>
                        <div class="btn-toolbar mb-2 mb-md-0">
                            <div class="btn-group me-2">
                                <asp:Button ID="btnNewTransaction" runat="server" Text="New Transaction" 
                                    CssClass="btn btn-primary"  />
                            </div>
                        </div>
                    </div>

                    <!-- Search and Filter -->
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <div class="input-group">
                                <asp:TextBox ID="txtSearchLoan" runat="server" CssClass="form-control" 
                                    placeholder="Search by book title, member name, or Borrow ID..."></asp:TextBox>
                                <asp:Button ID="btnSearchLoan" runat="server" Text="Search" 
                                    CssClass="btn btn-outline-secondary" OnClick="btnSearchLoan_Click" />
                            </div>
                        </div>
                        <div class="col-md-3">
                            <asp:DropDownList ID="ddlLoanStatus" runat="server" CssClass="form-select" 
                                AutoPostBack="true" OnSelectedIndexChanged="ddlLoanStatus_SelectedIndexChanged">
                                <asp:ListItem Value="">All Status</asp:ListItem>
                                <asp:ListItem Value="Active">Active</asp:ListItem>
                                <asp:ListItem Value="Returned">Returned</asp:ListItem>
                                <asp:ListItem Value="Overdue">Overdue</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-3">
                            <asp:DropDownList ID="ddlDateRange" runat="server" CssClass="form-select" 
                                AutoPostBack="true" OnSelectedIndexChanged="ddlDateRange_SelectedIndexChanged">
                                <asp:ListItem Value="">All Time</asp:ListItem>
                                <asp:ListItem Value="Today">Today</asp:ListItem>
                                <asp:ListItem Value="ThisWeek">This Week</asp:ListItem>
                                <asp:ListItem Value="ThisMonth">This Month</asp:ListItem>
                                <asp:ListItem Value="ThisYear">This Year</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>

                    <!-- Loans Grid -->
                    <div class="card shadow">
                        <div class="card-header py-3">
                            <h6 class="m-0 font-weight-bold text-primary">Transaction Records</h6>
                        </div>
                        <div class="card-body">
                            <asp:GridView ID="gvLoans" runat="server" CssClass="table table-hover" 
                                AutoGenerateColumns="false" GridLines="None" AllowPaging="true" 
                                PageSize="10" OnPageIndexChanging="gvLoans_PageIndexChanging"
                                OnRowCommand="gvLoans_RowCommand">
                                <Columns>
                                    <asp:BoundField DataField="LoanId" HeaderText="Borrow ID" />
                                    <asp:BoundField DataField="BookTitle" HeaderText="Book Title" />
                                    <asp:BoundField DataField="MemberName" HeaderText="Member" />
                                    <asp:BoundField DataField="LoanDate" HeaderText="Borrow Date" DataFormatString="{0:MM/dd/yyyy}" />
                                    <asp:BoundField DataField="DueDate" HeaderText="Due Date" DataFormatString="{0:MM/dd/yyyy}" />
                                    <asp:BoundField DataField="ReturnDate" HeaderText="Return Date" DataFormatString="{0:MM/dd/yyyy}" />
                                    <asp:BoundField DataField="Status" HeaderText="Status" />
                                    <asp:TemplateField HeaderText="Actions">
                                        <ItemTemplate>
                                            <div class="action-buttons">
                                                <asp:LinkButton ID="btnReturn" runat="server" CommandName="ReturnBook" 
                                                    CommandArgument='<%# Eval("LoanId") %>' CssClass="btn btn-sm btn-success"
                                                    Visible='<%# !Convert.ToBoolean(Eval("IsReturned")) %>'>
                                                    <i class="fas fa-undo"></i> Return
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnRenew" runat="server" CommandName="RenewLoan" 
                                                    CommandArgument='<%# Eval("LoanId") %>' CssClass="btn btn-sm btn-warning"
                                                    Visible='<%# !Convert.ToBoolean(Eval("IsReturned")) %>'>
                                                    <i class="fas fa-clock"></i> Renew
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnDetails" runat="server" CommandName="ViewDetails" 
                                                    CommandArgument='<%# Eval("LoanId") %>' CssClass="btn btn-sm btn-info">
                                                    <i class="fas fa-eye"></i>
                                                </asp:LinkButton>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>
                </main>
            </div>
        </div>

        <!-- New Loan Modal -->
        <div class="modal fade" id="loanModal" tabindex="-1" aria-labelledby="loanModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="loanModalLabel">New Loan</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <div class="row">
                            <div class="col-md-6 mb-3">
                                <label for="ddlMember" class="form-label">Select Member</label>
                                <asp:DropDownList ID="ddlMember" runat="server" CssClass="form-select" required>
                                    <asp:ListItem Value="">-- Select Member --</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label for="ddlBook" class="form-label">Select Book</label>
                                <asp:DropDownList ID="ddlBook" runat="server" CssClass="form-select" required>
                                    <asp:ListItem Value="">-- Select Book --</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6 mb-3">
                                <label for="txtLoanDate" class="form-label">Loan Date</label>
                                <asp:TextBox ID="txtLoanDate" runat="server" CssClass="form-control" 
                                    TextMode="Date" required></asp:TextBox>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label for="txtDueDate" class="form-label">Due Date</label>
                                <asp:TextBox ID="txtDueDate" runat="server" CssClass="form-control" 
                                    TextMode="Date" required></asp:TextBox>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12 mb-3">
                                <label for="txtNotes" class="form-label">Notes</label>
                                <asp:TextBox ID="txtNotes" runat="server" CssClass="form-control" 
                                    TextMode="MultiLine" Rows="3"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                        <asp:Button ID="btnSaveLoan" runat="server" Text="Create Loan" 
                            CssClass="btn btn-primary" OnClick="btnSaveLoan_Click" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Return Book Modal -->
        <div class="modal fade" id="returnModal" tabindex="-1" aria-labelledby="returnModalLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="returnModalLabel">Return Book</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <div class="mb-3">
                            <label for="txtReturnDate" class="form-label">Return Date</label>
                            <asp:TextBox ID="txtReturnDate" runat="server" CssClass="form-control" 
                                TextMode="Date" required></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label for="txtFineAmount" class="form-label">Fine Amount (if any)</label>
                            <asp:TextBox ID="txtFineAmount" runat="server" CssClass="form-control" 
                                TextMode="Number" step="0.01" min="0"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label for="txtReturnNotes" class="form-label">Return Notes</label>
                            <asp:TextBox ID="txtReturnNotes" runat="server" CssClass="form-control" 
                                TextMode="MultiLine" Rows="2"></asp:TextBox>
                        </div>
                        <asp:HiddenField ID="hfLoanId" runat="server" />
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                        <asp:Button ID="btnProcessReturn" runat="server" Text="Process Return" 
                            CssClass="btn btn-success" OnClick="btnProcessReturn_Click" />
                    </div>
                </div>
            </div>
        </div>
    </form>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"></script>
    <script>
        function showLoanModal() {
            var modal = new bootstrap.Modal(document.getElementById('loanModal'));
            modal.show();
        }

        function hideLoanModal() {
            var modal = bootstrap.Modal.getInstance(document.getElementById('loanModal'));
            if (modal) {
                modal.hide();
            }
        }

        function showReturnModal() {
            var modal = new bootstrap.Modal(document.getElementById('returnModal'));
            modal.show();
        }

        function hideReturnModal() {
            var modal = bootstrap.Modal.getInstance(document.getElementById('returnModal'));
            if (modal) {
                modal.hide();
            }
        }
    </script>
</body>
</html>
