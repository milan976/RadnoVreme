using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data;

namespace RadnoVreme
{
    public partial class DetaljiRadnikaForm : Form
    {
        private int radnikId;
        private int godina;
        private BazaService bazaService;
        private Radnik radnik;

        private Label lblNaslov;
        private Label lblStatistika;
        private DataGridView dgvDetalji;
        private Button btnZatvori;
        private Button btnExport;

        public DetaljiRadnikaForm(int radnikId, int godina)
        {
            this.radnikId = radnikId;
            this.godina = godina;
            this.bazaService = new BazaService();

            InitializeComponent();
            KreirajFormu();
            UcitajDetaljeRadnika();
        }


        private void KreirajFormu()
        {
            // Postavke forme
            this.Text = "📊 Детаљи радника";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            // Glavni panel
            Panel mainPanel = new Panel();
            mainPanel.Size = new Size(950, 630);
            mainPanel.Location = new Point(10, 10);
            mainPanel.BackColor = Color.White;
            mainPanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(mainPanel);

            // Naslov
            lblNaslov = new Label();
            lblNaslov.Text = "📊 УЧИТАВАМ ПОДАТКЕ...";
            lblNaslov.Font = new Font("Arial", 16, FontStyle.Bold);
            lblNaslov.ForeColor = Color.DarkBlue;
            lblNaslov.Size = new Size(900, 40);
            lblNaslov.Location = new Point(25, 20);
            lblNaslov.TextAlign = ContentAlignment.MiddleCenter;
            mainPanel.Controls.Add(lblNaslov);

            // DataGridView za detalje
            dgvDetalji = new DataGridView();
            dgvDetalji.Location = new Point(25, 70);
            dgvDetalji.Size = new Size(900, 400);
            dgvDetalji.BackgroundColor = Color.White;
            dgvDetalji.BorderStyle = BorderStyle.FixedSingle;
            dgvDetalji.ReadOnly = true;
            dgvDetalji.AllowUserToAddRows = false;
            dgvDetalji.AllowUserToDeleteRows = false;
            dgvDetalji.AllowUserToResizeRows = false;
            dgvDetalji.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDetalji.RowHeadersVisible = false;
            dgvDetalji.Font = new Font("Arial", 9, FontStyle.Regular);
            dgvDetalji.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.DodgerBlue,
                ForeColor = Color.White,
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            mainPanel.Controls.Add(dgvDetalji);

            // Label za ukupnu statistiku
            lblStatistika = new Label();
            lblStatistika.Font = new Font("Arial", 10, FontStyle.Bold);
            lblStatistika.ForeColor = Color.DarkGreen;
            lblStatistika.Size = new Size(900, 80);
            lblStatistika.Location = new Point(25, 480);
            lblStatistika.TextAlign = ContentAlignment.MiddleLeft;
            lblStatistika.BackColor = Color.LightYellow;
            lblStatistika.BorderStyle = BorderStyle.FixedSingle;
            mainPanel.Controls.Add(lblStatistika);

            // Dugme za export
            btnExport = new Button();
            btnExport.Text = "💾 Извези у Excel";
            btnExport.Location = new Point(25, 570);
            btnExport.Size = new Size(150, 35);
            btnExport.BackColor = Color.Green;
            btnExport.ForeColor = Color.White;
            btnExport.Font = new Font("Arial", 10, FontStyle.Bold);
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Cursor = Cursors.Hand;
            btnExport.Click += BtnExport_Click;
            mainPanel.Controls.Add(btnExport);

            // Dugme za zatvaranje
            btnZatvori = new Button();
            btnZatvori.Text = "🚪 Затвори";
            btnZatvori.Location = new Point(825, 570);
            btnZatvori.Size = new Size(100, 35);
            btnZatvori.BackColor = Color.Gray;
            btnZatvori.ForeColor = Color.White;
            btnZatvori.Font = new Font("Arial", 10, FontStyle.Bold);
            btnZatvori.FlatStyle = FlatStyle.Flat;
            btnZatvori.FlatAppearance.BorderSize = 0;
            btnZatvori.Cursor = Cursors.Hand;
            btnZatvori.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(btnZatvori);
        }

