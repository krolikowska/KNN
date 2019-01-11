namespace DataAccess
{
    public partial class User
    {
        public override bool Equals(object obj)
        {
            User other = (User)obj;
            return other.UserId == this.UserId;
        }

        public override int GetHashCode()
        {
            return UserId.GetHashCode();
        }
    }
}