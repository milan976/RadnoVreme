using System;
using System.Drawing;
using System.Windows.Forms;

namespace RadnoVreme
{
    public partial class DodavanjeKorisnikaForm : Form
    {
        private TextBox txtKorisnickoIme;
        private TextBox txtLozinka;
        private TextBox txtPotvrdaLozinke;
        private TextBox txtIme;
        private TextBox txtPrezime;
        private TextBox txtEmail;
        private ComboBox cmbUloga;
        private ComboBox cmbSmena;
        private CheckBox chkAktivan;
        private Button btnSacuvaj;
        private Button btnOcisti;
        private Button btnNazad;

        private BazaService bazaService;

        public DodavanjeKorisnikaForm()
        {
            this.bazaService = new BazaService();
            this.InitializeComponent();
            this.KreirajFormu();
        }

        private void KreirajFormu()
        {
            // Postavke forme
            this.Text = "Додавање новог корисника";
            this.Size = new Size(540, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            // Glavni panel
            Panel mainPanel = new Panel();
            mainPanel.Size = new Size(480, 470);
            mainPanel.Location = new Point(20, 20);
            mainPanel.BackColor = Color.White;
            mainPanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(mainPanel);

            // Naslov
            Label lblNaslov = new Label();
            lblNaslov.Text = "👑 ДОДАВАЊЕ КОРИСНИКА";
            lblNaslov.Font = new Font("Arial", 14, FontStyle.Bold);
            lblNaslov.ForeColor = Color.DarkBlue;
            lblNaslov.Size = new Size(300, 30);
            lblNaslov.Location = new Point(70, 20);
            lblNaslov.TextAlign = ContentAlignment.MiddleCenter;
            mainPanel.Controls.Add(lblNaslov);

            int yPozicija = 70;
            int labelSirina = 120;
            int textBoxSirina = 250;

            // Korisničko ime
            KreirajLabelIPolje(mainPanel, "Корисничко име:", ref yPozicija, labelSirina, textBoxSirina, out txtKorisnickoIme);

            // Lozinka
            KreirajLabelIPolje(mainPanel, "Лозинка:", ref yPozicija, labelSirina, textBoxSirina, out txtLozinka);
            txtLozinka.PasswordChar = '•';

            // Potvrda lozinke
            KreirajLabelIPolje(mainPanel, "Потврда лозинке:", ref yPozicija, labelSirina, textBoxSirina, out txtPotvrdaLozinke);
            txtPotvrdaLozinke.PasswordChar = '•';

            // Ime
            KreirajLabelIPolje(mainPanel, "Име:", ref yPozicija, labelSirina, textBoxSirina, out txtIme);

            // Prezime
            KreirajLabelIPolje(mainPanel, "Презиме:", ref yPozicija, labelSirina, textBoxSirina, out txtPrezime);

            // Email
            KreirajLabelIPolje(mainPanel, "Емаил:", ref yPozicija, labelSirina, textBoxSirina, out txtEmail);

            // Uloga
            Label lblUloga = new Label();
            lblUloga.Text = "Улога:";
            lblUloga.Font = new Font("Arial", 9, FontStyle.Bold);
            lblUloga.ForeColor = Color.DarkSlateGray;
            lblUloga.Location = new Point(50, yPozicija);
            lblUloga.Size = new Size(labelSirina, 20);
            mainPanel.Controls.Add(lblUloga);

            cmbUloga = new ComboBox();
            cmbUloga.Location = new Point(180, yPozicija);
            cmbUloga.Size = new Size(textBoxSirina, 25);
            cmbUloga.Font = new Font("Arial", 10, FontStyle.Regular);
            cmbUloga.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbUloga.Items.Add("Администратор");
            cmbUloga.Items.Add("Korisnik");
            cmbUloga.SelectedIndex = 1; // Podrazumevano "korisnik"
            mainPanel.Controls.Add(cmbUloga);

            yPozicija += 35;

            // Smena (samo za korisnike)
            Label lblSmena = new Label();
            lblSmena.Text = "Смена:";
            lblSmena.Font = new Font("Arial", 9, FontStyle.Bold);
            lblSmena.ForeColor = Color.DarkSlateGray;
            lblSmena.Location = new Point(50, yPozicija);
            lblSmena.Size = new Size(labelSirina, 20);
            mainPanel.Controls.Add(lblSmena);

            cmbSmena = new ComboBox();
            cmbSmena.Location = new Point(180, yPozicija);
            cmbSmena.Size = new Size(textBoxSirina, 25);
            cmbSmena.Font = new Font("Arial", 10, FontStyle.Regular);
            cmbSmena.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSmena.Items.Add("I смена");
            cmbSmena.Items.Add("II смена");
            cmbSmena.Items.Add("III смена");
            cmbSmena.Items.Add("IV смена");
            cmbSmena.SelectedIndex = 0;
            mainPanel.Controls.Add(cmbSmena);

            yPozicija += 35;

            // Aktivan checkbox
            chkAktivan = new CheckBox();
            chkAktivan.Text = "Корисник је активан";
            chkAktivan.Font = new Font("Arial", 9, FontStyle.Bold);
            chkAktivan.ForeColor = Color.DarkSlateGray;
            chkAktivan.Location = new Point(180, yPozicija);
            chkAktivan.Size = new Size(200, 25);
            chkAktivan.Checked = true;
            mainPanel.Controls.Add(chkAktivan);

            yPozicija += 40;

            // Dugme za čuvanje
            btnSacuvaj = new Button();
            btnSacuvaj.Text = "💾 САЧУВАЈ";
            btnSacuvaj.Location = new Point(80, yPozicija);
            btnSacuvaj.Size = new Size(120, 35);
            btnSacuvaj.BackColor = Color.LimeGreen;
            btnSacuvaj.ForeColor = Color.White;
            btnSacuvaj.Font = new Font("Arial", 10, FontStyle.Bold);
            btnSacuvaj.FlatStyle = FlatStyle.Flat;
            btnSacuvaj.FlatAppearance.BorderSize = 0;
            btnSacuvaj.Cursor = Cursors.Hand;
            btnSacuvaj.Click += BtnSacuvaj_Click;
            mainPanel.Controls.Add(btnSacuvaj);

            // Dugme za čišćenje
            btnOcisti = new Button();
            btnOcisti.Text = "🗑️ ОЧИСТИ";
            btnOcisti.Location = new Point(220, yPozicija);
            btnOcisti.Size = new Size(80, 35);
            btnOcisti.BackColor = Color.Orange;
            btnOcisti.ForeColor = Color.White;
            btnOcisti.Font = new Font("Arial", 9, FontStyle.Bold);
            btnOcisti.FlatStyle = FlatStyle.Flat;
            btnOcisti.FlatAppearance.BorderSize = 0;
            btnOcisti.Cursor = Cursors.Hand;
            btnOcisti.Click += BtnOcisti_Click;
            mainPanel.Controls.Add(btnOcisti);

            // Dugme za nazad
            btnNazad = new Button();
            btnNazad.Text = "↩️ НАЗАД";
            btnNazad.Location = new Point(320, yPozicija);
            btnNazad.Size = new Size(80, 35);
            btnNazad.BackColor = Color.Gray;
            btnNazad.ForeColor = Color.White;
            btnNazad.Font = new Font("Arial", 9, FontStyle.Bold);
            btnNazad.FlatStyle = FlatStyle.Flat;
            btnNazad.FlatAppearance.BorderSize = 0;
            btnNazad.Cursor = Cursors.Hand;
            btnNazad.Click += (s, e) => { this.Close(); };
            mainPanel.Controls.Add(btnNazad);

            // Event za promenu uloge - ako je admin, onemogući smenu
            cmbUloga.SelectedIndexChanged += CmbUloga_SelectedIndexChanged;
            CmbUloga_SelectedIndexChanged(null, null); // Postavi početno stanje
        }

        private void KreirajLabelIPolje(Panel panel, string labelText, ref int yPozicija, int labelSirina, int textBoxSirina, out TextBox textBox)
        {
            Label label = new Label();
            label.Text = labelText;
            label.Font = new Font("Arial", 9, FontStyle.Bold);
            label.ForeColor = Color.DarkSlateGray;
            label.Location = new Point(50, yPozicija);
            label.Size = new Size(labelSirina, 20);
            panel.Controls.Add(label);

            textBox = new TextBox();
            textBox.Location = new Point(180, yPozicija);
            textBox.Size = new Size(textBoxSirina, 25);
            textBox.Font = new Font("Arial", 10, FontStyle.Regular);
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = Color.WhiteSmoke;
            panel.Controls.Add(textBox);

            yPozicija += 35;
        }

        private void CmbUloga_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Ako je izabrana uloga "admin", onemogući i resetuj smenu
            if (cmbUloga.SelectedItem?.ToString() == "Администратор")
            {
                cmbSmena.Enabled = false;
                cmbSmena.SelectedIndex = -1;
            }
            else
            {
                cmbSmena.Enabled = true;
                if (cmbSmena.SelectedIndex == -1)
                    cmbSmena.SelectedIndex = 0;
            }
        }

