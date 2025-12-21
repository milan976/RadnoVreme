using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RadnoVreme
{
    public partial class MainForm : Form
    {
        private MenuStrip menuStrip;
        private ToolStripMenuItem pregledRadnikaItem;
        private ToolStripMenuItem pregledSmeneItem;
        private ToolStripMenuItem izlazItem;
        private ToolStripMenuItem pregledPromenaRadnikaItem;
        private Label lblSadrzaj;
        private Panel mainPanel;
        private string uloga;
        private string imePrezime;
        private string korisnikSmena;
        private string smena;
        private string connectionString = "Data Source=MILANDJ\\SQLEXPRESS;Initial Catalog=RadnoVreme;Integrated Security=True;Encrypt=False";

        public MainForm(string uloga, string imePrezime, string korisnikSmena)
        {
            // ISPRAVLJEN KOD ZA CIRILICU
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("sr-Cyrl-RS");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("sr-Cyrl-RS");

            this.uloga = uloga;
            this.imePrezime = imePrezime;
            this.korisnikSmena = korisnikSmena;
            this.InitializeComponent();
            this.KreirajKontrole();
        }

        private void InitializeComponent()
        {
            // Ova metoda može ostati prazna jer ručno kreiramo kontrole
        }

        private void KreirajKontrole()
        {
            // Postavke glavne forme
            this.Text = $"Радно Време - Главни мени ({uloga.Normalize()})";
            this.Size = new Size(910, 630);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += MainForm_FormClosing;
            this.BackColor = Color.White;

            // Kreiranje menija
            KreirajMenij();

            // Glavni panel za sadržaj
            mainPanel = new Panel();
            mainPanel.Size = new Size(880, 530);
            mainPanel.Location = new Point(10, 60);
            mainPanel.BackColor = Color.White;
            mainPanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(mainPanel);

            // Logo i naslov
            Label lblLogo = new Label();
            lblLogo.Text = "⏰ Радно време";
            lblLogo.Font = new Font("Arial", 20, FontStyle.Bold);
            lblLogo.ForeColor = Color.DarkBlue;
            lblLogo.Size = new Size(300, 40);
            lblLogo.Location = new Point(300, 50);
            lblLogo.TextAlign = ContentAlignment.MiddleCenter;
            mainPanel.Controls.Add(lblLogo);

            // Label za pozdravnu poruku
            lblSadrzaj = new Label();
            lblSadrzaj.Text = $"Добродошли {imePrezime} у апликацију Радно време!\n\n" +
                            "Систем за евиденцију радног времена и управљањем радних сати радника.\n\n" +
                            "Изаберите опцију из менија за наставак рада.\n\n" + 
                            "Пребаците језик на тастатури на Српски ћирилица за унос података!!!";
            lblSadrzaj.Font = new Font("Arial", 11, FontStyle.Regular);
            lblSadrzaj.ForeColor = Color.DarkSlateGray;
            lblSadrzaj.Size = new Size(600, 150);
            lblSadrzaj.Location = new Point(150, 120);
            lblSadrzaj.TextAlign = ContentAlignment.MiddleCenter;
            mainPanel.Controls.Add(lblSadrzaj);

            // Informacije na dnu
            Label lblInfo = new Label();
            lblInfo.Text = "© Милан Ђорђевић 2025 РадноВреме App | Верзија 1.0";
            lblInfo.Font = new Font("Arial", 8, FontStyle.Italic);
            lblInfo.ForeColor = Color.Gray;
            lblInfo.Size = new Size(300, 20);
            lblInfo.Location = new Point(300, 450);
            lblInfo.TextAlign = ContentAlignment.MiddleCenter;
            mainPanel.Controls.Add(lblInfo);
        }

        private void KreirajMenij()
        {
            // MenuStrip
            menuStrip = new MenuStrip();
            menuStrip.BackColor = Color.DarkBlue;
            menuStrip.ForeColor = Color.White;
            menuStrip.Font = new Font("Arial", 10, FontStyle.Regular);

            ToolStripMenuItem radniciMenu = new ToolStripMenuItem("👥 Радници");
            radniciMenu.ForeColor = Color.White;

            ToolStripMenuItem unosRadnikaItem = new ToolStripMenuItem("➕ Унос радника");
            ToolStripMenuItem izmenaRadnikaItem = new ToolStripMenuItem("✏️ Измена и 🗑️ Брисање радника");

            unosRadnikaItem.Click += UnosRadnikaItem_Click;
            izmenaRadnikaItem.Click += IzmenaRadnikaItem_Click;

            radniciMenu.DropDownItems.Add(unosRadnikaItem);
            radniciMenu.DropDownItems.Add(izmenaRadnikaItem);

            // Osnovne stavke menija za sve korisnike
            pregledRadnikaItem = new ToolStripMenuItem("👥 Преглед радника");
            pregledSmeneItem = new ToolStripMenuItem("📊 Преглед смене");
            pregledPromenaRadnikaItem = new ToolStripMenuItem("📋 Преглед промена радника");

            // Dodaj event handlere
            pregledRadnikaItem.Click += PregledRadnikaItem_Click;
            pregledSmeneItem.Click += PregledSmeneItem_Click;
            pregledPromenaRadnikaItem.Click += PregledPromenaRadnikaItem_Click;

            // Dodaj osnovne stavke u meni
            menuStrip.Items.Add(radniciMenu);
            menuStrip.Items.Add(pregledRadnikaItem);
            menuStrip.Items.Add(pregledSmeneItem);
            menuStrip.Items.Add(pregledPromenaRadnikaItem);

            // ADMIN ONLY: Svi administratorski meniji grupisani u jedan padajući meni
            if (uloga == "Администратор")
            {
                ToolStripMenuItem adminMenu = new ToolStripMenuItem("👑 Администратор");
                adminMenu.ForeColor = Color.White;
                adminMenu.BackColor = Color.Purple;

                // Kreiranje stavki za administratorski meni
                ToolStripMenuItem korisniciPodmeni = new ToolStripMenuItem("👥 Управљање корисницима");

                // Stavke podmenija za korisnike
                ToolStripMenuItem dodajKorisnikaItem = new ToolStripMenuItem("➕ Додај корисника");
                ToolStripMenuItem izmeniKorisnikaItem = new ToolStripMenuItem("✏️ Измени корисника");
                ToolStripMenuItem pregledKorisnikaItem = new ToolStripMenuItem("👀 Преглед корисника");
                ToolStripMenuItem pregledPromenaItem = new ToolStripMenuItem("📋 Преглед промена");

                // Event handleri za podmeni
                dodajKorisnikaItem.Click += DodajKorisnikaItem_Click;
                izmeniKorisnikaItem.Click += IzmeniKorisnikaItem_Click;
                pregledKorisnikaItem.Click += PregledKorisnikaItem_Click;
                pregledPromenaItem.Click += PregledPromenaItem_Click;

                // Dodaj stavke u podmeni za korisnike
                korisniciPodmeni.DropDownItems.Add(dodajKorisnikaItem);
                korisniciPodmeni.DropDownItems.Add(izmeniKorisnikaItem);
                korisniciPodmeni.DropDownItems.Add(pregledKorisnikaItem);
                korisniciPodmeni.DropDownItems.Add(new ToolStripSeparator());
                korisniciPodmeni.DropDownItems.Add(pregledPromenaItem);

                // Ostale administratorske opcije
                ToolStripMenuItem rasporedSmenaItem = new ToolStripMenuItem("📅 Распоред смена");
                rasporedSmenaItem.Click += (s, e) =>
                {
                    RasporedSmenaForm rasporedForm = new RasporedSmenaForm();
                    rasporedForm.ShowDialog();
                };

                ToolStripMenuItem pocetniDatumSmenaItem = new ToolStripMenuItem("📅 Почетни датум смене");
                pocetniDatumSmenaItem.ForeColor = Color.White;
                pocetniDatumSmenaItem.BackColor = Color.DarkGreen;
                pocetniDatumSmenaItem.Click += PocetniDatumSmenaItem_Click;

                // DUGME ZA BRISANJE RASPOREDA - DODATO
                ToolStripMenuItem obrisiRasporedItem = new ToolStripMenuItem("🗑️ Обриши комплетан распоред");
                obrisiRasporedItem.ForeColor = Color.White;
                obrisiRasporedItem.BackColor = Color.DarkRed;
                obrisiRasporedItem.Click += ObrisiRasporedItem_Click;

                // Dodaj sve stavke u administratorski meni
                adminMenu.DropDownItems.Add(korisniciPodmeni);
                adminMenu.DropDownItems.Add(new ToolStripSeparator());
                adminMenu.DropDownItems.Add(rasporedSmenaItem);
                adminMenu.DropDownItems.Add(pocetniDatumSmenaItem);
                adminMenu.DropDownItems.Add(new ToolStripSeparator());
                adminMenu.DropDownItems.Add(obrisiRasporedItem); // DODATO

                // Dodaj administratorski meni u glavni meni
                menuStrip.Items.Add(adminMenu);
            }
            // Izlaz za sve korisnike
            menuStrip.Items.Add(new ToolStripSeparator());

            // DODAJ OVDE OVAJ KOD: DUGME ZA ODJAVU
            ToolStripMenuItem odjavaItem = new ToolStripMenuItem("🚪 Одјави се");
            odjavaItem.ForeColor = Color.White;
            odjavaItem.BackColor = Color.Orange;
            odjavaItem.Click += OdjavaItem_Click;
            menuStrip.Items.Add(odjavaItem);

            izlazItem = new ToolStripMenuItem("🚪 Излаз");
            izlazItem.ForeColor = Color.White;
            izlazItem.BackColor = Color.DarkRed;
            izlazItem.Click += IzlazItem_Click;
            menuStrip.Items.Add(izlazItem);

            // Postavi meni na formu
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;
        }

        private void DodajKorisnikaItem_Click(object sender, EventArgs e)
        {
            DodavanjeKorisnikaForm dodavanjeForm = new DodavanjeKorisnikaForm();
            dodavanjeForm.ShowDialog();
        }

        private void IzmeniKorisnikaItem_Click(object sender, EventArgs e)
        {
            IzmenaKorisnikaForm izmenaForm = new IzmenaKorisnikaForm();
            izmenaForm.ShowDialog();
        }

        private void PregledKorisnikaItem_Click(object sender, EventArgs e)
        {
            Label lblPoruka = new Label();
            lblPoruka.Text = "Ова функција још није завршена\nна корисничким налозима.\n\nИмплементација у току...";
        }

        private void PregledPromenaItem_Click(object sender, EventArgs e)
        {
            // Otvaranje postojeće PregledPromenaForm
            PregledPromenaForm pregledForm = new PregledPromenaForm();
            pregledForm.ShowDialog();
        }

        private void PregledPromenaRadnikaItem_Click(object sender, EventArgs e)
        {
            // Ovi podaci treba da budu dostupni iz vaše glavne forme
            string trenutnaSmena = null;
            bool jeAdmin = false;

            // Proverite da li imate ove podatke u glavnoj formi
            // Primer:
            if (uloga == "Администратор") // uloga je iz vašeg prijavljenog korisnika
            {
                jeAdmin = true;
                trenutnaSmena = null; // Adminu ne treba smena
            }
            else
            {
                jeAdmin = false;
                trenutnaSmena = smena; // smena je iz vašeg prijavljenog korisnika
            }

            PregledPromenaRadnikaForm form = new PregledPromenaRadnikaForm(trenutnaSmena, jeAdmin);
            form.ShowDialog();
        }
        
        private void UnosRadnikaItem_Click(object sender, EventArgs e)
        {
            UnosRadnikaForm unosForm = new UnosRadnikaForm();
            unosForm.ShowDialog();
        }

        private void IzmenaRadnikaItem_Click(object sender, EventArgs e)
        {
            IzmenaRadnikaForm izmenaForm = new IzmenaRadnikaForm(uloga, korisnikSmena);
            izmenaForm.ShowDialog();
        }
        
        private void PregledRadnikaItem_Click(object sender, EventArgs e)
        {
            PregledRadnikaForm pregledForm = new PregledRadnikaForm(korisnikSmena);
            pregledForm.ShowDialog();
        }

        private void PregledSmeneItem_Click(object sender, EventArgs e)
        {
            PregledSmeneForm pregledSmeneForm = new PregledSmeneForm(korisnikSmena);
            pregledSmeneForm.ShowDialog();
        }

        private void DodavanjeKorisnikaItem_Click(object sender, EventArgs e)
        {
            DodavanjeKorisnikaForm dodavanjeForm = new DodavanjeKorisnikaForm();
            dodavanjeForm.ShowDialog();
        }

        private void PocetniDatumSmenaItem_Click(object sender, EventArgs e)
        {
            PocetniDatumSmenaForm pocetniDatumForm = new PocetniDatumSmenaForm();
            pocetniDatumForm.ShowDialog();
        }

        private void ObrisiRasporedItem_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "⚠️ УПОЗОРЕЊЕ: Да ли сте СИГУРНИ да желите да ОБРИШЕТЕ КОМПЛЕТАН генерисани распоред смена?\n\n" +
                    "Ова акција ће:\n" +
                    "• Трајно обрисати све генерисане распореде\n" +
                    "• Ресетовати бројач генерисања\n\n" +
                    "Ова акција се не може поништити!",
                    "ПОТВРДА БРИСАЊА РАСПОРЕДА",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (result == DialogResult.Yes)
                {
                    bool uspesnoBrisanje = ObrisiKompletanRasporedIzBaze();
                    bool uspesnoReset = ResetujBrojacIzBaze();

                    if (uspesnoBrisanje || uspesnoReset)
                    {
                        string poruka = "";

                        if (uspesnoBrisanje && uspesnoReset)
                        {
                            poruka = "✅ Комплетан генерисани распоред смене је успешно обрисан!\n✅ Бројач је ресетован!";
                        }
                        else if (uspesnoBrisanje)
                        {
                            poruka = "✅ омплетан генерисани распоред смене је успешно обрисан!\nℹ️ Бројач није ресетован (можда не постоји).";
                        }
                        else if (uspesnoReset)
                        {
                            poruka = "ℹ️ Није било распореда за брисање.\n✅ Бројач је ресетован!";
                        }

                        MessageBox.Show(poruka, "Операција завршена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            "❌ Дошло је до грешке приликом брисања распореда и ресетовања бројача.",
                            "Грешка при операцији",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Грешка приликом брисања распореда: {ex.Message}", "Грешка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ObrisiKompletanRasporedIzBaze()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM RasporedSmena"; // Prilagodite naziv tabele
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        int affectedRows = cmd.ExecuteNonQuery();
                        return affectedRows >= 0; // Vraća true čak i ako je 0 (nema šta da se briše)
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при брисању из базе: {ex.Message}", "Грешка базе", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool ResetujBrojacIzBaze()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Prilagodite SQL upit prema vašoj bazi:
                    string query = "UPDATE Brojaci SET TrenutniBroj = 0 WHERE NazivBrojaca = 'Raspored'";
                    // ILI: "DELETE FROM Brojaci WHERE Tip = 'Raspored'"
                    // ILI: "UPDATE Konfiguracija SET Vrednost = '0' WHERE Kljuc = 'BrojacRasporeda'"

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        int affectedRows = cmd.ExecuteNonQuery();
                        return affectedRows >= 0; // Vraća true čak i ako je 0 (brojač možda ne postoji)
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при ресетовању бројача: {ex.Message}", "Грешка базе", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void OdjavaItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Да ли сте сигурни да желите да се одјавите?\n\n" +
                "Бићете враћени на почетну пријавну форму.",
                "Потврда одјаве",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.OK;

                this.Hide();
                LoginForm loginForm = new LoginForm();
                loginForm.Show();
                this.Close();
            }
        }

        private void IzlazItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Да ли сте сигурни да желите да изађете из апликације?",
                "Потврда излаза",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Ako je DialogResult već postavljen (za odjavu), preskoči
            if (this.DialogResult == DialogResult.OK)
            {
                return;
            }

            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult result = MessageBox.Show(
                    "Да ли сте сигурни да желите да изађете из апликације?",
                    "Потврда излаза",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}