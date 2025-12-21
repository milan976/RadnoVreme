using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RadnoVreme
{
    public partial class RadnoVremeForm : Form
    {
        private string radnikIme;
        private Button btnPrvih6;
        private Button btnDrugih6;
        private Panel panelKalendar;
        private Label lblRadnik;
        private Label lblGodina;
        private NumericUpDown numGodina;
        private ToolTip toolTip;
        private Label lblGlobalnaStatistika;
        private Button btnDetaljiRadnika;
        private Button btnOsvezi;
        private BazaService bazaService;

        // Trenutno prikazano polugodište
        private bool prikazPrvih6 = true;
        private int trenutnaGodina;

        // Čuva broj radnih dana po mesecima
        private Dictionary<int, int> radniDaniPoMesecu = new Dictionary<int, int>();

        private Dictionary<DateTime, string> generisaniRaspored;
        private Dictionary<DateTime, string> sacuvaniStatusi;
        private int trenutniRadnikId;

        public RadnoVremeForm(string radnikIme)
        {
            System.Diagnostics.Debug.WriteLine($"🚀 KREIRANJE RadnoVremeForm za: '{radnikIme}'");

            this.radnikIme = radnikIme;
            this.trenutnaGodina = DateTime.Now.Year;
            this.prikazPrvih6 = DateTime.Now.Month <= 6;
            this.toolTip = new ToolTip();
            this.generisaniRaspored = new Dictionary<DateTime, string>();
            this.sacuvaniStatusi = new Dictionary<DateTime, string>();
            this.bazaService = new BazaService();

            this.InicijalizujRadneDane();
            this.InitializeComponent();
            this.KreirajFormu();

            bazaService.TestirajSate();
            bazaService.AzurirajStrukturuBaze();

            UcitajPodatkeORadniku();
            NacrtajKalendar();
        }

        private void InitializeComponent()
        {
            // Ova metoda može ostati prazna jer ručno kreiramo kontrole
        }


        /*private void UcitajPodatkeORadniku()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 ===== УЧИТАВАЊЕ ПОДАТАКА ЗА: '{radnikIme}' =====");

                BazaService bazaService = new BazaService();
                var sviRadnici = bazaService.UzmiSveRadnike();

                System.Diagnostics.Debug.WriteLine($"📋 Пронађено {sviRadnici.Count} радника у бази:");
                foreach (var r in sviRadnici)
                {
                    System.Diagnostics.Debug.WriteLine($"   - ИД: {r.Id}, Име: '{r.Ime}', Презиме: '{r.Prezime}', Пуно име: '{r.PunoIme}'");
                }

                // Pronađi radnika - POBOLJŠANA PRETRAGA
                var radnik = sviRadnici.FirstOrDefault(r =>
                    radnikIme.Trim().Equals(r.Ime.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    radnikIme.Trim().Equals(r.Prezime.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    radnikIme.Trim().Equals(r.PunoIme.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    r.PunoIme.Trim().Equals(radnikIme.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    r.PunoIme.Trim().Contains(radnikIme.Trim()));

                if (radnik != null)
                {
                    trenutniRadnikId = radnik.Id;
                    System.Diagnostics.Debug.WriteLine($"✅ ПРОНАЂЕН РАДНИК: {radnik.PunoIme} - {radnik.Smena} (ID: {radnik.Id})");

                    // OČISTI SVE PRE UČITAVANJA
                    generisaniRaspored = new Dictionary<DateTime, string>();
                    sacuvaniStatusi = new Dictionary<DateTime, string>();

                    // 1. UČITAJ GENERISANI RASPORED ZA CELE 6 MESECI
                    System.Diagnostics.Debug.WriteLine($"📅 Учитавам генерисани распоред за смену: {radnik.Smena}");
                    int startMesec = prikazPrvih6 ? 1 : 7;
                    int endMesec = prikazPrvih6 ? 6 : 12;

                    for (int mesec = startMesec; mesec <= endMesec; mesec++)
                    {
                        var rasporedZaMesec = bazaService.UzmiRasporedZaSmenu(radnik.Smena, trenutnaGodina, mesec);
                        System.Diagnostics.Debug.WriteLine($"   - Месец {mesec}: {rasporedZaMesec.Count} дана");
                        foreach (var stavka in rasporedZaMesec)
                        {
                            generisaniRaspored[stavka.Key] = stavka.Value;
                        }
                    }

                    // 2. UČITAJ LIČNE STATUSE
                    System.Diagnostics.Debug.WriteLine($"💾 Учитавам личне статусе за радника ИД: {radnik.Id}");
                    sacuvaniStatusi = bazaService.UcitajSacuvaneStatuse(radnik.Id, trenutnaGodina);

                    System.Diagnostics.Debug.WriteLine($"📊 ЗАВРШЕНО: {generisaniRaspored.Count} генерисаних + {sacuvaniStatusi.Count} личних статуса");

                    AzurirajStatistiku();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ РАДНИК '{radnikIme}' НИЈЕ ПРОНАЂЕН!");
                    MessageBox.Show($"❌ Радник '{radnikIme}' није пронађен!\n\nПроверите да ли је тачно унето име.", "Грешка");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 ГРЕШКА у UcitajPodatkeORadniku: {ex.Message}");
                MessageBox.Show($"Грешка при учитавању података: {ex.Message}", "Грешка");
            }
        }*/

        private void LblDan_Click(object sender, EventArgs e)
        {
            Label lblDan = (Label)sender;
            Tuple<int, int> tag = (Tuple<int, int>)lblDan.Tag;
            int mesec = tag.Item1;
            int dan = tag.Item2;
            DateTime datum = new DateTime(trenutnaGodina, mesec, dan);

            // Provera trenutnog stanja
            string trenutniStatus = "Непознато";
            string izvorStatusa = "Непознат";

            if (generisaniRaspored.ContainsKey(datum))
            {
                string smena = generisaniRaspored[datum];
                if (smena == "ДНЕВНА")
                {
                    trenutniStatus = "ДНЕВНА СМЕНА (07:00-19:00) - 12 сати";
                    izvorStatusa = "📅 Генерални распоред (Дневна)";
                }
                else if (smena == "НОЋНА_ПРВИ_ДЕО")
                {
                    trenutniStatus = "НОЋНА СМЕНА - ПРВИ ДЕО (19:00-24:00) - 5 сати";
                    izvorStatusa = "📅 Генерални распоред (Ноћна - први део)";
                }
                else if (smena == "ПРЕДАЈА_ДУЖНОСТИ")
                {
                    trenutniStatus = "ПРЕДАЈА ДУЖНОСТИ (00:00-07:00) - 7 сати";
                    izvorStatusa = "📅 Генерални распоред (Предаја дужности)";
                }
            }
            else
            {
                if (datum.DayOfWeek == DayOfWeek.Saturday || datum.DayOfWeek == DayOfWeek.Sunday)
                {
                    trenutniStatus = "Слободан";
                    izvorStatusa = "Викенд";
                }
                else
                {
                    trenutniStatus = "Слободан";
                    izvorStatusa = "Радни дан";
                }
            }

            System.Diagnostics.Debug.WriteLine($"🔄 Кликнут дан: {datum:dd.MM.yyyy}");
            System.Diagnostics.Debug.WriteLine($"   - Тренутни статус: {trenutniStatus} ({izvorStatusa})");
            System.Diagnostics.Debug.WriteLine($"   - Радник ИД: {trenutniRadnikId}");
            System.Diagnostics.Debug.WriteLine($"   - Радник: {radnikIme}");

            // ★★★ КОРИГОВАНО: Користимо ажурирани IzborStatusaForm СА УНОСОМ САТИ ★★★
            using (IzborStatusaForm statusForm = new IzborStatusaForm(mesec, dan, trenutnaGodina, trenutniStatus))
            {
                if (statusForm.ShowDialog() == DialogResult.OK)
                {
                    string noviStatus = statusForm.IzabraniStatus;
                    bool jeNocnaSmena = statusForm.JeNocnaSmena;
                    bool jePrviDeoNocne = statusForm.JePrviDeoNocne;
                    int izabraniSati = statusForm.IzabraniSati;
                    int izabraneMinute = statusForm.IzabraneMinute;

                    System.Diagnostics.Debug.WriteLine($"🔄 Корисник је изабрао:");
                    System.Diagnostics.Debug.WriteLine($"   - Нови статус: {noviStatus}");
                    System.Diagnostics.Debug.WriteLine($"   - Ноћна смена: {jeNocnaSmena}");
                    System.Diagnostics.Debug.WriteLine($"   - Први део ноћне: {jePrviDeoNocne}");
                    System.Diagnostics.Debug.WriteLine($"   - Сати: {izabraniSati}:{izabraneMinute:00}");

                    BazaService bazaService = new BazaService();
                    bool uspesno = false;

                    // ★★★ POSEBNA LOGIKA ZA VRATANJE NA GENERALNI RASPORED ★★★
                    if (noviStatus == "VRATI_NA_GENERALNI")
                    {
                        System.Diagnostics.Debug.WriteLine($"🗑️ Враћање на генерални распоред за {datum:dd.MM.yyyy}");

                        uspesno = bazaService.ObrisiStatusRadnika(trenutniRadnikId, datum);

                        if (uspesno)
                        {
                            if (sacuvaniStatusi.ContainsKey(datum))
                            {
                                sacuvaniStatusi.Remove(datum);
                            }

                            System.Diagnostics.Debug.WriteLine($"✅ УКЛОЊЕН лични статус за {datum:dd.MM.yyyy}");

                            MessageBox.Show($"✅ Статус је враћен на генерални распоред!\n{datum:dd.MM.yyyy}",
                                          "Враћање", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        // ★★★ ЗА СВЕ СТАТУСЕ САДА ЧУВАМО САТЕ ★★★
                        System.Diagnostics.Debug.WriteLine($"💾 Чувам статус са сатима: {noviStatus} ({izabraniSati}:{izabraneMinute:00})");

                        // ★★★ НОВА ЛОГИКА: ПОДЕЛА НОЋНИХ СМЕНА ★★★
                        if (jeNocnaSmena)
                        {
                            System.Diagnostics.Debug.WriteLine($"🌙 Обрада ноћне смене:");
                            System.Diagnostics.Debug.WriteLine($"   - Датум: {datum:dd.MM.yyyy}");
                            System.Diagnostics.Debug.WriteLine($"   - Први део: {jePrviDeoNocne}");
                            System.Diagnostics.Debug.WriteLine($"   - Сати: {izabraniSati}:{izabraneMinute:00}");

                            if (jePrviDeoNocne)
                            {
                                // Први део ноћне: 19:00-24:00
                                uspesno = bazaService.SacuvajStatusRadnikaSaSatima(
                                    trenutniRadnikId, datum, noviStatus, izabraniSati, izabraneMinute,
                                    jeNocnaSmena, jePrviDeoNocne);

                                if (uspesno && izabraniSati > 0)
                                {
                                    string statusZaCuvanje = $"{noviStatus} ({izabraniSati}:{izabraneMinute:00})";

                                    if (sacuvaniStatusi.ContainsKey(datum))
                                    {
                                        sacuvaniStatusi[datum] = statusZaCuvanje;
                                    }
                                    else
                                    {
                                        sacuvaniStatusi.Add(datum, statusZaCuvanje);
                                    }

                                    // ★★★ АУТОМАТСКИ ДОДАЈ ПРЕДАЈУ ДУЖНОСТИ ЗА СЛЕДЕЋИ ДАН ★★★
                                    // Ако је први део ноћне (>0 сати), додај предају дужности за следећи дан
                                    DateTime sledeciDan = datum.AddDays(1);
                                    bazaService.SacuvajStatusRadnikaSaSatima(
                                        trenutniRadnikId, sledeciDan, "ПРЕДАЈА_ДУЖНОСТИ",
                                        Math.Max(izabraniSati, 7), izabraneMinute, true, false);

                                    MessageBox.Show($"✅ Ноћна смена постављена!\n" +
                                                  $"{datum:dd.MM.yyyy} → НОЋНА (19:00-24:00)\n" +
                                                  $"Сати: {izabraniSati}:{izabraneMinute:00}\n\n" +
                                                  $"💡 Аутоматски је додата и Предаја дужности за {sledeciDan:dd.MM.yyyy}",
                                                  "Промена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            else
                            {
                                // Други део (предаја дужности): 00:00-07:00
                                uspesno = bazaService.SacuvajStatusRadnikaSaSatima(
                                    trenutniRadnikId, datum, noviStatus, izabraniSati, izabraneMinute,
                                    jeNocnaSmena, jePrviDeoNocne);

                                if (uspesno)
                                {
                                    string statusZaCuvanje = $"{noviStatus} ({izabraniSati}:{izabraneMinute:00})";

                                    if (sacuvaniStatusi.ContainsKey(datum))
                                    {
                                        sacuvaniStatusi[datum] = statusZaCuvanje;
                                    }
                                    else
                                    {
                                        sacuvaniStatusi.Add(datum, statusZaCuvanje);
                                    }

                                    MessageBox.Show($"✅ Предаја дужности постављена!\n" +
                                                  $"{datum:dd.MM.yyyy} → ПРЕДАЈА ДУЖНОСТИ (00:00-07:00)\n" +
                                                  $"Сати: {izabraniSati}:{izabraneMinute:00}",
                                                  "Промена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                        else
                        {
                            // ★★★ ОБИЧНИ СТАТУСИ (НЕНОЋНИ) ★★★
                            System.Diagnostics.Debug.WriteLine($"☀️ Обрада обичног статуса: {noviStatus}");

                            uspesno = bazaService.SacuvajStatusRadnikaSaSatima(
                                trenutniRadnikId, datum, noviStatus, izabraniSati, izabraneMinute);

                            if (uspesno)
                            {
                                string statusZaCuvanje = $"{noviStatus} ({izabraniSati}:{izabraneMinute:00})";

                                if (sacuvaniStatusi.ContainsKey(datum))
                                {
                                    sacuvaniStatusi[datum] = statusZaCuvanje;
                                }
                                else
                                {
                                    sacuvaniStatusi.Add(datum, statusZaCuvanje);
                                }

                                MessageBox.Show($"✅ Статус успешно промењен!\n" +
                                              $"{datum:dd.MM.yyyy} → {noviStatus}\n" +
                                              $"Сати: {izabraniSati}:{izabraneMinute:00}",
                                              "Промена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }

                    if (uspesno)
                    {
                        // ★★★ ОСВЕЖАВАЊЕ ПРИКАЗА ★★★
                        System.Diagnostics.Debug.WriteLine($"🔄 Почињем освежавање приказа...");

                        // 1. PONOVO UČITAJ SVE PODATKE IZ BAZE
                        System.Diagnostics.Debug.WriteLine("📥 Учитавам податке из базе...");
                        UcitajPodatkeORadniku();

                        // 2. PONOVO NACRTAJ CELI KALENDAR
                        System.Diagnostics.Debug.WriteLine("🎨 Поново цртам календар...");
                        NacrtajKalendar();

                        // 3. AŽURIRAJ GLOBALNU STATISTIKU
                        System.Diagnostics.Debug.WriteLine("📈 Ажурирам статистику...");
                        AzurirajStatistiku();

                        // 4. AŽURIRAJ STATISTIKU ZA TEKUĆI I SUSEDNI MESEC
                        System.Diagnostics.Debug.WriteLine($"📊 Ажурирам статистику за месец {mesec}");
                        OsveziStatistikuZaMesec(mesec);

                        // Ako je ноћна смена, osveži i sledeći mesec
                        if (jeNocnaSmena && !jePrviDeoNocne)
                        {
                            DateTime sledeciMesecDatum = datum.AddDays(1);
                            OsveziStatistikuZaMesec(sledeciMesecDatum.Month);
                        }

                        System.Diagnostics.Debug.WriteLine($"✅ ОСВЕЖАВАЊЕ ЗАВРШЕНО");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ НИЈЕ УСПЕО: Није сачуван статус");
                        MessageBox.Show("❌ Грешка при чувању статуса!", "Грешка");
                    }
                }
            }
        }
        private void OznaciDaneUKalendaru()
        {
            if (generisaniRaspored == null && sacuvaniStatusi == null)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Нема података за приказ!");
                return;
            }

            int brojOznacenihDana = 0;
            int brojLicnihStatusa = 0;
            int brojGenerisanihStatusa = 0;
            int brojPodrazumevanihStatusa = 0;
            int brojNocnihSmena = 0;
            int brojPredajaDuznosti = 0;

            foreach (var panel in panelKalendar.Controls.OfType<Panel>())
            {
                foreach (var label in panel.Controls.OfType<Label>())
                {
                    if (label.Tag is Tuple<int, int> tag)
                    {
                        int mesec = tag.Item1;
                        int dan = tag.Item2;

                        try
                        {
                            DateTime datum = new DateTime(trenutnaGodina, mesec, dan);
                            string statusZaPrikaz = "Слободан";
                            string tooltipText = $"{datum:dd.MM.yyyy} ({datum:dddd})";
                            string izvor = "Подразумевано";
                            string satnica = "";

                            //  ПРВО ПРОВЕРИ ЛИЧНЕ СТАТУСЕ (ИМАЈУ ПРВЕНСТВО) 
                            if (sacuvaniStatusi.ContainsKey(datum))
                            {
                                string punStatus = sacuvaniStatusi[datum];

                                // ★★★ РАЗДВАЈАЊЕ СТАТУСА И ВРЕМЕНА ★★★
                                if (punStatus.Contains("(") && punStatus.Contains(")"))
                                {
                                    int zagradaIndex = punStatus.IndexOf("(");
                                    statusZaPrikaz = punStatus.Substring(0, zagradaIndex).Trim();
                                    string vreme = punStatus.Substring(zagradaIndex);
                                    tooltipText += $" - {statusZaPrikaz} {vreme}";
                                }
                                else
                                {
                                    statusZaPrikaz = punStatus;
                                    tooltipText += $" - {punStatus}";
                                }

                                // ★★★ ПРОВЕРИ ДА ЛИ ЈЕ НОВИ ТИП СТАТУСА ★★★
                                if (statusZaPrikaz.Contains("НОЋНА_ПРВИ_ДЕО"))
                                {
                                    izvor = "★ Ноћна смена (први део: 19:00-24:00)";
                                    brojNocnihSmena++;
                                }
                                else if (statusZaPrikaz.Contains("ПРЕДАЈА_ДУЖНОСТИ"))
                                {
                                    izvor = "★ Предаја дужности (00:00-07:00)";
                                    brojPredajaDuznosti++;
                                }
                                else
                                {
                                    izvor = "★ Лични статус";
                                }

                                brojLicnihStatusa++;
                            }
                            //  ОНДА ПРОВЕРИ ГЕНЕРИСАНИ РАСПОРЕД 
                            else if (generisaniRaspored.ContainsKey(datum))
                            {
                                
                                string smena = generisaniRaspored[datum];

                                if (smena == "ДНЕВНА")
                                {
                                    statusZaPrikaz = "Рад";
                                    satnica = "07:00-19:00 (12 sati)";
                                    tooltipText += $"\n☀️ Дневна смена\n{satnica}";
                                    izvor = "📅 Генерисано аутоматски";
                                }
                                else if (smena == "НОЋНА_ПРВИ_ДЕО")
                                {
                                    statusZaPrikaz = "НОЋНА_ПРВИ_ДЕО";
                                    satnica = "19:00-24:00 (5 сати)";
                                    tooltipText += $"\n🌙 Ноћна смена (први део)\n{satnica}";
                                    izvor = "📅 Генерисано аутоматски";
                                }
                                else if (smena == "ПРЕДАЈА_ДУЖНОСТИ")
                                {
                                    statusZaPrikaz = "ПРЕДАЈА_ДУЖНОСТИ";
                                    satnica = "00:00-07:00 (7 сати)";
                                    tooltipText += $"\n🌃 Предаја дужности\n{satnica}";
                                    izvor = "📅 Генерисано аутоматски";
                                }
                                else if (smena == "ОДМОР")
                                {
                                    statusZaPrikaz = "Слободан";
                                    tooltipText += " - Одмор";
                                    izvor = "😴 Генерисано аутоматски";
                                    brojGenerisanihStatusa++;
                                }
                            }
                            else
                            {
                                // Подразумевани статус
                                if (datum.DayOfWeek == DayOfWeek.Saturday || datum.DayOfWeek == DayOfWeek.Sunday)
                                {
                                    statusZaPrikaz = "Слободан";
                                    tooltipText += " - Викенд";
                                    izvor = "🎉 Викенд";
                                }
                                else
                                {
                                    statusZaPrikaz = "Слободан";
                                    tooltipText += " - Радни дан";
                                    izvor = "📅 Радни дан";
                                }
                                brojPodrazumevanihStatusa++;
                            }

                            tooltipText += $"\n{izvor}";

                            //  ПРИКАЖИ СТАТУС 
                            AzurirajPrikazDana(label, statusZaPrikaz);
                            toolTip.SetToolTip(label, tooltipText);
                            brojOznacenihDana++;

                            //  ДОДАЈ САТНИЦУ У TOOLTIP АКО ПОСТОЈИ 
                            if (!string.IsNullOrEmpty(satnica))
                            {
                                tooltipText += $"\n⏰ {satnica}";
                            }

                            tooltipText += $"\n{izvor}";
                            toolTip.SetToolTip(label, tooltipText);

                            brojOznacenihDana++;
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            // Прескочи невалидне датуме (нпр. 30. фебруар)
                            System.Diagnostics.Debug.WriteLine($"⚠️ Невалидан датум: {mesec}/{dan}/{trenutnaGodina}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"💥 Грешка при означавању дана {mesec}/{dan}: {ex.Message}");
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"🎯 Освежено: {brojOznacenihDana} дана");
            System.Diagnostics.Debug.WriteLine($"   - Лично: {brojLicnihStatusa}");
            System.Diagnostics.Debug.WriteLine($"   - Генерисано: {brojGenerisanihStatusa}");
            System.Diagnostics.Debug.WriteLine($"   - Подразумевано: {brojPodrazumevanihStatusa}");
            System.Diagnostics.Debug.WriteLine($"   - Ноћних смена: {brojNocnihSmena}");
            System.Diagnostics.Debug.WriteLine($"   - Предаја дужности: {brojPredajaDuznosti}");
        }

        
        private void AzurirajPrikazDana(Label lblDan, string status)
        {
            //  УКЛОНИ САТЕ ИЗ СТАТУСА АКО ИМА 
            string cistStatus = status;
            if (status.Contains("("))
            {
                cistStatus = status.Substring(0, status.IndexOf("(")).Trim();
            }

            switch (cistStatus)
            {
                case "ДНЕВНА":
                case "Рад":
                    lblDan.BackColor = Color.LightGreen;      // Дневна - зелена
                    lblDan.ForeColor = Color.Black;
                    lblDan.Font = new Font(lblDan.Font, FontStyle.Bold);
                    break;

                case "НОЋНА_ПРВИ_ДЕО":
                    lblDan.BackColor = Color.LightBlue;       // Ноћна први део - светло плава
                    lblDan.ForeColor = Color.Black;
                    lblDan.Font = new Font(lblDan.Font, FontStyle.Bold);
                    break;

                case "ПРЕДАЈА_ДУЖНОСТИ":
                    lblDan.BackColor = Color.DarkBlue;        // Предаја дужности - тамно плава
                    lblDan.ForeColor = Color.White;
                    lblDan.Font = new Font(lblDan.Font, FontStyle.Bold);
                    break;

                case "Годишњи":
                    lblDan.BackColor = Color.Orange;
                    lblDan.ForeColor = Color.Black;
                    lblDan.Font = new Font(lblDan.Font, FontStyle.Bold);
                    break;

                case "Боловање":
                    lblDan.BackColor = Color.Red;
                    lblDan.ForeColor = Color.White;
                    lblDan.Font = new Font(lblDan.Font, FontStyle.Bold);
                    break;

                case "Слободан":
                    lblDan.BackColor = Color.LightGray;
                    lblDan.ForeColor = Color.Black;
                    lblDan.Font = new Font(lblDan.Font, FontStyle.Regular);
                    break;

                case "Плаћено":
                    lblDan.BackColor = Color.Purple;
                    lblDan.ForeColor = Color.White;
                    lblDan.Font = new Font(lblDan.Font, FontStyle.Bold);
                    break;

                case "ССПК":
                    lblDan.BackColor = Color.Brown;
                    lblDan.ForeColor = Color.White;
                    lblDan.Font = new Font(lblDan.Font, FontStyle.Bold);
                    break;

                case "Службено":
                    lblDan.BackColor = Color.Cyan;            // Светло плава за службено
                    lblDan.ForeColor = Color.Black;
                    lblDan.Font = new Font(lblDan.Font, FontStyle.Bold);
                    break;

                case "Слава":
                    lblDan.BackColor = Color.Gold;
                    lblDan.ForeColor = Color.Black;
                    lblDan.Font = new Font(lblDan.Font, FontStyle.Bold);
                    break;

                default:
                    lblDan.BackColor = Color.LightGray;
                    lblDan.ForeColor = Color.Black;
                    lblDan.Font = new Font(lblDan.Font, FontStyle.Regular);
                    break;
            }
        }
        
        private string GetVremeSmene(string smena)
        {
            if (smena == "ДНЕВНА")
                return "07:00 - 19:00";
            else if (smena == "НОЋНА_ПРВИ_ДЕО")
                return "19:00 - 24:00";
            else if (smena == "ПРЕДАЈА_ДУЖНОСТИ")
                return "00:00 - 07:00";
            else if (smena == "НОЋНА")
                return "19:00 - 07:00 (стари формат)";
            else
                return "ОДМОР";
        }
        private void AzurirajStatistiku()
        {
            try
            {
                if (trenutniRadnikId == 0)
                {
                    System.Diagnostics.Debug.WriteLine("📊 СТАТИСТИКА: Нема података у приказу");
                    lblGlobalnaStatistika.Text = "📊 Нема података за приказ статистике";
                    return;
                }

                int ukupnoRadnihDana = 0;
                int ukupnoRadnihSati = 0;
                int ukupnoSmena = 0;
                int ukupnoSatiSmena = 0;

                DateTime startDatum = prikazPrvih6 ?
                    new DateTime(trenutnaGodina, 1, 1) :
                    new DateTime(trenutnaGodina, 7, 1);
                DateTime endDatum = prikazPrvih6 ?
                    new DateTime(trenutnaGodina, 6, 30) :
                    new DateTime(trenutnaGodina, 12, 31);

                //  КРЕИРАЈ БАЗУ И УЗМИ САТЕ ЗА СЕСТОМЕСЕЋЕ 
                BazaService bazaService = new BazaService();

                //  УЗМИ СВЕ САТЕ ИЗ БАЗЕ ЗА СЕСТОМЕСЕЋЕ 
                Dictionary<DateTime, (int sati, int minute)> sviSati = new Dictionary<DateTime, (int, int)>();

                int startMesec = prikazPrvih6 ? 1 : 7;
                int endMesec = prikazPrvih6 ? 6 : 12;

                for (int mesec = startMesec; mesec <= endMesec; mesec++)
                {
                    var satiPoDanu = bazaService.UzmiSateIzBazeZaMesec(trenutniRadnikId, mesec, trenutnaGodina);

                    foreach (var dan in satiPoDanu)
                    {
                        DateTime datum = dan.Key;
                        int satiDana1 = dan.Value.satiDana1;
                        int satiDana2 = dan.Value.satiDana2;
                        bool jeNocnaSmena = dan.Value.jeNocnaSmena;

                        // ★★★ ПРАВИЛНО САБИРАЊЕ ЗА НОЋНЕ СМЕНЕ ★★★
                        int ukupnoSatiZaDan = satiDana1 + satiDana2;

                        if (!sviSati.ContainsKey(datum))
                        {
                            sviSati.Add(datum, (ukupnoSatiZaDan, 0));

                            // ★★★ БРОЈАЊЕ СМЕНА ★★★
                            if (ukupnoSatiZaDan > 0)
                            {
                                if (jeNocnaSmena)
                                {
                                    // За ноћну смену рачунамо као једну смену ако има сате
                                    if (satiDana1 > 0 || satiDana2 > 0)
                                    {
                                        ukupnoSmena++;
                                    }
                                }
                                else
                                {
                                    // За обичне смене
                                    ukupnoSmena++;
                                }
                            }

                            ukupnoSatiSmena += ukupnoSatiZaDan;
                        }
                    }
                }

                // Број радних дана (понедељак-петак)
                for (DateTime datum = startDatum; datum <= endDatum; datum = datum.AddDays(1))
                {
                    if (datum.DayOfWeek != DayOfWeek.Saturday && datum.DayOfWeek != DayOfWeek.Sunday)
                    {
                        ukupnoRadnihDana++;
                    }
                }

                //  АКО НЕМА ПОДАТАКА ИЗ БАЗЕ, КОРИСТИ СТАРИ ПРИСТУП 
                if (sviSati.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Нема података из базе, користим стари приступ");

                    // Ресетујемо вредности
                    ukupnoSmena = 0;
                    ukupnoSatiSmena = 0;

                    for (DateTime datum = startDatum; datum <= endDatum; datum = datum.AddDays(1))
                    {
                        //  ПРВО ПРОВЕРИ ЛИЧНЕ СТАТУСЕ 
                        if (sacuvaniStatusi.ContainsKey(datum))
                        {
                            string status = sacuvaniStatusi[datum];

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
                                        float decimalniSati = sati + (minute / 60.0f);
                                        ukupnoSatiSmena += (int)Math.Round(decimalniSati);

                                        if (sati > 0)
                                        {
                                            ukupnoSmena++;
                                        }
                                        continue;
                                    }
                                }
                            }

                            // Стара логика за статусе без сати
                            string cistStatus = status;
                            if (status.Contains("("))
                            {
                                cistStatus = status.Substring(0, status.IndexOf("(")).Trim();
                            }

                            switch (cistStatus)
                            {
                                case "Рад":
                                case "ДНЕВНА":
                                    ukupnoSmena++;
                                    ukupnoSatiSmena += 12;
                                    break;

                                case "НОЋНА_ПРВИ_ДЕО":
                                    ukupnoSmena++;
                                    ukupnoSatiSmena += 5;
                                    break;

                                case "ПРЕДАЈА_ДУЖНОСТИ":
                                    // Само сате, не и смену
                                    ukupnoSatiSmena += 7;
                                    break;

                                case "Слободан":
                                    break;

                                default:
                                    ukupnoSmena++;
                                    ukupnoSatiSmena += 8;
                                    break;
                            }
                        }
                        //  ОНДА ПРОВЕРИ ГЕНЕРИСАНИ РАСПОРЕД 
                        else if (generisaniRaspored.ContainsKey(datum))
                        {
                            string smena = generisaniRaspored[datum];

                            switch (smena)
                            {
                                case "ДНЕВНА":
                                    ukupnoSmena++;
                                    ukupnoSatiSmena += 12;
                                    break;

                                case "НОЋНА_ПРВИ_ДЕО":
                                    ukupnoSmena++;
                                    ukupnoSatiSmena += 5;
                                    break;

                                case "ПРЕДАЈА_ДУЖНОСТИ":
                                    ukupnoSatiSmena += 7;
                                    break;

                                case "ОДМОР":
                                    break;
                            }
                        }
                    }
                }

                ukupnoRadnihSati = ukupnoRadnihDana * 8;
                int ukupnaRazlika = ukupnoSatiSmena - ukupnoRadnihSati;
                string znakUkupneRazlike = ukupnaRazlika >= 0 ? "+" : "";

                // ★★★ ДЕТАЉНИЈИ DEBUG ИСПИС ★★★
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("📊 ШЕСТОМЕСЕЧНА СТАТИСТИКА - ДЕТАЉИ:");
                System.Diagnostics.Debug.WriteLine($"   - Период: {startDatum:dd.MM.yyyy} - {endDatum:dd.MM.yyyy}");
                System.Diagnostics.Debug.WriteLine($"   - Дана у периоду: {(endDatum - startDatum).Days + 1}");
                System.Diagnostics.Debug.WriteLine($"   - Радних дана (Пон-Пет): {ukupnoRadnihDana}");
                System.Diagnostics.Debug.WriteLine($"   - Радних сати (Пон-Пет * 8h): {ukupnoRadnihSati}");
                System.Diagnostics.Debug.WriteLine($"   - Укупно смена: {ukupnoSmena}");
                System.Diagnostics.Debug.WriteLine($"   - Укупно сати смена: {ukupnoSatiSmena}");
                System.Diagnostics.Debug.WriteLine($"   - Разлика: {znakUkupneRazlike}{ukupnaRazlika} сати");

                //  ПРИКАЗ ПО МЕСЕЦИМА ЗА ДЕТАЉНУ ПРОВЕРУ 
                for (int mesec = startMesec; mesec <= endMesec; mesec++)
                {
                    int radniDaniUMesecu = IzracunajRadneDaneUMesecu(mesec, trenutnaGodina);
                    int radniSatiUMesecu = radniDaniUMesecu * 8;

                    var satiZaMesec = bazaService.UzmiSateIzBazeZaMesec(trenutniRadnikId, mesec, trenutnaGodina);
                    int smeneUMesecu = 0;
                    int satiSmenaUMesecu = 0;

                    foreach (var dan in satiZaMesec)
                    {
                        int satiDana1 = dan.Value.satiDana1;
                        int satiDana2 = dan.Value.satiDana2;
                        bool jeNocnaSmena = dan.Value.jeNocnaSmena;

                        int ukupnoSati = satiDana1 + satiDana2;
                        satiSmenaUMesecu += ukupnoSati;

                        if (ukupnoSati > 0)
                        {
                            if (jeNocnaSmena)
                            {
                                // Ноћна смена = 1 смена
                                smeneUMesecu++;
                            }
                            else
                            {
                                smeneUMesecu++;
                            }
                        }
                    }

                    int razlikaUMesecu = satiSmenaUMesecu - radniSatiUMesecu;
                    string znakRazlike = razlikaUMesecu >= 0 ? "+" : "";

                    System.Diagnostics.Debug.WriteLine($"   📅 Месец {mesec}: Рад.дана: {radniDaniUMesecu}, " +
                                                     $"Смена: {smeneUMesecu}, Сати: {satiSmenaUMesecu}, " +
                                                     $"Разлика: {znakRazlike}{razlikaUMesecu}");
                }
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════");

                //  ФОРМАТИРАЊЕ ЗА ПРИКАЗ 
                string statistikaText = $"📊 ШЕСТОМЕСЕЧНА СТАТИСТИКА ({trenutnaGodina}. година) | " +
                                       $"Рад.дана: {ukupnoRadnihDana} | " +
                                       $"Рад.сати: {ukupnoRadnihSati} | " +
                                       $"Смена: {ukupnoSmena} | " +
                                       $"Сати смена: {ukupnoSatiSmena} | " +
                                       $"Разлика: {znakUkupneRazlike}{ukupnaRazlika}";

                lblGlobalnaStatistika.Text = statistikaText;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при рачунању статистике: {ex.Message}");
                lblGlobalnaStatistika.Text = "📊 Грешка при рачунању статистике";
            }
        }
        private string IzracunajStatistikuZaMesec(int mesec)
        {
            try
            {
                if (trenutniRadnikId == 0)
                    return $"Радних дана: 0     Смена: 0\nРадних сати: 0     Сати смена: 0\nРазлика: 0";

                int brojSmenaUMesecu = 0;
                int brojRadnihSatiSmena = 0;

                int brojDanaUMesecu = DateTime.DaysInMonth(trenutnaGodina, mesec);

                // ★★★ UZMI SVE STATUSE IZ BAZE ZA OVAJ MESEC ★★★
                BazaService bazaService = new BazaService();
                var statusiZaMesec = bazaService.UcitajSacuvaneStatuse(trenutniRadnikId, trenutnaGodina)
                    .Where(x => x.Key.Month == mesec)
                    .ToDictionary(x => x.Key, x => x.Value);

                for (int dan = 1; dan <= brojDanaUMesecu; dan++)
                {
                    DateTime datum = new DateTime(trenutnaGodina, mesec, dan);

                    // ★★★ PRVO PROVERI SAČUVANE STATUSE (LIČNE) ★★★
                    if (statusiZaMesec.ContainsKey(datum))
                    {
                        string status = statusiZaMesec[datum];

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
                                    float decimalniSati = sati + (minute / 60.0f);
                                    brojRadnihSatiSmena += (int)Math.Round(decimalniSati);

                                    if (sati > 0)
                                    {
                                        brojSmenaUMesecu++;
                                    }
                                    continue;
                                }
                            }
                        }

                        // Stara logika za statuse bez sati
                        if (status.StartsWith("Рад"))
                        {
                            brojSmenaUMesecu++;
                            brojRadnihSatiSmena += 12;
                        }
                        else if (status == "Слободан")
                        {
                            // Слободан = 0 сати
                        }
                        else if (status.StartsWith("НОЋНА_ПРВИ_ДЕО"))
                        {
                            brojSmenaUMesecu++;
                            brojRadnihSatiSmena += 5;
                        }
                        else if (status.StartsWith("ПРЕДАЈА_ДУЖНОСТИ"))
                        {
                            // Предаја дужности = 7 сати (не рачуна се као смена)
                            brojRadnihSatiSmena += 7;
                        }
                        else
                        {
                            brojSmenaUMesecu++;
                            brojRadnihSatiSmena += 8;
                        }
                    }
                    // ★★★ ONDA PROVERI GENERISANI RASPORED ★★★
                    else if (generisaniRaspored.ContainsKey(datum))
                    {
                        string smena = generisaniRaspored[datum];

                        if (smena == "ДНЕВНА")
                        {
                            brojSmenaUMesecu++;
                            brojRadnihSatiSmena += 12;
                        }
                        else if (smena == "НОЋНА_ПРВИ_ДЕО")
                        {
                            brojSmenaUMesecu++;
                            brojRadnihSatiSmena += 5;
                        }
                        else if (smena == "ПРЕДАЈА_ДУЖНОСТИ")
                        {
                            brojRadnihSatiSmena += 7;
                        }
                        else if (smena == "НОЋНА") // За компатибилност са старим подацима
                        {
                            brojSmenaUMesecu++;
                            brojRadnihSatiSmena += 12;
                        }
                    }
                }

                //  ИЗРАЧУНАЈ РАЗЛИКУ 
                int radniSati = radniDaniPoMesecu[mesec] * 8;
                int razlika = brojRadnihSatiSmena - radniSati;
                string znakRazlike = razlika >= 0 ? "+" : "";

                string statistika = $"Радних дана: {radniDaniPoMesecu[mesec]}     Смена: {brojSmenaUMesecu}\n" +
                                   $"Радних сати: {radniSati}     Сати смена: {brojRadnihSatiSmena}\n" +
                                   $"Разлика: {znakRazlike}{razlika}";

                System.Diagnostics.Debug.WriteLine($"📈 СТАТИСТИКА за {mesec}/{trenutnaGodina}:");
                System.Diagnostics.Debug.WriteLine($"   - Радних дана: {radniDaniPoMesecu[mesec]}");
                System.Diagnostics.Debug.WriteLine($"   - Број смена: {brojSmenaUMesecu}");
                System.Diagnostics.Debug.WriteLine($"   - Радних сати: {radniSati}");
                System.Diagnostics.Debug.WriteLine($"   - Сати смена: {brojRadnihSatiSmena}");
                System.Diagnostics.Debug.WriteLine($"   - Разлика: {znakRazlike}{razlika}");

                return statistika;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при рачунању статистике за месец {mesec}: {ex.Message}");
                return $"Радних дана: 0     Смена: 0\nРадних сати: 0     Сати смена: 0\nРазлика: 0";
            }
        }
        private void OsveziStatistikuZaMesec(int mesec)
        {
            try
            {
                // Pronađi panel za dati mesec
                foreach (Panel panelMesec in panelKalendar.Controls.OfType<Panel>())
                {
                    foreach (Label lblStatistika in panelMesec.Controls.OfType<Label>())
                    {
                        if (lblStatistika.Tag != null && lblStatistika.Tag.ToString() == $"STAT_{mesec}")
                        {
                            string novaStatistika = IzracunajStatistikuZaMesec(mesec);
                            lblStatistika.Text = novaStatistika;

                            // Ako imate susedni mesec (za noćne smene koje se protežu)
                            if (mesec == 12)
                            {
                                // Proveri januar sledeće godine za noćne smene
                            }
                            else
                            {
                                // Proveri sledeći mesec za noćne smene
                                OsveziStatistikuZaMesec(mesec + 1);
                            }

                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при освежавању статистике за месец {mesec}: {ex.Message}");
            }
        }

        //  NOVA METODA ZA UZIMANJE SATI IZ BAZE 
        private Dictionary<DateTime, (int satiDana1, int satiDana2, bool jeNocnaSmena)>
            UzmiSateIzBazeZaMesec(int radnikId, int mesec, int godina)
        {
            var satiPoDanu = new Dictionary<DateTime, (int, int, bool)>();

            try
            {
                // ★★★ KORISTITE bazaService ILI KREIRAJTE NOVU KONEKCIJU ★★★

                // Opcija 1: Koristite postojeći BazaService
                string connString = @"Data Source=MILANDJ\SQLEXPRESS;Initial Catalog=RadnoVreme;Integrated Security=True;Encrypt=False;Connect Timeout=30";

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    string query = @"SELECT Datum, SatiDana1, SatiDana2, JeNocnaSmena 
                       FROM RadnikStatusi 
                       WHERE RadnikId = @RadnikId 
                         AND YEAR(Datum) = @Godina 
                         AND MONTH(Datum) = @Mesec
                       ORDER BY Datum";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RadnikId", radnikId);
                        cmd.Parameters.AddWithValue("@Godina", godina);
                        cmd.Parameters.AddWithValue("@Mesec", mesec);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DateTime datum = (DateTime)reader["Datum"];
                                int? satiDana1 = reader["SatiDana1"] as int? ?? 0;
                                int? satiDana2 = reader["SatiDana2"] as int? ?? 0;
                                bool jeNocnaSmena = Convert.ToBoolean(reader["JeNocnaSmena"]);

                                satiPoDanu[datum] = (satiDana1.Value, satiDana2.Value, jeNocnaSmena);

                                System.Diagnostics.Debug.WriteLine($"📊 Учитано из базе {datum:dd.MM.yyyy}: {satiDana1}+{satiDana2} сати, Noćна={jeNocnaSmena}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при учитавању сати из базе: {ex.Message}");
            }

            return satiPoDanu;
        }

        
        public bool ProveriDaLiPostojeNoveKolone()
        {
            try
            {
                // ★★★ КОРИСТИТЕ bazaService ИНСТАНЦУ ★★★
                return bazaService.ProveriKoloneUBazi();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при провери колона: {ex.Message}");
                return false;
            }
        }
        private (int sati, int minute) IzvuciSateIzStatusa(string status)
        {
            System.Diagnostics.Debug.WriteLine($"🔍 Извлачење сати из статуса: '{status}'");

            // Ako status nema zagrade, vrati podrazumevane vrednosti
            if (!status.Contains("(") || !status.Contains(")"))
            {
                System.Diagnostics.Debug.WriteLine($"   - Нема заграде, подразумевано за '{status}'");

                if (status.StartsWith("Рад"))
                {
                    System.Diagnostics.Debug.WriteLine($"   - Враћање 12 сати за Рад");
                    return (12, 0); // Podrazumevano za Рад
                }
                else if (status == "Слободан")
                {
                    System.Diagnostics.Debug.WriteLine($"   - Враћање 0 сати за Слободан");
                    return (0, 0);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"   - Враћање 8 сати за остале статусе");
                    return (8, 0); // Podrazumevano za ostale statuse
                }
            }

            // Izvuci sate iz zagrade
            try
            {
                int startIndex = status.IndexOf("(") + 1;
                int endIndex = status.IndexOf(")");
                string vreme = status.Substring(startIndex, endIndex - startIndex);

                System.Diagnostics.Debug.WriteLine($"   - Време у загради: '{vreme}'");

                if (vreme.Contains(":"))
                {
                    string[] delovi = vreme.Split(':');
                    if (delovi.Length == 2 &&
                        int.TryParse(delovi[0], out int sati) &&
                        int.TryParse(delovi[1], out int minute))
                    {
                        System.Diagnostics.Debug.WriteLine($"   - Пронађени сати: {sati}, минути: {minute}");
                        return (sati, minute);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при извлачењу сати: {ex.Message}");
            }

            System.Diagnostics.Debug.WriteLine($"   - Враћање подразумеваних вредности");
            return (0, 0);
        }

        private void InicijalizujRadneDane()
        {
            // Inicijalizuj broj radnih dana za svaki mesec
            for (int mesec = 1; mesec <= 12; mesec++)
            {
                radniDaniPoMesecu[mesec] = IzracunajRadneDaneUMesecu(mesec, trenutnaGodina);
            }
        }

        private void DodajDaneUNedelji(Panel panelMesec, int startY)
        {
            string[] daniUNedelji = { "ПОН", "УТО", "СРЕ", "ЧЕТ", "ПЕТ", "СУБ", "НЕД" };

            for (int i = 0; i < 7; i++)
            {
                Label lblDanUNedelji = new Label();
                lblDanUNedelji.Text = daniUNedelji[i];
                lblDanUNedelji.Size = new Size(40, 20);
                lblDanUNedelji.Location = new Point(10 + (i * (40 + 5)), startY);
                lblDanUNedelji.Font = new Font("Arial", 8, FontStyle.Bold);
                lblDanUNedelji.ForeColor = Color.DarkBlue;
                lblDanUNedelji.TextAlign = ContentAlignment.MiddleCenter;
                lblDanUNedelji.BackColor = Color.LightYellow;
                lblDanUNedelji.BorderStyle = BorderStyle.FixedSingle;
                panelMesec.Controls.Add(lblDanUNedelji);
            }
        }

        private int IzracunajRadneDaneUMesecu(int mesec, int godina)
        {
            int brojRadnihDana = 0;
            int brojDanaUMesecu = DateTime.DaysInMonth(godina, mesec);

            for (int dan = 1; dan <= brojDanaUMesecu; dan++)
            {
                DateTime datum = new DateTime(godina, mesec, dan);
                // Radni dani su od ponedeljka do petka
                if (datum.DayOfWeek != DayOfWeek.Saturday && datum.DayOfWeek != DayOfWeek.Sunday)
                {
                    brojRadnihDana++;
                }
            }

            return brojRadnihDana;
        }

        private void KreirajFormu()
        {
            // Postavke forme - POVEĆANA VISINA za globalnu statistiku
            this.Text = $"Радно време - {radnikIme}";
            this.Size = new Size(1250, 850); // ★★★ POVEĆANA VISINA ★★★
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.Padding = new Padding(10);
            this.MinimumSize = new Size(1000, 700);

            // Naslov - informacije o radniku
            lblRadnik = new Label();
            lblRadnik.Text = $"📅 Радно време за: {radnikIme}";
            lblRadnik.Font = new Font("Arial", 16, FontStyle.Bold);
            lblRadnik.ForeColor = Color.DarkBlue;
            lblRadnik.Size = new Size(800, 30);
            lblRadnik.Location = new Point(225, 10);
            lblRadnik.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblRadnik);

            // Godina
            lblGodina = new Label();
            lblGodina.Text = "Година:";
            lblGodina.Font = new Font("Arial", 10, FontStyle.Bold);
            lblGodina.ForeColor = Color.DarkSlateGray;
            lblGodina.Size = new Size(60, 20);
            lblGodina.Location = new Point(550, 50);
            lblGodina.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lblGodina);

            numGodina = new NumericUpDown();
            numGodina.Minimum = 2020;
            numGodina.Maximum = 2030;
            numGodina.Value = trenutnaGodina;
            numGodina.Size = new Size(80, 20);
            numGodina.Location = new Point(620, 50);
            numGodina.Font = new Font("Arial", 10, FontStyle.Regular);
            numGodina.ValueChanged += NumGodina_ValueChanged;
            this.Controls.Add(numGodina);

            // Dugmad za polugodišta
            btnPrvih6 = new Button();
            btnPrvih6.Text = "⬅️ Првих 6 месеци (Јан-Јун)";
            btnPrvih6.Location = new Point(50, 50);
            btnPrvih6.Size = new Size(200, 30);
            btnPrvih6.BackColor = Color.DodgerBlue;
            btnPrvih6.ForeColor = Color.White;
            btnPrvih6.Font = new Font("Arial", 9, FontStyle.Bold);
            btnPrvih6.FlatStyle = FlatStyle.Flat;
            btnPrvih6.FlatAppearance.BorderSize = 0;
            btnPrvih6.Cursor = Cursors.Hand;
            btnPrvih6.Click += BtnPrvih6_Click;
            this.Controls.Add(btnPrvih6);

            btnDrugih6 = new Button();
            btnDrugih6.Text = "Других 6 месеци (Јул-Дец) ➡️";
            btnDrugih6.Location = new Point(260, 50);
            btnDrugih6.Size = new Size(200, 30);
            btnDrugih6.BackColor = Color.LightGray;
            btnDrugih6.ForeColor = Color.Black;
            btnDrugih6.Font = new Font("Arial", 9, FontStyle.Bold);
            btnDrugih6.FlatStyle = FlatStyle.Flat;
            btnDrugih6.FlatAppearance.BorderSize = 0;
            btnDrugih6.Cursor = Cursors.Hand;
            btnDrugih6.Click += BtnDrugih6_Click;
            this.Controls.Add(btnDrugih6);

            btnDetaljiRadnika = new Button();
            btnDetaljiRadnika.Text = "📊 Детаљи радника";
            btnDetaljiRadnika.Location = new Point(720, 50); // Pomerite lokaciju po potrebi
            btnDetaljiRadnika.Size = new Size(150, 30);
            btnDetaljiRadnika.BackColor = Color.Teal;
            btnDetaljiRadnika.ForeColor = Color.White;
            btnDetaljiRadnika.Font = new Font("Arial", 9, FontStyle.Bold);
            btnDetaljiRadnika.FlatStyle = FlatStyle.Flat;
            btnDetaljiRadnika.FlatAppearance.BorderSize = 0;
            btnDetaljiRadnika.Cursor = Cursors.Hand;
            btnDetaljiRadnika.Click += BtnDetaljiRadnika_Click;
            this.Controls.Add(btnDetaljiRadnika);

            btnOsvezi = new Button();
            btnOsvezi.Text = "🔄 Освежи";
            btnOsvezi.Location = new Point(880, 50);
            btnOsvezi.Size = new Size(100, 30);
            btnOsvezi.BackColor = Color.Green;
            btnOsvezi.ForeColor = Color.White;
            btnOsvezi.Font = new Font("Arial", 9, FontStyle.Bold);
            btnOsvezi.FlatStyle = FlatStyle.Flat;
            btnOsvezi.FlatAppearance.BorderSize = 0;
            btnOsvezi.Cursor = Cursors.Hand;
            btnOsvezi.Click += BtnOsvezi_Click;
            this.Controls.Add(btnOsvezi);

            // ★★★ PRVO DODAJTE PANEL ZA KALENDAR ★★★
            panelKalendar = new Panel();
            panelKalendar.Size = new Size(1200, 500); // ★★★ SMANJENA VISINA ★★★
            panelKalendar.Location = new Point(15, 90);
            panelKalendar.BackColor = Color.White;
            panelKalendar.BorderStyle = BorderStyle.FixedSingle;
            panelKalendar.AutoScroll = true;
            panelKalendar.HorizontalScroll.Enabled = true;
            panelKalendar.HorizontalScroll.Visible = true;
            panelKalendar.VerticalScroll.Enabled = true;
            panelKalendar.VerticalScroll.Visible = true;
            panelKalendar.AutoScrollMargin = new Size(20, 20);
            this.Controls.Add(panelKalendar);

            // ★★★ ONDA DODAJTE GLOBALNU STATISTIKU ★★★
            lblGlobalnaStatistika = new Label();
            lblGlobalnaStatistika.Text = "📊 Учитавам статистику...";
            lblGlobalnaStatistika.Font = new Font("Arial", 10, FontStyle.Bold);
            lblGlobalnaStatistika.ForeColor = Color.DarkBlue;
            lblGlobalnaStatistika.Size = new Size(1200, 30); // ★★★ ISTA ŠIRINA KAO PANEL ★★★
            lblGlobalnaStatistika.Location = new Point(15, 600); // ★★★ ISPOD KALENDARA ★★★
            lblGlobalnaStatistika.TextAlign = ContentAlignment.MiddleCenter;
            lblGlobalnaStatistika.BackColor = Color.LightYellow;
            lblGlobalnaStatistika.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(lblGlobalnaStatistika);

            // ★★★ NA KRAJU LEGENDA ★★★
            KreirajLegendu();
        }

        private void KreirajLegendu()
        {
            Panel panelLegenda = new Panel();
            panelLegenda.Size = new Size(1200, 60); // ★★★ ПОВЕЋАНА ВИСИНА ★★★
            panelLegenda.Location = new Point(15, 640);
            panelLegenda.BackColor = Color.WhiteSmoke;
            panelLegenda.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(panelLegenda);

            // ★★★ НОВА ЛЕГЕНДА СА СВИМ БОЈАМА ★★★
            var stavkeLegende = new[]
            {
        new { Emoji = "☀️", Naziv = "Дневна", Boja = Color.LightGreen, Satnica = "12h (07-19)" },
        new { Emoji = "🌙", Naziv = "Ноћна (први део)", Boja = Color.LightBlue, Satnica = "5h (19-24)" },
        new { Emoji = "🌃", Naziv = "Предаја дужности", Boja = Color.DarkBlue, Satnica = "7h (00-07)" },
        new { Emoji = "🎉", Naziv = "Годишњи", Boja = Color.Orange, Satnica = "8h" },
        new { Emoji = "🤒", Naziv = "Боловање", Boja = Color.Red, Satnica = "8h" },
        new { Emoji = "😴", Naziv = "Слободан", Boja = Color.LightGray, Satnica = "0h" },
        new { Emoji = "💰", Naziv = "Плаћено", Boja = Color.Purple, Satnica = "8h" },
        new { Emoji = "📋", Naziv = "ССПК", Boja = Color.Brown, Satnica = "8h" }
    };

            int xPozicija = 10;
            int yPozicija = 10;
            int sirinaStavke = 145;

            foreach (var stavka in stavkeLegende)
            {
                // Боја
                Panel pnlBoja = new Panel();
                pnlBoja.Size = new Size(20, 20);
                pnlBoja.Location = new Point(xPozicija, yPozicija);
                pnlBoja.BackColor = stavka.Boja;
                pnlBoja.BorderStyle = BorderStyle.FixedSingle;
                panelLegenda.Controls.Add(pnlBoja);

                // Емоџи
                Label lblEmoji = new Label();
                lblEmoji.Text = stavka.Emoji;
                lblEmoji.Font = new Font("Arial", 10);
                lblEmoji.Size = new Size(25, 20);
                lblEmoji.Location = new Point(xPozicija + 25, yPozicija);
                lblEmoji.TextAlign = ContentAlignment.MiddleLeft;
                panelLegenda.Controls.Add(lblEmoji);

                // Назив
                Label lblNaziv = new Label();
                lblNaziv.Text = stavka.Naziv;
                lblNaziv.Font = new Font("Arial", 8, FontStyle.Bold);
                lblNaziv.ForeColor = Color.DarkSlateGray;
                lblNaziv.Size = new Size(70, 20);
                lblNaziv.Location = new Point(xPozicija + 55, yPozicija);
                lblNaziv.TextAlign = ContentAlignment.MiddleLeft;
                panelLegenda.Controls.Add(lblNaziv);

                // Сатница
                Label lblSatnica = new Label();
                lblSatnica.Text = stavka.Satnica;
                lblSatnica.Font = new Font("Arial", 7, FontStyle.Italic);
                lblSatnica.ForeColor = Color.Gray;
                lblSatnica.Size = new Size(40, 20);
                lblSatnica.Location = new Point(xPozicija + 125, yPozicija);
                lblSatnica.TextAlign = ContentAlignment.MiddleLeft;
                panelLegenda.Controls.Add(lblSatnica);

                xPozicija += sirinaStavke;
            }

            // Наслов легенде
            Label lblNaslovLegende = new Label();
            lblNaslovLegende.Text = "🔷 ЛЕГЕНДА:";
            lblNaslovLegende.Font = new Font("Arial", 10, FontStyle.Bold);
            lblNaslovLegende.ForeColor = Color.DarkBlue;
            lblNaslovLegende.Size = new Size(80, 20);
            lblNaslovLegende.Location = new Point(10, 35);
            lblNaslovLegende.TextAlign = ContentAlignment.MiddleLeft;
            panelLegenda.Controls.Add(lblNaslovLegende);
        }
        private void NacrtajKalendar()
        {
            panelKalendar.Controls.Clear();
            panelKalendar.AutoScrollPosition = new Point(0, 0);

            InicijalizujRadneDane();

            string[] meseci = prikazPrvih6 ?
                new string[] { "ЈАНУАР", "ФЕБРУАР", "МАРТ", "АПРИЛ", "МАЈ", "ЈУН" } :
                new string[] { "ЈУЛ", "АВГУСТ", "СЕПТЕМБАР", "ОКТОБАР", "НОВЕМБАР", "ДЕЦЕМБАР" };

            int startMesec = prikazPrvih6 ? 1 : 7;
            int sirinaPanela = 350;
            int visinaPanela = 350;

            // Prvi red - prva 3 meseca
            for (int i = 0; i < 3; i++)
            {
                int mesec = startMesec + i;
                Panel panelMesec = KreirajPanelMeseca(meseci[i], mesec, sirinaPanela, visinaPanela);
                panelMesec.Location = new Point(20 + (i * (sirinaPanela + 20)), 20);
                panelKalendar.Controls.Add(panelMesec);
            }

            // Drugi red - druga 3 meseca
            for (int i = 3; i < 6; i++)
            {
                int mesec = startMesec + i;
                Panel panelMesec = KreirajPanelMeseca(meseci[i], mesec, sirinaPanela, visinaPanela);
                panelMesec.Location = new Point(20 + ((i - 3) * (sirinaPanela + 20)), 20 + visinaPanela + 30);
                panelKalendar.Controls.Add(panelMesec);
            }

            int ukupnaVisina = (visinaPanela * 2) + 100;
            int ukupnaSirina = (sirinaPanela * 3) + 100;
            panelKalendar.AutoScrollMinSize = new Size(ukupnaSirina, ukupnaVisina);

            // ★★★ NAKON CRTANJA KALENDARA - OZNAČI DANE ★★★
            OznaciDaneUKalendaru();
        }

        private Panel KreirajPanelMeseca(string nazivMeseca, int brojMeseca, int sirina, int visina)
        {
            Panel panel = new Panel();
            panel.Size = new Size(sirina, visina);
            panel.BackColor = Color.WhiteSmoke;
            panel.BorderStyle = BorderStyle.FixedSingle;

            // Naslov meseca
            Label lblMesec = new Label();
            lblMesec.Text = nazivMeseca;
            lblMesec.Font = new Font("Arial", 14, FontStyle.Bold);
            lblMesec.ForeColor = Color.DarkBlue;
            lblMesec.Size = new Size(sirina - 10, 30);
            lblMesec.Location = new Point(5, 5);
            lblMesec.TextAlign = ContentAlignment.MiddleCenter;
            panel.Controls.Add(lblMesec);

            // DODAJ DANE U NEDELJI
            DodajDaneUNedelji(panel, 40);

            // Dani u mesecu
            int brojDana = DateTime.DaysInMonth(trenutnaGodina, brojMeseca);

            DateTime prviDanUMesecu = new DateTime(trenutnaGodina, brojMeseca, 1);
            int pocetniDanUNedelji = (int)prviDanUMesecu.DayOfWeek;
            int startKolona = (pocetniDanUNedelji + 6) % 7;

            int red = 0;
            int kolona = startKolona;
            int cellSirina = 40;
            int cellVisina = 30;
            int startY = 65;

            for (int dan = 1; dan <= brojDana; dan++)
            {
                Label lblDan = new Label();
                lblDan.Text = dan.ToString();
                lblDan.Size = new Size(cellSirina, cellVisina);
                lblDan.Location = new Point(10 + (kolona * (cellSirina + 5)), startY + (red * (cellVisina + 5)));
                lblDan.Font = new Font("Arial", 10, FontStyle.Bold);
                lblDan.BorderStyle = BorderStyle.FixedSingle;
                lblDan.TextAlign = ContentAlignment.MiddleCenter;
                lblDan.Cursor = Cursors.Hand;
                lblDan.Tag = new Tuple<int, int>(brojMeseca, dan);
                lblDan.Click += LblDan_Click;

                // Podrazumevana boja - biće prepisana u OznaciDaneUKalendaru
                lblDan.BackColor = Color.LightGray;
                lblDan.ForeColor = Color.Black;

                panel.Controls.Add(lblDan);

                kolona++;
                if (kolona > 6)
                {
                    kolona = 0;
                    red++;
                }
            }

            //  NOVA STATISTIKA - 4 LINIJE (ZAMENJENA STARU) 
            string statistikaMeseca = IzracunajStatistikuZaMesec(brojMeseca);

            Label lblStatistika = new Label();
            lblStatistika.Text = statistikaMeseca;
            lblStatistika.Font = new Font("Arial", 8, FontStyle.Bold);
            lblStatistika.ForeColor = Color.DarkGreen;
            lblStatistika.Size = new Size(sirina - 10, 55);
            lblStatistika.Location = new Point(5, visina - 60); 
            lblStatistika.TextAlign = ContentAlignment.MiddleCenter;
            lblStatistika.BackColor = Color.LightYellow;
            lblStatistika.BorderStyle = BorderStyle.FixedSingle;
            panel.Controls.Add(lblStatistika);

            return panel;
        }

        private void UcitajPodatkeORadniku()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 ===== УЧИТАВАЊЕ ПОДАТАКА ЗА: '{radnikIme}' =====");

                var sviRadnici = bazaService.UzmiSveRadnike();

                System.Diagnostics.Debug.WriteLine($"📋 Пронађено {sviRadnici.Count} радника у бази:");
                foreach (var r in sviRadnici)
                {
                    System.Diagnostics.Debug.WriteLine($"   - ИД: {r.Id}, Име: '{r.Ime}', Презиме: '{r.Prezime}', Пуно име: '{r.PunoIme}'");
                }

                // Pronađi radnika - POBOLJŠANA PRETRAGA
                var radnik = sviRadnici.FirstOrDefault(r =>
                    radnikIme.Trim().Equals(r.Ime.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    radnikIme.Trim().Equals(r.Prezime.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    radnikIme.Trim().Equals(r.PunoIme.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    r.PunoIme.Trim().Equals(radnikIme.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    r.PunoIme.Trim().Contains(radnikIme.Trim()));

                if (radnik != null)
                {
                    trenutniRadnikId = radnik.Id;
                    System.Diagnostics.Debug.WriteLine($"✅ ПРОНАЂЕН РАДНИК: {radnik.PunoIme} - {radnik.Smena} (ID: {radnik.Id})");

                    // OČISTI SVE PRE UČITAVANJA
                    generisaniRaspored = new Dictionary<DateTime, string>();
                    sacuvaniStatusi = new Dictionary<DateTime, string>();

                    // 1. UČITAJ GENERISANI RASPORED ZA CELE 6 MESECI
                    System.Diagnostics.Debug.WriteLine($"📅 Учитавам генерисани распоред за смену: {radnik.Smena}");
                    int startMesec = prikazPrvih6 ? 1 : 7;
                    int endMesec = prikazPrvih6 ? 6 : 12;

                    for (int mesec = startMesec; mesec <= endMesec; mesec++)
                    {
                        var rasporedZaMesec = bazaService.UzmiRasporedZaSmenu(radnik.Smena, trenutnaGodina, mesec);
                        System.Diagnostics.Debug.WriteLine($"   - Месец {mesec}: {rasporedZaMesec.Count} дана");
                        foreach (var stavka in rasporedZaMesec)
                        {
                            generisaniRaspored[stavka.Key] = stavka.Value;
                        }
                    }

                    // 2. UČITAJ LIČNE STATUSE
                    System.Diagnostics.Debug.WriteLine($"💾 Учитавам личне статусе за радника ИД: {radnik.Id}");
                    sacuvaniStatusi = bazaService.UcitajSacuvaneStatuse(radnik.Id, trenutnaGodina);

                    System.Diagnostics.Debug.WriteLine($"📊 ЗАВРШЕНО: {generisaniRaspored.Count} генерисаних + {sacuvaniStatusi.Count} личних статуса");

                    AzurirajStatistiku();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ РАДНИК '{radnikIme}' НИЈЕ ПРОНАЂЕН!");
                    MessageBox.Show($"❌ Радник '{radnikIme}' није пронађен!\n\nПроверите да ли је тачно унето име.", "Грешка");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 ГРЕШКА у UcitajPodatkeORadniku: {ex.Message}");
                MessageBox.Show($"Грешка при учитавању података: {ex.Message}", "Грешка");
            }
        }

        private void BtnDetaljiRadnika_Click(object sender, EventArgs e)
        {
            try
            {
                if (trenutniRadnikId == 0)
                {
                    MessageBox.Show("Молимо изаберите радника!", "Грешка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ★★★ OVO TREBA DA POZIVA NOVU FORMU ★★★
                using (DetaljiRadnikaForm detaljiForm = new DetaljiRadnikaForm(trenutniRadnikId, trenutnaGodina))
                {
                    detaljiForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при отварању детаља: {ex.Message}");
                MessageBox.Show($"Грешка при отварању детаља радника: {ex.Message}", "Грешка");
            }
        }

        private void BtnOsvezi_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 ===== ПОЧЕТАК ОСВЕЖАВАЊА =====");

                //  1. PROVERA TRENUTNOG STANJA 
                System.Diagnostics.Debug.WriteLine($"📊 Пре освежавања:");
                System.Diagnostics.Debug.WriteLine($"   - Радник ИД: {trenutniRadnikId}");
                System.Diagnostics.Debug.WriteLine($"   - Генерисани распоред: {generisaniRaspored?.Count ?? 0} дана");
                System.Diagnostics.Debug.WriteLine($"   - Сачувани статуси: {sacuvaniStatusi?.Count ?? 0} дана");

                //  2. PONOVO UČITAJ SVE PODATKE IZ BAZE 
                System.Diagnostics.Debug.WriteLine("📥 Учитавам податке из базе...");
                UcitajPodatkeORadniku();

                //  3. PONOVO NACRTAJ CELI KALENDAR 
                System.Diagnostics.Debug.WriteLine("🎨 Поново цртам календар...");
                NacrtajKalendar(); //  OVO JE KLJUČNO! 

                //  4. AŽURIRAJ STATISTIKU 
                System.Diagnostics.Debug.WriteLine("📈 Ажурирам статистику...");
                AzurirajStatistiku();

                System.Diagnostics.Debug.WriteLine("✅ ===== ОСВЕЖАВАЊЕ ЗАВРШЕНО =====");

                MessageBox.Show($"Подаци су успешно освежени!\nСада ће те видети ажуриране сате у статистици.",
                              "Освежавање", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 ГРЕШКА при освежавању: {ex.Message}");
                MessageBox.Show($"Грешка при освежавању: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrvih6_Click(object sender, EventArgs e)
        {
            if (!prikazPrvih6)
            {
                prikazPrvih6 = true;
                btnPrvih6.BackColor = Color.DodgerBlue;
                btnPrvih6.ForeColor = Color.White;
                btnDrugih6.BackColor = Color.LightGray;
                btnDrugih6.ForeColor = Color.Black;

                // ★★★ OSVEŽI SVE ★★★
                UcitajPodatkeORadniku();
                NacrtajKalendar();
                AzurirajStatistiku();
            }
        }
      
        private void BtnDrugih6_Click(object sender, EventArgs e)
        {
            if (prikazPrvih6)
            {
                prikazPrvih6 = false;
                btnDrugih6.BackColor = Color.DodgerBlue;
                btnDrugih6.ForeColor = Color.White;
                btnPrvih6.BackColor = Color.LightGray;
                btnPrvih6.ForeColor = Color.Black;

                // ★★★ OSVEŽI SVE ★★★
                UcitajPodatkeORadniku();
                NacrtajKalendar();
                AzurirajStatistiku();
            }
        }
      
        private void NumGodina_ValueChanged(object sender, EventArgs e)
        {
            trenutnaGodina = (int)numGodina.Value;

            // ★★★ OSVEŽI SVE ★★★
            UcitajPodatkeORadniku();
            NacrtajKalendar();
            AzurirajStatistiku();
        }
       
        private void DebugPrikazPodataka()
        {
            // Prikaz prvih 5 dana iz generisanog rasporeda
            System.Diagnostics.Debug.WriteLine("🔍 GENERISANI RASPORED (prvih 5 dana):");
            var prvih5Generisanih = generisaniRaspored.OrderBy(x => x.Key).Take(5);
            foreach (var dan in prvih5Generisanih)
            {
                System.Diagnostics.Debug.WriteLine($"   {dan.Key:dd.MM.yyyy} - {dan.Value}");
            }

            // Prikaz svih ručno sačuvanih statusa
            System.Diagnostics.Debug.WriteLine($"🔍 SAČUVANI STATUSI ({sacuvaniStatusi.Count}):");
            foreach (var dan in sacuvaniStatusi)
            {
                System.Diagnostics.Debug.WriteLine($"   {dan.Key:dd.MM.yyyy} - {dan.Value}");
            }

            // Provera preklapanja
            var preklopljeniDani = generisaniRaspored.Keys.Intersect(sacuvaniStatusi.Keys);
            System.Diagnostics.Debug.WriteLine($"🔍 PREKLOPLJENI DANI ({preklopljeniDani.Count()}):");
            foreach (var datum in preklopljeniDani)
            {
                System.Diagnostics.Debug.WriteLine($"   {datum:dd.MM.yyyy} - Generisano: {generisaniRaspored[datum]}, Sačuvano: {sacuvaniStatusi[datum]}");
            }
        }
    }
}