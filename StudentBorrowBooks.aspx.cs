using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace prjLibrarySystem
{
    public partial class StudentBorrowBooks : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if user is logged in
            if (Session["Username"] == null || Session["UserRole"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            // Check if user is student
            if (Session["UserRole"].ToString() != "Student")
            {
                Response.Redirect("WebForm1.aspx");
                return;
            }

            // Set student name
            lblStudentName.Text = "Welcome, " + Session["Username"].ToString();

            if (!IsPostBack)
            {
                LoadAvailableBooks();
            }
        }

        private void LoadAvailableBooks()
        {
            try
            {
                // Create sample data for demonstration
                DataTable dt = new DataTable();
                dt.Columns.Add("BookId");
                dt.Columns.Add("Title");
                dt.Columns.Add("Author");
                dt.Columns.Add("Category");
                dt.Columns.Add("AvailableCopies");
                
                dt.Rows.Add(1, "Effective Java", "Joshua Bloch", "Technology", 3);
                dt.Rows.Add(2, "Clean Code", "Robert C. Martin", "Technology", 2);
                dt.Rows.Add(3, "The Great Gatsby", "F. Scott Fitzgerald", "Fiction", 1);
                dt.Rows.Add(4, "To Kill a Mockingbird", "Harper Lee", "Fiction", 0);
                dt.Rows.Add(5, "Sapiens", "Yuval Noah Harari", "Non-Fiction", 4);
                dt.Rows.Add(6, "The Lean Startup", "Eric Ries", "Business", 2);
                
                // Apply search filter
                if (!string.IsNullOrEmpty(txtSearchBooks.Text))
                {
                    string search = txtSearchBooks.Text.ToLower();
                    DataView dv = dt.DefaultView;
                    dv.RowFilter = $"Title LIKE '%{search}%' OR Author LIKE '%{search}%'";
                    dt = dv.ToTable();
                }
                
                // Apply category filter
                if (!string.IsNullOrEmpty(ddlCategory.SelectedValue))
                {
                    DataView dv = dt.DefaultView;
                    dv.RowFilter = $"Category = '{ddlCategory.SelectedValue}'";
                    dt = dv.ToTable();
                }
                
                gvAvailableBooks.DataSource = dt;
                gvAvailableBooks.DataBind();
                
                // Original database code (commented out until MySQL is installed):
                /*
                string query = "SELECT BookId, Title, Author, Category, AvailableCopies FROM Books WHERE AvailableCopies > 0";
                
                // Add search filter
                if (!string.IsNullOrEmpty(txtSearchBooks.Text))
                {
                    query += $" AND (Title LIKE '%{txtSearchBooks.Text}%' OR Author LIKE '%{txtSearchBooks.Text}%')";
                }
                
                // Add category filter
                if (!string.IsNullOrEmpty(ddlCategory.SelectedValue))
                {
                    query += $" AND Category = '{ddlCategory.SelectedValue}'";
                }
                
                using (var reader = DatabaseHelper.ExecuteReader(query))
                {
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    gvAvailableBooks.DataSource = dt;
                    gvAvailableBooks.DataBind();
                }
                */
            }
            catch (Exception ex)
            {
                // Handle errors gracefully
                gvAvailableBooks.DataSource = null;
                gvAvailableBooks.DataBind();
            }
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            txtSearchBooks.Text = "";
            ddlCategory.SelectedIndex = 0;
            LoadAvailableBooks();
        }

        protected void txtSearchBooks_TextChanged(object sender, EventArgs e)
        {
            LoadAvailableBooks();
        }

        protected void btnSearchBooks_Click(object sender, EventArgs e)
        {
            LoadAvailableBooks();
        }

        protected void ddlCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadAvailableBooks();
        }

        protected void gvAvailableBooks_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAvailableBooks.PageIndex = e.NewPageIndex;
            LoadAvailableBooks();
        }

        protected void gvAvailableBooks_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "BorrowBook")
            {
                int bookId = Convert.ToInt32(e.CommandArgument);
                
                try
                {
                    // For demo purposes, show a success message
                    // In real implementation, this would insert into the database
                    string studentId = Session["UserId"]?.ToString() ?? "1";
                    
                    // Sample database operation (commented out until MySQL is installed):
                    /*
                    string query = @"
                        INSERT INTO Loans (BookId, MemberId, LoanDate, DueDate, IsReturned)
                        VALUES (@BookId, @MemberId, CURDATE(), DATE_ADD(CURDATE(), INTERVAL 14 DAY), 0);
                        
                        UPDATE Books SET AvailableCopies = AvailableCopies - 1 WHERE BookId = @BookId;
                    ";
                    
                    var parameters = new Dictionary<string, object>
                    {
                        {"@BookId", bookId},
                        {"@MemberId", studentId}
                    };
                    
                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    */
                    
                    // Show success message
                    ScriptManager.RegisterClientScriptBlock(this, GetType(), "success", 
                        "alert('Book borrowed successfully! Due date: " + DateTime.Now.AddDays(14).ToString("MM/dd/yyyy") + "');", true);
                    
                    // Refresh the book list
                    LoadAvailableBooks();
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this, GetType(), "error", 
                        "alert('Error borrowing book. Please try again.');", true);
                }
            }
        }
    }
}
