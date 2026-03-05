<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StudentMyBooks.aspx.cs" Inherits="prjLibrarySystem.StudentMyBooks" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>My Books - Library System</title>
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
        .return-btn {
            background: #ffc107;
            color: #212529;
            border: none;
            padding: 8px 15px;
            border-radius: 20px;
            font-size: 0.9rem;
        }
        .return-btn:hover {
            background: #e0a800;
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
                        <div class="text-center mb-4">
                            <div class="library-icon">
                                <i class="fas fa-book-open"></i>
                            </div>
                            <h4 class="text-white">Student Portal</h4>
                            <small class="text-white-50">
                                <asp:Label ID="lblStudentName" runat="server" Text="Student"></asp:Label>
                            </small>
                        </div>
                        <ul class="nav flex-column">
                            <li class="nav-item">
                                <a class="nav-link" href="StudentDashboard.aspx">
                                    <i class="fas fa-tachometer-alt me-2"></i> Dashboard
                                </a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link" href="StudentBorrowBooks.aspx">
                                    <i class="fas fa-book me-2"></i> Borrow Books
                                </a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link active" href="StudentMyBooks.aspx">
                                    <i class="fas fa-book-reader me-2"></i> My Books
                                </a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link" href="Logout.aspx">
                                    <i class="fas fa-sign-out-alt me-2"></i> Logout
                                </a>
                            </li>
                        </ul>
                    </div>
                </nav>

                <!-- Main Content -->
                <main class="col-md-9 ms-sm-auto col-lg-10 px-md-4 main-content">
                    <div class="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
                        <h1 class="h2">My Borrowed Books</h1>
                        <div class="btn-toolbar mb-2 mb-md-0">
                            <div class="btn-group me-2">
                                <asp:Button ID="btnRefresh" runat="server" Text="Refresh" 
                                    CssClass="btn btn-outline-secondary" OnClick="btnRefresh_Click" />
                            </div>
                        </div>
                    </div>

                    <!-- My Books Section -->
                    <div class="card shadow mb-4">
                        <div class="card-header py-3">
                            <h6 class="m-0 font-weight-bold text-primary">My Borrowed Books</h6>
                        </div>
                        <div class="card-body">
                            <asp:GridView ID="gvMyBooks" runat="server" CssClass="table table-hover" 
                                AutoGenerateColumns="false" GridLines="None" AllowPaging="true" 
                                PageSize="10" OnPageIndexChanging="gvMyBooks_PageIndexChanging"
                                OnRowCommand="gvMyBooks_RowCommand">
                                <Columns>
                                    <asp:BoundField DataField="Title" HeaderText="Book Title" />
                                    <asp:BoundField DataField="Author" HeaderText="Author" />
                                    <asp:BoundField DataField="BorrowDate" HeaderText="Borrow Date" DataFormatString="{0:MM/dd/yyyy}" />
                                    <asp:BoundField DataField="DueDate" HeaderText="Due Date" DataFormatString="{0:MM/dd/yyyy}" />
                                    <asp:TemplateField HeaderText="Status">
                                        <ItemTemplate>
                                            <span class='badge <%# GetStatusBadgeClass(Eval("DueDate")) %>'>
                                                <%# GetBookStatus(Eval("DueDate")) %>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Action">
                                        <ItemTemplate>
                                            <asp:Button ID="btnReturn" runat="server" Text="Return" 
                                                CssClass="btn return-btn" CommandName="ReturnBook"
                                                CommandArgument='<%# Eval("LoanId") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>
                </main>
            </div>
        </div>
    </form>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
