namespace DataAccess
{
    public partial class Book
    {
        /// <summary>
        ///     Determines whether the specified object is equal to the current
        ///     object.
        /// </summary>
        /// <param name="obj">
        ///     The object to compare with the current object.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the specified object is equal to the
        ///     current object; otherwise, <see langword="false" /> .
        /// </returns>
        public override bool Equals(object obj)
        {
            var other = (Book) obj;
            return other.ISBN == ISBN;
        }

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///     A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}