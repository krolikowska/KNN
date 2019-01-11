namespace RecommendationAPI.Models
{
    public class BookModel
    {
        public string ISBN { get; set; }
        public string BookTitle { get; set; }
        public string BookAuthor { get; set; }
        public int? YearOfPublication { get; set; }
        public string Publisher { get; set; }
        public string ImageURLS { get; set; }
        public string ImageURLM { get; set; }
        public string ImageURLL { get; set; }
    }
}