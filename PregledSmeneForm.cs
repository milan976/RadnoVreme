using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RadnoVreme
{
    public partial class PregledSmeneForm : Form
    {
        private FlowLayoutPanel flowPanelRadnici;
        private Label lblNaslov;
        private Button btnZatvori;
        private string korisnikSmena;

        public PregledSmeneForm(string korisnikSmena = null)
        {
            this.korisnikSmena = korisnikSmena;
            InitializeComponent();
            KreirajFormu();
            UcitajRadnikePoZvanju();
        }

        private void KreirajFormu()
        {
            // Postavke forme
            this.Text = "Преглед смене - Хијарархија радника";
            this.Size = new Size(600, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            // Naslov
            lblNaslov = new Label();
            lblNaslov.Text = GetNaslovText();
            lblNaslov.Font = new Font("Arial", 14, FontStyle.Bold);
            lblNaslov.ForeColor = Color.DarkBlue;
            lblNaslov.Size = new Size(500, 40);
            lblNaslov.Location = new Point(50, 20);
            lblNaslov.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblNaslov);

            // Flow Panel za radnike
            flowPanelRadnici = new FlowLayoutPanel();
            flowPanelRadnici.Location = new Point(50, 80);
            flowPanelRadnici.Size = new Size(500, 500);
            flowPanelRadnici.AutoScroll = true;
            flowPanelRadnici.BackColor = Color.WhiteSmoke;
            flowPanelRadnici.BorderStyle = BorderStyle.FixedSingle;
            flowPanelRadnici.FlowDirection = FlowDirection.TopDown;
            flowPanelRadnici.WrapContents = false;
            this.Controls.Add(flowPanelRadnici);

            // Dugme za zatvaranje
            btnZatvori = new Button();
            btnZatvori.Text = "🚪 Затвори";
            btnZatvori.Location = new Point(250, 600);
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

        private string GetNaslovText()
        {
            if (korisnikSmena == null)
                return "👑 СВИ РАДНИЦИ - Преглед смене";
            else
                return $"👑 {korisnikSmena} - Преглед смене";
        }

        private void UcitajRadnikePoZvanju()
        {
            BazaService bazaService = new BazaService();
            List<Radnik> radnici = bazaService.UzmiRadnikePoSmeni(korisnikSmena);

            // ★★★ SORTIRANJE PO HIJERARHIJI ZVANJA ★★★
            var sortiraniRadnici = radnici.OrderBy(r => GetRedosledZvanja(r.Zvanje))
                                         .ThenBy(r => r.Prezime)
                                         .ThenBy(r => r.Ime)
                                         .ToList();

            flowPanelRadnici.Controls.Clear();

            foreach (Radnik radnik in sortiraniRadnici)
            {
                DodajRadnikaUPanel(radnik);
            }
        }

        private void DodajRadnikaUPanel(Radnik radnik)
        {
            Panel panelRadnik = new Panel();
            panelRadnik.Size = new Size(480, 60);
            panelRadnik.Margin = new Padding(5);
            panelRadnik.BackColor = Color.White;
            panelRadnik.BorderStyle = BorderStyle.FixedSingle;

            // Ikonica prema zvanju
            Label lblIkona = new Label();
            lblIkona.Text = GetIkonaZaZvanje(radnik.Zvanje);
            lblIkona.Font = new Font("Arial", 12, FontStyle.Bold);
            lblIkona.Size = new Size(40, 60);
            lblIkona.Location = new Point(5, 0);
            lblIkona.TextAlign = ContentAlignment.MiddleCenter;
            panelRadnik.Controls.Add(lblIkona);

            // Ime i prezime
            Label lblImePrezime = new Label();
            lblImePrezime.Text = radnik.PunoIme;
            lblImePrezime.Font = new Font("Arial", 11, FontStyle.Bold);
            lblImePrezime.Size = new Size(200, 25);
            lblImePrezime.Location = new Point(50, 10);
            lblImePrezime.TextAlign = ContentAlignment.MiddleLeft;
            panelRadnik.Controls.Add(lblImePrezime);

            // Zvanje
            Label lblZvanje = new Label();
            lblZvanje.Text = radnik.Zvanje ?? "Нема звање";
            lblZvanje.Font = new Font("Arial", 9, FontStyle.Regular);
            lblZvanje.ForeColor = Color.DarkSlateGray;
            lblZvanje.Size = new Size(200, 20);
            lblZvanje.Location = new Point(50, 35);
            lblZvanje.TextAlign = ContentAlignment.MiddleLeft;
            panelRadnik.Controls.Add(lblZvanje);

            // Smena (ako je admin)
            if (korisnikSmena == null)
            {
                Label lblSmena = new Label();
                lblSmena.Text = radnik.Smena;
                lblSmena.Font = new Font("Arial", 8, FontStyle.Italic);
                lblSmena.ForeColor = Color.Gray;
                lblSmena.Size = new Size(80, 15);
                lblSmena.Location = new Point(380, 10);
                lblSmena.TextAlign = ContentAlignment.MiddleRight;
                panelRadnik.Controls.Add(lblSmena);
            }

            // Dugme za detalje (možete kasnije dodati funkcionalnost)
            Button btnDetalji = new Button();
            btnDetalji.Text = "📋";
            btnDetalji.Size = new Size(40, 25);
            btnDetalji.Location = new Point(420, 30);
            btnDetalji.BackColor = Color.LightBlue;
            btnDetalji.FlatStyle = FlatStyle.Flat;
            btnDetalji.Click += (s, e) => PrikaziDetaljeRadnika(radnik);
            panelRadnik.Controls.Add(btnDetalji);

            flowPanelRadnici.Controls.Add(panelRadnik);
        }

        private string GetIkonaZaZvanje(string zvanje)
        {
            if (string.IsNullOrEmpty(zvanje)) return "👤";

            if (zvanje.ToLower().Contains("šef") || zvanje.ToLower().Contains("sef"))
                return "👑";
            else if (zvanje.ToLower().Contains("vodja") || zvanje.ToLower().Contains("vodja"))
                return "⭐";
            else if (zvanje.ToLower().Contains("vatrogasac"))
                return "🚒";
            else
                return "👤";
        }

        private int GetRedosledZvanja(string zvanje)
        {
            if (string.IsNullOrEmpty(zvanje))
                return 999;

            var hijerarhija = new Dictionary<string, int>
            {
                {"Шеф смене", 1},
                {"Вођа ВС чете", 2},
                {"Вођа ВС вода", 3},
                {"Вођа ВС одељења", 4},
                {"Вођа ВС групе", 5},
                {"Вођа ВС одељења за ОПР", 6},
                {"Вођа ВС групе за возаче", 7},
                {"Ватрогасац спасилац возач", 8},
                {"Ватрогасац спасилац", 9},
                {"ВСВ", 8},
                {"ВС", 9}
            };

            if (hijerarhija.ContainsKey(zvanje.Trim()))
                return hijerarhija[zvanje.Trim()];

            foreach (var key in hijerarhija.Keys)
            {
                if (zvanje.ToLower().Contains(key.ToLower()))
                    return hijerarhija[key];
            }

            return 999;
        }

        private void PrikaziDetaljeRadnika(Radnik radnik)
        {
            // ★★★ OVDE MOŽETE KASNIJE DODATI DETALJNI PRIKAZ ★★★
            MessageBox.Show($"Детаљи за: {radnik.PunoIme}\nЗвање: {radnik.Zvanje}\nСмена: {radnik.Smena}",
                          "Детаљи за радника", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}