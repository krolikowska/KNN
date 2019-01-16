﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataAccess
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class BooksRecomendationsEntities : DbContext
    {
        public BooksRecomendationsEntities()
            : base("name=BooksRecomendationsEntities")
        {
            Configuration.AutoDetectChangesEnabled = false;
            

        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<BookRecomendation> BookRecomendations { get; set; }
        public virtual DbSet<Book> Books { get; set; }
        public virtual DbSet<BooksRating> BooksRatings { get; set; }
        public virtual DbSet<DistanceSimilarityType> DistanceSimilarityTypes { get; set; }
        public virtual DbSet<Parameter> Parameters { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserSimilar> UserSimilars { get; set; }
        public virtual DbSet<Test> Tests { get; set; }
    
        public virtual ObjectResult<SelectMutualBooks_Result> SelectMutualBooks(Nullable<int> userId1, Nullable<int> userId2)
        {
            var userId1Parameter = userId1.HasValue ?
                new ObjectParameter("UserId1", userId1) :
                new ObjectParameter("UserId1", typeof(int));
    
            var userId2Parameter = userId2.HasValue ?
                new ObjectParameter("UserId2", userId2) :
                new ObjectParameter("UserId2", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<SelectMutualBooks_Result>("SelectMutualBooks", userId1Parameter, userId2Parameter);
        }
    }
}
