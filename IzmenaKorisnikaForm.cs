using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace RadnoVreme
{
    public partial class IzmenaKorisnikaForm : Form
    {
        private ListBox listBoxKorisnici;
        private TextBox txtKorisnickoIme;
        private TextBox txtLozinka;
        private TextBox txtIme;
        private TextBox txtPrezime;
        private TextBox txtEmail;
        private ComboBox cmbUloga;
        private ComboBox cmbSmena;
        private CheckBox chkAktivan;
        private Button btnSacuvajIzmene;
        private Button btnObrisi;
        private Button btnOtkazi;
        private Label lblNaslov;
        private Panel panelDetalji;

        private BazaService bazaService;
        private List<Korisnik> sviKorisnici;
        private Korisnik trenutniKorisnik;

        public IzmenaKorisnikaForm()
        {
            this.bazaService = new BazaService();
            this.InitializeComponent();
            this.KreirajFormu();
            this.UcitajKorisnike();
        }

        private void KreirajFormu()
        {
            // Postavke forme
            this.Text = "Измена и брисање корисника";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            // Naslov
            lblNaslov = new Label();
            lblNaslov.Text = "✏️ ИЗМЕНА И БРИСАЊЕ КОРИСНИКА";
            lblNaslov.Font = new Font("Arial", 14, FontStyle.Bold);
            lblNaslov.ForeColor = Color.DarkBlue;
            lblNaslov.Size = new Size(400, 30);
            lblNaslov.Location = new Point(250, 10);
            lblNaslov.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblNaslov);

            // ListBox za korisnike (levo)
            Label lblLista = new Label();
            lblLista.Text = "Листа корисника:";
            lblLista.Font = new Font("Arial", 10, FontStyle.Bold);
            lblLista.ForeColor = Color.DarkSlateGray;
            lblLista.Location = new Point(30, 60);
            lblLista.Size = new Size(150, 20);
            this.Controls.Add(lblLista);

            listBoxKorisnici = new ListBox();
            listBoxKorisnici.Location = new Point(30, 85);
            listBoxKorisnici.Size = new Size(300, 300);
            listBoxKorisnici.Font = new Font("Arial", 10, FontStyle.Regular);
            listBoxKorisnici.BorderStyle = BorderStyle.FixedSingle;
            listBoxKorisnici.SelectionMode = SelectionMode.One;
            listBoxKorisnici.SelectedIndexChanged += ListBoxKorisnici_SelectedIndexChanged;
            this.Controls.Add(listBoxKorisnici);

            // Panel za detalje (desno)
            panelDetalji = new Panel();
            panelDetalji.Size = new Size(450, 400);
            panelDetalji.Location = new Point(350, 60);
            panelDetalji.BackColor = Color.WhiteSmoke;
            panelDetalji.BorderStyle = BorderStyle.FixedSingle;
            panelDetalji.Enabled = false; // Onemogući dok se ne izabere korisnik
            this.Controls.Add(panelDetalji);

            // Naslov panela
            Label lblDetalji = new Label();
            lblDetalji.Text = "Детаљи корисника";
            lblDetalji.Font = new Font("Arial", 12, FontStyle.Bold);
            lblDetalji.ForeColor = Color.DarkBlue;
            lblDetalji.Size = new Size(200, 25);
            lblDetalji.Location = new Point(125, 15);
            lblDetalji.TextAlign = ContentAlignment.MiddleCenter;
            panelDetalji.Controls.Add(lblDetalji);

            int yPozicija = 60;
            int labelSirina = 100;
            int textBoxSirina = 200;

            // Korisničko ime
            KreirajLabelIPolje(panelDetalji, "Корисничко име:", ref yPozicija, labelSirina, textBoxSirina, out txtKorisnickoIme);
            
            // Lozinka
            KreirajLabelIPolje(panelDetalji, "Нова лозинка:", ref yPozicija, labelSirina, textBoxSirina, out txtLozinka);
            txtLozinka.PasswordChar = '•';

            //Tooltip za korisnicno ime
            ToolTip toolTipKorisnickoIme = new ToolTip();
            toolTipKorisnickoIme.SetToolTip(txtKorisnickoIme, "Можете променити корисничко име.");

            //Tooltip za lozinku
            ToolTip toolTipLozinka = new ToolTip();
            toolTipLozinka.SetToolTip(txtLozinka, "Остави празно ако не желите мењати лозинку.");
            
            // Ime
            KreirajLabelIPolje(panelDetalji, "Име:", ref yPozicija, labelSirina, textBoxSirina, out txtIme);

            // Prezime
            KreirajLabelIPolje(panelDetalji, "Презиме:", ref yPozicija, labelSirina, textBoxSirina, out txtPrezime);

            // Email
            KreirajLabelIPolje(panelDetalji, "Емаил:", ref yPozicija, labelSirina, textBoxSirina, out txtEmail);

            // Uloga
            Label lblUloga = new Label();
            lblUloga.Text = "Улога:";
            lblUloga.Font = new Font("Arial", 9, FontStyle.Bold);
            lblUloga.ForeColor = Color.DarkSlateGray;
            lblUloga.Location = new Point(50, yPozicija);
            lblUloga.Size = new Size(labelSirina, 20);
            panelDetalji.Controls.Add(lblUloga);

            cmbUloga = new ComboBox();
            cmbUloga.Location = new Point(160, yPozicija);
            cmbUloga.Size = new Size(textBoxSirina, 25);
            cmbUloga.Font = new Font("Arial", 10, FontStyle.Regular);
            cmbUloga.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbUloga.Items.Add("Администратор");
            cmbUloga.Items.Add("Корисник");
            panelDetalji.Controls.Add(cmbUloga);

            yPozicija += 35;

            // Smena
            Label lblSmena = new Label();
            lblSmena.Text = "Смена:";
            lblSmena.Font = new Font("Arial", 9, FontStyle.Bold);
            lblSmena.ForeColor = Color.DarkSlateGray;
            lblSmena.Location = new Point(50, yPozicija);
            lblSmena.Size = new Size(labelSirina, 20);
            panelDetalji.Controls.Add(lblSmena);

            cmbSmena = new ComboBox();
            cmbSmena.Location = new Point(160, yPozicija);
            cmbSmena.Size = new Size(textBoxSirina, 25);
            cmbSmena.Font = new Font("Arial", 10, FontStyle.Regular);
            cmbSmena.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSmena.Items.Add("I смена");
            cmbSmena.Items.Add("II смена");
            cmbSmena.Items.Add("III смена");
            cmbSmena.Items.Add("IV смена");
            panelDetalji.Controls.Add(cmbSmena);

            yPozicija += 35;

            // Aktivan checkbox
            chkAktivan = new CheckBox();
            chkAktivan.Text = "Корисник је активан";
            chkAktivan.Font = new Font("Arial", 9, FontStyle.Bold);
            chkAktivan.ForeColor = Color.DarkSlateGray;
            chkAktivan.Location = new Point(160, yPozicija);
            chkAktivan.Size = new Size(200, 25);
            panelDetalji.Controls.Add(chkAktivan);

            yPozicija += 40;

            // Dugme za čuvanje izmena
            btnSacuvajIzmene = new Button();
            btnSacuvajIzmene.Text = "💾 САЧУВАЈ ИЗМЕНЕ";
            btnSacuvajIzmene.Location = new Point(50, yPozicija);
            btnSacuvajIzmene.Size = new Size(150, 35);
            btnSacuvajIzmene.BackColor = Color.LimeGreen;
            btnSacuvajIzmene.ForeColor = Color.White;
            btnSacuvajIzmene.Font = new Font("Arial", 9, FontStyle.Bold);
            btnSacuvajIzmene.FlatStyle = FlatStyle.Flat;
            btnSacuvajIzmene.FlatAppearance.BorderSize = 0;
            btnSacuvajIzmene.Cursor = Cursors.Hand;
            btnSacuvajIzmene.Click += BtnSacuvajIzmene_Click;
            panelDetalji.Controls.Add(btnSacuvajIzmene);

            // Dugme za brisanje
            btnObrisi = new Button();
            btnObrisi.Text = "🗑️ ОБРИСИ КОРИСНИКА";
            btnObrisi.Location = new Point(210, yPozicija);
            btnObrisi.Size = new Size(150, 35);
            btnObrisi.BackColor = Color.Red;
            btnObrisi.ForeColor = Color.White;
            btnObrisi.Font = new Font("Arial", 9, FontStyle.Bold);
            btnObrisi.FlatStyle = FlatStyle.Flat;
            btnObrisi.FlatAppearance.BorderSize = 0;
            btnObrisi.Cursor = Cursors.Hand;
            btnObrisi.Click += BtnObrisi_Click;
            panelDetalji.Controls.Add(btnObrisi);

            yPozicija += 50;

            // Dugme za otkazivanje
            btnOtkazi = new Button();
            btnOtkazi.Text = "❌ ЗАТВОРИ";
            btnOtkazi.Location = new Point(130, yPozicija);
            btnOtkazi.Size = new Size(150, 35);
            btnOtkazi.BackColor = Color.Gray;
            btnOtkazi.ForeColor = Color.White;
            btnOtkazi.Font = new Font("Arial", 9, FontStyle.Bold);
            btnOtkazi.FlatStyle = FlatStyle.Flat;
            btnOtkazi.FlatAppearance.BorderSize = 0;
            btnOtkazi.Cursor = Cursors.Hand;
            btnOtkazi.Click += (s, e) => { this.Close(); };
            this.Controls.Add(btnOtkazi);

            // Event za promenu uloge
            cmbUloga.SelectedIndexChanged += CmbUloga_SelectedIndexChanged;
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
            textBox.Location = new Point(160, yPozicija);
            textBox.Size = new Size(textBoxSirina, 25);
            textBox.Font = new Font("Arial", 10, FontStyle.Regular);
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = Color.White;
            panel.Controls.Add(textBox);

            yPozicija += 35;
        }

        private void UcitajKorisnike()
        {
            try
            {
                sviKorisnici = bazaService.UzmiSveKorisnike();
                listBoxKorisnici.Items.Clear();

                foreach (Korisnik korisnik in sviKorisnici)
                {
                    string status = korisnik.Aktivan ? "🟢" : "🔴";
                    string smenaInfo = korisnik.Smena != null ? $" - {korisnik.Smena}" : " - ADMIN";
                    string promeneInfo = " 📋"; // Ikona za pregled promena

                    listBoxKorisnici.Items.Add($"{status} {korisnik.KorisnickoIme} ({korisnik.Ime} {korisnik.Prezime}){smenaInfo}{promeneInfo}");
                }

                if (listBoxKorisnici.Items.Count > 0)
                    listBoxKorisnici.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању корисника: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ListBoxKorisnici_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxKorisnici.SelectedIndex >= 0 && listBoxKorisnici.SelectedIndex < sviKorisnici.Count)
            {
                trenutniKorisnik = sviKorisnici[listBoxKorisnici.SelectedIndex];
                PopuniDetaljeKorisnika();
                panelDetalji.Enabled = true;
            }
        }

        private void PopuniDetaljeKorisnika()
        {
            if (trenutniKorisnik == null) return;

            txtKorisnickoIme.Text = trenutniKorisnik.KorisnickoIme;
            txtLozinka.Text = ""; // Lozinka se ne prikazuje
            txtIme.Text = trenutniKorisnik.Ime;
            txtPrezime.Text = trenutniKorisnik.Prezime;
            txtEmail.Text = trenutniKorisnik.Email ?? "";

            // Postavi ulogu
            cmbUloga.SelectedItem = trenutniKorisnik.Uloga;

            // Postavi smenu
            if (!string.IsNullOrEmpty(trenutniKorisnik.Smena))
            {
                cmbSmena.SelectedItem = trenutniKorisnik.Smena;
            }
            else
            {
                cmbSmena.SelectedIndex = -1;
            }

            chkAktivan.Checked = trenutniKorisnik.Aktivan;

            // Onemogući brisanje samog sebe
            btnObrisi.Enabled = (trenutniKorisnik.Uloga != "Администратор" || trenutniKorisnik.KorisnickoIme != "Администратор");
        }

        private void CmbUloga_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbUloga.SelectedItem?.ToString() == "Администратор")
            {
                cmbSmena.Enabled = false;
                cmbSmena.SelectedIndex = -1;
            }
            else
            {
                cmbSmena.Enabled = true;
                if (cmbSmena.SelectedIndex == -1 && cmbSmena.Items.Count > 0)
                    cmbSmena.SelectedIndex = 0;
            }
        }

        private void BtnSacuvajIzmene_Click(object sender, EventArgs e)
        {
            if (trenutniKorisnik == null)
            {
                MessageBox.Show("Изаберите корисника за смену!", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidirajFormu())
                return;

            try
            {
                // Ažuriraj korisnika
                string staroKorisnickoIme = trenutniKorisnik.KorisnickoIme;
                trenutniKorisnik.KorisnickoIme = txtKorisnickoIme.Text.Trim();
                trenutniKorisnik.Ime = txtIme.Text.Trim();
                trenutniKorisnik.Prezime = txtPrezime.Text.Trim();
                trenutniKorisnik.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
                trenutniKorisnik.Uloga = cmbUloga.SelectedItem.ToString();
                trenutniKorisnik.Smena = cmbUloga.SelectedItem.ToString() == "Администратор" ? null : cmbSmena.SelectedItem?.ToString();
                trenutniKorisnik.Aktivan = chkAktivan.Checked;

                // Ako je uneta nova lozinka, ažuriraj je
                if (!string.IsNullOrWhiteSpace(txtLozinka.Text))
                {
                    trenutniKorisnik.Lozinka = txtLozinka.Text;
                }

                // Proveri da li je korisničko ime promenjeno
                if (trenutniKorisnik.KorisnickoIme != staroKorisnickoIme)
                {
                    // Koristi bazaService za proveru jedinstvenosti
                    if (bazaService.KorisnickoImePostoji(trenutniKorisnik.KorisnickoIme))
                    {
                        MessageBox.Show($"Корисничко име '{trenutniKorisnik.KorisnickoIme}' већ постоји!", "Грешка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        trenutniKorisnik.KorisnickoIme = staroKorisnickoIme;
                        txtKorisnickoIme.Text = staroKorisnickoIme;
                        txtKorisnickoIme.Focus();
                        return;
                    }
                }

                // Koristi bazaService za ažuriranje
                bool uspesno = bazaService.AzurirajKorisnika(trenutniKorisnik, 1);

                if (uspesno)
                {
                    MessageBox.Show($"Корисник '{trenutniKorisnik.KorisnickoIme}' је успешно ажуриран!", "Грешка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UcitajKorisnike();
                }
                else
                {
                    MessageBox.Show("Дошло је до грешке приликом ажурирања корисника!", "Грешка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при ажурирању корисника: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnObrisi_Click(object sender, EventArgs e)
        {
            if (trenutniKorisnik == null)
            {
                MessageBox.Show("Изаберите кориника за брисање!", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Provera da li pokušavamo da obrišemo samog sebe
            if (trenutniKorisnik.Uloga == "Администратор" && trenutniKorisnik.KorisnickoIme == "Администратор")
            {
                MessageBox.Show("Не можете обрисати главног Администратора!", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Да ли сте сигурни да желите да обришете корисника '{trenutniKorisnik.KorisnickoIme}'?\n\n" +
                "Ова акција не може бити поништена!",
                "Потврда брисања",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                try
                {
                    bool uspesno = bazaService.ObrisiKorisnika(trenutniKorisnik.Id, 1);

                    if (uspesno)
                    {
                        MessageBox.Show($"Корисник '{trenutniKorisnik.KorisnickoIme}' је успешно обрисан!", "Успешно",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                        UcitajKorisnike(); // Osveži listu
                        panelDetalji.Enabled = false;
                    }
                    else
                    {
                        MessageBox.Show("Дошло је до грешке при брисању корисника!", "Грешка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Грешка при брисању корисника: {ex.Message}", "Грешка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool ValidirajFormu()
        {
            if (string.IsNullOrWhiteSpace(txtKorisnickoIme.Text))
            {
                MessageBox.Show("Унесите корисничко име!", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtKorisnickoIme.Focus();
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

            if (cmbUloga.SelectedItem.ToString() == "korisnik" && cmbSmena.SelectedItem == null)
            {
                MessageBox.Show("Изаберите смену за корисника!", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}