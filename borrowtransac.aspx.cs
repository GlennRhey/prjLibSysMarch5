using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using prjLibrarySystem.Models;

namespace prjLibrarySystem
{
    public partial class Loans : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadTransactions();
                LoadMembersDropdown();
                LoadAvailableBooksDropdown();

                txtLoanDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                txtDueDate.Text = DateTime.Now.AddDays(14).ToString("yyyy-MM-dd");
                txtReturnDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        // ── Load grid ────────────────────────────────────────────────────────

        private void LoadTransactions()
        {
            try
            {
                string query = @"
                    SELECT t.BorrowID                                               AS LoanId,
                           b.Title                                                  AS BookTitle,
                           m.FullName                                               AS MemberName,
                           t.BorrowDate                                             AS LoanDate,
                           t.DueDate,
                           t.ReturnDate,
                           t.Status,
                           t.RequestStatus,
                           CAST(NULL AS DECIMAL(10,2))                              AS FineAmount,
                           CASE WHEN t.Status = 'Returned'
                                THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END        AS IsReturned
                    FROM tblTransactions t
                    INNER JOIN tblMembers m ON t.MemberID = m.MemberID
                    INNER JOIN tblBooks   b ON t.ISBN     = b.ISBN
                    WHERE 1=1";

                var parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(ddlLoanStatus.SelectedValue))
                {
                    query += " AND t.Status = @Status";
                    parameters.Add(new SqlParameter("@Status", ddlLoanStatus.SelectedValue));
                }

                if (!string.IsNullOrEmpty(ddlDateRange.SelectedValue))
                {
                    switch (ddlDateRange.SelectedValue)
                    {
                        case "Today":
                            query += " AND CAST(t.BorrowDate AS DATE) = CAST(GETDATE() AS DATE)";
                            break;
                        case "ThisWeek":
                            query += " AND t.BorrowDate >= DATEADD(DAY, -7, GETDATE())";
                            break;
                        case "ThisMonth":
                            query += " AND t.BorrowDate >= DATEADD(MONTH, -1, GETDATE())";
                            break;
                    }
                }

                query += " ORDER BY t.BorrowDate DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters.ToArray());
                gvLoans.DataSource = dt;
                gvLoans.DataBind();
            }
            catch (Exception ex)
            {
                gvLoans.DataSource = null;
                gvLoans.DataBind();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "error",
                    $"alert('Error loading transactions: {ex.Message}');", true);
            }
        }

        private void LoadMembersDropdown()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT MemberID, FullName FROM tblMembers WHERE IsActive = 1 ORDER BY FullName", new SqlParameter[0]);
                ddlMember.DataSource = dt;
                ddlMember.DataTextField = "FullName";
                ddlMember.DataValueField = "MemberID";
                ddlMember.DataBind();
                ddlMember.Items.Insert(0, new ListItem("-- Select Member --", ""));
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "error",
                    $"alert('Error loading members: {ex.Message}');", true);
            }
        }

        private void LoadAvailableBooksDropdown()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT ISBN, Title FROM tblBooks WHERE AvailableCopies > 0 ORDER BY Title", new SqlParameter[0]);
                ddlBook.DataSource = dt;
                ddlBook.DataTextField = "Title";
                ddlBook.DataValueField = "ISBN";
                ddlBook.DataBind();
                ddlBook.Items.Insert(0, new ListItem("-- Select Book --", ""));
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "error",
                    $"alert('Error loading books: {ex.Message}');", true);
            }
        }

        protected void btnSearchLoan_Click(object sender, EventArgs e)
        {
            LoadTransactions();
        }

        protected void ddlLoanStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTransactions();
        }

        protected void ddlDateRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTransactions();
        }

        protected void gvLoans_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvLoans.PageIndex = e.NewPageIndex;
            LoadTransactions();
        }

        protected void gvLoans_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int borrowId = Convert.ToInt32(e.CommandArgument);
            switch (e.CommandName)
            {
                case "ReturnBook": ShowReturnModal(borrowId); break;
                case "RenewLoan": RenewLoan(borrowId); break;
                case "ViewDetails": LoadTransactions(); break;
            }
        }

        protected void btnSaveLoan_Click(object sender, EventArgs e)
        {
            try
            {
                string isbn = ddlBook.SelectedValue;
                int memberId = Convert.ToInt32(ddlMember.SelectedValue);
                string adminId = Session["UserID"]?.ToString();
                DateTime borrowDate = DateTime.Parse(txtLoanDate.Text);
                DateTime dueDate = DateTime.Parse(txtDueDate.Text);

                string query = @"
                    INSERT INTO tblTransactions (MemberID, ISBN, AdminID, RequestType, RequestStatus, BorrowDate, DueDate, Status)
                    VALUES (@MemberID, @ISBN, @AdminID, 'Borrow', 'Accepted', @BorrowDate, @DueDate, 'Borrowed')";

                DatabaseHelper.ExecuteNonQuery(query, new SqlParameter[]
                {
                    new SqlParameter("@MemberID", memberId),
                    new SqlParameter("@ISBN", isbn),
                    new SqlParameter("@AdminID", adminId),
                    new SqlParameter("@BorrowDate", borrowDate),
                    new SqlParameter("@DueDate", dueDate)
                });

                LoadTransactions();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "hideModal",
                    "hideLoanModal();", true);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "success",
                    "alert('Borrow transaction created successfully.');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "error",
                    $"alert('Error creating transaction: {ex.Message}');", true);
            }
        }

        private void RenewLoan(int borrowId)
        {
            try
            {
                string query = "UPDATE tblTransactions SET DueDate = DATEADD(DAY, 14, DueDate) WHERE BorrowID = @BorrowID";
                DatabaseHelper.ExecuteNonQuery(query, new SqlParameter[] { new SqlParameter("@BorrowID", borrowId) });
                LoadTransactions();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "success",
                    "alert('Loan renewed successfully.');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "error",
                    $"alert('Error renewing loan: {ex.Message}');", true);
            }
        }

        private void ShowReturnModal(int borrowId)
        {
            hfLoanId.Value = borrowId.ToString();
            txtReturnDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtReturnNotes.Text = "";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "showReturnModal",
                "showReturnModal();", true);
        }

        protected void btnProcessReturn_Click(object sender, EventArgs e)
        {
            try
            {
                int borrowId = Convert.ToInt32(hfLoanId.Value);
                string adminId = Session["UserID"]?.ToString();
                DateTime returnDate = DateTime.Parse(txtReturnDate.Text);

                string query = @"
                    UPDATE tblTransactions 
                    SET ReturnDate = @ReturnDate, Status = 'Returned', AdminID = @AdminID 
                    WHERE BorrowID = @BorrowID";

                DatabaseHelper.ExecuteNonQuery(query, new SqlParameter[]
                {
                    new SqlParameter("@BorrowID", borrowId),
                    new SqlParameter("@ReturnDate", returnDate),
                    new SqlParameter("@AdminID", adminId)
                });

                LoadTransactions();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "hideModal",
                    "hideReturnModal();", true);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "success",
                    "alert('Book returned successfully.');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "error",
                    $"alert('Error processing return: {ex.Message}');", true);
            }
        }

        protected string GetLoanStatusBadgeClass(object isReturned, object dueDate)
        {
            if (Convert.ToBoolean(isReturned)) return "bg-success";
            if (dueDate != DBNull.Value && Convert.ToDateTime(dueDate) < DateTime.Now) return "bg-danger";
            return "bg-primary";
        }

        protected string GetLoanStatus(object isReturned, object dueDate)
        {
            if (Convert.ToBoolean(isReturned)) return "Returned";
            if (dueDate != DBNull.Value && Convert.ToDateTime(dueDate) < DateTime.Now) return "Overdue";
            return "Active";
        }
    }
}