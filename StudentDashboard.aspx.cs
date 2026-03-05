using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace prjLibrarySystem
{
    public partial class StudentDashboard : System.Web.UI.Page
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
                Response.Redirect("Login.aspx");
                return;
            }

            lblStudentName.Text = Session["Username"].ToString();

            if (!IsPostBack)
            {
                LoadStudentStatistics();
            }
        }

        private void LoadStudentStatistics()
        {
            try
            {
                // Demo data - in real app, this would query database
                lblAvailableBooks.Text = "8";
                lblBorrowedBooks.Text = "2";
                lblOverdueBooks.Text = "0";
                lblTotalBorrowed.Text = "5";
            }
            catch (Exception ex)
            {
                // Handle errors gracefully
                lblAvailableBooks.Text = "0";
                lblBorrowedBooks.Text = "0";
                lblOverdueBooks.Text = "0";
                lblTotalBorrowed.Text = "0";
            }
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadStudentStatistics();
        }
    }
}
