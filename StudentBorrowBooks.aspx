<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StudentBorrowBooks.aspx.cs" Inherits="prjLibrarySystem.StudentBorrowBooks" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Borrow Books - Library System</title>
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
        .book-card {
            border: 1px solid #ddd;
            border-radius: 10px;
            padding: 15px;
            margin-bottom: 15px;
            transition: all 0.3s;
        }
        .book-card:hover {
            box-shadow: 0 5px 15px rgba(0,0,0,0.1);
            transform: translateY(-2px);
        }
        .book-cover {
            width: 80px;
            height: 120px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 5px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 2rem;
        }
        .borrow-btn {
            background: #28a745;
            color: white;
            border: none;
            padding: 8px 15px;
            border-radius: 20px;
            font-size: 0.9rem;
        }
        .borrow-btn:hover {
            background: #218838;
        }
        .borrow-btn:disabled {
            background: #6c757d;
            cursor: not-allowed;
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
                                <a class="nav-link active" href="StudentBorrowBooks.aspx">
                                    <i class="fas fa-book me-2"></i> Borrow Books
                                </a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link" href="StudentMyBooks.aspx">
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
                        <h1 class="h2">Borrow Books</h1>
                        <div class="btn-toolbar mb-2 mb-md-0">
                            <div class="btn-group me-2">
                                <asp:Button ID="btnRefresh" runat="server" Text="Refresh" 
                                    CssClass="btn btn-outline-secondary" OnClick="btnRefresh_Click" />
                            </div>
                        </div>
                    </div>

                    <!-- Available Books Section -->
                    <div class="card shadow mb-4">
                        <div class="card-header py-3">
                            <h6 class="m-0 font-weight-bold text-primary">Available Books</h6>
                        </div>
                        <div class="card-body">
                            <div class="row mb-3">
                                <div class="col-md-6">
                                    <div class="input-group">
                                        <asp:TextBox ID="txtSearchBooks" runat="server" CssClass="form-control" 
                                            placeholder="Search books..." AutoPostBack="true" OnTextChanged="txtSearchBooks_TextChanged"></asp:TextBox>
                                        <asp:Button ID="btnSearchBooks" runat="server" Text="Search" 
                                            CssClass="btn btn-outline-secondary" OnClick="btnSearchBooks_Click" />
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select" 
                                        AutoPostBack="true" OnSelectedIndexChanged="ddlCategory_SelectedIndexChanged">
                                        <asp:ListItem Value="">All Categories</asp:ListItem>
                                        <asp:ListItem Value="Fiction">Fiction</asp:ListItem>
                                        <asp:ListItem Value="Non-Fiction">Non-Fiction</asp:ListItem>
                                        <asp:ListItem Value="Science">Science</asp:ListItem>
                                        <asp:ListItem Value="Technology">Technology</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <asp:GridView ID="gvAvailableBooks" runat="server" CssClass="table table-hover" 
                                AutoGenerateColumns="false" GridLines="None" AllowPaging="true" 
                                PageSize="6" OnPageIndexChanging="gvAvailableBooks_PageIndexChanging"
                                OnRowCommand="gvAvailableBooks_RowCommand">
                                <Columns>
                                    <asp:TemplateField HeaderText="Cover">
                                        <ItemTemplate>
                                            <div class="book-cover">
                                                <i class="fas fa-book"></i>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Title" HeaderText="Title" />
                                    <asp:BoundField DataField="Author" HeaderText="Author" />
                                    <asp:BoundField DataField="Category" HeaderText="Category" />
                                    <asp:TemplateField HeaderText="Available">
                                        <ItemTemplate>
                                            <span class='badge <%# Convert.ToInt32(Eval("AvailableCopies")) > 0 ? "bg-success" : "bg-danger" %>'>
                                                <%# Eval("AvailableCopies") + " copies" %>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Action">
                                        <ItemTemplate>
                                            <asp:Button ID="btnBorrow" runat="server" Text="Borrow" 
                                                CssClass="btn borrow-btn" CommandName="BorrowBook"
                                                CommandArgument='<%# Eval("BookId") %>'
                                                Enabled='<%# Convert.ToInt32(Eval("AvailableCopies")) > 0 %>' />
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
