﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace RadnoVreme
{
    public partial class IzmenaRadnikaForm : Form
    {
        private ComboBox cmbRadnici;
        private TextBox txtIme;
        private TextBox txtPrezime;
        private ComboBox cmbZvanje;
        private ComboBox cmbSmena;
        private CheckBox chkAktivan;
        private Button btnSacuvaj;
        private Button btnObrisi;
        private Button btnOdustani;
        private Label lblStatus;

        private string connectionString = "Data Source=MILANDJ\\SQLEXPRESS;Initial Catalog=RadnoVreme;Integrated Security=True;Encrypt=False";
        private List<Radnik> radnici;
        private Radnik trenutniRadnik;
        private string ulogaKorisnika;
        private string smenaKorisnika;
        private BazaService bazaService;
        private int izvrsioKorisnikId; // ★★★ DODATO: ID korisnika koji vrši izmenu ★★★

        // ★★★ DODAT: Novi konstruktor sa korisnik ID ★★★
        public IzmenaRadnikaForm(string uloga, string smena, int korisnikId = 0)
        {
            this.ulogaKorisnika = uloga;
            this.smenaKorisnika = smena;
            this.izvrsioKorisnikId = korisnikId; // ★★★ Sačuvaj ID korisnika ★★★
            this.bazaService = new BazaService();

            // BITNO: Prvo pozovite InitializeComponent() 
            InitializeComponent();
            KreirajKontrole();
            UcitajRadnike();
            UcitajZvanja();
            UcitajSmane();
            ResetujPolja();
        }

        private void KreirajKontrole()
        {
            this.Text = $"Измена и брисање радника ({ulogaKorisnika})";
            this.Size = new Size(500, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Label za naslov
            Label lblNaslov = new Label();
            lblNaslov.Text = "✏️ Измена и брисање радника";
            lblNaslov.Font = new Font("Arial", 16, FontStyle.Bold);
            lblNaslov.ForeColor = Color.DarkBlue;
            lblNaslov.Size = new Size(400, 40);
            lblNaslov.Location = new Point(50, 20);
            lblNaslov.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblNaslov);

            // Label koji prikazuje ograničenja
            Label lblOgranicenje = new Label();
            if (ulogaKorisnika == "Администратор")
            {
                lblOgranicenje.Text = "🔓 Администратор: Може мењати све раднике";
                lblOgranicenje.ForeColor = Color.Green;
            }
            else
            {
                lblOgranicenje.Text = $"🔐 Ограничен преглед: Можете видети само {smenaKorisnika}";
                lblOgranicenje.ForeColor = Color.Orange;
            }
            lblOgranicenje.Font = new Font("Arial", 9, FontStyle.Regular);
            lblOgranicenje.Size = new Size(400, 20);
            lblOgranicenje.Location = new Point(50, 60);
            lblOgranicenje.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblOgranicenje);

            // ★★★ DODAT: Prikaz korisnika koji vrši izmenu ★★★
            if (izvrsioKorisnikId > 0)
            {
                Label lblIzvrsilac = new Label();
                lblIzvrsilac.Text = $"👤 Измене врши корисник ИД: {izvrsioKorisnikId}";
                lblIzvrsilac.Font = new Font("Arial", 8, FontStyle.Italic);
                lblIzvrsilac.ForeColor = Color.DarkGray;
                lblIzvrsilac.Size = new Size(300, 15);
                lblIzvrsilac.Location = new Point(100, 85);
                lblIzvrsilac.TextAlign = ContentAlignment.MiddleCenter;
                this.Controls.Add(lblIzvrsilac);
            }

            // ComboBox za odabir radnika
            Label lblIzaberiRadnika = new Label();
            lblIzaberiRadnika.Text = "Изаберите радника:";
            lblIzaberiRadnika.Font = new Font("Arial", 10, FontStyle.Regular);
            lblIzaberiRadnika.Size = new Size(150, 25);
            lblIzaberiRadnika.Location = new Point(50, 110);
            this.Controls.Add(lblIzaberiRadnika);

            cmbRadnici = new ComboBox();
            cmbRadnici.Size = new Size(350, 25);
            cmbRadnici.Location = new Point(50, 135);
            cmbRadnici.Font = new Font("Arial", 10, FontStyle.Regular);
            cmbRadnici.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRadnici.SelectedIndexChanged += CmbRadnici_SelectedIndexChanged;
            this.Controls.Add(cmbRadnici);

            // Polja za unos
            Label lblIme = new Label();
            lblIme.Text = "Име:";
            lblIme.Font = new Font("Arial", 10, FontStyle.Regular);
            lblIme.Size = new Size(150, 25);
            lblIme.Location = new Point(50, 180);
            this.Controls.Add(lblIme);

            txtIme = new TextBox();
            txtIme.Size = new Size(350, 25);
            txtIme.Location = new Point(50, 205);
            txtIme.Font = new Font("Arial", 10, FontStyle.Regular);
            this.Controls.Add(txtIme);

            Label lblPrezime = new Label();
            lblPrezime.Text = "Презиме:";
            lblPrezime.Font = new Font("Arial", 10, FontStyle.Regular);
            lblPrezime.Size = new Size(150, 25);
            lblPrezime.Location = new Point(50, 240);
            this.Controls.Add(lblPrezime);

            txtPrezime = new TextBox();
            txtPrezime.Size = new Size(350, 25);
            txtPrezime.Location = new Point(50, 265);
            txtPrezime.Font = new Font("Arial", 10, FontStyle.Regular);
            this.Controls.Add(txtPrezime);

            // ComboBox za zvanje
            Label lblZvanje = new Label();
            lblZvanje.Text = "Звање:";
            lblZvanje.Font = new Font("Arial", 10, FontStyle.Regular);
            lblZvanje.Size = new Size(150, 25);
            lblZvanje.Location = new Point(50, 300);
            this.Controls.Add(lblZvanje);

            cmbZvanje = new ComboBox();
            cmbZvanje.Size = new Size(350, 25);
            cmbZvanje.Location = new Point(50, 325);
            cmbZvanje.Font = new Font("Arial", 10, FontStyle.Regular);
            cmbZvanje.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(cmbZvanje);

            Label lblSmena = new Label();
            lblSmena.Text = "Смена:";
            lblSmena.Font = new Font("Arial", 10, FontStyle.Regular);
            lblSmena.Size = new Size(150, 25);
            lblSmena.Location = new Point(50, 360);
            this.Controls.Add(lblSmena);

            cmbSmena = new ComboBox();
            cmbSmena.Size = new Size(350, 25);
            cmbSmena.Location = new Point(50, 385);
            cmbSmena.Font = new Font("Arial", 10, FontStyle.Regular);
            cmbSmena.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSmena.Enabled = true;
            this.Controls.Add(cmbSmena);

            chkAktivan = new CheckBox();
            chkAktivan.Text = "Активан радник";
            chkAktivan.Font = new Font("Arial", 10, FontStyle.Regular);
            chkAktivan.Size = new Size(150, 25);
            chkAktivan.Location = new Point(50, 420);
            chkAktivan.Checked = true;
            this.Controls.Add(chkAktivan);

            // Status label
            lblStatus = new Label();
            lblStatus.Text = "";
            lblStatus.Font = new Font("Arial", 9, FontStyle.Regular);
            lblStatus.Size = new Size(400, 25);
            lblStatus.Location = new Point(50, 450);
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            this.Controls.Add(lblStatus);

            // Dugme za čuvanje
            btnSacuvaj = new Button();
            btnSacuvaj.Text = "💾 Sačuvaj izmene";
            btnSacuvaj.Size = new Size(140, 35);
            btnSacuvaj.Location = new Point(50, 480);
            btnSacuvaj.BackColor = Color.DarkGreen;
            btnSacuvaj.ForeColor = Color.White;
            btnSacuvaj.Font = new Font("Arial", 10, FontStyle.Bold);
            btnSacuvaj.Click += BtnSacuvaj_Click;
            this.Controls.Add(btnSacuvaj);

            // Dugme za brisanje
            btnObrisi = new Button();
            btnObrisi.Text = "🗑️ Обриши радника";
            btnObrisi.Size = new Size(140, 35);
            btnObrisi.Location = new Point(200, 480);
            btnObrisi.BackColor = Color.DarkRed;
            btnObrisi.ForeColor = Color.White;
            btnObrisi.Font = new Font("Arial", 10, FontStyle.Bold);
            btnObrisi.Click += BtnObrisi_Click;
            this.Controls.Add(btnObrisi);

            // Dugme za odustajanje
            btnOdustani = new Button();
            btnOdustani.Text = "❌ Одустани";
            btnOdustani.Size = new Size(120, 35);
            btnOdustani.Location = new Point(350, 480);
            btnOdustani.BackColor = Color.Gray;
            btnOdustani.ForeColor = Color.White;
            btnOdustani.Font = new Font("Arial", 10, FontStyle.Bold);
            btnOdustani.Click += BtnOdustani_Click;
            this.Controls.Add(btnOdustani);

            OmoguciKontrole();
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању звања: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UcitajRadnike()
        {
            try
            {
                radnici = new List<Radnik>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query;
                    SqlCommand cmd;

                    if (ulogaKorisnika == "Администратор")
                    {
                        query = @"SELECT Id, Ime, Prezime, Zvanje, Smena, Aktivan, DatumKreiranja 
                       FROM Radnici 
                       ORDER BY Prezime, Ime";
                        cmd = new SqlCommand(query, conn);
                    }
                    else
                    {
                        query = @"SELECT Id, Ime, Prezime, Zvanje, Smena, Aktivan, DatumKreiranja 
                       FROM Radnici 
                       WHERE Smena = @Smena AND Aktivan = 1
                       ORDER BY Prezime, Ime";
                        cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Smena", smenaKorisnika);
                    }

                    using (cmd)
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            radnici.Add(new Radnik
                            {
                                Id = (int)reader["Id"],
                                Ime = reader["Ime"].ToString(),
                                Prezime = reader["Prezime"].ToString(),
                                Zvanje = reader["Zvanje"]?.ToString(),
                                Smena = reader["Smena"].ToString(),
                                Aktivan = (bool)reader["Aktivan"],
                                DatumKreiranja = (DateTime)reader["DatumKreiranja"]
                            });
                        }
                    }
                }

                cmbRadnici.Items.Clear();
                cmbRadnici.Items.Add("(Изаберите радника...)");

                foreach (var radnik in radnici)
                {
                    string status = radnik.Aktivan ? "🟢" : "🔴";
                    cmbRadnici.Items.Add($"{status} {radnik.Prezime} {radnik.Ime} ({radnik.Smena})");
                }

                if (cmbRadnici.Items.Count > 0)
                {
                    cmbRadnici.SelectedIndex = 0;
                    lblStatus.Text = $"Пронађено {radnici.Count} радника";
                    lblStatus.ForeColor = Color.Blue;
                }
                else
                {
                    lblStatus.Text = "Нема радника за приказ";
                    lblStatus.ForeColor = Color.Orange;
                    ResetujPolja();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању радника: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnemoguciKontrole()
        {
            txtIme.Enabled = false;
            txtPrezime.Enabled = false;
            cmbZvanje.Enabled = false;
            chkAktivan.Enabled = false;
            btnSacuvaj.Enabled = false;
            btnObrisi.Enabled = false;
        }

        private void UcitajSmane()
        {
            try
            {
                cmbSmena.Items.Clear();
                cmbSmena.Items.Add("Изаберите смену...");

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
            }
            catch (Exception)
            {
                // Fallback smene
                cmbSmena.Items.AddRange(new string[] { "Изаберите смену...", "I смена", "II смена", "III смена", "IV смена" });
            }
        }

        private void CmbRadnici_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbRadnici.SelectedIndex == 0)
            {
                ResetujPolja();
                return;
            }
            if (cmbRadnici.SelectedIndex > 0 && cmbRadnici.SelectedIndex - 1 < radnici.Count)
            {
                trenutniRadnik = radnici[cmbRadnici.SelectedIndex - 1];
                PopuniPodatke();
            }
            else
            {
                ResetujPolja();
            }
        }

        private void ResetujPolja()
        {
            txtIme.Clear();
            txtPrezime.Clear();
            cmbZvanje.SelectedIndex = -1;
            cmbSmena.SelectedIndex = -1;
            chkAktivan.Checked = true;

            OnemoguciKontrole();
            lblStatus.Text = "Изаберите радника за измену или брисање.";
            lblStatus.ForeColor = Color.Black;
        }

        private void PopuniPodatke()
        {
            if (trenutniRadnik != null)
            {
                txtIme.Text = trenutniRadnik.Ime;
                txtPrezime.Text = trenutniRadnik.Prezime;

                // Postavi zvanje
                if (!string.IsNullOrEmpty(trenutniRadnik.Zvanje))
                {
                    cmbZvanje.SelectedItem = trenutniRadnik.Zvanje;
                }

                cmbSmena.SelectedItem = trenutniRadnik.Smena;
                chkAktivan.Checked = trenutniRadnik.Aktivan;

                lblStatus.Text = $"Учитани подаци за радника ИД: {trenutniRadnik.Id}";
                lblStatus.ForeColor = Color.Blue;
                OmoguciKontrole();
            }
        }

        private void OmoguciKontrole()
        {
            txtIme.Enabled = true;
            txtPrezime.Enabled = true;
            cmbZvanje.Enabled = true;
            chkAktivan.Enabled = true;
            btnSacuvaj.Enabled = true;
            btnObrisi.Enabled = true;

            // Smenu mogu menjati samo administratori
            cmbSmena.Enabled = (ulogaKorisnika == "Администратор");
        }

        private void BtnSacuvaj_Click(object sender, EventArgs e)
        {
            if (trenutniRadnik == null)
            {
                MessageBox.Show("Морате изабрати радника!", "Упозорење",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtIme.Text) || string.IsNullOrWhiteSpace(txtPrezime.Text))
            {
                MessageBox.Show("Име и презиме су обавезна поља!", "Упозорење",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // ★★★ PRVO UZMI STARE PODATKE ZA LOG ★★★
                string stariPodaci = $"Име: {trenutniRadnik.Ime}, " +
                                   $"Презиме: {trenutniRadnik.Prezime}, " +
                                   $"Звање: {trenutniRadnik.Zvanje ?? "Није постављено"}, " +
                                   $"Активан: {trenutniRadnik.Aktivan}, " +
                                   $"Смена: {trenutniRadnik.Smena}";

                // ★★★ FORMIRAJ NOVE PODATKE ★★★
                string noviPodaci = $"Име: {txtIme.Text.Trim()}, " +
                                  $"Презиме: {txtPrezime.Text.Trim()}, " +
                                  $"Звање: {cmbZvanje.SelectedItem?.ToString() ?? "Није постављено"}, " +
                                  $"Активан: {chkAktivan.Checked}, " +
                                  $"Смена: {trenutniRadnik.Smena}"; // Smena se ne menja osim za admina

                // ★★★ DODAJ LOGIKU ZA PROMENU SMENE AKO JE ADMIN ★★★
                if (ulogaKorisnika == "Администратор" && cmbSmena.SelectedItem != null)
                {
                    string novaSmena = cmbSmena.SelectedItem.ToString();
                    noviPodaci = $"Име: {txtIme.Text.Trim()}, " +
                               $"Презиме: {txtPrezime.Text.Trim()}, " +
                               $"Звање: {cmbZvanje.SelectedItem?.ToString() ?? "Није постављено"}, " +
                               $"Активан: {chkAktivan.Checked}, " +
                               $"Смена: {novaSmena}";
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // ★★★ KREIRAJ QUERY SA PROMENOM SMENE ZA ADMINA ★★★
                    string query;
                    if (ulogaKorisnika == "Администратор" && cmbSmena.SelectedItem != null)
                    {
                        query = @"UPDATE Radnici 
                               SET Ime = @Ime, Prezime = @Prezime, Zvanje = @Zvanje, 
                                   Aktivan = @Aktivan, Smena = @Smena
                               WHERE Id = @Id";
                    }
                    else
                    {
                        query = @"UPDATE Radnici 
                               SET Ime = @Ime, Prezime = @Prezime, Zvanje = @Zvanje, 
                                   Aktivan = @Aktivan
                               WHERE Id = @Id";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", trenutniRadnik.Id);
                        cmd.Parameters.AddWithValue("@Ime", txtIme.Text.Trim());
                        cmd.Parameters.AddWithValue("@Prezime", txtPrezime.Text.Trim());
                        cmd.Parameters.AddWithValue("@Zvanje", cmbZvanje.SelectedItem?.ToString() ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Aktivan", chkAktivan.Checked);

                        // Dodaj smenu samo ako je admin
                        if (ulogaKorisnika == "Администратор" && cmbSmena.SelectedItem != null)
                        {
                            cmd.Parameters.AddWithValue("@Smena", cmbSmena.SelectedItem.ToString());
                        }

                        int affectedRows = cmd.ExecuteNonQuery();

                        if (affectedRows > 0)
                        {
                            // ★★★ LOGUJ PROMENU U RadnikPromeneLog ★★★
                            DateTime datumPromene = DateTime.Now;

                            // Za radnike logujemo kao "Измена података"
                            bazaService.LogujPromenuRadnika(
                                trenutniRadnik.Id,           // RadnikId
                                datumPromene,                // Datum (danasnji datum za log)
                                stariPodaci,                 // StariStatus (stari podaci)
                                noviPodaci,                  // NoviStatus (novi podaci)
                                null,                        // StariSati (nema sati za radnike)
                                null,                        // NoviSati (nema sati za radnike)
                                null,                        // StariMinute (nema minuta)
                                null,                        // NoviMinute (nema minuta)
                                false,                       // JeNocnaSmena (nije nocna smena)
                                "Измена података",           // TipPromene
                                izvrsioKorisnikId,           // IzvrsioKorisnikId
                                "Измена основних података о раднику"  // Komentar
                            );

                            lblStatus.Text = "✅ Подаци су успешно сачувани и логовани!";
                            lblStatus.ForeColor = Color.Green;

                            // Osveži listu radnika
                            UcitajRadnike();

                            // Prikaži poruku o logovanju
                            System.Diagnostics.Debug.WriteLine($"📝 Логована промена радника ИД {trenutniRadnik.Id}");
                            System.Diagnostics.Debug.WriteLine($"   Старо: {stariPodaci}");
                            System.Diagnostics.Debug.WriteLine($"   Ново: {noviPodaci}");
                        }
                        else
                        {
                            lblStatus.Text = "❌ Грешка при чувању података!";
                            lblStatus.ForeColor = Color.Red;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при чувању података: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnObrisi_Click(object sender, EventArgs e)
        {
            if (trenutniRadnik == null)
            {
                MessageBox.Show("Морате изабрати радника!", "Упозорење",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string imePrezime = $"{trenutniRadnik.Prezime} {trenutniRadnik.Ime}";

            DialogResult result = MessageBox.Show(
                $"Да ли сте СИГУРНИ да желите да обришете радника:\n\n{imePrezime}\n\n" +
                "Ова акција ће означити радника као неактивног.\nРадник ће и даље бити у бази али се неће приказивати у листама атктивних радника.",
                "Потврда брисања",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // ★★★ UZMI PODATKE PRE BRISANJA ZA LOG ★★★
                    string podaciRadnika = $"Име: {trenutniRadnik.Ime}, " +
                                         $"Презиме: {trenutniRadnik.Prezime}, " +
                                         $"Звање: {trenutniRadnik.Zvanje ?? "Није постављено"}, " +
                                         $"Смена: {trenutniRadnik.Smena}, " +
                                         $"Активан: {trenutniRadnik.Aktivan}";

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        // SOFT DELETE - samo postavimo Aktivan na false
                        string query = "UPDATE Radnici SET Aktivan = 0 WHERE Id = @Id";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", trenutniRadnik.Id);
                            int affectedRows = cmd.ExecuteNonQuery();

                            if (affectedRows > 0)
                            {
                                // ★★★ LOGUJ BRISANJE U RadnikPromeneLog ★★★
                                DateTime datumPromene = DateTime.Now;

                                bazaService.LogujPromenuRadnika(
                                    trenutniRadnik.Id,           // RadnikId
                                    datumPromene,                // Datum (danasnji datum za log)
                                    podaciRadnika,              // StariStatus (podaci pre brisanja)
                                    null,                       // NoviStatus (null za brisanje)
                                    null,                       // StariSati
                                    null,                       // NoviSati
                                    null,                       // StariMinute
                                    null,                       // NoviMinute
                                    false,                      // JeNocnaSmena
                                    "Брисање",                  // TipPromene
                                    izvrsioKorisnikId,          // IzvrsioKorisnikId
                                    "Софт брисање радника"      // Komentar
                                );

                                lblStatus.Text = $"✅ Радник {imePrezime} успешно обрисан и логовано!";
                                lblStatus.ForeColor = Color.Green;

                                // Osveži listu radnika
                                UcitajRadnike();

                                // Resetuj polja
                                trenutniRadnik = null;
                                txtIme.Clear();
                                txtPrezime.Clear();
                                cmbZvanje.SelectedIndex = -1;
                                cmbSmena.SelectedIndex = -1;
                                chkAktivan.Checked = true;

                                // Prikaži poruku o logovanju
                                System.Diagnostics.Debug.WriteLine($"🗑️ Логовано брисање радника ИД {trenutniRadnik?.Id}");
                            }
                            else
                            {
                                lblStatus.Text = "❌ Грешка при брисању радника!";
                                lblStatus.ForeColor = Color.Red;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Грешка при брисању радника: {ex.Message}", "Грешка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnOdustani_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ★★★ DODAT: Property za pristup ID-ju korisnika ★★★
        public int IzvrsioKorisnikId
        {
            get { return izvrsioKorisnikId; }
            set { izvrsioKorisnikId = value; }
        }
    }
}
