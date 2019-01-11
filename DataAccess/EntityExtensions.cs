using System.Data.Entity;

namespace DataAccess
{
    public static class EntityExtensions
    {
        public static void Clear<T>(this DbSet<T> dbSet) where T : class
        {
            dbSet.RemoveRange(dbSet);
        }
    }
}