        private void BtnSacuvaj_Click(object sender, EventArgs e)
        {
            if (!ValidirajFormu())
                return;

            try
            {
                // Kreiraj novog korisnika
                Korisnik noviKorisnik = new Korisnik
                {
                    KorisnickoIme = txtKorisnickoIme.Text.Trim(),
                    Lozinka = txtLozinka.Text,
                    Uloga = cmbUloga.SelectedItem.ToString(),
                    Ime = txtIme.Text.Trim(),
                    Prezime = txtPrezime.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                    Smena = cmbUloga.SelectedItem.ToString() == "Администратор" ? null : cmbSmena.SelectedItem.ToString(),
                    Aktivan = chkAktivan.Checked
                };

                // Sačuvaj u bazu
                bool uspesno = bazaService.DodajKorisnika(noviKorisnik, 1);

                if (uspesno)
                {
                    MessageBox.Show($"Корисник '{noviKorisnik.KorisnickoIme}' је успешно додат!", "Успешно",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OcistiFormu();
                }
                else
                {
                    MessageBox.Show("Дошло је до грешке при чувању корисника!", "Грешка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при чувању корисника: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidirajFormu()
        {
            // Provera obaveznih polja
            if (string.IsNullOrWhiteSpace(txtKorisnickoIme.Text))
            {
                MessageBox.Show("Унесите корисничко име!", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtKorisnickoIme.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLozinka.Text))
            {
                MessageBox.Show("Унесите лозинку!", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLozinka.Focus();
                return false;
            }

            if (txtLozinka.Text != txtPotvrdaLozinke.Text)
            {
                MessageBox.Show("Лозинке се не поклапају!", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPotvrdaLozinke.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtIme.Text))
            {
                MessageBox.Show("Унесите име!", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIme.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPrezime.Text))
            {
                MessageBox.Show("Унесите презиме!", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrezime.Focus();
                return false;
            }

            if (cmbUloga.SelectedItem == null)
            {
                MessageBox.Show("Изаберите улогу!", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Provera za korisnike - moraju imati smenu
            if (cmbUloga.SelectedItem.ToString() == "Korisnik" && cmbSmena.SelectedItem == null)
            {
                MessageBox.Show("Изаберите смену за корисника!", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void BtnOcisti_Click(object sender, EventArgs e)
        {
            OcistiFormu();
        }

        private void OcistiFormu()
        {
            txtKorisnickoIme.Text = "";
            txtLozinka.Text = "";
            txtPotvrdaLozinke.Text = "";
            txtIme.Text = "";
            txtPrezime.Text = "";
            txtEmail.Text = "";
            cmbUloga.SelectedIndex = 1; // Podrazumevano "Korisnik"
            cmbSmena.SelectedIndex = 0;
            chkAktivan.Checked = true;
            txtKorisnickoIme.Focus();
        }
    }
}