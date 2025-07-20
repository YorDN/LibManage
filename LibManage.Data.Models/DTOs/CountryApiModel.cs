namespace LibManage.Data.Models.DTOs
{
    public class CountryApiModel
    {
        public Name name { get; set; }
        public string cca2 { get; set; }
        public Flags flags { get; set; }
    }

    public class Name
    {
        public string common { get; set; }
    }

    public class Flags
    {
        public string png { get; set; }
    }


}
