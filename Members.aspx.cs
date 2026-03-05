using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using prjLibrarySystem.Models;

namespace prjLibrarySystem
{
    public partial class Members : System.Web.UI.Page
    {
        private string SearchTerm
        {
            get { return ViewState["SearchTerm"] as string ?? ""; }
            set { ViewState["SearchTerm"] = value; }
        }

        // ddlYearLevel in Members.aspx uses Text values: "1st Year","2nd Year","3rd Year","4th Year"
        // These map to int YearLevel in tblMembers: 1,2,3,4
        private int ParseYearLevel(string text)
        {
            switch (text)
            {
                case "1st Year": return 1;
                case "2nd Year": return 2;
                case "3rd Year": return 3;
                case "4th Year": return 4;
                default:
                    // fallback: try parsing the first character
                    if (text.Length > 0 && char.IsDigit(text[0]))
                        return int.Parse(text[0].ToString());
                    return 1;
            }
        }

        private string YearLevelToText(int yearLevel)
        {
            switch (yearLevel)
            {
                case 1: return "1st Year";
                case 2: return "2nd Year";
                case 3: return "3rd Year";
                case 4: return "4th Year";
                default: return "1st Year";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                LoadMembers();
        }

        private void LoadMembers()
        {
            try
            {
                string currentSearch = SearchTerm;

                // Aliases match .aspx BoundField DataField names:
                // MemberID, FullName, Username (→UserID), Email, Course, YearLevel, Role, RegistrationDate, Status
                string query = @"
                    SELECT m.MemberID,
                           m.FullName,
                           u.UserID       AS Username,
                           u.Email,
                           m.Course,
                           m.YearLevel,
                           'Student'      AS Role,
                           u.CreatedAt    AS RegistrationDate,
                           CASE WHEN u.IsActive = 1 THEN 'Active' ELSE 'Inactive' END AS Status
                    FROM tblMembers m
                    INNER JOIN tblUsers u ON m.UserID = u.UserID
                    WHERE 1=1";

                var parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(currentSearch))
                {
                    query += " AND (m.FullName LIKE @Search OR u.Email LIKE @Search OR u.UserID LIKE @Search)";
                    parameters.Add(new SqlParameter("@Search", "%" + currentSearch + "%"));
                }

                // ddlMembershipType: "Student" or "Admin" — tblMembers is students only
                if (!string.IsNullOrEmpty(ddlMembershipType.SelectedValue))
                {
                    if (ddlMembershipType.SelectedValue == "Admin")
                        query += " AND 1=0"; // no admins in tblMembers
                    // Student: no extra filter needed
                }

                if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                {
                    query += " AND u.IsActive = @IsActive";
                    parameters.Add(new SqlParameter("@IsActive",
                        ddlStatus.SelectedValue == "Active" ? 1 : 0));
                }

                query += " ORDER BY m.MemberID ASC";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters.ToArray());
                gvMembers.DataSource = dt;
                gvMembers.DataBind();
                txtSearchMember.Text = currentSearch;
            }
            catch (Exception ex)
            {
                gvMembers.DataSource = null;
                gvMembers.DataBind();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "error",
                    $"alert('Error loading members: {ex.Message}');", true);
            }
        }

        // All control IDs below verified against Members.aspx:
        // txtUserId, txtFullName, txtEmail, txtCourse, ddlYearLevel, txtPassword
        // hfSelectedRole, hfEditingMemberId, lblRegisterTitle

        protected void btnSaveMember_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedRole = hfSelectedRole.Value; // "Student" or "Admin"

                if (!string.IsNullOrEmpty(hfEditingMemberId.Value))
                {
                    // ── EDIT ────────────────────────────────────────────────
                    int memberId = int.Parse(hfEditingMemberId.Value);

                    DataTable userDt = DatabaseHelper.ExecuteQuery(
                        "SELECT UserID FROM tblMembers WHERE MemberID = @MemberID",
                        new SqlParameter[] { new SqlParameter("@MemberID", memberId) });

                    if (userDt.Rows.Count == 0) throw new Exception("Member not found.");
                    string userId = userDt.Rows[0]["UserID"].ToString();

                    // Update email in tblUsers
                    DatabaseHelper.ExecuteQuery(
                        "UPDATE tblUsers SET Email = @Email WHERE UserID = @UserID",
                        new SqlParameter[]
                        {
                            new SqlParameter("@Email",  txtEmail.Text),
                            new SqlParameter("@UserID", userId)
                        });

                    // Update password only if filled in
                    if (!string.IsNullOrEmpty(txtPassword.Text))
                        DatabaseHelper.ExecuteQuery(
                            "UPDATE tblUsers SET PasswordHash = @PasswordHash WHERE UserID = @UserID",
                            new SqlParameter[]
                            {
                                new SqlParameter("@PasswordHash", txtPassword.Text),
                                new SqlParameter("@UserID",       userId)
                            });

                    // Update member profile
                    DatabaseHelper.ExecuteQuery(
                        "UPDATE tblMembers SET FullName=@FullName, Course=@Course, YearLevel=@YearLevel WHERE MemberID=@MemberID",
                        new SqlParameter[]
                        {
                            new SqlParameter("@FullName",  txtFullName.Text),
                            new SqlParameter("@Course",    txtCourse.Text),
                            new SqlParameter("@YearLevel", ParseYearLevel(ddlYearLevel.SelectedValue)),
                            new SqlParameter("@MemberID",  memberId)
                        });
                }
                else
                {
                    // ── ADD NEW ──────────────────────────────────────────────
                    string newUserId = txtUserId.Text.Trim();

                    if (selectedRole == "Student")
                    {
                        // Insert login record first (FK constraint)
                        DatabaseHelper.ExecuteQuery(
                            "INSERT INTO tblUsers (UserID, PasswordHash, Role, Email, IsActive) VALUES (@UserID, @PasswordHash, 'Student', @Email, 1)",
                            new SqlParameter[]
                            {
                                new SqlParameter("@UserID",       newUserId),
                                new SqlParameter("@PasswordHash", txtPassword.Text),
                                new SqlParameter("@Email",        txtEmail.Text)
                            });

                        // Insert student profile
                        DatabaseHelper.ExecuteQuery(
                            "INSERT INTO tblMembers (UserID, FullName, Course, YearLevel) VALUES (@UserID, @FullName, @Course, @YearLevel)",
                            new SqlParameter[]
                            {
                                new SqlParameter("@UserID",    newUserId),
                                new SqlParameter("@FullName",  txtFullName.Text),
                                new SqlParameter("@Course",    txtCourse.Text),
                                new SqlParameter("@YearLevel", ParseYearLevel(ddlYearLevel.SelectedValue))
                            });
                    }
                    else
                    {
                        // Admin — FullName lives in tblUsers, no tblMembers row
                        DatabaseHelper.ExecuteQuery(
                            "INSERT INTO tblUsers (UserID, PasswordHash, Role, FullName, Email, IsActive) VALUES (@UserID, @PasswordHash, 'Admin', @FullName, @Email, 1)",
                            new SqlParameter[]
                            {
                                new SqlParameter("@UserID",       newUserId),
                                new SqlParameter("@PasswordHash", txtPassword.Text),
                                new SqlParameter("@FullName",     txtFullName.Text),
                                new SqlParameter("@Email",        txtEmail.Text)
                            });
                    }
                }

                ClearMemberForm();
                hfEditingMemberId.Value = "";
                LoadMembers();

                ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop",
                    "var myModal = bootstrap.Modal.getInstance(document.getElementById('memberModal')); myModal.hide();", true);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "success",
                    "alert('Member saved successfully.');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "error",
                    $"alert('Error saving member: {ex.Message}');", true);
            }
        }

        private void ClearMemberForm()
        {
            txtUserId.Text = "";
            txtFullName.Text = "";
            txtEmail.Text = "";
            txtCourse.Text = "";
            ddlYearLevel.SelectedIndex = 0;
            txtPassword.Text = "";
        }

        protected void gvMembers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvMembers.PageIndex = e.NewPageIndex;
            LoadMembers();
        }

        protected void gvMembers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int memberId = int.Parse(e.CommandArgument.ToString());

                // ── TOGGLE Active/Inactive ─────────────────────────────────
                if (e.CommandName == "ToggleStatus")
                {
                    DataTable userDt = DatabaseHelper.ExecuteQuery(@"
                        SELECT u.UserID, u.IsActive
                        FROM tblUsers u
                        INNER JOIN tblMembers m ON u.UserID = m.UserID
                        WHERE m.MemberID = @MemberID",
                        new SqlParameter[] { new SqlParameter("@MemberID", memberId) });

                    if (userDt.Rows.Count > 0)
                    {
                        string userId = userDt.Rows[0]["UserID"].ToString();
                        int newActive = Convert.ToInt32(userDt.Rows[0]["IsActive"]) == 1 ? 0 : 1;

                        DatabaseHelper.ExecuteQuery(
                            "UPDATE tblUsers SET IsActive = @IsActive WHERE UserID = @UserID",
                            new SqlParameter[]
                            {
                                new SqlParameter("@IsActive", newActive),
                                new SqlParameter("@UserID",   userId)
                            });
                    }
                    LoadMembers();
                    return;
                }

                // ── DELETE ────────────────────────────────────────────────
                if (e.CommandName == "DeleteMember")
                {
                    DataTable checkDt = DatabaseHelper.ExecuteQuery(
                        "SELECT COUNT(*) FROM tblTransactions WHERE MemberID = @MemberID AND Status = 'Borrowed'",
                        new SqlParameter[] { new SqlParameter("@MemberID", memberId) });

                    if (Convert.ToInt32(checkDt.Rows[0][0]) > 0)
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "alert",
                            "alert('Cannot delete a member with active borrow transactions.');", true);
                        return;
                    }

                    DataTable userDt = DatabaseHelper.ExecuteQuery(
                        "SELECT UserID FROM tblMembers WHERE MemberID = @MemberID",
                        new SqlParameter[] { new SqlParameter("@MemberID", memberId) });

                    if (userDt.Rows.Count > 0)
                        // ON DELETE CASCADE removes tblMembers row automatically
                        DatabaseHelper.ExecuteQuery(
                            "DELETE FROM tblUsers WHERE UserID = @UserID",
                            new SqlParameter[] { new SqlParameter("@UserID", userDt.Rows[0]["UserID"].ToString()) });

                    LoadMembers();
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "success",
                        "alert('Member deleted successfully.');", true);
                    return;
                }

                // ── EDIT ──────────────────────────────────────────────────
                if (e.CommandName == "EditMember")
                {
                    DataTable dt = DatabaseHelper.ExecuteQuery(@"
                        SELECT m.MemberID, m.FullName, m.Course, m.YearLevel,
                               u.UserID, u.Email, u.IsActive
                        FROM tblMembers m
                        INNER JOIN tblUsers u ON m.UserID = u.UserID
                        WHERE m.MemberID = @MemberID",
                        new SqlParameter[] { new SqlParameter("@MemberID", memberId) });

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];

                        txtUserId.Text = row["UserID"].ToString();
                        txtFullName.Text = row["FullName"].ToString();
                        txtEmail.Text = row["Email"].ToString();
                        txtCourse.Text = row["Course"].ToString();

                        // Convert int YearLevel back to text for ddlYearLevel
                        ddlYearLevel.SelectedValue = YearLevelToText(Convert.ToInt32(row["YearLevel"]));
                        txtPassword.Text = "";

                        hfEditingMemberId.Value = row["MemberID"].ToString();
                        lblRegisterTitle.Text = "Edit Member";

                        ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop",
                            "var myModal = new bootstrap.Modal(document.getElementById('memberModal')); myModal.show();", true);
                    }
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "error",
                    $"alert('Error: {ex.Message}');", true);
            }
        }

        // Called from .aspx TemplateField: GetStatusBadgeClass(Eval("Status"))
        protected string GetStatusBadgeClass(object statusObj)
        {
            if (statusObj == null) return "status-inactive";
            return statusObj.ToString() == "Active" ? "status-active" : "status-inactive";
        }

        protected string GetMemberStatus(object statusObj)
        {
            if (statusObj == null) return "Inactive";
            return statusObj.ToString() == "Active" ? "Active" : "Inactive";
        }

        protected void btnSearchMember_Click(object sender, EventArgs e)
        {
            SearchTerm = txtSearchMember.Text.Trim();
            gvMembers.PageIndex = 0;
            LoadMembers();
        }

        protected void ddlMembershipType_SelectedIndexChanged(object sender, EventArgs e)
        {
            gvMembers.PageIndex = 0;
            LoadMembers();
        }

        protected void ddlStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            gvMembers.PageIndex = 0;
            LoadMembers();
        }
    }
}