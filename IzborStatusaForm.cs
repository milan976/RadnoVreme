﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace RadnoVreme
{
    public partial class IzborStatusaForm : Form
    {
        public string IzabraniStatus { get; private set; }
        public bool JeNocnaSmena { get; private set; }
        public bool JePrviDeoNocne { get; private set; }
        public int IzabraniSati { get; private set; }
        public int IzabraneMinute { get; private set; }

        private int mesec;
        private int dan;
        private int godina;
        private string trenutniStatus;

        private CheckBox chkNocnaSmena;
        private GroupBox groupNocnaDeo;
        private RadioButton rbPrviDeo;
        private RadioButton rbDrugiDeo;
        private Button btnPotvrdi;
        private Button btnOdustani;
        private ComboBox cmbStatus;

        public IzborStatusaForm(int mesec, int dan, int godina, string trenutniStatus = null)
        {
            this.mesec = mesec;
            this.dan = dan;
            this.godina = godina;
            this.trenutniStatus = trenutniStatus;

            this.JeNocnaSmena = false;
            this.JePrviDeoNocne = true;
            this.IzabraniSati = 0;
            this.IzabraneMinute = 0;

            InitializeComponent();
            KreirajFormu();
        }

        private void InitializeComponent()
        {
            // Ова метода може остати празна
        }

        private void KreirajFormu()
        {
            this.Text = "Изаберите статус за дан";
            this.Size = new Size(400, 400); // ★★★ ПОВЕЋАНА ВИСИНА ★★★
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            // Датум информација
            DateTime datum = new DateTime(godina, mesec, dan);
            Label lblDatum = new Label();
            lblDatum.Text = $"📅 {datum:dd.MM.yyyy} ({datum:dddd})";
            lblDatum.Font = new Font("Arial", 12, FontStyle.Bold);
            lblDatum.ForeColor = Color.DarkBlue;
            lblDatum.Size = new Size(350, 25);
            lblDatum.Location = new Point(20, 20);
            lblDatum.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblDatum);

            if (!string.IsNullOrEmpty(trenutniStatus))
            {
                Label lblTrenutni = new Label();
                lblTrenutni.Text = $"Тренутно: {trenutniStatus}";
                lblTrenutni.Font = new Font("Arial", 9, FontStyle.Italic);
                lblTrenutni.ForeColor = Color.DarkSlateGray;
                lblTrenutni.Size = new Size(350, 20);
                lblTrenutni.Location = new Point(20, 50);
                lblTrenutni.TextAlign = ContentAlignment.MiddleCenter;
                this.Controls.Add(lblTrenutni);
            }

            // Статус
            Label lblStatus = new Label();
            lblStatus.Text = "Тип смене/статус:";
            lblStatus.Font = new Font("Arial", 10, FontStyle.Bold);
            lblStatus.ForeColor = Color.DarkSlateGray;
            lblStatus.Size = new Size(150, 20);
            lblStatus.Location = new Point(20, 85);
            this.Controls.Add(lblStatus);

            cmbStatus = new ComboBox();
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.Font = new Font("Arial", 10);
            cmbStatus.Size = new Size(250, 25);
            cmbStatus.Location = new Point(20, 110);

            // ★★★ ПРОМЕНА: СВИ СТАТУСИ МОГУ ДА ИМАЈУ САТЕ ★★★
            cmbStatus.Items.AddRange(new object[] {
        "ДНЕВНА СМЕНА (07:00-19:00)",
        "НОЋНА СМЕНА - ПРВИ ДЕО (19:00-24:00)",
        "ПРЕДАЈА ДУЖНОСТИ (00:00-07:00)",
        "ОДМОР",
        "ГОДИШЊИ",
        "БОЛОВАЊЕ",
        "ПЛАЋЕНИ ОДСУСТВО",
        "ССПК",
        "СЛУЖБЕНИ ПУТ",
        "СЛАВА",
        "ДРУГО (ручни унос)",
        "--- ВРАТИ НА ГЕНЕРАЛНИ РАСПОРЕД ---"
    });

            // ★★★ СЕЛЕКТУЈ ТРЕНУТНИ СТАТУС ★★★
            SelektujTrenutniStatus();

            cmbStatus.SelectedIndexChanged += CmbStatus_SelectedIndexChanged;
            this.Controls.Add(cmbStatus);

            // ★★★ ОПИС ★★★
            Label lblOpis = new Label();
            lblOpis.Text = "💡 Ноћна смена се аутоматски дели на два дана:\n• Први део (19:00-24:00)\n• Други део (00:00-07:00)";
            lblOpis.Font = new Font("Arial", 8, FontStyle.Italic);
            lblOpis.ForeColor = Color.Gray;
            lblOpis.Size = new Size(340, 40);
            lblOpis.Location = new Point(20, 140);
            this.Controls.Add(lblOpis);

            //  ДОДАТ ПОЛЕ ЗА УНОС САТИ ЗА СВЕ СТАТУСЕ 
            Label lblSati = new Label();
            lblSati.Text = "Унесите сате:";
            lblSati.Font = new Font("Arial", 10, FontStyle.Bold);
            lblSati.ForeColor = Color.DarkSlateGray;
            lblSati.Size = new Size(100, 20);
            lblSati.Location = new Point(20, 190);
            this.Controls.Add(lblSati);

            NumericUpDown numSati = new NumericUpDown();
            numSati.Name = "numSati";
            numSati.Minimum = 0;
            numSati.Maximum = 24;
            numSati.Value = 0;
            numSati.Size = new Size(60, 20);
            numSati.Location = new Point(130, 190);
            numSati.Font = new Font("Arial", 10);
            this.Controls.Add(numSati);

            Label lblMinute = new Label();
            lblMinute.Text = "минути:";
            lblMinute.Font = new Font("Arial", 10, FontStyle.Bold);
            lblMinute.ForeColor = Color.DarkSlateGray;
            lblMinute.Size = new Size(50, 20);
            lblMinute.Location = new Point(200, 190);
            this.Controls.Add(lblMinute);

            NumericUpDown numMinute = new NumericUpDown();
            numMinute.Name = "numMinute";
            numMinute.Minimum = 0;
            numMinute.Maximum = 59;
            numMinute.Increment = 15;
            numMinute.Value = 0;
            numMinute.Size = new Size(60, 20);
            numMinute.Location = new Point(260, 190);
            numMinute.Font = new Font("Arial", 10);
            this.Controls.Add(numMinute);

            //  ПОСТАВИ ПОДРАЗУМЕВАНЕ ВРЕДНОСТИ ПРЕМА ИЗАБРАНОМ СТАТУСУ 
            cmbStatus.SelectedIndexChanged += (s, e) =>
            {
                string izabrani = cmbStatus.SelectedItem?.ToString() ?? "";

                //  ПОДРАЗУМЕВАНЕ ВРЕДНОСТИ 
                if (izabrani.Contains("ДНЕВНА СМЕНА"))
                {
                    numSati.Value = 12;
                    numMinute.Value = 0;
                }
                else if (izabrani.Contains("НОЋНА СМЕНА - ПРВИ ДЕО"))
                {
                    numSati.Value = 5;
                    numMinute.Value = 0;
                }
                else if (izabrani.Contains("ПРЕДАЈА ДУЖНОСТИ"))
                {
                    numSati.Value = 7;
                    numMinute.Value = 0;
                }
                else if (izabrani.Contains("ОДМОР"))
                {
                    numSati.Value = 0;
                    numMinute.Value = 0;
                }
                else if (izabrani.Contains("ГОДИШЊИ") || izabrani.Contains("БОЛОВАЊЕ") ||
                         izabrani.Contains("ПЛАЋЕНИ") || izabrani.Contains("ССПК") ||
                         izabrani.Contains("СЛУЖБЕНИ") || izabrani.Contains("СЛАВА"))
                {
                    numSati.Value = 8;
                    numMinute.Value = 0;
                }
                else if (izabrani.Contains("ДРУГО"))
                {
                    numSati.Value = 8;
                    numMinute.Value = 0;
                }
            };

            // Дугмићи
            btnPotvrdi = new Button();
            btnPotvrdi.Text = "✅ Потврди";
            btnPotvrdi.Size = new Size(120, 35);
            btnPotvrdi.Location = new Point(60, 230);
            btnPotvrdi.BackColor = Color.DodgerBlue;
            btnPotvrdi.ForeColor = Color.White;
            btnPotvrdi.Font = new Font("Arial", 10, FontStyle.Bold);
            btnPotvrdi.FlatStyle = FlatStyle.Flat;
            btnPotvrdi.FlatAppearance.BorderSize = 0;
            btnPotvrdi.Cursor = Cursors.Hand;
            btnPotvrdi.Click += (s, e) => BtnPotvrdi_Click(s, e, numSati.Value, numMinute.Value);
            this.Controls.Add(btnPotvrdi);

            btnOdustani = new Button();
            btnOdustani.Text = "❌ Одustani";
            btnOdustani.Size = new Size(120, 35);
            btnOdustani.Location = new Point(200, 230);
            btnOdustani.BackColor = Color.Gray;
            btnOdustani.ForeColor = Color.White;
            btnOdustani.Font = new Font("Arial", 10, FontStyle.Bold);
            btnOdustani.FlatStyle = FlatStyle.Flat;
            btnOdustani.FlatAppearance.BorderSize = 0;
            btnOdustani.Cursor = Cursors.Hand;
            btnOdustani.Click += BtnOdustani_Click;
            this.Controls.Add(btnOdustani);
        }

        private void SelektujTrenutniStatus()
        {
            if (string.IsNullOrEmpty(trenutniStatus))
            {
                cmbStatus.SelectedIndex = 0;
                return;
            }

            for (int i = 0; i < cmbStatus.Items.Count; i++)
            {
                string item = cmbStatus.Items[i].ToString();

                if (trenutniStatus.Contains("ДНЕВНА") || (trenutniStatus.Contains("Рад") && !trenutniStatus.Contains("ПРЕДАЈА")))
                {
                    if (item.Contains("ДНЕВНА СМЕНА"))
                    {
                        cmbStatus.SelectedIndex = i;
                        return;
                    }
                }
                else if (trenutniStatus.Contains("НОЋНА_ПРВИ_ДЕО"))
                {
                    if (item.Contains("НОЋНА СМЕНА - ПРВИ ДЕО"))
                    {
                        cmbStatus.SelectedIndex = i;
                        return;
                    }
                }
                else if (trenutniStatus.Contains("ПРЕДАЈА_ДУЖНОСТИ"))
                {
                    if (item.Contains("ПРЕДАЈА ДУЖНОСТИ"))
                    {
                        cmbStatus.SelectedIndex = i;
                        return;
                    }
                }
                else if (trenutniStatus.Contains("Слободан") || trenutniStatus.Contains("ОДМОР"))
                {
                    if (item.Contains("ОДМОР"))
                    {
                        cmbStatus.SelectedIndex = i;
                        return;
                    }
                }
                else if (trenutniStatus.Contains("Годишњи"))
                {
                    if (item.Contains("ГОДИШЊИ"))
                    {
                        cmbStatus.SelectedIndex = i;
                        return;
                    }
                }
                else if (trenutniStatus.Contains("Боловање"))
                {
                    if (item.Contains("БОЛОВАЊЕ"))
                    {
                        cmbStatus.SelectedIndex = i;
                        return;
                    }
                }
                else if (trenutniStatus.Contains("Плаћено"))
                {
                    if (item.Contains("ПЛАЋЕНИ"))
                    {
                        cmbStatus.SelectedIndex = i;
                        return;
                    }
                }
                else if (trenutniStatus.Contains("ССПК"))
                {
                    if (item.Contains("ССПК"))
                    {
                        cmbStatus.SelectedIndex = i;
                        return;
                    }
                }
                else if (trenutniStatus.Contains("Службено"))
                {
                    if (item.Contains("СЛУЖБЕНИ"))
                    {
                        cmbStatus.SelectedIndex = i;
                        return;
                    }
                }
                else if (trenutniStatus.Contains("Слава"))
                {
                    if (item.Contains("СЛАВА"))
                    {
                        cmbStatus.SelectedIndex = i;
                        return;
                    }
                }
            }

            cmbStatus.SelectedIndex = 0;
        }

        private void CmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Ништа више не радимо овде - све је већ дефинисано у текстоптима
        }

        private void BtnPotvrdi_Click(object sender, EventArgs e, decimal sati, decimal minute)
        {
            string izabrani = cmbStatus.SelectedItem.ToString();
            IzabraniSati = (int)sati;
            IzabraneMinute = (int)minute;

            System.Diagnostics.Debug.WriteLine($"✅ Изабрано: {izabrani} ({IzabraniSati}:{IzabraneMinute:00})");

            // ★★★ МАПИРАЊЕ ИЗАБРАНОГ ТЕКСТА НА СТАТУС ★★★
            if (izabrani.Contains("ДНЕВНА СМЕНА"))
            {
                IzabraniStatus = "ДНЕВНА";
                JeNocnaSmena = false;
                JePrviDeoNocne = false;
            }
            else if (izabrani.Contains("НОЋНА СМЕНА - ПРВИ ДЕО"))
            {
                IzabraniStatus = "НОЋНА_ПРВИ_ДЕО";
                JeNocnaSmena = true;
                JePrviDeoNocne = true;
            }
            else if (izabrani.Contains("ПРЕДАЈА ДУЖНОСТИ"))
            {
                IzabraniStatus = "ПРЕДАЈА_ДУЖНОСТИ";
                JeNocnaSmena = true;
                JePrviDeoNocne = false;
            }
            else if (izabrani.Contains("ОДМОР"))
            {
                IzabraniStatus = "Слободан";
                JeNocnaSmena = false;
                JePrviDeoNocne = false;
            }
            else if (izabrani.Contains("ГОДИШЊИ"))
            {
                IzabraniStatus = "Годишњи";
                JeNocnaSmena = false;
                JePrviDeoNocne = false;
            }
            else if (izabrani.Contains("БОЛОВАЊЕ"))
            {
                IzabraniStatus = "Боловање";
                JeNocnaSmena = false;
                JePrviDeoNocne = false;
            }
            else if (izabrani.Contains("ПЛАЋЕНИ"))
            {
                IzabraniStatus = "Плаћено";
                JeNocnaSmena = false;
                JePrviDeoNocne = false;
            }
            else if (izabrani.Contains("ССПК"))
            {
                IzabraniStatus = "ССПК";
                JeNocnaSmena = false;
                JePrviDeoNocne = false;
            }
            else if (izabrani.Contains("СЛУЖБЕНИ"))
            {
                IzabraniStatus = "Службено";
                JeNocnaSmena = false;
                JePrviDeoNocne = false;
            }
            else if (izabrani.Contains("СЛАВА"))
            {
                IzabraniStatus = "Слава";
                JeNocnaSmena = false;
                JePrviDeoNocne = false;
            }
            else if (izabrani.Contains("ДРУГО"))
            {
                IzabraniStatus = "Друго";
                JeNocnaSmena = false;
                JePrviDeoNocne = false;
            }
            else if (izabrani.Contains("ВРАТИ НА ГЕНЕРАЛНИ"))
            {
                IzabraniStatus = "VRATI_NA_GENERALNI";
                JeNocnaSmena = false;
                JePrviDeoNocne = false;
                IzabraniSati = 0;
                IzabraneMinute = 0;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnOdustani_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
