using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RadnoVreme
{
    public partial class PregledRadnikaForm : Form
    {
        private ListBox listBoxRadnici;
        private Button btnOtvoriRadnoVreme;
        private Button btnIzadji;
        private Label lblNaslov;
        private string korisnikSmena;

        public PregledRadnikaForm(string korisnikSmena = null)
        {
            this.korisnikSmena = korisnikSmena;
            this.InitializeComponent();
            this.KreirajFormu();
            this.UcitajRadnikeIzBaze();
        }

        private void InitializeComponent()
        {
            // Ova metoda može ostati prazna jer ručno kreiramo kontrole
        }

        private void KreirajFormu()
        {
            // Postavke forme
            this.Text = "Преглед радника - Избор за радно време";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            // Glavni panel
            Panel mainPanel = new Panel();
            mainPanel.Size = new Size(440, 370);
            mainPanel.Location = new Point(20, 20);
            mainPanel.BackColor = Color.White;
            mainPanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(mainPanel);

            // Naslov
            lblNaslov = new Label();
            lblNaslov.Text = "📋 Изаберите радника";
            lblNaslov.Font = new Font("Arial", 14, FontStyle.Bold);
            lblNaslov.ForeColor = Color.DarkBlue;
            lblNaslov.Size = new Size(300, 30);
            lblNaslov.Location = new Point(70, 20);
            lblNaslov.TextAlign = ContentAlignment.MiddleCenter;
            mainPanel.Controls.Add(lblNaslov);

            // ListBox za radnike
            Label lblLista = new Label();
            lblLista.Text = "Листа радника:";
            lblLista.Font = new Font("Arial", 9, FontStyle.Bold);
            lblLista.ForeColor = Color.DarkSlateGray;
            lblLista.Location = new Point(50, 70);
            lblLista.Size = new Size(150, 20);
            mainPanel.Controls.Add(lblLista);

            listBoxRadnici = new ListBox();
            listBoxRadnici.Location = new Point(50, 95);
            listBoxRadnici.Size = new Size(340, 180);
            listBoxRadnici.Font = new Font("Arial", 10, FontStyle.Regular);
            listBoxRadnici.BorderStyle = BorderStyle.FixedSingle;
            listBoxRadnici.SelectionMode = SelectionMode.One;
            mainPanel.Controls.Add(listBoxRadnici);

            // Dugme za otvaranje radnog vremena
            btnOtvoriRadnoVreme = new Button();
            btnOtvoriRadnoVreme.Text = "📅 Отвори радно време";
            btnOtvoriRadnoVreme.Location = new Point(120, 300);
            btnOtvoriRadnoVreme.Size = new Size(180, 35);
            btnOtvoriRadnoVreme.BackColor = Color.DodgerBlue;
            btnOtvoriRadnoVreme.ForeColor = Color.White;
            btnOtvoriRadnoVreme.Font = new Font("Arial", 10, FontStyle.Bold);
            btnOtvoriRadnoVreme.FlatStyle = FlatStyle.Flat;
            btnOtvoriRadnoVreme.FlatAppearance.BorderSize = 0;
            btnOtvoriRadnoVreme.Cursor = Cursors.Hand;
            btnOtvoriRadnoVreme.Click += BtnOtvoriRadnoVreme_Click;
            mainPanel.Controls.Add(btnOtvoriRadnoVreme);

            // Dugme za izlaz
            btnIzadji = new Button();
            btnIzadji.Text = "🚪 Изађи";
            btnIzadji.Location = new Point(310, 300);
            btnIzadji.Size = new Size(80, 35);
            btnIzadji.BackColor = Color.Gray;
            btnIzadji.ForeColor = Color.White;
            btnIzadji.Font = new Font("Arial", 9, FontStyle.Bold);
            btnIzadji.FlatStyle = FlatStyle.Flat;
            btnIzadji.FlatAppearance.BorderSize = 0;
            btnIzadji.Cursor = Cursors.Hand;
            btnIzadji.Click += BtnIzadji_Click;
            mainPanel.Controls.Add(btnIzadji);
        }

        private void UcitajRadnikeIzBaze()
        {
            BazaService bazaService = new BazaService();
            List<Radnik> radnici = bazaService.UzmiRadnikePoSmeni(korisnikSmena);

            listBoxRadnici.Items.Clear();
            foreach (Radnik radnik in radnici)
            {
                listBoxRadnici.Items.Add($"{radnik.PunoIme} - {radnik.Smena} - {radnik.Zvanje}");
            }

            if (listBoxRadnici.Items.Count > 0)
                listBoxRadnici.SelectedIndex = 0;

            if (korisnikSmena == null)
            {
                lblNaslov.Text = "📋 Сви радници (Администратор)";
            }
            else
            {
                lblNaslov.Text = $"📋 Радници - {korisnikSmena}";
            }
        }

        private void BtnOtvoriRadnoVreme_Click(object sender, EventArgs e)
        {
            if (listBoxRadnici.SelectedItem == null)
            {
                MessageBox.Show("Молим изаберите радника!", "Упозорење",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string izabraniRadnik = listBoxRadnici.SelectedItem.ToString();

            // ★★★ POBOLJŠANO IZVLACENJE IMENA I PREZIMENA ★★★
            string samoImePrezime = IzvuciImePrezime(izabraniRadnik);

            // ★★★ DEBUG ISPIS ★★★
            System.Diagnostics.Debug.WriteLine($"📋 Изабрани радник из листе: '{izabraniRadnik}'");
            System.Diagnostics.Debug.WriteLine($"👤 Само име и презиме: '{samoImePrezime}'");

            // Otvaranje forme za radno vreme
            RadnoVremeForm radnoVremeForm = new RadnoVremeForm(samoImePrezime);
            radnoVremeForm.ShowDialog();
        }

        private string IzvuciImePrezime(string kompletanString)
        {
            if (string.IsNullOrEmpty(kompletanString))
                return kompletanString;

            System.Diagnostics.Debug.WriteLine($"🔍 Извлачење имена из: '{kompletanString}'");

            // Različiti formati koje možemo imati:
            // "Milan Djoric - I smena - Medicinska sestra"
            // "Milan Djoric"
            // "Milan - I smena - Medicinska sestra"

            string rezultat = kompletanString;

            // Ako string sadrži "-", uzmi samo deo pre prvog "-"
            if (kompletanString.Contains("-"))
            {
                string[] delovi = kompletanString.Split('-');
                if (delovi.Length > 0)
                {
                    rezultat = delovi[0].Trim();
                    System.Diagnostics.Debug.WriteLine($"   - Пре првог '-': '{rezultat}'");
                }
            }

            // Ukloni eventualne brojeve ili dodatne informacije
            if (rezultat.Any(char.IsDigit))
            {
                rezultat = new string(rezultat.Where(c => !char.IsDigit(c)).ToArray()).Trim();
                System.Diagnostics.Debug.WriteLine($"   - Без бројева: '{rezultat}'");
            }

            System.Diagnostics.Debug.WriteLine($"   - Коначно: '{rezultat}'");
            return rezultat;
        }

        private void BtnIzadji_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}