namespace RadnoVreme
{
    public class Korisnik
    {
        public int Id { get; set; }
        public string KorisnickoIme { get; set; }
        public string Lozinka { get; set; }
        public string Uloga { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Smena { get; set; }
        public string Email { get; set; }
        public bool Aktivan { get; set; }
    }
}