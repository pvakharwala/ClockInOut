using ClockInOut.Properties;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;

namespace ClockInOut
{
    public partial class Form1 : Form
    {

        public int login_state = 0;
        public int valid = 0;
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "Timesheet";
        static readonly string SpreadsheetId = "1LkE2sVnjBM7bDo-C2pVa2k8INrrMpTUlgzpppmVRzQE";
        static string name; 
        static SheetsService service;
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void loginBtn_Click(object sender, EventArgs e)
        {
            
            string[] tempid = Settings.Default["id"].ToString().Split(',');
            int[] ids = Array.ConvertAll(tempid, int.Parse);
            if (login_state == 0)
            {
                int pass;
                if (Int32.TryParse(passwordTextbox.Text.ToString(), out pass))
                {
                    for (int i = 0; i < ids.Length; i++)
                    {
                        if (ids[i] == pass)
                        {
                            valid = 1;
                        }
                    }
                    if (valid == 1)
                    {
                        nameTextbox.ReadOnly = true;
                        passwordTextbox.ReadOnly = true;
                        loginBtn.Text = "Logout";
                        login_state = 1;
                        loginLabel.ForeColor = System.Drawing.Color.Green;
                        loginLabel.Text = "Login Successful";
                    }
                    else
                    {
                        valid = 0;
                        loginLabel.ForeColor = System.Drawing.Color.Red;
                        loginLabel.Text = "Login Unsuccessful";
                    }
                }
                else
                {
                    MessageBox.Show("Entered password is not a number!", "Inavlid Data");
                }
                
                
            }
            else
            {
                nameTextbox.ReadOnly = false;
                passwordTextbox.ReadOnly = false;
                loginBtn.Text = "Login";
                login_state = 0;
                valid = 0;
                loginLabel.Text = "Logout Successful";
            }
        }

        private void punchBtn_Click(object sender, EventArgs e)
        {
            if (valid==1)
            {
               name = nameTextbox.Text.ToString();
                GoogleCredential credential;
                using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(Scopes);
                }
                service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
                double clockin;
                double clockout;
                if (Double.TryParse(clockInTextbox.Text.ToString(), out clockin))
                {
                    if (Double.TryParse(clockOutTextbox.Text.ToString(), out clockout))
                    {
                        double hours = clockout - clockin;
                        if (hours>=0)
                        {
                            try
                            {
                                var range = $"{name}!A:C";
                                var valueRange = new ValueRange();

                                var oblist = new List<object>() { dateTimePicker.Text.ToString(), clockin.ToString(), clockout.ToString(), hours.ToString() };
                                valueRange.Values = new List<IList<object>> { oblist };
                                var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
                                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                                var appendReponse = appendRequest.Execute();
                                punchLabel.ForeColor = System.Drawing.Color.Green;
                                punchLabel.Text = hours.ToString() + " Hour/s Submitted!";
                            }
                            catch (Exception ex)
                            {

                                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Clock Out cannot be before Clock In", "Inavlid Data");
                        }
                        
                    }
                    else
                    {
                        MessageBox.Show("Entered Clock Out is not a number!", "Inavlid Data");
                    }
                }
                else
                {
                    MessageBox.Show("Entered Clock In is not a number!", "Inavlid Data");
                }
            }
            else
            {
                MessageBox.Show("Login required", "Error!");
            }

        }
    }
}
