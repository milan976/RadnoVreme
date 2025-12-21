using System;

namespace RadnoVreme
{
    public class Radnik
    {
        public int Id { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Zvanje { get; set; }
        public string Smena { get; set; }
        public string BojaSmena { get; set; } // NOVO
        public bool Aktivan { get; set; } = true;
        public DateTime DatumKreiranja { get; set; } = DateTime.Now;

        public string PunoIme
        {
            get { return $"{Ime} {Prezime}"; }
        }
    }
}