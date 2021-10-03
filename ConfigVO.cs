namespace stresser{
    public class ConfigVO {
        public TypeOfTest TypeOfTest { get; set; }
        public string FileName { get; set; }
        public string StressRequestURL { get; set; }
        public string DeleteRequestURL { get; set; }
        public int Attempt { get; set; }
    }
}