        private string FormatirajVreme(int sati, int minute)
        {
            if (sati == 0 && minute == 0)
                return "0 сати";

            if (minute == 0)
                return $"{sati} сати";

            if (sati == 0)
                return $"{minute} минута";

            return $"{sati} сати {minute} минута";
        }

        private void UcitajDetaljeRadnika()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Учитавам детаље за радника ИД: {radnikId}, Година: {godina}");

                // 1. Uzmi osnovne podatke o radniku
                var sviRadnici = bazaService.UzmiSveRadnike();
                radnik = sviRadnici.FirstOrDefault(r => r.Id == radnikId);

                if (radnik == null)
                {
                    MessageBox.Show("Радник није пронађен!", "Грешка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                lblNaslov.Text = $"📊 ДЕТАЉИ РАДНИКА: {radnik.PunoIme} - {radnik.Smena} ({godina}. година)";

                // 2. Uzmi sve statuse radnika za ovu godinu
                var sacuvaniStatusi = bazaService.UcitajSacuvaneStatuse(radnikId, godina);
                System.Diagnostics.Debug.WriteLine($"💾 Личних статуса за {godina}: {sacuvaniStatusi.Count}");

                // 3. Uzmi generisani raspored za ovu godinu
                var generisaniRaspored = new Dictionary<DateTime, string>();
                for (int mesec = 1; mesec <= 12; mesec++)
                {
                    var rasporedZaMesec = bazaService.UzmiRasporedZaSmenu(radnik.Smena, godina, mesec);
                    foreach (var stavka in rasporedZaMesec)
                    {
                        generisaniRaspored[stavka.Key] = stavka.Value;
                    }
                }
                System.Diagnostics.Debug.WriteLine($"📅 Генерисаних дана за {godina}: {generisaniRaspored.Count}");

                // 4. Odredi vremenski opseg - ★★★ УВЕК ДО ДАНАШЊЕГ ДАТУМА ★★★
                DateTime pocetakGodine = new DateTime(godina, 1, 1);
                DateTime danas = DateTime.Now.Date;

                // ★★★ УВЕК ПРИКАЗУЈЕМО ДО ДАНАШЊЕГ ДАТУМА ★★★
                int ukupnoDanaDoDanas;

                if (godina < DateTime.Now.Year)
                {
                    // Za prošle године - cela godina (jer je kraj već prošao)
                    ukupnoDanaDoDanas = 365 + (DateTime.IsLeapYear(godina) ? 1 : 0);
                }
                else if (godina == DateTime.Now.Year)
                {
                    // Za tekuћу годину - до danas
                    ukupnoDanaDoDanas = (danas - pocetakGodine).Days + 1;
                }
                else
                {
                    // За будуће године - 0 дана (јер данас није у тој години)
                    ukupnoDanaDoDanas = 0;
                }

                System.Diagnostics.Debug.WriteLine($"📅 Приказујем до {danas:dd.MM.yyyy}: {ukupnoDanaDoDanas} дана");

                // 5. Pripremi DataGridView
                dgvDetalji.Columns.Clear();

                // Dodaj kolone
                dgvDetalji.Columns.Add("Tip", "ТИП СТАТУСА");
                dgvDetalji.Columns.Add("BrojDana", "БРОЈ ДАНА");
                dgvDetalji.Columns.Add("UkupnoVreme", "УКУПНО ВРЕМЕ");
                dgvDetalji.Columns.Add("Procenat", "ПРОЦЕНАТ (%)");

                // Formatiraj kolone
                dgvDetalji.Columns["BrojDana"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvDetalji.Columns["UkupnoVreme"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvDetalji.Columns["Procenat"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvDetalji.Columns["Procenat"].DefaultCellStyle.Format = "F1";

                // 6. Grupiši statuse po tipu (ДО ДАНАС)
                var statusiDoDanas = new Dictionary<string, int>();
                var minutiPoStatusu = new Dictionary<string, int>();

                // Inicijalizuj sve statuse
                string[] sviStatusi = { "Рад", "Годишњи", "Боловање", "Слободан", "Плаћено", "ССПК", "Службено", "Слава" };
                foreach (var status in sviStatusi)
                {
                    statusiDoDanas[status] = 0;
                    minutiPoStatusu[status] = 0;
                }

                // Prođi kroz sve dane од почетка године до данас
                for (DateTime datum = pocetakGodine; datum <= danas && datum.Year == godina; datum = datum.AddDays(1))
                {
                    string statusZaDan = "";

                    // Prvo proveri lične statuse
                    if (sacuvaniStatusi.ContainsKey(datum))
                    {
                        statusZaDan = sacuvaniStatusi[datum];
                        System.Diagnostics.Debug.WriteLine($"   {datum:dd.MM.yyyy}: Лични статус - {statusZaDan}");
                    }
                    // Onda generisani raspored
                    else if (generisaniRaspored.ContainsKey(datum))
                    {
                        string smena = generisaniRaspored[datum];
                        statusZaDan = (smena == "ДНЕВНА" || smena == "НОЋНА") ? "Рад" : "Слободан";
                        System.Diagnostics.Debug.WriteLine($"   {datum:dd.MM.yyyy}: Генерисано - {smena} → {statusZaDan}");
                    }
                    // На крају подразумевано
                    else
                    {
                        statusZaDan = "Слободан";
                    }

                    // Извучи чист статус (без времена у загради)
                    string cistStatus = statusZaDan;
                    if (statusZaDan.Contains("("))
                    {
                        cistStatus = statusZaDan.Substring(0, statusZaDan.IndexOf("(")).Trim();
                    }

                    // Појединај број дана за овај статус
                    if (statusiDoDanas.ContainsKey(cistStatus))
                    {
                        statusiDoDanas[cistStatus]++;

                        // ★★★ ИЗРАЧУНАЈ ВРЕМЕ У МИНУТАМА ★★★
                        int minuti = IzracunajMinuteZaStatus(statusZaDan);
                        minutiPoStatusu[cistStatus] += minuti;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ Обрађено дана до {danas:dd.MM.yyyy}");

                // 7. Popuni DataGridView
                int ukupnoMinutaDoDanas = 0;

                foreach (var status in statusiDoDanas.Keys.OrderBy(k => k))
                {
                    int brojDana = statusiDoDanas[status];
                    int ukupnoMinuta = minutiPoStatusu[status];

                    if (brojDana > 0 && ukupnoDanaDoDanas > 0)
                    {
                        // Претвори минуте у сате и минуте
                        int sati = ukupnoMinuta / 60;
                        int minute = ukupnoMinuta % 60;

                        double procenat = (double)brojDana / ukupnoDanaDoDanas * 100;
                        ukupnoMinutaDoDanas += ukupnoMinuta;

                        // Форматирај време за приказ
                        string formatiranoVreme = FormatirajVreme(sati, minute);

                        dgvDetalji.Rows.Add(
                            GetEmojiForStatus(status) + " " + status,
                            brojDana,
                            formatiranoVreme,
                            $"{procenat:F1}%"
                        );

                        System.Diagnostics.Debug.WriteLine($"📊 {status}: {brojDana} дана, {ukupnoMinuta} минута → {formatiranoVreme} ({procenat:F1}%)");
                    }
                }

                // ★★★ ПРЕТВОРИ УКУПНЕ МИНУТЕ У САТЕ И МИНУТЕ ★★★
                int ukupnoSati = ukupnoMinutaDoDanas / 60;
                int ukupnoMinutaOstatak = ukupnoMinutaDoDanas % 60;
                string ukupnoVremeFormatirano = FormatirajVreme(ukupnoSati, ukupnoMinutaOstatak);

                // 8. Израчунај детаљну статистику
                int ukupnoRadnihSmena = statusiDoDanas.ContainsKey("Рад") ? statusiDoDanas["Рад"] : 0;
                int ukupnoMinutaRad = minutiPoStatusu.ContainsKey("Рад") ? minutiPoStatusu["Рад"] : 0;
                int radSati = ukupnoMinutaRad / 60;
                int radMinute = ukupnoMinutaRad % 60;
                string radVremeFormatirano = FormatirajVreme(radSati, radMinute);

                int ukupnoGodisnji = statusiDoDanas.ContainsKey("Годишњи") ? statusiDoDanas["Годишњи"] : 0;
                int ukupnoPlaceno = statusiDoDanas.ContainsKey("Плаћено") ? statusiDoDanas["Плаћено"] : 0;
                int ukupnoSlava = statusiDoDanas.ContainsKey("Слава") ? statusiDoDanas["Слава"] : 0;
                int ukupnoBolovanje = statusiDoDanas.ContainsKey("Боловање") ? statusiDoDanas["Боловање"] : 0;
                int ukupnoSlobodno = statusiDoDanas.ContainsKey("Слободан") ? statusiDoDanas["Слободан"] : 0;
                int ukupnoSluzbeno = statusiDoDanas.ContainsKey("Службено") ? statusiDoDanas["Службено"] : 0;
                int ukupnoSSPK = statusiDoDanas.ContainsKey("ССПК") ? statusiDoDanas["ССПК"] : 0;

                // 9. Прикажи детаљну статистику
                string statistikaText = $"📈 СТАТИСТИКА ДО {danas:dd.MM.yyyy} ({ukupnoDanaDoDanas} дана):\n" +
                                      $"✅ Рад: {ukupnoRadnihSmena} смена ({radVremeFormatirano}) | " +
                                      $"🏖️ Годишњи: {ukupnoGodisnji} дана | " +
                                      $"💰 Плаћено: {ukupnoPlaceno} дана |\n" +
                                      $"⭐ Слава: {ukupnoSlava} дана | " +
                                      $"🤒 Боловање: {ukupnoBolovanje} дана | " +
                                      $"🏢 Службено: {ukupnoSluzbeno} дана |\n" +
                                      $"📝 ССПК: {ukupnoSSPK} дана | " +
                                      $"⚪ Слободно: {ukupnoSlobodno} дана | " +
                                      $"⏰ Укупно време: {ukupnoVremeFormatirano}";

                lblStatistika.Text = statistikaText;

                System.Diagnostics.Debug.WriteLine($"✅ Учитани детаљи за радника {radnik.PunoIme}");
                System.Diagnostics.Debug.WriteLine($"📊 Укупно време: {ukupnoVremeFormatirano} ({ukupnoMinutaDoDanas} минута)");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при учитавању детаља: {ex.Message}");
                MessageBox.Show($"Грешка при учитавању детаља: {ex.Message}", "Грешка");
            }
        }

        private int IzracunajMinuteZaStatus(string status)
        {
            // Ако статус има време у загради: "Рад (8:30)" → 8×60 + 30 = 510 минута
            if (status.Contains("(") && status.Contains(")"))
            {
                int startIndex = status.IndexOf("(") + 1;
                int endIndex = status.IndexOf(")");
                string vreme = status.Substring(startIndex, endIndex - startIndex);

                if (vreme.Contains(":"))
                {
                    string[] delovi = vreme.Split(':');
                    if (delovi.Length == 2 &&
                        int.TryParse(delovi[0], out int sati) &&
                        int.TryParse(delovi[1], out int minute))
                    {
                        return (sati * 60) + minute;
                    }
                }
            }

            // Подразумеване вредности
            string cistStatus = status;
            if (status.Contains("("))
            {
                cistStatus = status.Substring(0, status.IndexOf("(")).Trim();
            }

            switch (cistStatus)
            {
                case "Рад":
                    return 12 * 60; // 12 сати = 720 минута
                case "Годишњи":
                case "Боловање":
                case "Плаћено":
                case "ССПК":
                case "Службено":
                case "Слава":
                    return 8 * 60; // 8 сати = 480 минута
                case "Слободан":
                default:
                    return 0;
            }
        }

        /*private string FormatirajVreme(int sati, int minute)
        {
            if (sati == 0 && minute == 0)
                return "0 сати";

            if (minute == 0)
                return $"{sati} сати";

            if (sati == 0)
                return $"{minute} минута";

            return $"{sati} сати {minute} минута";
        }*/

        private int IzracunajSateZaStatus(string status)
        {
            // ★★★ PROVERI DA LI STATUS IMA SATE U ZAGRADI ★★★
            if (status.Contains("(") && status.Contains(")"))
            {
                int startIndex = status.IndexOf("(") + 1;
                int endIndex = status.IndexOf(")");
                string vreme = status.Substring(startIndex, endIndex - startIndex);

                if (vreme.Contains(":"))
                {
                    string[] delovi = vreme.Split(':');
                    if (delovi.Length == 2 &&
                        int.TryParse(delovi[0], out int sati) &&
                        int.TryParse(delovi[1], out int minute))
                    {
                        // Vrati ukupne minute
                        return (sati * 60) + minute;
                    }
                }
            }

            // ★★★ PODRAZUMEVANE VREDNOSTI - OVO SE SADA NEĆE KORISTITI AKO IMA SATI ★★★
            // Ovaj deo će se koristiti samo za stare statuse bez sati
            string cistStatus = status;
            if (status.Contains("("))
            {
                cistStatus = status.Substring(0, status.IndexOf("(")).Trim();
            }

            switch (cistStatus)
            {
                case "Рад":
                    return 12 * 60; // 12 sati = 720 minuta
                case "Годишњи":
                case "Боловање":
                case "Плаћено":
                case "ССПК":
                case "Службено":
                case "Слава":
                    return 8 * 60; // 8 sati = 480 minuta
                case "Слободан":
                default:
                    return 0;
            }
        }

        private string GetEmojiForStatus(string status)
        {
            switch (status)
            {
                case "Рад": return "⏰";
                case "Годишњи": return "🏖️";
                case "Боловање": return "🤒";
                case "Слободан": return "😴";
                case "Плаћено": return "💰";
                case "ССПК": return "📝";
                case "Службено": return "🏢";
                case "Слава": return "⭐";
                default: return "⚪";
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel файл (*.xlsx)|*.xlsx|CSV фајл (*.csv)|*.csv";
                saveDialog.FileName = $"Детаљи_{radnik.Prezime}_{radnik.Ime}_{godina}";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (saveDialog.FileName.EndsWith(".csv"))
                    {
                        ExportToCSV(saveDialog.FileName);
                    }
                    else
                    {
                        ExportToExcel(saveDialog.FileName);
                    }

                    MessageBox.Show($"Подаци су успешно извезени!\n{saveDialog.FileName}",
                                  "Извоз", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при извозу: {ex.Message}", "Грешка");
            }
        }

        private void ExportToCSV(string fileName)
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(fileName, false, System.Text.Encoding.UTF8))
            {
                // Header
                writer.WriteLine("Тип статуса;Број дана;Укупно сати;Проценат");

                // Podaci
                foreach (DataGridViewRow row in dgvDetalji.Rows)
                {
                    if (row.Cells[0].Value != null)
                    {
                        writer.WriteLine($"{row.Cells[0].Value};{row.Cells[1].Value};{row.Cells[2].Value};{row.Cells[3].Value}");
                    }
                }

                // Prazna linija
                writer.WriteLine();

                // Statistika
                writer.WriteLine($"Статистика до: {DateTime.Now:dd.MM.yyyy}");
                writer.WriteLine(lblStatistika.Text.Replace("\n", ";"));
            }
        }

        private void ExportToExcel(string fileName)
        {
            // Za Excel export bi trebalo dodati referencu na Microsoft.Office.Interop.Excel
            // Pošto to zahteva dodatne instalacije, koristimo CSV koji može da otvori Excel
            ExportToCSV(fileName.Replace(".xlsx", ".csv"));

            MessageBox.Show("Извезено као CSV фајл (можете га отворити у Excel-у).", "Информација");
        }
    }
}