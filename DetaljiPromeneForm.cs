using System;
using System.Drawing;
using System.Windows.Forms;

namespace RadnoVreme
{
    public partial class DetaljiPromeneForm : Form
    {
        public DetaljiPromeneForm(string stariPodaci, string noviPodaci, string tipPromene, string datumPromene, string korisnikNaKomeJeRadjeno)
        {
            InitializeComponent();
            KreirajFormu(stariPodaci, noviPodaci, tipPromene, datumPromene, korisnikNaKomeJeRadjeno);
        }

        private void KreirajFormu(string stariPodaci, string noviPodaci, string tipPromene, string datumPromene, string korisnikNaKomeJeRadjeno)
        {
            // Postavke forme
            this.Text = "Детаљи промене";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            // Naslov
            Label lblNaslov = new Label();
            lblNaslov.Text = "📊 Детаљи промене";
            lblNaslov.Font = new Font("Arial", 14, FontStyle.Bold);
            lblNaslov.ForeColor = Color.DarkBlue;
            lblNaslov.Size = new Size(300, 30);
            lblNaslov.Location = new Point(200, 20);
            lblNaslov.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblNaslov);

            // Informacije o promeni
            Label lblInfo = new Label();
            lblInfo.Text = $"Тип промене: {tipPromene}\nДатум: {datumPromene}\nКорисник на коме је урађено: {korisnikNaKomeJeRadjeno}";
            lblInfo.Font = new Font("Arial", 10, FontStyle.Regular);
            lblInfo.ForeColor = Color.DarkSlateGray;
            lblInfo.Size = new Size(400, 80);
            lblInfo.Location = new Point(150, 60);
            lblInfo.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblInfo);

            int yPozicija = 150;

            // Stare vrednosti
            if (!string.IsNullOrEmpty(stariPodaci))
            {
                Label lblStariNaslov = new Label();
                lblStariNaslov.Text = "🔴 Пре промене:";
                lblStariNaslov.Font = new Font("Arial", 11, FontStyle.Bold);
                lblStariNaslov.ForeColor = Color.Red;
                lblStariNaslov.Size = new Size(200, 25);
                lblStariNaslov.Location = new Point(50, yPozicija);
                this.Controls.Add(lblStariNaslov);

                TextBox txtStari = new TextBox();
                txtStari.Multiline = true;
                txtStari.ScrollBars = ScrollBars.Vertical;
                txtStari.Location = new Point(50, yPozicija + 30);
                txtStari.Size = new Size(580, 80);
                txtStari.Text = FormatirajPodatke(stariPodaci);
                txtStari.Font = new Font("Arial", 9, FontStyle.Regular);
                txtStari.BackColor = Color.LightCoral;
                txtStari.ReadOnly = true;
                this.Controls.Add(txtStari);

                yPozicija += 120;
            }

            // Nove vrednosti
            if (!string.IsNullOrEmpty(noviPodaci))
            {
                Label lblNoviNaslov = new Label();
                lblNoviNaslov.Text = "🟢 После промене:";
                lblNoviNaslov.Font = new Font("Arial", 11, FontStyle.Bold);
                lblNoviNaslov.ForeColor = Color.Green;
                lblNoviNaslov.Size = new Size(200, 25);
                lblNoviNaslov.Location = new Point(50, yPozicija);
                this.Controls.Add(lblNoviNaslov);

                TextBox txtNovi = new TextBox();
                txtNovi.Multiline = true;
                txtNovi.ScrollBars = ScrollBars.Vertical;
                txtNovi.Location = new Point(50, yPozicija + 30);
                txtNovi.Size = new Size(580, 80);
                txtNovi.Text = FormatirajPodatke(noviPodaci);
                txtNovi.Font = new Font("Arial", 9, FontStyle.Regular);
                txtNovi.BackColor = Color.LightGreen;
                txtNovi.ReadOnly = true;
                this.Controls.Add(txtNovi);

                yPozicija += 120;
            }

            // Dugme za zatvaranje
            Button btnZatvori = new Button();
            btnZatvori.Text = "❌ Затвори";
            btnZatvori.Location = new Point(300, yPozicija + 10);
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

        private string FormatirajPodatke(string podaci)
        {
            // Formatiraj podatke za bolji prikaz
            return podaci.Replace(", ", "\n").Replace(":", ": ");
        }
    }
}