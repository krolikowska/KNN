//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System.ComponentModel.DataAnnotations.Schema;
namespace DataAccess
{
    using System;
    using System.Collections.Generic;
    
    
    public partial class BookRecomendation
    {
        [Column(TypeName = "varchar")]
        public string BookId { get; set; }
        public int UserId { get; set; }
        public Nullable<double> PredictedRate { get; set; }
        public int Id { get; set; }
    
        public virtual Book Book { get; set; }
        public virtual User User { get; set; }
    }
}
