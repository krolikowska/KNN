namespace DataAccess
{
    public partial class User
    {
        public override bool Equals(object obj)
        {
            var other = (User) obj;
            return other.UserId == UserId;
        }

        public override int GetHashCode()
        {
            return UserId.GetHashCode();
        }
    }
}