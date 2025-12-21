using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RadnoVreme
{
    public partial class RasporedSmenaForm : Form
    {
        private DataGridView dataGridRaspored;
        private ComboBox cmbSmena;
        private NumericUpDown numGodina;
        private ComboBox cmbMeseci;
        private Button btnGenerisi;
        private Button btnGenerisiSmenu;
        private Button btnZatvori;
        private Label lblInfo;

        private BazaService bazaService;

        public RasporedSmenaForm()
        {
            this.bazaService = new BazaService();
            this.InitializeComponent();
            this.KreirajFormu();
            this.UcitajSmane();
            this.UcitajMesece();
        }

        private void KreirajFormu()
        {
            // Postavke forme
            this.Text = "📅 Генерисање распореда смена";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            // Naslov
            Label lblNaslov = new Label();
            lblNaslov.Text = "📅 ГЕНЕРИСАЊЕ РАСПОРЕДА СМЕНА  ";
            lblNaslov.Font = new Font("Arial", 16, FontStyle.Bold);
            lblNaslov.ForeColor = Color.DarkBlue;
            lblNaslov.Size = new Size(500, 40);
            lblNaslov.Location = new Point(250, 20);
            lblNaslov.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblNaslov);

            int yPozicija = 80;

            // Izbor SMENE
            Label lblSmena = new Label();
            lblSmena.Text = "Смена:";
            lblSmena.Font = new Font("Arial", 10, FontStyle.Bold);
            lblSmena.ForeColor = Color.DarkSlateGray;
            lblSmena.Size = new Size(80, 25);
            lblSmena.Location = new Point(50, yPozicija);
            this.Controls.Add(lblSmena);

            cmbSmena = new ComboBox();
            cmbSmena.Location = new Point(140, yPozicija);
            cmbSmena.Size = new Size(200, 25);
            cmbSmena.Font = new Font("Arial", 10, FontStyle.Regular);
            cmbSmena.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(cmbSmena);

            yPozicija += 40;

            // Godina
            Label lblGodina = new Label();
            lblGodina.Text = "Година:";
            lblGodina.Font = new Font("Arial", 10, FontStyle.Bold);
            lblGodina.ForeColor = Color.DarkSlateGray;
            lblGodina.Size = new Size(80, 25);
            lblGodina.Location = new Point(50, yPozicija);
            this.Controls.Add(lblGodina);

            numGodina = new NumericUpDown();
            numGodina.Location = new Point(140, yPozicija);
            numGodina.Size = new Size(100, 25);
            numGodina.Font = new Font("Arial", 10, FontStyle.Regular);
            numGodina.Minimum = 2020;
            numGodina.Maximum = 2030;
            numGodina.Value = DateTime.Now.Year;
            this.Controls.Add(numGodina);

            // Mesec
            Label lblMesec = new Label();
            lblMesec.Text = "Месец:";
            lblMesec.Font = new Font("Arial", 10, FontStyle.Bold);
            lblMesec.ForeColor = Color.DarkSlateGray;
            lblMesec.Size = new Size(80, 25);
            lblMesec.Location = new Point(260, yPozicija);
            this.Controls.Add(lblMesec);

            cmbMeseci = new ComboBox();
            cmbMeseci.Location = new Point(350, yPozicija);
            cmbMeseci.Size = new Size(150, 25);
            cmbMeseci.Font = new Font("Arial", 10, FontStyle.Regular);
            cmbMeseci.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(cmbMeseci);

            yPozicija += 40;

            // Dugme za generisanje CELOG RASPOREDA SMENE
            btnGenerisiSmenu = new Button();
            btnGenerisiSmenu.Text = "🚀 Генериши распоред за смену";
            btnGenerisiSmenu.Location = new Point(50, yPozicija);
            btnGenerisiSmenu.Size = new Size(250, 35);
            btnGenerisiSmenu.BackColor = Color.LimeGreen;
            btnGenerisiSmenu.ForeColor = Color.White;
            btnGenerisiSmenu.Font = new Font("Arial", 10, FontStyle.Bold);
            btnGenerisiSmenu.FlatStyle = FlatStyle.Flat;
            btnGenerisiSmenu.FlatAppearance.BorderSize = 0;
            btnGenerisiSmenu.Cursor = Cursors.Hand;
            btnGenerisiSmenu.Click += BtnGenerisiSmenu_Click;
            this.Controls.Add(btnGenerisiSmenu);

            // Dugme za prikaz rasporeda
            btnGenerisi = new Button();
            btnGenerisi.Text = "🔍 Прикажи распоред";
            btnGenerisi.Location = new Point(320, yPozicija);
            btnGenerisi.Size = new Size(180, 35);
            btnGenerisi.BackColor = Color.DodgerBlue;
            btnGenerisi.ForeColor = Color.White;
            btnGenerisi.Font = new Font("Arial", 10, FontStyle.Bold);
            btnGenerisi.FlatStyle = FlatStyle.Flat;
            btnGenerisi.FlatAppearance.BorderSize = 0;
            btnGenerisi.Cursor = Cursors.Hand;
            btnGenerisi.Click += BtnGenerisi_Click;
            this.Controls.Add(btnGenerisi);

            yPozicija += 50;

            // Dodaj u KreirajFormu() metodu:
            Button btnProvera = new Button();
            btnProvera.Text = "🔍 Провера Шеме";
            btnProvera.Location = new Point(680, yPozicija);
            btnProvera.Size = new Size(120, 35);
            btnProvera.BackColor = Color.Purple;
            btnProvera.ForeColor = Color.White;
            btnProvera.Font = new Font("Arial", 9, FontStyle.Bold);
            btnProvera.FlatStyle = FlatStyle.Flat;
            btnProvera.FlatAppearance.BorderSize = 0;
            btnProvera.Cursor = Cursors.Hand;
            btnProvera.Click += BtnProvera_Click;
            this.Controls.Add(btnProvera);

            // Label za informacije
            lblInfo = new Label();
            lblInfo.Text = "Изабери смену и генериши распоред за целу годину";
            lblInfo.Font = new Font("Arial", 9, FontStyle.Italic);
            lblInfo.ForeColor = Color.DarkGreen;
            lblInfo.Size = new Size(500, 25);
            lblInfo.Location = new Point(50, yPozicija);
            lblInfo.TextAlign = ContentAlignment.MiddleLeft;
            this.Controls.Add(lblInfo);

            yPozicija += 40;

            // DataGridView za raspored
            dataGridRaspored = new DataGridView();
            dataGridRaspored.Location = new Point(50, yPozicija);
            dataGridRaspored.Size = new Size(880, 350);
            dataGridRaspored.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridRaspored.ReadOnly = true;
            dataGridRaspored.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridRaspored.BackgroundColor = Color.White;
            dataGridRaspored.RowHeadersVisible = false;

            // Postavi kolone
            dataGridRaspored.Columns.Add("Датум", "Датум");
            dataGridRaspored.Columns.Add("Дан", "Дан");
            dataGridRaspored.Columns.Add("Смена", "Смена");
            dataGridRaspored.Columns.Add("Време", "Време");
            dataGridRaspored.Columns.Add("Статус", "Статус");

            this.Controls.Add(dataGridRaspored);

            // Dugme za zatvaranje
            btnZatvori = new Button();
            btnZatvori.Text = "❌ Затвори";
            btnZatvori.Location = new Point(450, yPozicija + 370);
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

        // ★★★ METODA ZA PROVERU ŠEME ★★★
        private void BtnProvera_Click(object sender, EventArgs e)
        {
            if (cmbSmena.SelectedItem == null)
            {
                MessageBox.Show("Изаберите смену!", "Грешка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string izabranaSmena = cmbSmena.SelectedItem.ToString();
            int godina = (int)numGodina.Value;

            try
            {
                var pocetniDatum = bazaService.UzmiPocetniDatumZaSmenu(izabranaSmena, godina);

                if (pocetniDatum.HasValue)
                {
                    bazaService.ProveriShemu(izabranaSmena, pocetniDatum.Value);
                }
                else
                {
                    MessageBox.Show($"Није постављен почетни датум за {izabranaSmena}!", "Грешка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при провери: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void UcitajSmane()
        {
            cmbSmena.Items.Clear();
            cmbSmena.Items.Add("I смена");
            cmbSmena.Items.Add("II смена");
            cmbSmena.Items.Add("III смена");
            cmbSmena.Items.Add("IV смена");
            cmbSmena.SelectedIndex = 0;
        }

        private void UcitajMesece()
        {
            string[] meseci = { "Јануар", "Фебруар", "Март", "Април", "Мај", "Јун",
                              "Јул", "Август", "Септембар", "Октобар", "Новембар", "Децембар" };

            cmbMeseci.Items.AddRange(meseci);
            cmbMeseci.SelectedIndex = DateTime.Now.Month - 1;
        }

        private void BtnGenerisiSmenu_Click(object sender, EventArgs e)
        {
            if (cmbSmena.SelectedItem == null)
            {
                MessageBox.Show("Изаберите смену!", "смена", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string izabranaSmena = cmbSmena.SelectedItem.ToString();
            int godina = (int)numGodina.Value;

            // ★★★ UPZORENJE PRE GENERISANJA ★★★
            DialogResult result = MessageBox.Show(
                $"Ово ће ГЕНЕРИСАТИ и САЧУВАТИ распоред за {izabranaSmena} у {godina}. години.\n\n" +
                "Генерисани распоред ће бити ТРАЈНО сачуван у бази података.\n" +
                "Да ли желите да наставите?",
                "Потврда генерисања распореда",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            try
            {
                bool uspesno = bazaService.GenerisiRasporedZaSmenu(izabranaSmena, godina);

                if (uspesno)
                {
                    MessageBox.Show($"Распоред је успешно генерисан и САЧУВАН у бази за {izabranaSmena} у {godina}. години!\n\n" +
                                  "Сада можете прегледати распоред у календару.",
                                  "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при генерисању распореда: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void BtnGenerisi_Click(object sender, EventArgs e)
        {
            if (cmbSmena.SelectedIndex < 0)
            {
                MessageBox.Show("Изаберите смену!", "Грешка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string izabranaSmena = cmbSmena.SelectedItem.ToString();
                int godina = (int)numGodina.Value;
                int mesec = cmbMeseci.SelectedIndex + 1;

                // Proveri da li postoji generisan raspored
                if (!bazaService.RasporedPostojiZaSmenu(izabranaSmena, godina))
                {
                    DialogResult result = MessageBox.Show(
                        $"Распоред за {izabranaSmena} у {godina}. години није генерисан.\nЖелите ли да га генеришете сада?",
                        "Распоред није генерисан",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        BtnGenerisiSmenu_Click(sender, e);
                    }
                    return;
                }

                // Uzmi raspored za smenu iz baze
                var raspored = bazaService.UzmiRasporedZaSmenu(izabranaSmena, godina, mesec);

                // Prikaži raspored
                PrikaziRaspored(raspored, godina, mesec, izabranaSmena);

                lblInfo.Text = $"📊 Распоред за {izabranaSmena} - {cmbMeseci.SelectedItem} {godina}.";
                lblInfo.ForeColor = Color.DarkBlue;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при приказивању распореда: {ex.Message}", "смена",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrikaziRaspored(Dictionary<DateTime, string> raspored, int godina, int mesec, string smena)
        {
            dataGridRaspored.Rows.Clear();

            // Prikaz radnika u ovoj smeni
            var radniciUSmeni = bazaService.UzmiRadnikeUSmeni(smena);
            string radniciText = radniciUSmeni.Count > 0 ?
                string.Join(", ", radniciUSmeni.ConvertAll(r => r.PunoIme)) :
                "Нема радника у овој смени";

            lblInfo.Text = $"📊 {smena} - {cmbMeseci.SelectedItem} {godina}. | Радници: {radniciText}";

            for (int dan = 1; dan <= DateTime.DaysInMonth(godina, mesec); dan++)
            {
                DateTime datum = new DateTime(godina, mesec, dan);
                string danUNedelji = datum.ToString("dddd");
                string smenaTip = raspored.ContainsKey(datum) ? raspored[datum] : "ОДМОР";
                string vreme = GetVremeSmane(smenaTip);
                string status = smenaTip == "ОДМОР" ? "ОДМОР" : "РАД";

                dataGridRaspored.Rows.Add(
                    datum.ToString("dd.MM.yyyy"),
                    danUNedelji,
                    smenaTip,
                    vreme,
                    status
                );

                // Oboji redove
                DataGridViewRow row = dataGridRaspored.Rows[dataGridRaspored.Rows.Count - 1];
                row.DefaultCellStyle.BackColor = GetBojaZaSmenu(smenaTip);
            }
        }

        private string GetVremeSmane(string smena)
        {
            if (smena == "ДНЕВНА")
                return "07:00 - 19:00";
            else if (smena == "НОЋНА")
                return "19:00 - 07:00";
            else
                return "-";
        }

        private Color GetBojaZaSmenu(string smena)
        {
            if (smena == "ДНЕВНА")
                return Color.LightGreen;
            else if (smena == "НОЋНА")
                return Color.LightBlue;
            else if (smena == "ОДМОР")
                return Color.LightGray;
            else
                return Color.White;
        }
    }
}