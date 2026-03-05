using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace prjLibrarySystem
{
    public partial class StudentMyBooks : System.Web.UI.Page
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
                LoadMyBooks();
            }
        }

        private void LoadMyBooks()
        {
            try
            {
                // Create sample data for demonstration
                DataTable dt = new DataTable();
                dt.Columns.Add("LoanId");
                dt.Columns.Add("Title");
                dt.Columns.Add("Author");
                dt.Columns.Add("BorrowDate");
                dt.Columns.Add("DueDate");
                
                dt.Rows.Add(1, "Effective Java", "Joshua Bloch", DateTime.Now.AddDays(-10), DateTime.Now.AddDays(4));
                dt.Rows.Add(2, "Clean Code", "Robert C. Martin", DateTime.Now.AddDays(-5), DateTime.Now.AddDays(9));
                dt.Rows.Add(3, "The Great Gatsby", "F. Scott Fitzgerald", DateTime.Now.AddDays(-20), DateTime.Now.AddDays(-6));
                
                gvMyBooks.DataSource = dt;
                gvMyBooks.DataBind();
                
                // Original database code (commented out until MySQL is installed):
                /*
                string studentId = Session["UserId"]?.ToString() ?? "1";
                string query = @"
                    SELECT l.LoanId, b.Title, b.Author, l.LoanDate, l.DueDate
                    FROM Loans l
                    INNER JOIN Books b ON l.BookId = b.BookId
                    WHERE l.MemberId = @MemberId AND l.IsReturned = 0
                    ORDER BY l.DueDate ASC";
                    
                var parameters = new Dictionary<string, object>
                {
                    {"@MemberId", studentId}
                };
                
                using (var reader = DatabaseHelper.ExecuteReader(query, parameters))
                {
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    gvMyBooks.DataSource = dt;
                    gvMyBooks.DataBind();
                }
                */
            }
            catch (Exception ex)
            {
                // Handle errors gracefully
                gvMyBooks.DataSource = null;
                gvMyBooks.DataBind();
            }
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadMyBooks();
        }

        protected void gvMyBooks_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvMyBooks.PageIndex = e.NewPageIndex;
            LoadMyBooks();
        }

        protected void gvMyBooks_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ReturnBook")
            {
                int loanId = Convert.ToInt32(e.CommandArgument);
                
                try
                {
                    // For demo purposes, show a success message
                    // In real implementation, this would update the database
                    /*
                    string query = @"
                        UPDATE Loans SET IsReturned = 1, ReturnDate = CURDATE() WHERE LoanId = @LoanId;
                        
                        UPDATE Books SET AvailableCopies = AvailableCopies + 1 
                        WHERE BookId = (SELECT BookId FROM Loans WHERE LoanId = @LoanId);
                    ";
                    
                    var parameters = new Dictionary<string, object>
                    {
                        {"@LoanId", loanId}
                    };
                    
                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    */
                    
                    // Show success message
                    ScriptManager.RegisterClientScriptBlock(this, GetType(), "success", 
                        "alert('Book returned successfully!');", true);
                    
                    // Refresh the book list
                    LoadMyBooks();
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this, GetType(), "error", 
                        "alert('Error returning book. Please try again.');", true);
                }
            }
        }

        protected string GetStatusBadgeClass(object dueDate)
        {
            if (dueDate == null) return "bg-secondary";
            
            DateTime due = Convert.ToDateTime(dueDate);
            DateTime today = DateTime.Today;
            
            if (due < today)
                return "bg-danger"; // Overdue
            else if (due <= today.AddDays(3))
                return "bg-warning"; // Due soon
            else
                return "bg-success"; // Good
        }

        protected string GetBookStatus(object dueDate)
        {
            if (dueDate == null) return "Unknown";
            
            DateTime due = Convert.ToDateTime(dueDate);
            DateTime today = DateTime.Today;
            
            if (due < today)
                return "Overdue";
            else if (due <= today.AddDays(3))
                return "Due Soon";
            else
                return "On Time";
        }
    }
}
