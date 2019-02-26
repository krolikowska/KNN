namespace DataAccess
{
    
    public class BookScore
    {
        public string BookId { get; set; }
       
        public short Rate { get; set; }

        public double PredictedRate { get; set; }

        public int UserId { get; set; }
    }
}