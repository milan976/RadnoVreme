using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace RadnoVreme
{
    public partial class UnosRadnikaForm : Form
    {
        private TextBox txtIme;
        private TextBox txtPrezime;
        private ComboBox cmbZvanje;
        private ComboBox cmbSmena;
        private Button btnSacuvaj;
        private Button btnOdustani;
        private Label lblStatus;

        private string connectionString = "Data Source=MILANDJ\\SQLEXPRESS;Initial Catalog=RadnoVreme;Integrated Security=True;Encrypt=False";
        private BazaService bazaService;

        public UnosRadnikaForm()
        {
            bazaService = new BazaService();
            InitializeComponent();
            KreirajKontrole();
            UcitajZvanja();
            UcitajSmane();
        }

        private void InitializeComponent()
        {
            // This method is intentionally left empty because all controls are created in KreirajKontrole.
            // It is required to satisfy the Form constructor call.
        }

        private void KreirajKontrole()
        {
            this.Text = "Унос новог радника";
            this.Size = new Size(450, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Label za naslov
            Label lblNaslov = new Label();
            lblNaslov.Text = "➕ Унос новог радника";
            lblNaslov.Font = new Font("Arial", 16, FontStyle.Bold);
            lblNaslov.ForeColor = Color.DarkBlue;
            lblNaslov.Size = new Size(350, 40);
            lblNaslov.Location = new Point(50, 20);
            lblNaslov.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblNaslov);

            // Polja za unos
            Label lblIme = new Label();
            lblIme.Text = "Име:*";
            lblIme.Font = new Font("Arial", 10, FontStyle.Regular);
            lblIme.Size = new Size(150, 25);
            lblIme.Location = new Point(50, 80);
            this.Controls.Add(lblIme);

            txtIme = new TextBox();
            txtIme.Size = new Size(300, 25);
            txtIme.Location = new Point(50, 105);
            txtIme.Font = new Font("Arial", 10, FontStyle.Regular);
            this.Controls.Add(txtIme);

            Label lblPrezime = new Label();
            lblPrezime.Text = "Презиме:*";
            lblPrezime.Font = new Font("Arial", 10, FontStyle.Regular);
            lblPrezime.Size = new Size(150, 25);
            lblPrezime.Location = new Point(50, 140);
            this.Controls.Add(lblPrezime);

            txtPrezime = new TextBox();
            txtPrezime.Size = new Size(300, 25);
            txtPrezime.Location = new Point(50, 165);
            txtPrezime.Font = new Font("Arial", 10, FontStyle.Regular);
            this.Controls.Add(txtPrezime);

            Label lblZvanje = new Label();
            lblZvanje.Text = "Звање:*";
            lblZvanje.Font = new Font("Arial", 10, FontStyle.Regular);
            lblZvanje.Size = new Size(150, 25);
            lblZvanje.Location = new Point(50, 200);
            this.Controls.Add(lblZvanje);

            cmbZvanje = new ComboBox();
            cmbZvanje.Size = new Size(300, 25);
            cmbZvanje.Location = new Point(50, 225);
            cmbZvanje.Font = new Font("Arial", 10, FontStyle.Regular);
            cmbZvanje.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(cmbZvanje);

            Label lblSmena = new Label();
            lblSmena.Text = "Смена:*";
            lblSmena.Font = new Font("Arial", 10, FontStyle.Regular);
            lblSmena.Size = new Size(150, 25);
            lblSmena.Location = new Point(50, 260);
            this.Controls.Add(lblSmena);

            cmbSmena = new ComboBox();
            cmbSmena.Size = new Size(300, 25);
            cmbSmena.Location = new Point(50, 285);
            cmbSmena.Font = new Font("Arial", 10, FontStyle.Regular);
            cmbSmena.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(cmbSmena);

            // Status label
            lblStatus = new Label();
            lblStatus.Text = "Поља означена са * су обавезна";
            lblStatus.Font = new Font("Arial", 8, FontStyle.Italic);
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Size = new Size(300, 20);
            lblStatus.Location = new Point(50, 320);
            this.Controls.Add(lblStatus);

            // Dugme za čuvanje
            btnSacuvaj = new Button();
            btnSacuvaj.Text = "💾 Сачувај радника";
            btnSacuvaj.Size = new Size(150, 35);
            btnSacuvaj.Location = new Point(80, 350);
            btnSacuvaj.BackColor = Color.DarkGreen;
            btnSacuvaj.ForeColor = Color.White;
            btnSacuvaj.Font = new Font("Arial", 10, FontStyle.Bold);
            btnSacuvaj.Click += BtnSacuvaj_Click;
            this.Controls.Add(btnSacuvaj);

            // Dugme za odustajanje
            btnOdustani = new Button();
            btnOdustani.Text = "❌ Одустани";
            btnOdustani.Size = new Size(150, 35);
            btnOdustani.Location = new Point(240, 350);
            btnOdustani.BackColor = Color.Gray;
            btnOdustani.ForeColor = Color.White;
            btnOdustani.Font = new Font("Arial", 10, FontStyle.Bold);
            btnOdustani.Click += BtnOdustani_Click;
            this.Controls.Add(btnOdustani);

            this.Size = new Size(450, 430);
        }

        private void UcitajZvanja()
        {
            try
            {
                cmbZvanje.Items.Clear();
                cmbZvanje.Items.Add("Изаберите звање...");
                var zvanja = bazaService.UzmiSvaZvanja();

                foreach (var zvanje in zvanja)
                {
                    cmbZvanje.Items.Add(zvanje.Naziv);
                }

                cmbZvanje.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању звања: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UcitajSmane()
        {
            try
            {
                cmbSmena.Items.Clear();
                cmbZvanje.Items.Add("Изаберите смену...");

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Smena FROM SmenaBoe ORDER BY Smena";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbSmena.Items.Add(reader["Smena"].ToString());
                        }
                    }
                }

                cmbZvanje.SelectedIndex = 0;
            }
            catch (Exception)
            {
                // Fallback smene
                cmbSmena.Items.AddRange(new string[] { "I смена", "II смена", "III смена", "IV смена" });
                if (cmbSmena.Items.Count > 0)
                {
                    cmbSmena.SelectedIndex = 0;
                }
            }
        }

        private void BtnSacuvaj_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIme.Text) || string.IsNullOrWhiteSpace(txtPrezime.Text))
            {
                MessageBox.Show("Име и презиме су обавезна поља!", "Упозорење",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbZvanje.SelectedItem == null)
            {
                MessageBox.Show("Морате изабрати звање!", "Упозорење",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbSmena.SelectedItem == null)
            {
                MessageBox.Show("Морате изабрати смену!", "Упозорење",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Radnik noviRadnik = new Radnik
                {
                    Ime = txtIme.Text.Trim(),
                    Prezime = txtPrezime.Text.Trim(),
                    Zvanje = cmbZvanje.SelectedItem.ToString(),
                    Smena = cmbSmena.SelectedItem.ToString()
                };

                bool uspesno = bazaService.DodajRadnika(noviRadnik);

                if (uspesno)
                {
                    MessageBox.Show($"✅ Радник {noviRadnik.Ime} {noviRadnik.Prezime} успешно додат!", "Успешно",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Resetuj polja
                    txtIme.Clear();
                    txtPrezime.Clear();
                    cmbZvanje.SelectedIndex = 0;
                    cmbSmena.SelectedIndex = 0;
                    txtIme.Focus();
                }
                else
                {
                    MessageBox.Show("❌ Грешка при додавању радника!", "Грешка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при додавању радника: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOdustani_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}