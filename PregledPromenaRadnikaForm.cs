using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace RadnoVreme
{
    public partial class PregledPromenaRadnikaForm : Form
    {
        private ComboBox cmbRadnici;
        private DateTimePicker dtpOd;
        private DateTimePicker dtpDo;
        private DataGridView dataGridPromene;
        private Button btnUcitajPromene;
        private Button btnZatvori;
        private Label lblNaslov;
        private Label lblSmenaFilter;
        private ComboBox cmbSmenaFilter; // Dodajemo za admina da filtrira po smeni
        private BazaService bazaService;
        private List<Radnik> sviRadnici;
        private List<Radnik> filtriraniRadnici;
        private string korisnikSmena;
        private bool jeAdmin;

        // Konstruktor
        public PregledPromenaRadnikaForm(string smenaKorisnika = null, bool admin = false)
        {
            this.korisnikSmena = smenaKorisnika;
            this.jeAdmin = admin;
            this.bazaService = new BazaService();
            this.InitializeComponent();
            this.KreirajFormu();

            if (jeAdmin)
            {
                this.UcitajSveRadnike();
                this.UcitajSmenaFilter(); // Za admina učitaj sve smene za filter
            }
            else
            {
                this.UcitajRadnikeIzSvojeSmene(); // Za korisnika učitaj samo svoju smenu
            }
        }

        private void KreirajFormu()
        {
            // Postavke forme
            this.Text = "Преглед промена радника";
            this.Size = new Size(1000, 750); // Povećana visina
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            // Naslov
            lblNaslov = new Label();

            if (jeAdmin)
            {
                lblNaslov.Text = "📋 Преглед промена свих радника (Администратор)";
            }
            else if (!string.IsNullOrEmpty(korisnikSmena))
            {
                lblNaslov.Text = $"📋 Преглед промена радника - {korisnikSmena}";
            }
            else
            {
                lblNaslov.Text = "📋 Преглед промена радника";
            }

            lblNaslov.Font = new Font("Arial", 16, FontStyle.Bold);
            lblNaslov.ForeColor = Color.DarkBlue;
            lblNaslov.Size = new Size(600, 40);
            lblNaslov.Location = new Point(200, 20);
            lblNaslov.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblNaslov);

            // ★★★ FILTER PO SMENI (Samo za admina) ★★★
            if (jeAdmin)
            {
                lblSmenaFilter = new Label();
                lblSmenaFilter.Text = "Филтер по смени:";
                lblSmenaFilter.Font = new Font("Arial", 10, FontStyle.Bold);
                lblSmenaFilter.ForeColor = Color.DarkSlateGray;
                lblSmenaFilter.Size = new Size(120, 25);
                lblSmenaFilter.Location = new Point(50, 80);
                this.Controls.Add(lblSmenaFilter);

                cmbSmenaFilter = new ComboBox();
                cmbSmenaFilter.Location = new Point(180, 80);
                cmbSmenaFilter.Size = new Size(200, 25);
                cmbSmenaFilter.Font = new Font("Arial", 10, FontStyle.Regular);
                cmbSmenaFilter.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbSmenaFilter.SelectedIndexChanged += CmbSmenaFilter_SelectedIndexChanged;
                this.Controls.Add(cmbSmenaFilter);
            }

            int yPozicija = jeAdmin ? 120 : 80; // Pomeri za admina zbog filtera

            // Label za ComboBox sa radnicima
            Label lblRadnik = new Label();
            lblRadnik.Text = "Изаберите радника:";
            lblRadnik.Font = new Font("Arial", 10, FontStyle.Bold);
            lblRadnik.ForeColor = Color.DarkSlateGray;
            lblRadnik.Size = new Size(150, 25);
            lblRadnik.Location = new Point(50, yPozicija);
            this.Controls.Add(lblRadnik);

            // ComboBox za radnike
            cmbRadnici = new ComboBox();
            cmbRadnici.Location = new Point(200, yPozicija);
            cmbRadnici.Size = new Size(300, 25);
            cmbRadnici.Font = new Font("Arial", 10, FontStyle.Regular);
            cmbRadnici.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(cmbRadnici);

            yPozicija += 35;

            // Datum od
            Label lblOd = new Label();
            lblOd.Text = "Од датума:";
            lblOd.Font = new Font("Arial", 10, FontStyle.Bold);
            lblOd.ForeColor = Color.DarkSlateGray;
            lblOd.Size = new Size(100, 25);
            lblOd.Location = new Point(50, yPozicija);
            this.Controls.Add(lblOd);

            dtpOd = new DateTimePicker();
            dtpOd.Location = new Point(150, yPozicija);
            dtpOd.Size = new Size(120, 25);
            dtpOd.Value = DateTime.Now.AddMonths(-1);
            this.Controls.Add(dtpOd);

            // Datum do
            Label lblDo = new Label();
            lblDo.Text = "До датума:";
            lblDo.Font = new Font("Arial", 10, FontStyle.Bold);
            lblDo.ForeColor = Color.DarkSlateGray;
            lblDo.Size = new Size(100, 25);
            lblDo.Location = new Point(300, yPozicija);
            this.Controls.Add(lblDo);

            dtpDo = new DateTimePicker();
            dtpDo.Location = new Point(400, yPozicija);
            dtpDo.Size = new Size(120, 25);
            dtpDo.Value = DateTime.Now;
            this.Controls.Add(dtpDo);

            yPozicija += 40;

            // Dugme za učitavanje
            btnUcitajPromene = new Button();
            btnUcitajPromene.Text = "🔍 Учитај промене";
            btnUcitajPromene.Location = new Point(50, yPozicija);
            btnUcitajPromene.Size = new Size(150, 30);
            btnUcitajPromene.BackColor = Color.DodgerBlue;
            btnUcitajPromene.ForeColor = Color.White;
            btnUcitajPromene.Font = new Font("Arial", 9, FontStyle.Bold);
            btnUcitajPromene.FlatStyle = FlatStyle.Flat;
            btnUcitajPromene.FlatAppearance.BorderSize = 0;
            btnUcitajPromene.Cursor = Cursors.Hand;
            btnUcitajPromene.Click += BtnUcitajPromene_Click;
            this.Controls.Add(btnUcitajPromene);

            yPozicija += 45;

            // DataGridView
            dataGridPromene = new DataGridView();
            dataGridPromene.Location = new Point(50, yPozicija);
            dataGridPromene.Size = new Size(900, 400);
            dataGridPromene.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridPromene.ReadOnly = true;
            dataGridPromene.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridPromene.BackgroundColor = Color.White;
            dataGridPromene.RowHeadersVisible = false;
            this.Controls.Add(dataGridPromene);

            // Poruka ako nema podataka
            Label lblNemaPodataka = new Label();
            lblNemaPodataka.Text = "Нема пронађених промена за изабраног радника у овом периоду.";
            lblNemaPodataka.Font = new Font("Arial", 11, FontStyle.Italic);
            lblNemaPodataka.ForeColor = Color.Gray;
            lblNemaPodataka.Size = new Size(400, 80);
            lblNemaPodataka.Location = new Point(300, yPozicija + 150);
            lblNemaPodataka.TextAlign = ContentAlignment.MiddleCenter;
            lblNemaPodataka.Visible = false;
            lblNemaPodataka.Name = "lblNemaPodataka";
            this.Controls.Add(lblNemaPodataka);

            // Dugme za zatvaranje
            btnZatvori = new Button();
            btnZatvori.Text = "❌ Затвори";
            btnZatvori.Location = new Point(450, yPozicija + 420);
            btnZatvori.Size = new Size(100, 35);
            btnZatvori.BackColor = Color.Gray;
            btnZatvori.ForeColor = Color.White;
            btnZatvori.Font = new Font("Arial", 9, FontStyle.Bold);
            btnZatvori.FlatStyle = FlatStyle.Flat;
            btnZatvori.FlatAppearance.BorderSize = 0;
            btnZatvori.Cursor = Cursors.Hand;
            btnZatvori.Click += (s, e) => this.Close();
            this.Controls.Add(btnZatvori);
        }

        // ★★★ METODA ZA KORISNIKE (SAMO NJIHOVA SMENA) ★★★
        private void UcitajRadnikeIzSvojeSmene()
        {
            try
            {
                if (string.IsNullOrEmpty(korisnikSmena))
                {
                    MessageBox.Show("Није дефинисана смена за корисника!", "Грешка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Učitaj samo radnike iz ove smene
                sviRadnici = bazaService.UzmiRadnikePoSmeni(korisnikSmena);
                filtriraniRadnici = new List<Radnik>(sviRadnici);

                PopuniComboBoxRadnici();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању радника из смене {korisnikSmena}: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ★★★ METODA ZA ADMINA (SVE SMENE) ★★★
        private void UcitajSveRadnike()
        {
            try
            {
                // Učitaj sve radnike
                sviRadnici = bazaService.UzmiSveRadnike();
                filtriraniRadnici = new List<Radnik>(sviRadnici);

                PopuniComboBoxRadnici();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању свих радника: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ★★★ UČITAVANJE FILTERA PO SMENI ZA ADMINA ★★★
        private void UcitajSmenaFilter()
        {
            try
            {
                cmbSmenaFilter.Items.Clear();
                cmbSmenaFilter.Items.Add("📋 Све смене"); // Default option

                // Uzmi jedinstvene smene iz svih radnika
                HashSet<string> smene = new HashSet<string>();
                foreach (var radnik in sviRadnici)
                {
                    if (!string.IsNullOrEmpty(radnik.Smena) && !smene.Contains(radnik.Smena))
                    {
                        smene.Add(radnik.Smena);
                        cmbSmenaFilter.Items.Add($"📅 {radnik.Smena}");
                    }
                }

                cmbSmenaFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању смена: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ★★★ EVENT HANDLER ZA FILTER PO SMENI (samo za admina) ★★★
        private void CmbSmenaFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSmenaFilter.SelectedIndex == 0) // "Све смене"
            {
                filtriraniRadnici = new List<Radnik>(sviRadnici);
            }
            else
            {
                string izabranaSmena = cmbSmenaFilter.SelectedItem.ToString().Replace("📅 ", "");
                filtriraniRadnici = sviRadnici.FindAll(r => r.Smena == izabranaSmena);
            }

            PopuniComboBoxRadnici();
        }

        // ★★★ POPUNJAVANJE COMBOBOX-A SA RADNICIMA ★★★
        private void PopuniComboBoxRadnici()
        {
            cmbRadnici.Items.Clear();

            foreach (Radnik radnik in filtriraniRadnici)
            {
                string status = radnik.Aktivan ? "🟢" : "🔴";
                string smenaInfo = !string.IsNullOrEmpty(radnik.Smena) ? $" ({radnik.Smena})" : "";
                cmbRadnici.Items.Add($"{status} {radnik.Ime} {radnik.Prezime}{smenaInfo}");
            }

            if (cmbRadnici.Items.Count > 0)
                cmbRadnici.SelectedIndex = 0;
            else
                cmbRadnici.Items.Add("Нема доступних радника");
        }

        private void BtnUcitajPromene_Click(object sender, EventArgs e)
        {
            if (cmbRadnici.SelectedIndex < 0 || filtriraniRadnici == null ||
                cmbRadnici.SelectedIndex >= filtriraniRadnici.Count)
            {
                MessageBox.Show("Изаберите радника!", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Radnik izabraniRadnik = filtriraniRadnici[cmbRadnici.SelectedIndex];
                DateTime od = dtpOd.Value.Date;
                DateTime @do = dtpDo.Value.Date;

                if (od > @do)
                {
                    MessageBox.Show("Датум 'Од' не може бити после датума 'До'!", "Грешка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Koristite metodu iz BazaService
                DataTable promene = bazaService.UzmiPromeneRadnika(izabraniRadnik.Id, od, @do);

                if (promene.Rows.Count > 0)
                {
                    dataGridPromene.DataSource = promene;
                    dataGridPromene.Visible = true;
                    Controls.Find("lblNemaPodataka", true)[0].Visible = false;

                    // Formatiranje kolona
                    if (dataGridPromene.Columns.Contains("Datum"))
                    {
                        dataGridPromene.Columns["Datum"].DefaultCellStyle.Format = "dd.MM.yyyy";
                    }
                    if (dataGridPromene.Columns.Contains("DatumPromene"))
                    {
                        dataGridPromene.Columns["DatumPromene"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";
                    }

                    // Postavi širine kolona
                    dataGridPromene.Columns["Datum"].Width = 100;
                    dataGridPromene.Columns["StariStatus"].Width = 120;
                    dataGridPromene.Columns["NoviStatus"].Width = 120;
                    dataGridPromene.Columns["StariSati"].Width = 80;
                    dataGridPromene.Columns["NoviSati"].Width = 80;
                    dataGridPromene.Columns["StariMinute"].Width = 80;
                    dataGridPromene.Columns["NoviMinute"].Width = 80;
                    dataGridPromene.Columns["TipPromene"].Width = 100;
                    dataGridPromene.Columns["DatumPromene"].Width = 140;
                    dataGridPromene.Columns["Komentar"].Width = 150;
                    dataGridPromene.Columns["Izvršio"].Width = 150;
                }
                else
                {
                    dataGridPromene.Visible = false;
                    Label lblNemaPodataka = (Label)Controls.Find("lblNemaPodataka", true)[0];

                    // Proveri da li tabela uopšte postoji
                    if (!bazaService.RadnikPromeneLogTabelaPostoji())
                    {
                        lblNemaPodataka.Text = "❌ Табела RadnikPromeneLog није креирана у бази!\n\n" +
                                              "Да бисте пратили промене радника, морате покренути SQL скрипту:\n" +
                                              "CREATE TABLE RadnikPromeneLog (...)";
                    }
                    else
                    {
                        lblNemaPodataka.Text = $"📭 Нема пронађених промена за радника:\n" +
                                              $"👤 {izabraniRadnik.Ime} {izabraniRadnik.Prezime}\n" +
                                              $"📅 Период: {od:dd.MM.yyyy} до {@do:dd.MM.yyyy}";
                    }

                    lblNemaPodataka.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању промена: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}