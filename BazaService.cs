﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RadnoVreme
{
    public class BazaService
    {
        private readonly string connectionString;

        public BazaService()
        {
            connectionString = @"Data Source=MILANDJ\SQLEXPRESS;Initial Catalog=RadnoVreme;Integrated Security=True;Encrypt=False;Connect Timeout=30";
        }

        public string ConnectionString
        {
            get { return connectionString; }
        }

        public enum TipPocetneSmene
        {
            ДНЕВНА,
            НОЋНА
        }

        public bool AzurirajStrukturuBaze()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Proveri da li kolone postoje
                    string checkQuery = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'RadnikStatusi' 
                  AND (COLUMN_NAME = 'Sati' OR COLUMN_NAME = 'Minute')";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        int columnCount = (int)checkCmd.ExecuteScalar();

                        // Ako nedostaju obe kolone
                        if (columnCount < 2)
                        {
                            System.Diagnostics.Debug.WriteLine("🔧 Додајем колоне Sati и Minute у табелу RadnikStatusi...");

                            // Dodaj kolone
                            string alterQuery = @"
                        ALTER TABLE RadnikStatusi
                        ADD Sati INT NULL,
                            Minute INT NULL;";

                            using (SqlCommand alterCmd = new SqlCommand(alterQuery, conn))
                            {
                                alterCmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine("✅ Успешно додате колоне Sati и Minute");

                                // Ažuriraj postojeće podatke
                                string updateQuery = @"
                            UPDATE RadnikStatusi 
                            SET Sati = 
                                CASE 
                                    WHEN Status = 'Рад' THEN 12
                                    WHEN Status IN ('Годишњи', 'Боловање', 'Плаћено', 'ССПК', 'Службено', 'Слава') THEN 8
                                    WHEN Status = 'Слободан' THEN 0
                                    ELSE 8
                                END,
                                Minute = 0
                            WHERE Sati IS NULL;";

                                using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                                {
                                    int updatedRows = updateCmd.ExecuteNonQuery();
                                    System.Diagnostics.Debug.WriteLine($"✅ Ажурирано {updatedRows} постојећих записа");
                                }

                                return true;
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("ℹ️ Колоне Sati и Minute већ постоје");
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при ажурирању базе: {ex.Message}");
                MessageBox.Show($"Грешка при ажурирању базе: {ex.Message}\n\n" +
                               "Молим покрените SQL скрипту ручно.", "Грешка");
                return false;
            }
        }

        public Dictionary<DateTime, string> GenerisiRasporedSmena(DateTime pocetniDatum, string tipSmene, int brojDana = 365, string tipPocetneSmene = "DNEVNA")
        {
            Dictionary<DateTime, string> raspored = new Dictionary<DateTime, string>();

            DateTime trenutniDatum = pocetniDatum.Date;

            int pomeraj = 0;
            if (tipPocetneSmene == "НОЋНА_ПРВИ_ДЕО" || tipPocetneSmene == "НОЋНА")
            {
                pomeraj = 1; // Помери за један дан унапред у цикулусу
            }

            System.Diagnostics.Debug.WriteLine($"🔧 Генерисање распореда са НОВИМ цикулусом:");
            System.Diagnostics.Debug.WriteLine($"   - Почетни датум: {pocetniDatum:dd.MM.yyyy}");
            System.Diagnostics.Debug.WriteLine($"   - Тип почетне смене: {tipPocetneSmene}");
            System.Diagnostics.Debug.WriteLine($"   - Померај: {pomeraj}");

            for (int i = 0; i < brojDana; i++)
            {
                string smena = GetSmenaPoCiklusu(i + pomeraj, tipSmene);

                // ★★★ НОВО: Сви типови смена се директно додају ★★★
                if (smena != "ОДМОР")
                {
                    if (!raspored.ContainsKey(trenutniDatum))
                    {
                        raspored.Add(trenutniDatum, smena);

                        System.Diagnostics.Debug.WriteLine($"   - {trenutniDatum:dd.MM.yyyy}: {smena} " +
                            GetSatnicaInfo(smena));
                    }
                }

                trenutniDatum = trenutniDatum.AddDays(1);
            }

            System.Diagnostics.Debug.WriteLine($"🔧 Генерисан распоред: {raspored.Count} дана");
            return raspored;
        }

        public bool GenerisiRasporedZaSmenu(string smena, int godina)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // ★★★ PROVERA POČETNOG DATUMA ★★★
                    DateTime pocetniDatum;
                    string tipPocetneSmene;
                    string queryPocetniDatum = "SELECT PocetniDatum, TipPocetneSmene FROM PocetniDatumiSmena WHERE Smena = @Smena AND Godina = @Godina";

                    using (SqlCommand cmdPocetni = new SqlCommand(queryPocetniDatum, conn))
                    {
                        cmdPocetni.Parameters.AddWithValue("@Smena", smena);
                        cmdPocetni.Parameters.AddWithValue("@Godina", godina);

                        using (SqlDataReader reader = cmdPocetni.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show($"❌ Није постављен почетни датум за {smena} у {godina}. години!\n\nПрво поставите почетни датум.", "Грешка",
                                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }

                            pocetniDatum = (DateTime)reader["PocetniDatum"];
                            tipPocetneSmene = reader["TipPocetneSmene"]?.ToString() ?? "DNEVNA";
                        }
                    }

                    // ★★★ OBRISI STARI RASPORED ZA SMENU ★★★
                    string deleteQuery = "DELETE FROM RasporedSmena WHERE Smena = @Smena AND Godina = @Godina";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@Smena", smena);
                        deleteCmd.Parameters.AddWithValue("@Godina", godina);
                        int obrisanoRedova = deleteCmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"🗑️ Обрисано {obrisanoRedova} старих ставки распореда за {smena}");
                    }

                    // ★★★ GENERIŠI NOVI RASPORED ZA SMENU ★★★
                    var raspored = GenerisiRasporedSmena(pocetniDatum, smena, 365, tipPocetneSmene);

                    // ★★★ SAČUVAJ RASPORED ZA SMENU ★★★
                    using (var transaction = conn.BeginTransaction())
                    {
                        // ★★★ ИСПРАВЉЕН INSERT QUERY - ДОДАТА КОЛОНА Satnica ★★★
                        string insertQuery = @"INSERT INTO RasporedSmena (Smena, Datum, TipSmene, Godina, Mesec, Satnica) 
                                      VALUES (@Smena, @Datum, @TipSmene, @Godina, @Mesec, @Satnica)";

                        int brojDodatihStavki = 0;
                        foreach (var stavka in raspored)
                        {
                            using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn, transaction))
                            {
                                insertCmd.Parameters.AddWithValue("@Smena", smena);
                                insertCmd.Parameters.AddWithValue("@Datum", stavka.Key);
                                insertCmd.Parameters.AddWithValue("@TipSmene", stavka.Value);
                                insertCmd.Parameters.AddWithValue("@Godina", stavka.Key.Year);
                                insertCmd.Parameters.AddWithValue("@Mesec", stavka.Key.Month);

                                // ★★★ ДОДАЈЕМО САТНИЦУ У БАЗУ ★★★
                                string satnica = GetSatnicaZaSmenu(stavka.Value);
                                insertCmd.Parameters.AddWithValue("@Satnica", satnica);

                                insertCmd.ExecuteNonQuery();
                                brojDodatihStavki++;
                            }
                        }
                        transaction.Commit();
                        System.Diagnostics.Debug.WriteLine($"✅ Додато {brojDodatihStavki} нових ставки распореда за {smena}");
                    }

                    // ★★★ ПРВО ГЕНЕРИШИ РАДНИКЕ ЗА СМЕНУ ★★★
                    int brojAzuriranihRadnika = PovuciRasporedZaSveRadnikeSmena(smena, godina, conn);

                    // ★★★ ЗАТИМ АЖУРИРАЈ САТНИЦУ ЗА СВЕ РАДНИКЕ ★★★
                    AzurirajSatnicuZaSveRadnikeSmena(smena, godina, conn);

                    MessageBox.Show($"✅ Успешно генерисан нови распоред за {smena} u {godina}. години!\n\n" +
                                  $"📅 Почетни датум: {pocetniDatum:dd.MM.yyyy}\n" +
                                  $"🌅 Почиње са: {tipPocetneSmene} сменом\n" +
                                  $"📊 Генерисано дана: {raspored.Count}\n" +
                                  $"👥 Ажурирано радника: {brojAzuriranihRadnika}\n" +
                                  $"⏰ Ноћне смене подељене на 5+7 сати",
                                  "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при генерисању распореда за смену '{smena}': {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"💥 Greška u GenerisiRasporedZaSmenu: {ex.Message}");
                return false;
            }
        }

        public void GenerisiLicneRasporedeZaSmenu(string smena, DateTime pocetniDatum, int brojDana)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // 1. Pronađi sve radnike u datoj smeni
                string queryRadnici = "SELECT Id, Ime, Prezime FROM Radnici WHERE Smena = @Smena";
                List<Radnik> radnici = new List<Radnik>();

                using (SqlCommand cmd = new SqlCommand(queryRadnici, conn))
                {
                    cmd.Parameters.AddWithValue("@Smena", smena);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            radnici.Add(new Radnik
                            {
                                Id = reader.GetInt32(0),
                                Ime = reader.GetString(1),
                                Prezime = reader.GetString(2)
                            });
                        }
                    }
                }

                // 2. Za svakog radnika generiši lični raspored
                foreach (var radnik in radnici)
                {
                    string licniRaspored = GenerisiLicniRaspored(radnik, pocetniDatum, brojDana);

                    // 3. Sačuvaj lični raspored u bazi
                    string updateQuery = "UPDATE Radnici SET Raspored = @Raspored WHERE Id = @RadnikId";
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@Raspored", licniRaspored);
                        updateCmd.Parameters.AddWithValue("@RadnikId", radnik.Id);
                        updateCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private string GenerisiLicniRaspored(Radnik radnik, DateTime pocetniDatum, int brojDana)
        {
            StringBuilder raspored = new StringBuilder();
            DateTime datum = pocetniDatum;

            for (int i = 0; i < brojDana; i++)
            {
                string danUSedmici = datum.DayOfWeek.ToString();
                string radniDan = OdrediRadniDan(radnik, danUSedmici, datum);

                raspored.AppendLine($"{datum:dd.MM.yyyy} ({danUSedmici}): {radniDan}");
                datum = datum.AddDays(1);
            }

            return raspored.ToString();
        }

        private void AzurirajSatnicuZaSveRadnikeSmena(string smena, int godina, SqlConnection conn)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"⏰ Ажурирам сатницу за све раднике у {smena}...");

                // 1. Узми све раднике у смени
                string queryRadnici = "SELECT Id FROM Radnici WHERE Smena = @Smena AND Aktivan = 1";
                List<int> radniciIds = new List<int>();

                using (SqlCommand cmdRadnici = new SqlCommand(queryRadnici, conn))
                {
                    cmdRadnici.Parameters.AddWithValue("@Smena", smena);
                    using (SqlDataReader reader = cmdRadnici.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            radniciIds.Add(reader.GetInt32(0));
                        }
                    }
                }

                // 2. Узми генерисани распоред за ову годину
                var rasporedSmena = UzmiRasporedZaSmenuIzBaze(smena, godina, conn);

                // 3. За сваког радника, ажурирај статусе са сатницом
                foreach (int radnikId in radniciIds)
                {
                    AzurirajSatnicuZaRadnika(radnikId, rasporedSmena, conn);
                }

                System.Diagnostics.Debug.WriteLine($"✅ Ажурирана сатница за {radniciIds.Count} радника");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при ажурирању сатнице: {ex.Message}");
            }
        }

        private void AzurirajSatnicuZaRadnika(int radnikId, Dictionary<DateTime, string> raspored, SqlConnection conn)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"⏰ Ажурирам сатницу за радника ИД {radnikId}...");

                foreach (var stavka in raspored)
                {
                    DateTime datum = stavka.Key;
                    string tipSmene = stavka.Value;

                    // ★★★ ПРОВЕРИ ДА ЛИ ПОСТОЈИ СТАТУС ЗА ОВАЈ ДАН ★★★
                    string checkQuery = "SELECT COUNT(*) FROM RadnikStatusi WHERE RadnikId = @RadnikId AND Datum = @Datum";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@RadnikId", radnikId);
                        checkCmd.Parameters.AddWithValue("@Datum", datum);

                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            // ★★★ АЖУРИРАЈ ПОСТОЈЕЋИ СТАТУС СА САТНИЦОМ ★★★
                            string updateQuery = @"UPDATE RadnikStatusi 
                                          SET Sati = @Sati,
                                              Minute = @Minute,
                                              SatiDana1 = @SatiDana1,
                                              SatiDana2 = @SatiDana2,
                                              JeNocnaSmena = @JeNocnaSmena
                                          WHERE RadnikId = @RadnikId AND Datum = @Datum";

                            using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                            {
                                updateCmd.Parameters.AddWithValue("@RadnikId", radnikId);
                                updateCmd.Parameters.AddWithValue("@Datum", datum);

                                // ★★★ ПОСТАВИ САТНИЦУ ПРЕМА ТИПУ СМЕНЕ ★★★
                                if (tipSmene == "ДНЕВНА")
                                {
                                    updateCmd.Parameters.AddWithValue("@Sati", 12);
                                    updateCmd.Parameters.AddWithValue("@Minute", 0);
                                    updateCmd.Parameters.AddWithValue("@SatiDana1", 12);
                                    updateCmd.Parameters.AddWithValue("@SatiDana2", 0);
                                    updateCmd.Parameters.AddWithValue("@JeNocnaSmena", 0);
                                }
                                else if (tipSmene == "НОЋНА_ПРВИ_ДЕО")
                                {
                                    updateCmd.Parameters.AddWithValue("@Sati", 5);
                                    updateCmd.Parameters.AddWithValue("@Minute", 0);
                                    updateCmd.Parameters.AddWithValue("@SatiDana1", 5);
                                    updateCmd.Parameters.AddWithValue("@SatiDana2", 0);
                                    updateCmd.Parameters.AddWithValue("@JeNocnaSmena", 1);
                                }
                                else if (tipSmene == "ПРЕДАЈА_ДУЖНОСТИ")
                                {
                                    updateCmd.Parameters.AddWithValue("@Sati", 7);
                                    updateCmd.Parameters.AddWithValue("@Minute", 0);
                                    updateCmd.Parameters.AddWithValue("@SatiDana1", 0);
                                    updateCmd.Parameters.AddWithValue("@SatiDana2", 7);
                                    updateCmd.Parameters.AddWithValue("@JeNocnaSmena", 1);
                                }
                                else // ОДМОР или други
                                {
                                    updateCmd.Parameters.AddWithValue("@Sati", 0);
                                    updateCmd.Parameters.AddWithValue("@Minute", 0);
                                    updateCmd.Parameters.AddWithValue("@SatiDana1", 0);
                                    updateCmd.Parameters.AddWithValue("@SatiDana2", 0);
                                    updateCmd.Parameters.AddWithValue("@JeNocnaSmena", 0);
                                }

                                updateCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // ★★★ ДОДАЈ НОВИ СТАТУС СА САТНИЦОМ ★★★
                            string insertQuery = @"INSERT INTO RadnikStatusi 
                                          (RadnikId, Datum, Status, Sati, Minute, SatiDana1, SatiDana2, JeNocnaSmena, Izvor, DatumKreiranja) 
                                          VALUES (@RadnikId, @Datum, @Status, @Sati, @Minute, @SatiDana1, @SatiDana2, @JeNocnaSmena, 'GENERISANO', GETDATE())";

                            using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                            {
                                insertCmd.Parameters.AddWithValue("@RadnikId", radnikId);
                                insertCmd.Parameters.AddWithValue("@Datum", datum);

                                // ★★★ ПОСТАВИ СТАТУС И САТНИЦУ ★★★
                                string statusText = "Слободан";
                                if (tipSmene == "ДНЕВНА" || tipSmene == "НОЋНА_ПРВИ_ДЕО" || tipSmene == "ПРЕДАЈА_ДУЖНОСТИ")
                                {
                                    statusText = "Рад";
                                }

                                insertCmd.Parameters.AddWithValue("@Status", statusText);

                                // ★★★ ПОСТАВИ САТНИЦУ ПРЕМА ТИПУ СМЕНЕ ★★★
                                if (tipSmene == "ДНЕВНА")
                                {
                                    insertCmd.Parameters.AddWithValue("@Sati", 12);
                                    insertCmd.Parameters.AddWithValue("@Minute", 0);
                                    insertCmd.Parameters.AddWithValue("@SatiDana1", 12);
                                    insertCmd.Parameters.AddWithValue("@SatiDana2", 0);
                                    insertCmd.Parameters.AddWithValue("@JeNocnaSmena", 0);
                                }
                                else if (tipSmene == "НОЋНА_ПРВИ_ДЕО")
                                {
                                    insertCmd.Parameters.AddWithValue("@Sati", 5);
                                    insertCmd.Parameters.AddWithValue("@Minute", 0);
                                    insertCmd.Parameters.AddWithValue("@SatiDana1", 5);
                                    insertCmd.Parameters.AddWithValue("@SatiDana2", 0);
                                    insertCmd.Parameters.AddWithValue("@JeNocnaSmena", 1);
                                }
                                else if (tipSmene == "ПРЕДАЈА_ДУЖНОСТИ")
                                {
                                    insertCmd.Parameters.AddWithValue("@Sati", 7);
                                    insertCmd.Parameters.AddWithValue("@Minute", 0);
                                    insertCmd.Parameters.AddWithValue("@SatiDana1", 0);
                                    insertCmd.Parameters.AddWithValue("@SatiDana2", 7);
                                    insertCmd.Parameters.AddWithValue("@JeNocnaSmena", 1);
                                }
                                else // ОДМОР или други
                                {
                                    insertCmd.Parameters.AddWithValue("@Sati", 0);
                                    insertCmd.Parameters.AddWithValue("@Minute", 0);
                                    insertCmd.Parameters.AddWithValue("@SatiDana1", 0);
                                    insertCmd.Parameters.AddWithValue("@SatiDana2", 0);
                                    insertCmd.Parameters.AddWithValue("@JeNocnaSmena", 0);
                                }

                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ Ажурирана сатница за радника ИД {radnikId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при ажурирању сатнице за радника ИД {radnikId}: {ex.Message}");
            }
        }

        private int PovuciRasporedZaSveRadnikeSmena(string smena, int godina, SqlConnection conn)
        {
            int brojAzuriranihRadnika = 0;

            try
            {
                // ★★★ PRONAĐI SVE RADNIKE U OVOJ SMENI ★★★
                string queryRadnici = "SELECT Id FROM Radnici WHERE Smena = @Smena AND Aktivan = 1";
                List<int> radniciIds = new List<int>();

                using (SqlCommand cmdRadnici = new SqlCommand(queryRadnici, conn))
                {
                    cmdRadnici.Parameters.AddWithValue("@Smena", smena);
                    using (SqlDataReader reader = cmdRadnici.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            radniciIds.Add(reader.GetInt32(0));
                        }
                    }
                }

                // ★★★ UZMI GENERISANI RASPORED ZA OVU SMENU ★★★
                var rasporedSmena = UzmiRasporedZaSmenuIzBaze(smena, godina, conn);

                // ★★★ ZA SVAKOG RADNIKA, POSTAVI GENERISANI RASPORED KAO POČETNI ★★★
                foreach (int radnikId in radniciIds)
                {
                    if (PostaviPocetniRasporedZaRadnika(radnikId, rasporedSmena, conn))
                    {
                        brojAzuriranihRadnika++;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ Повучен распоред за {brojAzuriranihRadnika} радника из {smena}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при повлачењу распореда за радника: {ex.Message}");
            }

            return brojAzuriranihRadnika;
        }

        private Dictionary<DateTime, string> UzmiRasporedZaSmenuIzBaze(string smena, int godina, SqlConnection conn)
        {
            Dictionary<DateTime, string> raspored = new Dictionary<DateTime, string>();

            string query = "SELECT Datum, TipSmene FROM RasporedSmena WHERE Smena = @Smena AND Godina = @Godina ORDER BY Datum";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Smena", smena);
                cmd.Parameters.AddWithValue("@Godina", godina);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime datum = (DateTime)reader["Datum"];
                        string tipSmene = reader["TipSmene"].ToString();
                        raspored.Add(datum, tipSmene);
                    }
                }
            }

            return raspored;
        }

        private bool PostaviPocetniRasporedZaRadnika(int radnikId, Dictionary<DateTime, string> raspored, SqlConnection conn)
        {
            try
            {
                //  OBRISI SVE POSTOJEĆE STATUSE ZA OVOG RADNIKA 
                string deleteQuery = "DELETE FROM RadnikStatusi WHERE RadnikId = @RadnikId";
                using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn))
                {
                    deleteCmd.Parameters.AddWithValue("@RadnikId", radnikId);
                    deleteCmd.ExecuteNonQuery();
                }

                //  ISPRAVLJЕН INSERT - DODAJ DATUMKREIRANJA 
                string insertQuery = @"INSERT INTO RadnikStatusi (RadnikId, Datum, Status, Sati, Minute, Izvor, DatumKreiranja) 
                              VALUES (@RadnikId, @Datum, @Status, @Sati, @Minute, 'GENERISANO', GETDATE())";

                int brojDodatih = 0;
                foreach (var stavka in raspored)
                {
                    // Konvertuj tip smene u status
                    string status = stavka.Value;
                    int sati = 0;
                    int minute = 0;

                    switch (stavka.Value)
                    {
                        case "ДНЕВНА":
                            status = "Рад";
                            sati = 12;
                            break;
                        case "НОЋНА_ПРВИ_ДЕО":
                            status = "НОЋНА_ПРВИ_ДЕО";
                            sati = 5;
                            break;
                        case "ПРЕДАЈА_ДУЖНОСТИ":
                            status = "ПРЕДАЈА_ДУЖНОСТИ";
                            sati = 7;
                            break;
                        case "ОДМОР":
                            status = "Слободан";
                            sati = 0;
                            break;
                    }

                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@RadnikId", radnikId);
                        insertCmd.Parameters.AddWithValue("@Datum", stavka.Key);
                        insertCmd.Parameters.AddWithValue("@Status", status);
                        insertCmd.Parameters.AddWithValue("@Sati", sati);
                        insertCmd.Parameters.AddWithValue("@Minute", minute);
                        insertCmd.ExecuteNonQuery();
                        brojDodatih++;

                        System.Diagnostics.Debug.WriteLine($"   📅 Постављено: {stavka.Key:dd.MM.yyyy} -> {status}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ Постављен почетни распоред за радника ИД {radnikId} - {brojDodatih} дана");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при постављању распореда за радника ИД {radnikId}: {ex.Message}");
                return false;
            }
        }

        public Dictionary<DateTime, string> UzmiRasporedZaSmenu(string smena, int godina, int mesec)
        {
            Dictionary<DateTime, string> raspored = new Dictionary<DateTime, string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // ★★★ ИСПРАВЉЕН QUERY - САМО TipSmene ★★★
                    string query = "SELECT Datum, TipSmene FROM RasporedSmena WHERE Smena = @Smena AND Godina = @Godina AND Mesec = @Mesec ORDER BY Datum";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Smena", smena);
                        cmd.Parameters.AddWithValue("@Godina", godina);
                        cmd.Parameters.AddWithValue("@Mesec", mesec);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DateTime datum = (DateTime)reader["Datum"];
                                string tipSmene = reader["TipSmene"].ToString();

                                // ★★★ ИСПРАВЉЕНО: Само tip smene, не спајамо са satnicom ★★★
                                raspored.Add(datum, tipSmene);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању распореда за смену '{smena}': {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return raspored;
        }

        public Dictionary<DateTime, string> UzmiRasporedZaRadnika(int radnikId, int godina, int mesec)
        {
            try
            {
                // Prvo pronađi smenu radnika
                string smena = "";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Smena FROM Radnici WHERE Id = @Id AND Aktivan = 1";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", radnikId);
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            smena = result.ToString();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(smena))
                {
                    // Uzmi raspored za tu smenu
                    return UzmiRasporedZaSmenu(smena, godina, mesec);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Радник ИД {radnikId} није пронађен или нема додељену смену.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању распореда за радника ИД {radnikId}: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return new Dictionary<DateTime, string>();
        }

        private string GetSatnicaZaSmenu(string tipSmene)
        {
            switch (tipSmene)
            {
                case "ДНЕВНА":
                    return "07:00-19:00";
                case "НОЋНА_ПРВИ_ДЕО":
                    return "19:00-24:00";
                case "ПРЕДАЈА_ДУЖНОСТИ":
                    return "00:00-07:00";
                case "НОЋНА":
                    return "19:00-07:00";
                case "ОДМОР":
                    return "00:00-00:00";
                default:
                    return "08:00-16:00";
            }
        }

        private string GetSatnicaInfo(string smena)
        {
            switch (smena)
            {
                case "ДНЕВНА": return "(12 сати)";
                case "НОЋНА_ПРВИ_ДЕО": return "(5 сати - 19:00-24:00)";
                case "ПРЕДАЈА_ДУЖНОСТИ": return "(7 сати - 00:00-07:00)";
                default: return "";
            }
        }

        public bool SacuvajPocetniDatumSmena(string smena, DateTime pocetniDatum, int godina, string tipPocetneSmene, string napomena = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Prvo proveri da li postoji zapis za ovu smenu i godinu
                    string checkQuery = "SELECT COUNT(*) FROM PocetniDatumiSmena WHERE Smena = @Smena AND Godina = @Godina";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Smena", smena);
                        checkCmd.Parameters.AddWithValue("@Godina", godina);

                        int count = (int)checkCmd.ExecuteScalar();

                        string query;
                        if (count > 0)
                        {
                            // ★★★ ИСПРАВЉЕН UPDATE - САДА УРЕДУ ★★★
                            query = @"UPDATE PocetniDatumiSmena 
                             SET PocetniDatum = @PocetniDatum, 
                                 TipPocetneSmene = @TipPocetneSmene,
                                 Napomena = @Napomena,
                                 DatumKreiranja = GETDATE()
                             WHERE Smena = @Smena AND Godina = @Godina";

                            System.Diagnostics.Debug.WriteLine($"⚠️ Ажуриран постојећи почетни датум за {smena} {godina}. године");
                        }
                        else
                        {
                            // ★★★ ИСПРАВЉЕН INSERT - САДА УРЕДУ ★★★
                            query = @"INSERT INTO PocetniDatumiSmena (Smena, PocetniDatum, Godina, TipPocetneSmene, Napomena, DatumKreiranja) 
                             VALUES (@Smena, @PocetniDatum, @Godina, @TipPocetneSmene, @Napomena, GETDATE())";

                            System.Diagnostics.Debug.WriteLine($"✅ Додај нови почетни датум за {smena} {godina}. године");
                        }

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Smena", smena);
                            cmd.Parameters.AddWithValue("@PocetniDatum", pocetniDatum);
                            cmd.Parameters.AddWithValue("@Godina", godina);
                            cmd.Parameters.AddWithValue("@TipPocetneSmene", tipPocetneSmene);
                            cmd.Parameters.AddWithValue("@Napomena", napomena ?? (object)DBNull.Value);

                            int affectedRows = cmd.ExecuteNonQuery();

                            if (affectedRows > 0)
                            {
                                string poruka = count > 0 ?
                                    $"✅ Успешно ажуриран почетни датум за {smena}" :
                                    $"✅ Успешно додат нови почетни датум за {smena}";

                                System.Diagnostics.Debug.WriteLine(poruka);
                                return true;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"❌ Није успело {(count > 0 ? "ажурирање" : "додавање")} почетног датума за {smena}");
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при чувању почетног датума: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"💥 Грешка у SacuvajPocetniDatumSmena: {ex.Message}");
                return false;
            }
        }

        public Dictionary<string, DateTime> UzmiPocetneDatumeSmena(int godina)
        {
            Dictionary<string, DateTime> datumi = new Dictionary<string, DateTime>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("GetPocetniDatumiSmena", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Godina", godina);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string smena = reader["Smena"].ToString();
                                DateTime datum = (DateTime)reader["PocetniDatum"];
                                datumi.Add(smena, datum);
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"SQL Грешка при учитавању почетних датума: {sqlEx.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању почетних датума: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return datumi;
        }

        private string GetSmenaZaDatum(DateTime datum, string tipSmene, DateTime pocetniDatum)
        {
            int danOdPocetka = (int)(datum.Date - pocetniDatum.Date).TotalDays;

            if (danOdPocetka < 0) return "ОДМОР";

            // Isti ciklus za sve smene
            string[] ciklus = { "ДНЕВНА", "НОЋНА", "ОДМОР", "ОДМОР", "ДНЕВНА", "НОЋНА", "ОДМОР", "ОДМОР" };
            int pozicija = danOdPocetka % 8;

            return ciklus[pozicija];
        }

        private string GetSmenaPoCiklusu(int danUCiklusu, string tipSmene)
        {
            string[] ciklus = {
        "ДНЕВНА",           // Дан 0: Дневна смена (12h)
        "НОЋНА_ПРВИ_ДЕО",   // Дан 1: Ноћна смена - први део (5h) - 31. у месецу
        "ПРЕДАЈА_ДУЖНОСТИ", // Дан 2: Предаја дужности (7h) - 1. следећег месеца  
        "ОДМОР",            // Дан 3: Одмор
        "ДНЕВНА",           // Дан 4: Дневна смена
        "НОЋНА_ПРВИ_ДЕО",   // Дан 5: Ноћна смена - први део
        "ПРЕДАЈА_ДУЖНОСТИ", // Дан 6: Предаја дужности
        "ОДМОР"             // Дан 7: Одмор
    };

            int pozicija = danUCiklusu % 8;
            return ciklus[pozicija];
        }

        private string OdrediRadniDan(Radnik radnik, string danUSedmici, DateTime datum)
        {
            // Ovdje dodajte vašu logiku za određivanje radnog dana
            // Na primer: smenski rad, specificna pravila, itd.

            if (danUSedmici == "Saturday" || danUSedmici == "Sunday")
            {
                return "Слободан дан";
            }
            else
            {
                return "Радни дан 08-16h"; // Prilagodite prema smeni
            }
        }

        public bool RasporedPostojiZaSmenu(string smena, int godina)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM RasporedSmena WHERE Smena = @Smena AND Godina = @Godina";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Smena", smena);
                        cmd.Parameters.AddWithValue("@Godina", godina);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при провери распореда за смену '{smena}': {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public List<Radnik> UzmiRadnikeUSmeni(string smena)
        {
            List<Radnik> radnici = new List<Radnik>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT r.*, sb.BojaHex 
                                   FROM Radnici r 
                                   LEFT JOIN SmenaBoe sb ON r.Smena = sb.Smena 
                                   WHERE r.Aktivan = 1 AND r.Smena = @Smena
                                   ORDER BY r.Prezime, r.Ime";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Smena", smena);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                radnici.Add(new Radnik
                                {
                                    Id = (int)reader["Id"],
                                    Ime = reader["Ime"].ToString(),
                                    Prezime = reader["Prezime"].ToString(),
                                    Zvanje = reader["Zvanje"]?.ToString(),
                                    Smena = reader["Smena"].ToString(),
                                    BojaSmena = reader["BojaHex"]?.ToString() ?? "#FFFFFF",
                                    Aktivan = (bool)reader["Aktivan"],
                                    DatumKreiranja = (DateTime)reader["DatumKreiranja"]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању радника за смену '{smena}': {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return radnici;
        }

        public bool TestirajKonekciju()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Proveri da li postoje tabele
                    string checkTableQuery = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Korisnici'";
                    using (SqlCommand cmd = new SqlCommand(checkTableQuery, conn))
                    {
                        int tableCount = (int)cmd.ExecuteScalar();
                        if (tableCount == 0)
                        {
                            MessageBox.Show("База постоји али табеле нису креиране. Покрени SQL скрипту.", "Упозорење!",
                                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                    }

                    return true;
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"SQL Грешка: {sqlEx.Message}\n\nПровери да ли је база креирана.", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при повезивању са базом: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public Korisnik PrijaviKorisnika(string korisnickoIme, string lozinka)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Id, KorisnickoIme, Uloga, Ime, Prezime, Smena FROM Korisnici WHERE KorisnickoIme = @KorisnickoIme AND Lozinka = @Lozinka AND Aktivan = 1";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@KorisnickoIme", korisnickoIme);
                        cmd.Parameters.AddWithValue("@Lozinka", lozinka);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Korisnik
                                {
                                    Id = (int)reader["Id"],
                                    KorisnickoIme = reader["KorisnickoIme"].ToString(),
                                    Uloga = reader["Uloga"].ToString(),
                                    Ime = reader["Ime"].ToString(),
                                    Prezime = reader["Prezime"].ToString(),
                                    Smena = reader["Smena"]?.ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"SQL Грешка при пријави: {sqlEx.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при пријави: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        public List<Radnik> UzmiSveRadnike()
        {
            List<Radnik> radnici = new List<Radnik>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT r.*, sb.BojaHex 
                                   FROM Radnici r 
                                   LEFT JOIN SmenaBoe sb ON r.Smena = sb.Smena 
                                   WHERE r.Aktivan = 1 
                                   ORDER BY r.Prezime, r.Ime";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            radnici.Add(new Radnik
                            {
                                Id = (int)reader["Id"],
                                Ime = reader["Ime"].ToString(),
                                Prezime = reader["Prezime"].ToString(),
                                Zvanje = reader["Zvanje"]?.ToString(),
                                Smena = reader["Smena"].ToString(),
                                BojaSmena = reader["BojaHex"]?.ToString() ?? "#FFFFFF",
                                Aktivan = (bool)reader["Aktivan"],
                                DatumKreiranja = (DateTime)reader["DatumKreiranja"]
                            });
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"SQL Грешка при учитавању радника: {sqlEx.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању радника: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return radnici;
        }

        public List<Radnik> UzmiRadnikePoSmeni(string smena)
        {
            List<Radnik> radnici = new List<Radnik>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query;
                    SqlCommand cmd;

                    if (string.IsNullOrEmpty(smena))
                    {
                        // ADMIN - vidi sve radnike
                        query = @"SELECT r.*, sb.BojaHex 
                                 FROM Radnici r 
                                 LEFT JOIN SmenaBoe sb ON r.Smena = sb.Smena 
                                 WHERE r.Aktivan = 1 
                                 ORDER BY r.Prezime, r.Ime";
                        cmd = new SqlCommand(query, conn);
                    }
                    else
                    {
                        // KORISNIK - vidi samo svoju smenu
                        query = @"SELECT r.*, sb.BojaHex 
                                 FROM Radnici r 
                                 LEFT JOIN SmenaBoe sb ON r.Smena = sb.Smena 
                                 WHERE r.Aktivan = 1 AND r.Smena = @Smena
                                 ORDER BY r.Prezime, r.Ime";
                        cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Smena", smena);
                    }

                    using (cmd)
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            radnici.Add(new Radnik
                            {
                                Id = (int)reader["Id"],
                                Ime = reader["Ime"].ToString(),
                                Prezime = reader["Prezime"].ToString(),
                                Zvanje = reader["Zvanje"]?.ToString(),
                                Smena = reader["Smena"].ToString(),
                                BojaSmena = reader["BojaHex"]?.ToString() ?? "#FFFFFF",
                                Aktivan = (bool)reader["Aktivan"],
                                DatumKreiranja = (DateTime)reader["DatumKreiranja"]
                            });
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"SQL Грешка при учитавању радника: {sqlEx.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању радника: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return radnici;
        }

        public bool DodajRadnika(Radnik radnik)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Prvo dobij boju za smenu
                    string bojaQuery = "SELECT BojaHex FROM SmenaBoe WHERE Smena = @Smena";
                    string bojaSmena = "#FFFFFF";

                    using (SqlCommand bojaCmd = new SqlCommand(bojaQuery, conn))
                    {
                        bojaCmd.Parameters.AddWithValue("@Smena", radnik.Smena);
                        var result = bojaCmd.ExecuteScalar();
                        if (result != null)
                        {
                            bojaSmena = result.ToString();
                        }
                    }

                    // Sada dodaj radnika
                    string query = @"INSERT INTO Radnici (Ime, Prezime, Zvanje, Smena, BojaSmena) 
                                   VALUES (@Ime, @Prezime, @Zvanje, @Smena, @BojaSmena)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Ime", radnik.Ime);
                        cmd.Parameters.AddWithValue("@Prezime", radnik.Prezime);
                        cmd.Parameters.AddWithValue("@Zvanje", radnik.Zvanje ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Smena", radnik.Smena);
                        cmd.Parameters.AddWithValue("@BojaSmena", bojaSmena);

                        int affectedRows = cmd.ExecuteNonQuery();
                        return affectedRows > 0;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"SQL Грешка при додавању радника: {sqlEx.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при додавању радника: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public Dictionary<string, string> UzmiBoeSmena()
        {
            Dictionary<string, string> boje = new Dictionary<string, string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Smena, BojaHex FROM SmenaBoe";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string smena = reader["Smena"].ToString();
                            string boja = reader["BojaHex"].ToString();
                            boje.Add(smena, boja);
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"SQL Грешка при учитавању боја смена: {sqlEx.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Fallback boje
                boje.Add("I смена", "#FFFF00");
                boje.Add("II смена", "#00FF00");
                boje.Add("III смена", "#FF0000");
                boje.Add("IV смена", "#0000FF");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању боја смена: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Fallback boje
                boje.Add("I смена", "#FFFF00");
                boje.Add("II смена", "#00FF00");
                boje.Add("III смена", "#FF0000");
                boje.Add("IV смена", "#0000FF");
            }

            return boje;
        }

        public void ProveriShemu(string smena, DateTime pocetniDatum)
        {
            string result = $"🔍 ПРОВЕРА ШЕМЕ за {smena}\n";
            result += $"Почетни датум: {pocetniDatum:dd.MM.yyyy}\n\n";

            result += "12-Турност: Д-Н-O-O-Д-Н-O-O\n";
            result += "Д = ДНЕВНА, Н = НОЋНА, О = ОДМОР\n\n";

            for (int i = 0; i < 8; i++)
            {
                DateTime datum = pocetniDatum.AddDays(i);
                string tipSmene = GetSmenaZaDatum(datum, smena, pocetniDatum);
                string oznaka = tipSmene == "ДНЕВНА" ? "Д" : tipSmene == "НОЋНА" ? "Н" : "О";

                result += $"Dan {i}: {datum:dd.MM.yyyy} ({datum:dddd}) - {tipSmene} ({oznaka})\n";
            }

            MessageBox.Show(result, "Провера шеме", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ProveriStrukturuTabele()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'PocetniDatumiSmena'";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        System.Diagnostics.Debug.WriteLine("🔍 Структура табеле PocetniDatumiSmena:");
                        while (reader.Read())
                        {
                            string columnName = reader["COLUMN_NAME"].ToString();
                            string dataType = reader["DATA_TYPE"].ToString();
                            string isNullable = reader["IS_NULLABLE"].ToString();
                            System.Diagnostics.Debug.WriteLine($"   {columnName} ({dataType}) - Nullable: {isNullable}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при провери структуре: {ex.Message}");
            }
        }

        public List<SmenaTip> UzmiTipoveSmena()
        {
            List<SmenaTip> tipovi = new List<SmenaTip>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM SmenaTipovi ORDER BY Redosled";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tipovi.Add(new SmenaTip
                            {
                                Id = (int)reader["Id"],
                                Naziv = reader["Naziv"].ToString(),
                                Oznaka = reader["Oznaka"].ToString(),
                                Boja = reader["Boja"].ToString()
                            });
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"SQL Грешка при учитавању типова смена: {sqlEx.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању типова смена: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return tipovi;
        }

        public bool BazaPostoji()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public List<Korisnik> UzmiSveKorisnike()
        {
            List<Korisnik> korisnici = new List<Korisnik>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Id, KorisnickoIme, Uloga, Ime, Prezime, Email, Smena, Aktivan FROM Korisnici ORDER BY Uloga, KorisnickoIme";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            korisnici.Add(new Korisnik
                            {
                                Id = (int)reader["Id"],
                                KorisnickoIme = reader["KorisnickoIme"].ToString(),
                                Uloga = reader["Uloga"].ToString(),
                                Ime = reader["Ime"].ToString(),
                                Prezime = reader["Prezime"].ToString(),
                                Email = reader["Email"]?.ToString(),
                                Smena = reader["Smena"]?.ToString(),
                                Aktivan = (bool)reader["Aktivan"]
                            });
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"SQL Грешка при учитавању корисника: {sqlEx.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању корисника: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return korisnici;
        }

        public bool DodajKorisnika(Korisnik korisnik, int izvrsioKorisnikId = 0)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Prvo proveri da li korisničko ime već postoji
                    string checkQuery = "SELECT COUNT(*) FROM Korisnici WHERE KorisnickoIme = @KorisnickoIme";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@KorisnickoIme", korisnik.KorisnickoIme);
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show($"Корисничко име '{korisnik.KorisnickoIme}' већ постоји!", "Грешка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                    }

                    // Sada dodaj korisnika
                    string query = @"INSERT INTO Korisnici (KorisnickoIme, Lozinka, Uloga, Ime, Prezime, Email, Smena, Aktivan) 
                           VALUES (@KorisnickoIme, @Lozinka, @Uloga, @Ime, @Prezime, @Email, @Smena, @Aktivan);
                           SELECT SCOPE_IDENTITY();";

                    int noviKorisnikId;
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@KorisnickoIme", korisnik.KorisnickoIme);
                        cmd.Parameters.AddWithValue("@Lozinka", korisnik.Lozinka);
                        cmd.Parameters.AddWithValue("@Uloga", korisnik.Uloga);
                        cmd.Parameters.AddWithValue("@Ime", korisnik.Ime);
                        cmd.Parameters.AddWithValue("@Prezime", korisnik.Prezime);
                        cmd.Parameters.AddWithValue("@Email", korisnik.Email ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Smena", korisnik.Smena ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Aktivan", korisnik.Aktivan);

                        noviKorisnikId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // LOGUJ KREIRANJE KORISNIKA
                    if (noviKorisnikId > 0 && izvrsioKorisnikId > 0)
                    {
                        string noviPodaci = $"Корисничко име: {korisnik.KorisnickoIme}, " +
                                          $"Име: {korisnik.Ime}, " +
                                          $"Презиме: {korisnik.Prezime}, " +
                                          $"Емаил: {korisnik.Email}, " +
                                          $"Улога: {korisnik.Uloga}, " +
                                          $"Смена: {korisnik.Smena}, " +
                                          $"Активан: {korisnik.Aktivan}";

                        LogujPromenuKorisnika(noviKorisnikId, null, noviPodaci, "INSERT", izvrsioKorisnikId);
                    }

                    return noviKorisnikId > 0;
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"SQL Грешка при додавању корисника: {sqlEx.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при додавању корисника: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool KorisnickoImePostoji(string korisnickoIme)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Korisnici WHERE KorisnickoIme = @KorisnickoIme AND Aktivan = 1";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@KorisnickoIme", korisnickoIme);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при провери корисничког имена: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
        }

        public void LogujPromenuKorisnika(int korisnikId, string stariPodaci, string noviPodaci, string tipPromene, int izvrsioKorisnikId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO KorisnikPromeneLog (KorisnikId, StariPodaci, NoviPodaci, TipPromene, IzvršioKorisnikId) 
                           VALUES (@KorisnikId, @StariPodaci, @NoviPodaci, @TipPromene, @IzvrsioKorisnikId)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@KorisnikId", korisnikId);
                        cmd.Parameters.AddWithValue("@StariPodaci", stariPodaci ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@NoviPodaci", noviPodaci ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@TipPromene", tipPromene);
                        cmd.Parameters.AddWithValue("@IzvrsioKorisnikId", izvrsioKorisnikId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Грешка при логовању промене: {ex.Message}");
            }
        }

        public void LogujPromenuRadnika(int radnikId, DateTime datum, string stariStatus, string noviStatus,
                                 int? stariSati, int? noviSati, int? stariMinute, int? noviMinute,
                                 bool? jeNocnaSmena, string tipPromene, int izvrsioKorisnikId, string komentar = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO RadnikPromeneLog 
                   (RadnikId, Datum, StariStatus, NoviStatus, 
                    StariSati, NoviSati, StariMinute, NoviMinute,
                    JeNocnaSmena, TipPromene, IzvrsioKorisnikId, Komentar) 
                   VALUES (@RadnikId, @Datum, @StariStatus, @NoviStatus, 
                           @StariSati, @NoviSati, @StariMinute, @NoviMinute,
                           @JeNocnaSmena, @TipPromene, @IzvrsioKorisnikId, @Komentar)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RadnikId", radnikId);
                        cmd.Parameters.AddWithValue("@Datum", datum);
                        cmd.Parameters.AddWithValue("@StariStatus", stariStatus ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@NoviStatus", noviStatus ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@StariSati", stariSati ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@NoviSati", noviSati ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@StariMinute", stariMinute ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@NoviMinute", noviMinute ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@JeNocnaSmena", jeNocnaSmena ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@TipPromene", tipPromene);
                        cmd.Parameters.AddWithValue("@IzvrsioKorisnikId", izvrsioKorisnikId);
                        cmd.Parameters.AddWithValue("@Komentar", komentar ?? (object)DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Грешка при логовању промене радника: {ex.Message}");
            }
        }

        public bool RadnikPromeneLogTabelaPostoji()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                           WHERE TABLE_NAME = 'RadnikPromeneLog'";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public List<string> UzmiSveSmena()
        {
            List<string> smene = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT DISTINCT Smena FROM Radnici 
                           WHERE Smena IS NOT NULL AND Smena <> '' 
                           AND Aktivan = 1
                           ORDER BY Smena";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            smene.Add(reader["Smena"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при учитавању смена: {ex.Message}");
            }

            return smene;
        }

        public DataTable UzmiPromeneKorisnika(int korisnikId)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT 
                           kpl.DatumPromene,
                           kpl.TipPromene,
                           kpl.StariPodaci,
                           kpl.NoviPodaci,
                           k.Ime + ' ' + k.Prezime as KorisnikNaKomeJeRadjeno,
                           kpl.KorisnikId as KorisnikId
                           FROM KorisnikPromeneLog kpl
                           INNER JOIN Korisnici k ON kpl.KorisnikId = k.Id
                           WHERE kpl.IzvršioKorisnikId = @KorisnikId
                           ORDER BY kpl.DatumPromene DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@KorisnikId", korisnikId);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању промена: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dt;
        }

        public DataTable UzmiPromeneRadnika(int radnikId, DateTime od, DateTime @do)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Prvo proveri da li tabela postoji
                    string proveraTabele = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                                   WHERE TABLE_NAME = 'RadnikPromeneLog'";

                    using (SqlCommand cmdProvera = new SqlCommand(proveraTabele, conn))
                    {
                        int tabelaPostoji = (int)cmdProvera.ExecuteScalar();

                        if (tabelaPostoji == 0)
                        {
                            // Tabela ne postoji - vrati prazan DataTable
                            System.Diagnostics.Debug.WriteLine("❌ Табела RadnikPromeneLog не постоји у бази");
                            return dt;
                        }
                    }

                    string query = @"SELECT 
                   rpl.Datum,
                   rpl.StariStatus,
                   rpl.NoviStatus,
                   ISNULL(rpl.StariSati, 0) as StariSati,
                   ISNULL(rpl.NoviSati, 0) as NoviSati,
                   ISNULL(rpl.StariMinute, 0) as StariMinute,
                   ISNULL(rpl.NoviMinute, 0) as NoviMinute,
                   rpl.TipPromene,
                   rpl.DatumPromene,
                   ISNULL(rpl.Komentar, '') as Komentar,
                   k.Ime + ' ' + k.Prezime as Izvršio
                   FROM RadnikPromeneLog rpl
                   LEFT JOIN Korisnici k ON rpl.IzvrsioKorisnikId = k.Id
                   WHERE rpl.RadnikId = @RadnikId 
                     AND rpl.Datum BETWEEN @Od AND @Do
                   ORDER BY rpl.DatumPromene DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RadnikId", radnikId);
                        cmd.Parameters.AddWithValue("@Od", od);
                        cmd.Parameters.AddWithValue("@Do", @do);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при учитавању промена радника: {ex.Message}");
            }

            return dt;
        }

        public DataTable UzmiDetaljeStatusaZaRadnika(int radnikId, int godina)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Query koji vraća detalje po mesecima
                    string query = @"
                SELECT 
                    DATEPART(MONTH, Datum) as Mesec,
                    Status,
                    COUNT(*) as BrojDana,
                    SUM(CASE 
                        WHEN Status = 'Рад' THEN 12
                        WHEN Status = 'Слободан' THEN 0
                        ELSE 8 
                    END) as UkupnoSati
                FROM RadnikStatusi 
                WHERE RadnikId = @RadnikId 
                    AND YEAR(Datum) = @Godina
                    AND Datum <= GETDATE()
                GROUP BY DATEPART(MONTH, Datum), Status
                ORDER BY DATEPART(MONTH, Datum), Status";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RadnikId", radnikId);
                        cmd.Parameters.AddWithValue("@Godina", godina);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при учитавању детаља статуса: {ex.Message}");
            }

            return dt;
        }

        public bool AzurirajKorisnika(Korisnik korisnik, int izvrsioKorisnikId = 0)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Prvo uzmi stare podatke za log
                    string stariPodaci = "";
                    string queryStari = "SELECT KorisnickoIme, Ime, Prezime, Email, Uloga, Smena, Aktivan FROM Korisnici WHERE Id = @Id";

                    using (SqlCommand cmdStari = new SqlCommand(queryStari, conn))
                    {
                        cmdStari.Parameters.AddWithValue("@Id", korisnik.Id);
                        using (SqlDataReader reader = cmdStari.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                stariPodaci = $"Корисничко име: {reader["KorisnickoIme"]}, " +
                                            $"Име: {reader["Ime"]}, " +
                                            $"Презиме: {reader["Prezime"]}, " +
                                            $"Емаил: {reader["Email"]}, " +
                                            $"Улога: {reader["Uloga"]}, " +
                                            $"Смена: {reader["Smena"]}, " +
                                            $"Активан: {reader["Aktivan"]}";
                            }
                        }
                    }

                    // Proveri da li je korisničko ime promenjeno
                    string proveraQuery = "SELECT KorisnickoIme FROM Korisnici WHERE Id = @Id";
                    string staroKorisnickoIme = "";

                    using (SqlCommand proveraCmd = new SqlCommand(proveraQuery, conn))
                    {
                        proveraCmd.Parameters.AddWithValue("@Id", korisnik.Id);
                        var result = proveraCmd.ExecuteScalar();
                        if (result != null)
                        {
                            staroKorisnickoIme = result.ToString();
                        }
                    }

                    // Ako je korisničko ime promenjeno, proveri da li novo već postoji
                    if (staroKorisnickoIme != korisnik.KorisnickoIme)
                    {
                        if (KorisnickoImePostoji(korisnik.KorisnickoIme))
                        {
                            MessageBox.Show($"Корисничко име '{korisnik.KorisnickoIme}' већ постоји!", "Грешка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                    }

                    string query;
                    if (!string.IsNullOrEmpty(korisnik.Lozinka))
                    {
                        query = @"UPDATE Korisnici 
                         SET KorisnickoIme = @KorisnickoIme, Ime = @Ime, Prezime = @Prezime, 
                             Email = @Email, Uloga = @Uloga, Smena = @Smena, Aktivan = @Aktivan,
                             Lozinka = @Lozinka
                         WHERE Id = @Id";
                    }
                    else
                    {
                        query = @"UPDATE Korisnici 
                         SET KorisnickoIme = @KorisnickoIme, Ime = @Ime, Prezime = @Prezime, 
                             Email = @Email, Uloga = @Uloga, Smena = @Smena, Aktivan = @Aktivan
                         WHERE Id = @Id";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", korisnik.Id);
                        cmd.Parameters.AddWithValue("@KorisnickoIme", korisnik.KorisnickoIme);
                        cmd.Parameters.AddWithValue("@Ime", korisnik.Ime);
                        cmd.Parameters.AddWithValue("@Prezime", korisnik.Prezime);
                        cmd.Parameters.AddWithValue("@Email", korisnik.Email ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Uloga", korisnik.Uloga);
                        cmd.Parameters.AddWithValue("@Smena", korisnik.Smena ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Aktivan", korisnik.Aktivan);

                        if (!string.IsNullOrEmpty(korisnik.Lozinka))
                        {
                            cmd.Parameters.AddWithValue("@Lozinka", korisnik.Lozinka);
                        }

                        int affectedRows = cmd.ExecuteNonQuery();

                        // LOGUJ IZMENU KORISNIKA
                        if (affectedRows > 0 && izvrsioKorisnikId > 0)
                        {
                            string noviPodaci = $"Корисничко име: {korisnik.KorisnickoIme}, " +
                                              $"Име: {korisnik.Ime}, " +
                                              $"Презиме: {korisnik.Prezime}, " +
                                              $"Емаил: {korisnik.Email}, " +
                                              $"Улога: {korisnik.Uloga}, " +
                                              $"Смена: {korisnik.Smena}, " +
                                              $"Активан: {korisnik.Aktivan}";

                            LogujPromenuKorisnika(korisnik.Id, stariPodaci, noviPodaci, "UPDATE", izvrsioKorisnikId);
                        }

                        return affectedRows > 0;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"SQL Грешка при ажурирању корисника: {sqlEx.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при ажурирању корисника: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public DataTable UzmiPoslednjePromeneKorisnika(int korisnikId)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT TOP 10 
                           kpl.DatumPromene,
                           kpl.TipPromene,
                           kpl.StariPodaci,
                           kpl.NoviPodaci,
                           admin.Ime + ' ' + admin.Prezime as Izvrsio
                           FROM KorisnikPromeneLog kpl
                           LEFT JOIN Korisnici admin ON kpl.IzvršioKorisnikId = admin.Id
                           WHERE kpl.KorisnikId = @KorisnikId
                           ORDER BY kpl.DatumPromene DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@KorisnikId", korisnikId);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању промена: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dt;
        }

        public bool ObrisiKorisnika(int korisnikId, int izvrsioKorisnikId = 0)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Prvo uzmi podatke korisnika za log
                    string podaciKorisnika = "";
                    string queryStari = "SELECT KorisnickoIme, Ime, Prezime, Email, Uloga, Smena, Aktivan FROM Korisnici WHERE Id = @Id";

                    using (SqlCommand cmdStari = new SqlCommand(queryStari, conn))
                    {
                        cmdStari.Parameters.AddWithValue("@Id", korisnikId);
                        using (SqlDataReader reader = cmdStari.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                podaciKorisnika = $"Корисничко име: {reader["KorisnickoIme"]}, " +
                                                $"Име: {reader["Ime"]}, " +
                                                $"Презиме: {reader["Prezime"]}, " +
                                                $"Емаил: {reader["Email"]}, " +
                                                $"Улога: {reader["Uloga"]}, " +
                                                $"Смена: {reader["Smena"]}, " +
                                                $"Активан: {reader["Aktivan"]}";
                            }
                        }
                    }

                    // SOFT DELETE - samo postavimo Aktivan na false
                    string query = "UPDATE Korisnici SET Aktivan = 0 WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", korisnikId);
                        int affectedRows = cmd.ExecuteNonQuery();

                        // LOGUJ BRISANJE KORISNIKA
                        if (affectedRows > 0 && izvrsioKorisnikId > 0)
                        {
                            LogujPromenuKorisnika(korisnikId, podaciKorisnika, null, "DELETE", izvrsioKorisnikId);
                        }

                        return affectedRows > 0;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"SQL Грешка при брисању корисника: {sqlEx.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при брисању корисника: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool ObrisiStatusRadnika(int radnikId, DateTime datum)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    System.Diagnostics.Debug.WriteLine($"🗑️ Покушавам да обришем статус за радника: {radnikId}, Датум: {datum:dd.MM.yyyy}");

                    string query = "DELETE FROM RadnikStatusi WHERE RadnikId = @RadnikId AND Datum = @Datum";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RadnikId", radnikId);
                        cmd.Parameters.AddWithValue("@Datum", datum);

                        int affectedRows = cmd.ExecuteNonQuery();

                        if (affectedRows > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"✅ УСПЕШНО обрисан статус за радника {radnikId}");
                            return true;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"ℹ️ Није пронађен статус за брисање");
                            return true; // Vraća true jer nema šta da se obriše
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при брисању статуса: {ex.Message}");
                return false;
            }
        }

        public DateTime? UzmiPocetniDatumZaSmenu(string smena, int godina)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT PocetniDatum FROM PocetniDatumiSmena WHERE Smena = @Smena AND Godina = @Godina";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Smena", smena);
                        cmd.Parameters.AddWithValue("@Godina", godina);

                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            return (DateTime)result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при учитавању почетног датума: {ex.Message}", "Грешка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        public void DebugRaspored(string smena, DateTime pocetniDatum)
        {
            string result = $"🔍 DEBUG за {smena}\n";
            result += $"Почетни датум из базе: {pocetniDatum:dd.MM.yyyy} ({pocetniDatum:dddd})\n\n";

            result += "Првих 5 дана генерисања:\n";

            for (int i = 0; i < 5; i++)
            {
                DateTime datum = pocetniDatum.AddDays(i);
                int danOdPocetka = (int)(datum - pocetniDatum).TotalDays;
                string tipSmene = GetSmenaZaDatum(datum, smena, pocetniDatum);

                result += $"i={i} | Datum={datum:dd.MM.} | DanOdPocetka={danOdPocetka} | Smena={tipSmene}\n";
            }

            MessageBox.Show(result, "DEBUG", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void UporediSveSmane()
        {
            string result = "🔍 УПОРЕДНИ ПРИКАЗ СВИХ СМЕНА\n\n";

            // Pretpostavimo ove početne datume
            DateTime[] pocetniDatumi = new DateTime[]
            {
                new DateTime(2024, 12, 1), // I smena
                new DateTime(2024, 12, 3), // II smena  
                new DateTime(2024, 12, 5), // III smena
                new DateTime(2024, 12, 7)  // IV smena
            };

            string[] smene = { "I смена", "II смена", "III смена", "IV смена" };

            for (int s = 0; s < smene.Length; s++)
            {
                result += $"📅 {smene[s]} (почетак: {pocetniDatumi[s]:dd.MM.})\n";

                for (int i = 0; i < 3; i++) // Prva 3 dana
                {
                    DateTime datum = pocetniDatumi[s].AddDays(i);
                    string tipSmene = GetSmenaZaDatum(datum, smene[s], pocetniDatumi[s]);
                    result += $"  Dan {i}: {datum:dd.MM.} - {tipSmene}\n";
                }
                result += "\n";
            }

            MessageBox.Show(result, "Упоредни приказ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public bool SacuvajStatusRadnika(int radnikId, DateTime datum, string status)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // ★★★ DEBUG PROVERA PRE ČUVANJA ★★★
                    System.Diagnostics.Debug.WriteLine($"💾 ПОКУШАВАМ да сачувам за радника: {radnikId}, Датум: {datum:dd.MM.yyyy}, Статус: {status}");

                    string checkQuery = "SELECT COUNT(*) FROM RadnikStatusi WHERE RadnikId = @RadnikId AND Datum = @Datum";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@RadnikId", radnikId);
                        checkCmd.Parameters.AddWithValue("@Datum", datum);

                        int count = (int)checkCmd.ExecuteScalar();
                        System.Diagnostics.Debug.WriteLine($"🔍 Постоји {count} запис за овај дан");

                        string query;
                        if (count > 0)
                        {
                            query = @"UPDATE RadnikStatusi 
                             SET Status = @Status, Izvor = 'LICNO', DatumIzmene = GETDATE()
                             WHERE RadnikId = @RadnikId AND Datum = @Datum";
                        }
                        else
                        {
                            query = @"INSERT INTO RadnikStatusi (RadnikId, Datum, Status, Izvor, DatumKreiranja) 
                             VALUES (@RadnikId, @Datum, @Status, 'LICNO', GETDATE())";
                        }

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@RadnikId", radnikId);
                            cmd.Parameters.AddWithValue("@Datum", datum);
                            cmd.Parameters.AddWithValue("@Status", status);

                            int affectedRows = cmd.ExecuteNonQuery();

                            if (affectedRows > 0)
                            {
                                System.Diagnostics.Debug.WriteLine($"✅ УСПЕО: Сачуван ЛИЧНИ статус радника {radnikId}");

                                // ★★★ POZOVI DEBUG NAKON ČUVANJA ★★★
                                DebugProveraRadnikStatusi(radnikId, datum);
                                return true;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"❌ НИЈЕ УСПЕО: Није сачуван статус за радника {radnikId}");
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка у SacuvajStatusRadnika: {ex.Message}");
                MessageBox.Show($"Грешка при чувању ЛИЧНОГ статуса: {ex.Message}", "Грешка");
            }

            return false;
        }

        public bool SacuvajStatusRadnikaSaSatima(int radnikId, DateTime datum, string status, int sati, int minute = 0,
                                                 bool jeNocnaSmena = false, bool jePrviDeoNocne = true,
                                                 int izvrsioKorisnikId = 0, string komentar = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // ★★★ PRVO UZMI STARE PODATKE ZA LOG ★★★
                    string stariStatus = "";
                    int? stariSati = null;
                    int? stariMinute = null;
                    bool? stariJeNocnaSmena = null;

                    string queryStari = @"SELECT Status, Sati, Minute, JeNocnaSmena 
                                 FROM RadnikStatusi 
                                 WHERE RadnikId = @RadnikId AND Datum = @Datum";

                    using (SqlCommand cmdStari = new SqlCommand(queryStari, conn))
                    {
                        cmdStari.Parameters.AddWithValue("@RadnikId", radnikId);
                        cmdStari.Parameters.AddWithValue("@Datum", datum);

                        using (SqlDataReader reader = cmdStari.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                stariStatus = reader["Status"]?.ToString();
                                stariSati = reader["Sati"] as int?;
                                stariMinute = reader["Minute"] as int?;
                                stariJeNocnaSmena = reader["JeNocnaSmena"] as bool?;
                            }
                        }
                    }

                    // ★★★ FORMATIRANJE STATUSA ZA PRIKAZ ★★★
                    string statusZaPrikaz = status;
                    if (status == "ДНЕВНА") statusZaPrikaz = "Рад";
                    if (status == "Слободан") statusZaPrikaz = "Слободан";

                    string checkQuery = "SELECT COUNT(*) FROM RadnikStatusi WHERE RadnikId = @RadnikId AND Datum = @Datum";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@RadnikId", radnikId);
                        checkCmd.Parameters.AddWithValue("@Datum", datum);

                        int count = (int)checkCmd.ExecuteScalar();
                        System.Diagnostics.Debug.WriteLine($"🔍 Постоји {count} запис за овај дан");

                        string query;
                        if (count > 0)
                        {
                            query = @"UPDATE RadnikStatusi 
                             SET Status = @Status, 
                                 Sati = @Sati,
                                 Minute = @Minute,
                                 JeNocnaSmena = @JeNocnaSmena,
                                 SatiDana1 = @SatiDana1,
                                 SatiDana2 = @SatiDana2,
                                 Izvor = 'LICNO', 
                                 DatumIzmene = GETDATE()
                             WHERE RadnikId = @RadnikId AND Datum = @Datum";
                        }
                        else
                        {
                            query = @"INSERT INTO RadnikStatusi 
                             (RadnikId, Datum, Status, Sati, Minute, JeNocnaSmena, SatiDana1, SatiDana2, Izvor, DatumKreiranja) 
                             VALUES (@RadnikId, @Datum, @Status, @Sati, @Minute, @JeNocnaSmena, @SatiDana1, @SatiDana2, 'LICNO', GETDATE())";
                        }

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@RadnikId", radnikId);
                            cmd.Parameters.AddWithValue("@Datum", datum);
                            cmd.Parameters.AddWithValue("@Status", statusZaPrikaz);
                            cmd.Parameters.AddWithValue("@Sati", sati);
                            cmd.Parameters.AddWithValue("@Minute", minute);
                            cmd.Parameters.AddWithValue("@JeNocnaSmena", jeNocnaSmena ? 1 : 0);

                            // ★★★ PODELE SATI ZA NOĆNE SMENE ★★★
                            if (jeNocnaSmena)
                            {
                                if (jePrviDeoNocne)
                                {
                                    cmd.Parameters.AddWithValue("@SatiDana1", sati);
                                    cmd.Parameters.AddWithValue("@SatiDana2", 0);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@SatiDana1", 0);
                                    cmd.Parameters.AddWithValue("@SatiDana2", sati);
                                }
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@SatiDana1", sati);
                                cmd.Parameters.AddWithValue("@SatiDana2", 0);
                            }

                            int affectedRows = cmd.ExecuteNonQuery();

                            // ★★★ LOGUJ PROMENU NAKON ČUVANJA ★★★
                            if (affectedRows > 0 && izvrsioKorisnikId > 0)
                            {
                                // Odredi tip promene
                                string tipPromene = count > 0 ? "UPDATE" : "INSERT";

                                // Loguj promenu
                                LogujPromenuRadnika(radnikId, datum, stariStatus, statusZaPrikaz,
                                                   stariSati, sati, stariMinute, minute,
                                                   jeNocnaSmena, tipPromene, izvrsioKorisnikId, komentar);

                                System.Diagnostics.Debug.WriteLine($"✅ УСПЕШНО сачувано и логовано: {datum:dd.MM.yyyy} -> {status} ({sati}:{minute:00})");
                            }

                            return affectedRows > 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка у SacuvajStatusRadnikaSaSatima: {ex.Message}");
                MessageBox.Show($"Грешка при чувању статуса: {ex.Message}", "Грешка");
                return false;
            }
        }

        public bool ObrisiStatusRadnika(int radnikId, DateTime datum, int izvrsioKorisnikId = 0, string komentar = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // ★★★ PRVO UZMI STARE PODATKE ZA LOG ★★★
                    string stariStatus = "";
                    int? stariSati = null;
                    int? stariMinute = null;
                    bool? stariJeNocnaSmena = null;

                    string queryStari = @"SELECT Status, Sati, Minute, JeNocnaSmena 
                                 FROM RadnikStatusi 
                                 WHERE RadnikId = @RadnikId AND Datum = @Datum";

                    using (SqlCommand cmdStari = new SqlCommand(queryStari, conn))
                    {
                        cmdStari.Parameters.AddWithValue("@RadnikId", radnikId);
                        cmdStari.Parameters.AddWithValue("@Datum", datum);

                        using (SqlDataReader reader = cmdStari.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                stariStatus = reader["Status"]?.ToString();
                                stariSati = reader["Sati"] as int?;
                                stariMinute = reader["Minute"] as int?;
                                stariJeNocnaSmena = reader["JeNocnaSmena"] as bool?;
                            }
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"🗑️ Покушавам да обришем статус за радника: {radnikId}, Датум: {datum:dd.MM.yyyy}");

                    string query = "DELETE FROM RadnikStatusi WHERE RadnikId = @RadnikId AND Datum = @Datum";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RadnikId", radnikId);
                        cmd.Parameters.AddWithValue("@Datum", datum);

                        int affectedRows = cmd.ExecuteNonQuery();

                        // ★★★ LOGUJ BRISANJE ★★★
                        if (affectedRows > 0 && izvrsioKorisnikId > 0)
                        {
                            LogujPromenuRadnika(radnikId, datum, stariStatus, null,
                                               stariSati, null, stariMinute, null,
                                               stariJeNocnaSmena, "DELETE", izvrsioKorisnikId, komentar);

                            System.Diagnostics.Debug.WriteLine($"✅ УСПЕШНО обрисан и логован статус за радника {radnikId}");
                        }

                        return affectedRows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при брисању статуса: {ex.Message}");
                return false;
            }
        }

        private void SacuvajPredajuDuznosti(int radnikId, DateTime datum, SqlConnection conn)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔧 Аутоматски додајем Предају дужности за {datum:dd.MM.yyyy}");

                // Прво провери да ли већ постоји
                string checkQuery = "SELECT COUNT(*) FROM RadnikStatusi WHERE RadnikId = @RadnikId AND Datum = @Datum";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@RadnikId", radnikId);
                    checkCmd.Parameters.AddWithValue("@Datum", datum);

                    int count = (int)checkCmd.ExecuteScalar();

                    if (count == 0)
                    {
                        string query = @"INSERT INTO RadnikStatusi 
                         (RadnikId, Datum, Status, Sati, Minute, Izvor, DatumKreiranja) 
                         VALUES (@RadnikId, @Datum, 'ПРЕДАЈА_ДУЖНОСТИ', 7, 0, 'GENERISANO', GETDATE())";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@RadnikId", radnikId);
                            cmd.Parameters.AddWithValue("@Datum", datum);
                            cmd.ExecuteNonQuery();

                            System.Diagnostics.Debug.WriteLine($"✅ Аутоматски додата Предаја дужности за {datum:dd.MM.yyyy}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"ℹ️ Већ постоји запис за {datum:dd.MM.yyyy} - не додајем поново");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при додавању предаје дужности: {ex.Message}");
            }
        }

        // U BazaService.cs - proveri da li ova metoda pravilno vraća podatke
        public Dictionary<DateTime, (int satiDana1, int satiDana2, bool jeNocnaSmena)>
            UzmiSateIzBazeZaMesec(int radnikId, int mesec, int godina)
        {
            var satiPoDanu = new Dictionary<DateTime, (int, int, bool)>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"SELECT Datum, 
                           ISNULL(SatiDana1, 0) as SatiDana1, 
                           ISNULL(SatiDana2, 0) as SatiDana2, 
                           ISNULL(JeNocnaSmena, 0) as JeNocnaSmena,
                           ISNULL(Sati, 0) as Sati
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
                                int satiDana1 = Convert.ToInt32(reader["SatiDana1"]);
                                int satiDana2 = Convert.ToInt32(reader["SatiDana2"]);
                                bool jeNocnaSmena = Convert.ToBoolean(reader["JeNocnaSmena"]);
                                int sati = Convert.ToInt32(reader["Sati"]);

                                // ★★★ АКО НЕМА ПОДЕЉЕНЕ САТЕ, КОРИСТИ ОБИЧНЕ САТЕ ★★★
                                if (satiDana1 == 0 && satiDana2 == 0 && sati > 0)
                                {
                                    if (jeNocnaSmena)
                                    {
                                        // Подели ноћну смену на 5+7
                                        satiDana1 = 5;
                                        satiDana2 = 7;
                                    }
                                    else
                                    {
                                        // Обична смена
                                        satiDana1 = sati;
                                        satiDana2 = 0;
                                    }
                                }

                                satiPoDanu[datum] = (satiDana1, satiDana2, jeNocnaSmena);

                                System.Diagnostics.Debug.WriteLine($"📊 Учитано из базе {datum:dd.MM.yyyy}: " +
                                                                 $"{satiDana1}+{satiDana2} сати, Noćна={jeNocnaSmena}");
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
        private void SacuvajNastavakNocneSmene(int radnikId, DateTime datum, int sati, int minute, SqlConnection conn)
        {
            try
            {
                string checkQuery = "SELECT COUNT(*) FROM RadnikStatusi WHERE RadnikId = @RadnikId AND Datum = @Datum";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@RadnikId", radnikId);
                    checkCmd.Parameters.AddWithValue("@Datum", datum);

                    int count = (int)checkCmd.ExecuteScalar();

                    string query;
                    if (count > 0)
                    {
                        query = @"UPDATE RadnikStatusi 
                     SET SatiDana1 = @SatiDana1,
                         JeNocnaSmena = 1
                     WHERE RadnikId = @RadnikId AND Datum = @Datum";
                    }
                    else
                    {
                        query = @"INSERT INTO RadnikStatusi 
                     (RadnikId, Datum, Status, Sati, Minute, SatiDana1, SatiDana2, JeNocnaSmena, Izvor, DatumKreiranja) 
                     VALUES (@RadnikId, @Datum, 'Рад', @Sati, @Minute, @SatiDana1, 0, 1, 'GENERISANO', GETDATE())";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RadnikId", radnikId);
                        cmd.Parameters.AddWithValue("@Datum", datum);
                        cmd.Parameters.AddWithValue("@Sati", sati);
                        cmd.Parameters.AddWithValue("@Minute", minute);
                        cmd.Parameters.AddWithValue("@SatiDana1", sati);

                        cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"✅ Додат наставак ноћне смене за {datum:dd.MM.yyyy}: {sati} сати");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при чувању наставка ноћне смене: {ex.Message}");
            }
        }

        private void ProveriSateUBazi(int radnikId, DateTime datum, SqlConnection conn)
        {
            try
            {
                string proveraQuery = "SELECT Status, Sati, Minute FROM RadnikStatusi WHERE RadnikId = @RadnikId AND Datum = @Datum";

                using (SqlCommand proveraCmd = new SqlCommand(proveraQuery, conn))
                {
                    proveraCmd.Parameters.AddWithValue("@RadnikId", radnikId);
                    proveraCmd.Parameters.AddWithValue("@Datum", datum);

                    using (SqlDataReader reader = proveraCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string status = reader["Status"].ToString();
                            int? sati = reader["Sati"] as int?;
                            int? minute = reader["Minute"] as int?;

                            System.Diagnostics.Debug.WriteLine($"🔍 ПРОВЕРА након чувања:");
                            System.Diagnostics.Debug.WriteLine($"   - Статус у бази: {status}");
                            System.Diagnostics.Debug.WriteLine($"   - Сати у бази: {sati}");
                            System.Diagnostics.Debug.WriteLine($"   - Минути у бази: {minute}");

                            if (sati.HasValue)
                            {
                                System.Diagnostics.Debug.WriteLine($"   ✅ САТИ СУ У БАЗИ!");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"   ❌ САТИ НИСУ У БАЗИ!");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при провери: {ex.Message}");
            }
        }

        public bool ProveriKoloneUBazi()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"SELECT COUNT(*) 
                           FROM INFORMATION_SCHEMA.COLUMNS 
                           WHERE TABLE_NAME = 'RadnikStatusi' 
                             AND COLUMN_NAME IN ('SatiDana1', 'SatiDana2', 'JeNocnaSmena')";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        int count = (int)cmd.ExecuteScalar();
                        return count == 3;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при провери колона: {ex.Message}");
                return false;
            }
        }

        public Dictionary<DateTime, string> UcitajSacuvaneStatuse(int radnikId, int godina)
        {
            Dictionary<DateTime, string> statusi = new Dictionary<DateTime, string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // ★★★ ISPRAVLJEN UPIT - SAMO ZA OVOG RADNIKA ★★★
                    string query = @"SELECT Datum, Status, Izvor 
                           FROM RadnikStatusi 
                           WHERE RadnikId = @RadnikId AND YEAR(Datum) = @Godina
                           ORDER BY Datum";

                    System.Diagnostics.Debug.WriteLine($"🔍 Учитавам статусе за радника: {radnikId}, Година: {godina}");

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RadnikId", radnikId);
                        cmd.Parameters.AddWithValue("@Godina", godina);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            int brojUcitanih = 0;
                            while (reader.Read())
                            {
                                DateTime datum = (DateTime)reader["Datum"];
                                string status = reader["Status"].ToString();
                                string izvor = reader["Izvor"]?.ToString() ?? "GENERISANO";

                                statusi.Add(datum, status);
                                brojUcitanih++;

                                System.Diagnostics.Debug.WriteLine($"   📋 {datum:dd.MM.yyyy} -> {status} (Izvor: {izvor})");
                            }

                            System.Diagnostics.Debug.WriteLine($"✅ Учитано {brojUcitanih} ЛИЧНИХ статуса за радника {radnikId}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при учитавању ЛИЧНИХ статуса: {ex.Message}");
            }

            return statusi;
        }

        public void DebugProveraRadnikStatusi(int radnikId, DateTime datum)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Provera svih statusa za ovog radnika
                    string queryRadnik = "SELECT COUNT(*) as BrojStatusa FROM RadnikStatusi WHERE RadnikId = @RadnikId";

                    using (SqlCommand cmd = new SqlCommand(queryRadnik, conn))
                    {
                        cmd.Parameters.AddWithValue("@RadnikId", radnikId);
                        int brojStatusa = (int)cmd.ExecuteScalar();
                        System.Diagnostics.Debug.WriteLine($"🔍 Radnik ID {radnikId} ima {brojStatusa} statusa u bazi");
                    }

                    // Provera konkretnog datuma
                    string queryDatum = "SELECT RadnikId, Status FROM RadnikStatusi WHERE Datum = @Datum";

                    using (SqlCommand cmd = new SqlCommand(queryDatum, conn))
                    {
                        cmd.Parameters.AddWithValue("@Datum", datum);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            System.Diagnostics.Debug.WriteLine($"🔍 Статуси за датум {datum:dd.MM.yyyy}:");
                            while (reader.Read())
                            {
                                int rId = reader.GetInt32(0);
                                string status = reader.GetString(1);
                                System.Diagnostics.Debug.WriteLine($"   - Радник ИД {rId}: {status}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при дебуг провери: {ex.Message}");
            }
        }

        public List<Zvanje> UzmiSvaZvanja()
        {
            List<Zvanje> zvanja = new List<Zvanje>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Id, Naziv, Redosled, Oznaka FROM Zvanja ORDER BY Redosled";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            zvanja.Add(new Zvanje
                            {
                                Id = (int)reader["Id"],
                                Naziv = reader["Naziv"].ToString(),
                                Redosled = (int)reader["Redosled"],
                                Oznaka = reader["Oznaka"]?.ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback zvanja ako tabela ne postoji
                System.Diagnostics.Debug.WriteLine($"Грешка при учитавању звања: {ex.Message}");
                zvanja = GetFallbackZvanja();
            }

            return zvanja;
        }

        private List<Zvanje> GetFallbackZvanja()
        {
            return new List<Zvanje>
        {
            new Zvanje { Id = 1, Naziv = "Шеф смене", Redosled = 1, Oznaka = "ШС" },
            new Zvanje { Id = 2, Naziv = "Вођс ВС чете", Redosled = 2, Oznaka = "ВЧ" },
            new Zvanje { Id = 3, Naziv = "Вођа ВС вода", Redosled = 3, Oznaka = "ВВ" },
            new Zvanje { Id = 4, Naziv = "Вођа ВС одељења", Redosled = 4, Oznaka = "ВО" },
            new Zvanje { Id = 5, Naziv = "Вођа ВС групе", Redosled = 5, Oznaka = "ВГ" },
            new Zvanje { Id = 6, Naziv = "Вођа ВС одељења за ОПР", Redosled = 6, Oznaka = "ВООПР" },
            new Zvanje { Id = 7, Naziv = "Вођа ВС групе за возаче", Redosled = 7, Oznaka = "ВГВ" },
            new Zvanje { Id = 8, Naziv = "Ватрогасац спасилац возач", Redosled = 8, Oznaka = "ВСВ" },
            new Zvanje { Id = 9, Naziv = "Ватрогасац спасилац", Redosled = 9, Oznaka = "ВС" }
        };
        }

        public void TestirajSate()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Proveri nekoliko zapisa
                    string query = @"
                SELECT TOP 5 
                    RadnikId,
                    Datum,
                    Status,
                    Sati,
                    Minute
                FROM RadnikStatusi 
                WHERE Sati IS NOT NULL
                ORDER BY Datum DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        System.Diagnostics.Debug.WriteLine("🔍 Првих 5 записа са сатима:");
                        while (reader.Read())
                        {
                            int radnikId = reader.GetInt32(0);
                            DateTime datum = reader.GetDateTime(1);
                            string status = reader.GetString(2);
                            int sati = reader.GetInt32(3);
                            int minute = reader.GetInt32(4);

                            System.Diagnostics.Debug.WriteLine($"   Радник {radnikId}: {datum:dd.MM.yyyy} - {status} ({sati}:{minute:00})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Грешка при тестирању: {ex.Message}");
            }
        }
    }
}
