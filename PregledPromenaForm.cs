using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace RadnoVreme
{
    public partial class PregledPromenaForm : Form
    {
        private ComboBox cmbKorisnici;
        private DataGridView dataGridPromene;
        private Button btnUcitajPromene;
        private Button btnZatvori;
        private Label lblNaslov;
        private BazaService bazaService;
        private List<Korisnik> sviKorisnici;

        public PregledPromenaForm()
        {
            this.bazaService = new BazaService();
            this.InitializeComponent();
            this.KreirajFormu();
            this.UcitajKorisnike();
        }

        private void KreirajFormu()
        {
            // Postavke forme - POVEĆANA VISINA
            this.Text = "Преглед промена корисника";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            // Naslov
            lblNaslov = new Label();
            lblNaslov.Text = "📋 Преглед промена корисника";
            lblNaslov.Font = new Font("Arial", 16, FontStyle.Bold);
            lblNaslov.ForeColor = Color.DarkBlue;
            lblNaslov.Size = new Size(400, 40);
            lblNaslov.Location = new Point(200, 20);
            lblNaslov.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblNaslov);

            // Label za ComboBox
            Label lblIzbor = new Label();
            lblIzbor.Text = "Изаберите корисника:";
            lblIzbor.Font = new Font("Arial", 10, FontStyle.Bold);
            lblIzbor.ForeColor = Color.DarkSlateGray;
            lblIzbor.Size = new Size(150, 25);
            lblIzbor.Location = new Point(50, 80);
            lblIzbor.TextAlign = ContentAlignment.MiddleLeft;
            this.Controls.Add(lblIzbor);

            // ComboBox za korisnike
            cmbKorisnici = new ComboBox();
            cmbKorisnici.Location = new Point(200, 80);
            cmbKorisnici.Size = new Size(300, 25);
            cmbKorisnici.Font = new Font("Arial", 10, FontStyle.Regular);
            cmbKorisnici.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(cmbKorisnici);

            // Dugme za učitavanje promena
            btnUcitajPromene = new Button();
            btnUcitajPromene.Text = "🔍 Учитај промене";
            btnUcitajPromene.Location = new Point(520, 80);
            btnUcitajPromene.Size = new Size(150, 25);
            btnUcitajPromene.BackColor = Color.DodgerBlue;
            btnUcitajPromene.ForeColor = Color.White;
            btnUcitajPromene.Font = new Font("Arial", 9, FontStyle.Bold);
            btnUcitajPromene.FlatStyle = FlatStyle.Flat;
            btnUcitajPromene.FlatAppearance.BorderSize = 0;
            btnUcitajPromene.Cursor = Cursors.Hand;
            btnUcitajPromene.Click += BtnUcitajPromene_Click;
            this.Controls.Add(btnUcitajPromene);

            // DataGridView za prikaz promena
            dataGridPromene = new DataGridView();
            dataGridPromene.Location = new Point(50, 130);
            dataGridPromene.Size = new Size(680, 350);
            dataGridPromene.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridPromene.ReadOnly = true;
            dataGridPromene.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridPromene.BackgroundColor = Color.White;
            dataGridPromene.RowHeadersVisible = false;
            dataGridPromene.CellClick += DataGridPromene_CellClick;
            this.Controls.Add(dataGridPromene);

            // Poruka ako nema podataka
            Label lblNemaPodataka = new Label();
            lblNemaPodataka.Text = "Нема пронађених промена за изабраног корисника.\n\nТабела KorisnikPromeneLog није креирана у бази.";
            lblNemaPodataka.Font = new Font("Arial", 11, FontStyle.Italic);
            lblNemaPodataka.ForeColor = Color.Gray;
            lblNemaPodataka.Size = new Size(400, 80);
            lblNemaPodataka.Location = new Point(200, 250);
            lblNemaPodataka.TextAlign = ContentAlignment.MiddleCenter;
            lblNemaPodataka.Visible = false;
            lblNemaPodataka.Name = "lblNemaPodataka";
            this.Controls.Add(lblNemaPodataka);

            // Dugme za zatvaranje
            btnZatvori = new Button();
            btnZatvori.Text = "❌ Затвори";
            btnZatvori.Location = new Point(350, 500);
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

        private void DataGridPromene_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewRow row = dataGridPromene.Rows[e.RowIndex];

                string stariPodaci = row.Cells["StariPodaci"].Value?.ToString() ?? "";
                string noviPodaci = row.Cells["NoviPodaci"].Value?.ToString() ?? "";
                string tipPromene = row.Cells["TipPromene"].Value?.ToString() ?? "";
                string datumPromene = row.Cells["DatumPromene"].Value?.ToString() ?? "";
                string korisnikNaKomeJeRadjeno = row.Cells["KorisnikNaKomeJeRadjeno"].Value?.ToString() ?? "";

                // Otvori formu sa detaljima
                DetaljiPromeneForm detaljiForm = new DetaljiPromeneForm(
                    stariPodaci,
                    noviPodaci,
                    tipPromene,
                    datumPromene,
                    korisnikNaKomeJeRadjeno
                );
                detaljiForm.ShowDialog();
            }
        }

        private void UcitajKorisnike()
        {
            try
            {
                sviKorisnici = bazaService.UzmiSveKorisnike();
                cmbKorisnici.Items.Clear();

                foreach (Korisnik korisnik in sviKorisnici)
                {
                    string status = korisnik.Aktivan ? "🟢" : "🔴";
                    cmbKorisnici.Items.Add($"{status} {korisnik.KorisnickoIme} ({korisnik.Ime} {korisnik.Prezime})");
                }

                if (cmbKorisnici.Items.Count > 0)
                    cmbKorisnici.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању корисника: {ex.Message}", "Грашка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUcitajPromene_Click(object sender, EventArgs e)
        {
            if (cmbKorisnici.SelectedIndex < 0)
            {
                MessageBox.Show("Изаберите корисника!", "Грашка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Pronađi ID izabranog korisnika
                Korisnik izabraniKorisnik = sviKorisnici[cmbKorisnici.SelectedIndex];

                // Pokušaj da učitaš promene
                DataTable promene = bazaService.UzmiPromeneKorisnika(izabraniKorisnik.Id);

                if (promene.Rows.Count > 0)
                {
                    dataGridPromene.DataSource = promene;
                    dataGridPromene.Visible = true;
                    Controls.Find("lblNemaPodataka", true)[0].Visible = false;
                }
                else
                {
                    dataGridPromene.Visible = false;
                    Label lblNemaPodataka = (Label)Controls.Find("lblNemaPodataka", true)[0];
                    lblNemaPodataka.Text = $"Нема пронађених промена за корисника:\n{izabraniKorisnik.Ime} {izabraniKorisnik.Prezime}\n\nТабела KorisnikPromeneLog није креирана у бази.";
                    lblNemaPodataka.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању промена: {ex.Message}", "Грашка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}