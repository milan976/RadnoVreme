using System;
using System.Drawing;
using System.Windows.Forms;

namespace RadnoVreme
{
    public partial class LoginForm : Form
    {
        private TextBox txtKorisnickoIme;
        private TextBox txtLozinka;
        private Button btnPrijava;

        public LoginForm()
        {
            this.InitializeComponent();
            this.KreirajLoginKontrole();
        }

        private void InitializeComponent()
        {
            // Ova metoda može ostati prazna jer ručno kreiramo kontrole
        }

        private void KreirajLoginKontrole()
        {
            // Postavke za login formu
            this.Text = "Радно Време - Пријава";
            this.Size = new Size(430, 410);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            // Panel za sadržaj
            Panel mainPanel = new Panel();
            mainPanel.Size = new Size(350, 330);
            mainPanel.Location = new Point(30, 30);
            mainPanel.BackColor = Color.White;
            mainPanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(mainPanel);

            // Logo/Ikona
            Label lblLogo = new Label();
            lblLogo.Text = "⏰";
            lblLogo.Font = new Font("Arial", 32, FontStyle.Bold);
            lblLogo.Size = new Size(80, 80);
            lblLogo.Location = new Point(135, 20);
            lblLogo.TextAlign = ContentAlignment.MiddleCenter;
            mainPanel.Controls.Add(lblLogo);

            // Naslov
            Label lblNaslov = new Label();
            lblNaslov.Text = "РАДНО ВРЕМЕ";
            lblNaslov.Font = new Font("Arial", 16, FontStyle.Bold);
            lblNaslov.ForeColor = Color.DarkBlue;
            lblNaslov.Size = new Size(200, 30);
            lblNaslov.Location = new Point(75, 100);
            lblNaslov.TextAlign = ContentAlignment.MiddleCenter;
            mainPanel.Controls.Add(lblNaslov);

            // Podnaslov
            Label lblPodnaslov = new Label();
            lblPodnaslov.Text = "Систем за евиденцију радног времена";
            lblPodnaslov.Font = new Font("Arial", 9, FontStyle.Italic);
            lblPodnaslov.ForeColor = Color.Gray;
            lblPodnaslov.Size = new Size(250, 20);
            lblPodnaslov.Location = new Point(50, 130);
            lblPodnaslov.TextAlign = ContentAlignment.MiddleCenter;
            mainPanel.Controls.Add(lblPodnaslov);

            // Korisničko ime
            Label lblKorisnickoIme = new Label();
            lblKorisnickoIme.Text = "Корисничко име:";
            lblKorisnickoIme.Font = new Font("Arial", 9, FontStyle.Bold);
            lblKorisnickoIme.Location = new Point(50, 170);
            lblKorisnickoIme.Size = new Size(100, 20);
            mainPanel.Controls.Add(lblKorisnickoIme);

            txtKorisnickoIme = new TextBox();
            txtKorisnickoIme.Location = new Point(50, 190);
            txtKorisnickoIme.Size = new Size(250, 25);
            txtKorisnickoIme.Font = new Font("Arial", 10, FontStyle.Regular);
            txtKorisnickoIme.BorderStyle = BorderStyle.FixedSingle;
            txtKorisnickoIme.BackColor = Color.WhiteSmoke;
            mainPanel.Controls.Add(txtKorisnickoIme);

            // Lozinka
            Label lblLozinka = new Label();
            lblLozinka.Text = "Лозинка:";
            lblLozinka.Font = new Font("Arial", 9, FontStyle.Bold);
            lblLozinka.Location = new Point(50, 225);
            lblLozinka.Size = new Size(100, 20);
            mainPanel.Controls.Add(lblLozinka);

            txtLozinka = new TextBox();
            txtLozinka.Location = new Point(50, 245);
            txtLozinka.Size = new Size(250, 25);
            txtLozinka.Font = new Font("Arial", 10, FontStyle.Regular);
            txtLozinka.BorderStyle = BorderStyle.FixedSingle;
            txtLozinka.BackColor = Color.WhiteSmoke;
            txtLozinka.PasswordChar = '•';
            mainPanel.Controls.Add(txtLozinka);

            // Dugme za prijavu
            btnPrijava = new Button();
            btnPrijava.Text = "ПРИЈАВА";
            btnPrijava.Location = new Point(100, 290);
            btnPrijava.Size = new Size(150, 35);
            btnPrijava.BackColor = Color.DodgerBlue;
            btnPrijava.ForeColor = Color.White;
            btnPrijava.Font = new Font("Arial", 10, FontStyle.Bold);
            btnPrijava.FlatStyle = FlatStyle.Flat;
            btnPrijava.FlatAppearance.BorderSize = 0;
            btnPrijava.Cursor = Cursors.Hand;
            btnPrijava.Click += BtnPrijava_Click;
            mainPanel.Controls.Add(btnPrijava);

            // Enter key za prijavu
            this.AcceptButton = btnPrijava;
        }

        private void BtnPrijava_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtKorisnickoIme.Text) ||
                string.IsNullOrWhiteSpace(txtLozinka.Text))
            {
                MessageBox.Show("Унесите корисничко име и лозинку!", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            BazaService bazaService = new BazaService();
            Korisnik korisnik = bazaService.PrijaviKorisnika(txtKorisnickoIme.Text, txtLozinka.Text);

            if (korisnik != null)
            {
                // PROSLEDJUJEMO I SMENU
                MainForm mainForm = new MainForm(korisnik.Uloga, $"{korisnik.Ime} {korisnik.Prezime}", korisnik.Smena);
                mainForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Погрешно корисничко име или лозинка!", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string AutentifikujKorisnika(string korisnickoIme, string lozinka)
        {
            // Umesto hardkodovanih podataka, koristite BazaService
            BazaService bazaService = new BazaService();
            Korisnik korisnik = bazaService.PrijaviKorisnika(korisnickoIme, lozinka);

            if (korisnik != null)
            {
                return korisnik.Uloga;
            }

            return null;
        }
    }
}