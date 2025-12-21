using System;
using System.Drawing;
using System.Windows.Forms;

namespace RadnoVreme
{
    public partial class PocetniDatumSmenaForm : Form
    {
        private ComboBox cmbSmena;
        private DateTimePicker dtpPocetniDatum;
        private ComboBox cmbTipPocetneSmene; // ★★★ NOVO: ComboBox za tip početne smene ★★★
        private Button btnSacuvaj;
        private Button btnOtkazi;
        private BazaService bazaService;

        public PocetniDatumSmenaForm()
        {
            this.bazaService = new BazaService();
            this.InitializeComponent();
            this.KreirajFormu();
            this.UcitajPostojeceDatume();
        }

        private void InitializeComponent()
        {
            // Ova metoda može ostati prazna
        }

        private void UcitajPostojeceDatume()
        {
            try
            {
                // Uzmi trenutnu godinu
                int trenutnaGodina = DateTime.Now.Year;

                // Učitaj postojeće datume iz baze
                var postojeciDatumi = bazaService.UzmiPocetneDatumeSmena(trenutnaGodina);

                // Ako postoje podaci, postavi vrednosti
                foreach (var smenaDatum in postojeciDatumi)
                {
                    // Pronađi odgovarajuću stavku u combobox-u i postavi datum
                    for (int i = 0; i < cmbSmena.Items.Count; i++)
                    {
                        if (cmbSmena.Items[i].ToString() == smenaDatum.Key)
                        {
                            cmbSmena.SelectedIndex = i;
                            dtpPocetniDatum.Value = smenaDatum.Value;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при чувању постојећих датума: {ex.Message}",
                              "Грашка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void KreirajFormu()
        {
            // Postavke forme - POVEĆANA VISINA ZBOG DODATNE KONTROLE
            this.Text = "Почетни датум смене";
            this.Size = new Size(400, 300); // ★★★ POVEĆANA VISINA ★★★
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            // Glavni panel - POVEĆANA VISINA
            Panel mainPanel = new Panel();
            mainPanel.Size = new Size(340, 210); // ★★★ POVEĆANA VISINA ★★★
            mainPanel.Location = new Point(20, 20);
            mainPanel.BackColor = Color.White;
            mainPanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(mainPanel);

            // Naslov
            Label lblNaslov = new Label();
            lblNaslov.Text = "📅 Постави почетни датум смене";
            lblNaslov.Font = new Font("Arial", 12, FontStyle.Bold);
            lblNaslov.ForeColor = Color.DarkBlue;
            lblNaslov.Size = new Size(300, 30);
            lblNaslov.Location = new Point(20, 15);
            lblNaslov.TextAlign = ContentAlignment.MiddleCenter;
            mainPanel.Controls.Add(lblNaslov);

            // Smena
            Label lblSmena = new Label();
            lblSmena.Text = "Смена:";
            lblSmena.Font = new Font("Arial", 9, FontStyle.Bold);
            lblSmena.ForeColor = Color.DarkSlateGray;
            lblSmena.Location = new Point(30, 60);
            lblSmena.Size = new Size(80, 20);
            mainPanel.Controls.Add(lblSmena);

            cmbSmena = new ComboBox();
            cmbSmena.Location = new Point(120, 60);
            cmbSmena.Size = new Size(180, 25);
            cmbSmena.Font = new Font("Arial", 10, FontStyle.Regular);
            cmbSmena.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSmena.Items.Add("I смена");
            cmbSmena.Items.Add("II смена");
            cmbSmena.Items.Add("III смена");
            cmbSmena.Items.Add("IV смена");
            cmbSmena.SelectedIndex = 0;
            mainPanel.Controls.Add(cmbSmena);

            // Početni datum
            Label lblDatum = new Label();
            lblDatum.Text = "Почетни датум:";
            lblDatum.Font = new Font("Arial", 9, FontStyle.Bold);
            lblDatum.ForeColor = Color.DarkSlateGray;
            lblDatum.Location = new Point(30, 95);
            lblDatum.Size = new Size(80, 20);
            mainPanel.Controls.Add(lblDatum);

            dtpPocetniDatum = new DateTimePicker();
            dtpPocetniDatum.Location = new Point(120, 95);
            dtpPocetniDatum.Size = new Size(180, 25);
            dtpPocetniDatum.Font = new Font("Arial", 10, FontStyle.Regular);
            dtpPocetniDatum.Value = DateTime.Now;
            mainPanel.Controls.Add(dtpPocetniDatum);

            // ★★★ NOVO: Tip početne smene ★★★
            Label lblTipPocetne = new Label();
            lblTipPocetne.Text = "Почиње са:";
            lblTipPocetne.Font = new Font("Arial", 9, FontStyle.Bold);
            lblTipPocetne.ForeColor = Color.DarkSlateGray;
            lblTipPocetne.Location = new Point(30, 130);
            lblTipPocetne.Size = new Size(80, 20);
            mainPanel.Controls.Add(lblTipPocetne);

            cmbTipPocetneSmene = new ComboBox();
            cmbTipPocetneSmene.Location = new Point(120, 130);
            cmbTipPocetneSmene.Size = new Size(180, 25);
            cmbTipPocetneSmene.Font = new Font("Arial", 10, FontStyle.Regular);
            cmbTipPocetneSmene.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipPocetneSmene.Items.Add("Дневна смена"); // Index 0
            cmbTipPocetneSmene.Items.Add("Ноћна смена");  // Index 1
            cmbTipPocetneSmene.SelectedIndex = 0; // Podrazumevano DNEVNA
            mainPanel.Controls.Add(cmbTipPocetneSmene);

            // Dugme za čuvanje
            btnSacuvaj = new Button();
            btnSacuvaj.Text = "💾 Сачувај";
            btnSacuvaj.Location = new Point(80, 170);
            btnSacuvaj.Size = new Size(100, 30);
            btnSacuvaj.BackColor = Color.LimeGreen;
            btnSacuvaj.ForeColor = Color.White;
            btnSacuvaj.Font = new Font("Arial", 9, FontStyle.Bold);
            btnSacuvaj.FlatStyle = FlatStyle.Flat;
            btnSacuvaj.FlatAppearance.BorderSize = 0;
            btnSacuvaj.Cursor = Cursors.Hand;
            btnSacuvaj.Click += BtnSacuvaj_Click;
            mainPanel.Controls.Add(btnSacuvaj);

            // Dugme za otkazivanje
            btnOtkazi = new Button();
            btnOtkazi.Text = "❌ Откажи";
            btnOtkazi.Location = new Point(190, 170);
            btnOtkazi.Size = new Size(100, 30);
            btnOtkazi.BackColor = Color.LightCoral;
            btnOtkazi.ForeColor = Color.White;
            btnOtkazi.Font = new Font("Arial", 9, FontStyle.Bold);
            btnOtkazi.FlatStyle = FlatStyle.Flat;
            btnOtkazi.FlatAppearance.BorderSize = 0;
            btnOtkazi.Cursor = Cursors.Hand;
            btnOtkazi.Click += (s, e) => { this.Close(); };
            mainPanel.Controls.Add(btnOtkazi);
        }

        private void BtnSacuvaj_Click(object sender, EventArgs e)
        {
            if (cmbSmena.SelectedItem == null)
            {
                MessageBox.Show("Изаберите смену!", "Грашка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string izabranaSmena = cmbSmena.SelectedItem.ToString();
            DateTime pocetniDatum = dtpPocetniDatum.Value;
            int godina = pocetniDatum.Year;

            // ★★★ NOVO: Odredi tip početne smene ★★★
            string tipPocetneSmene = cmbTipPocetneSmene.SelectedIndex == 0 ? "ДНЕВНА" : "НОЋНА";

            try
            {
                // ★★★ AŽURIRANI POZIV: Sada šaljemo i tip početne smene ★★★
                bool uspesno = bazaService.SacuvajPocetniDatumSmena(izabranaSmena, pocetniDatum, godina, tipPocetneSmene);

                if (uspesno)
                {
                    MessageBox.Show($"Почетни датум успешно постављен за {izabranaSmena}:\n" +
                                  $"📅 Датум: {pocetniDatum:dd.MM.yyyy.}\n" +
                                  $"🌅 Почиње са: {tipPocetneSmene} сменом",
                                  "Успешно",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Дошло је до грешке приликом чувања података!", "Грашка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при чувању: {ex.Message}", "Грашка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}