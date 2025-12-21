using System;
using System.Drawing;
using System.Windows.Forms;

namespace RadnoVreme
{
    public partial class UnosSatiForm : Form
    {
        public int IzabraniSati { get; private set; }
        public int IzabraneMinute { get; private set; }

        private string status;
        private Label lblStatus;
        private NumericUpDown numSati;
        private NumericUpDown numMinute;
        private Button btnPotvrdi;
        private Button btnOtkazi;

        public UnosSatiForm(string status)
        {
            this.status = status;

            System.Diagnostics.Debug.WriteLine($"🔄 ОТВАРАЊЕ UnosSatiForm за статус: {status}");

            InitializeComponent();
            KreirajFormu();
        }

        private void KreirajFormu()
        {
            // Postavke forme
            this.Text = "Унос сати";
            this.Size = new Size(350, 200);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            // Glavni panel
            Panel mainPanel = new Panel();
            mainPanel.Size = new Size(310, 140);
            mainPanel.Location = new Point(10, 10);
            mainPanel.BackColor = Color.White;
            mainPanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(mainPanel);

            // Naslov sa statusom
            lblStatus = new Label();
            lblStatus.Text = $"Статус: {status}";
            lblStatus.Font = new Font("Arial", 12, FontStyle.Bold);
            lblStatus.ForeColor = Color.DarkBlue;
            lblStatus.Size = new Size(290, 30);
            lblStatus.Location = new Point(10, 10);
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            mainPanel.Controls.Add(lblStatus);

            // Label za sate
            Label lblSati = new Label();
            lblSati.Text = "Сати:";
            lblSati.Font = new Font("Arial", 10, FontStyle.Bold);
            lblSati.ForeColor = Color.DarkSlateGray;
            lblSati.Size = new Size(50, 25);
            lblSati.Location = new Point(70, 50);
            lblSati.TextAlign = ContentAlignment.MiddleRight;
            mainPanel.Controls.Add(lblSati);

            // NumericUpDown za sate
            numSati = new NumericUpDown();
            numSati.Minimum = 0;
            numSati.Maximum = 24;

            //  PODRAZUMEVANE VREDNOSTI PO STATUSU 
            if (status == "Рад")
                numSati.Value = 12;  // Podrazumevano za smenu
            else if (status == "ССПК")
                numSati.Value = 8;   // Podrazumevano za SSPK
            else if (status == "Слободан")
                numSati.Value = 0;   // Podrazumevano za slobodan
            else
                numSati.Value = 8;   // Podrazumevano za ostale statuse

            numSati.Size = new Size(60, 20);
            numSati.Location = new Point(130, 52);
            numSati.Font = new Font("Arial", 10, FontStyle.Regular);
            mainPanel.Controls.Add(numSati);

            // Label za minute
            Label lblMinute = new Label();
            lblMinute.Text = "Минути:";
            lblMinute.Font = new Font("Arial", 10, FontStyle.Bold);
            lblMinute.ForeColor = Color.DarkSlateGray;
            lblMinute.Size = new Size(60, 25);
            lblMinute.Location = new Point(200, 50);
            lblMinute.TextAlign = ContentAlignment.MiddleRight;
            mainPanel.Controls.Add(lblMinute);

            // NumericUpDown za minute
            numMinute = new NumericUpDown();
            numMinute.Minimum = 0;
            numMinute.Maximum = 59;
            numMinute.Increment = 15; // Koraci od 15 minuta
            numMinute.Value = 0;
            numMinute.Size = new Size(60, 20);
            numMinute.Location = new Point(270, 52);
            numMinute.Font = new Font("Arial", 10, FontStyle.Regular);
            mainPanel.Controls.Add(numMinute);

            // Dugme za potvrdu
            btnPotvrdi = new Button();
            btnPotvrdi.Text = "✅ Потврди";
            btnPotvrdi.Location = new Point(80, 90);
            btnPotvrdi.Size = new Size(100, 30);
            btnPotvrdi.BackColor = Color.DodgerBlue;
            btnPotvrdi.ForeColor = Color.White;
            btnPotvrdi.Font = new Font("Arial", 10, FontStyle.Bold);
            btnPotvrdi.FlatStyle = FlatStyle.Flat;
            btnPotvrdi.FlatAppearance.BorderSize = 0;
            btnPotvrdi.Cursor = Cursors.Hand;
            btnPotvrdi.Click += BtnPotvrdi_Click;
            mainPanel.Controls.Add(btnPotvrdi);

            // Dugme za otkazivanje
            btnOtkazi = new Button();
            btnOtkazi.Text = "❌ Откажи";
            btnOtkazi.Location = new Point(190, 90);
            btnOtkazi.Size = new Size(100, 30);
            btnOtkazi.BackColor = Color.Gray;
            btnOtkazi.ForeColor = Color.White;
            btnOtkazi.Font = new Font("Arial", 10, FontStyle.Bold);
            btnOtkazi.FlatStyle = FlatStyle.Flat;
            btnOtkazi.FlatAppearance.BorderSize = 0;
            btnOtkazi.Cursor = Cursors.Hand;
            btnOtkazi.Click += (s, e) => {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            mainPanel.Controls.Add(btnOtkazi);

            // ToolTip za podrazumevane vrednosti
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(numSati, "Подразумевано: 12 сати за Рад, 8 сати за остале статусе");
            toolTip.SetToolTip(numMinute, "Унесите минуте (0-59), корак 15 минута");

            // Auto-select prvog polja
            numSati.Select(0, numSati.Text.Length);
            numSati.Focus();
        }

        private void BtnPotvrdi_Click(object sender, EventArgs e)
        {
            IzabraniSati = (int)numSati.Value;
            IzabraneMinute = (int)numMinute.Value;

            System.Diagnostics.Debug.WriteLine($"✅ Изабрано време: {IzabraniSati}:{IzabraneMinute:00}");

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Pomocna metoda za formatiranje vremena
        public string GetFormatiranoVreme()
        {
            return $"{IzabraniSati:00}:{IzabraneMinute:00}";
        }

        // Pomocna metoda za ukupne minute
        public int GetUkupnoMinuta()
        {
            return (IzabraniSati * 60) + IzabraneMinute;
        }
    }
}