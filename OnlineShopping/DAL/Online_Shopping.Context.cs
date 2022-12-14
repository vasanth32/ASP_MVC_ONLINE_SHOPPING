//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OnlineShopping.DAL
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class Online_ShoppingEntities1 : DbContext
    {
        public Online_ShoppingEntities1()
            : base("name=Online_ShoppingEntities1")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Tbl_Cart> Tbl_Cart { get; set; }
        public virtual DbSet<Tbl_CartStatus> Tbl_CartStatus { get; set; }
        public virtual DbSet<Tbl_Category> Tbl_Category { get; set; }
        public virtual DbSet<Tbl_MemberRole> Tbl_MemberRole { get; set; }
        public virtual DbSet<Tbl_Members> Tbl_Members { get; set; }
        public virtual DbSet<Tbl_Product> Tbl_Product { get; set; }
        public virtual DbSet<Tbl_Roles> Tbl_Roles { get; set; }
        public virtual DbSet<Tbl_ShippingDetails> Tbl_ShippingDetails { get; set; }
    
        public virtual ObjectResult<USP_MemberShoppingCartDetails_Result> USP_MemberShoppingCartDetails(Nullable<int> memberId)
        {
            var memberIdParameter = memberId.HasValue ?
                new ObjectParameter("memberId", memberId) :
                new ObjectParameter("memberId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<USP_MemberShoppingCartDetails_Result>("USP_MemberShoppingCartDetails", memberIdParameter);
        }
    
        public virtual ObjectResult<USP_Search_Result> USP_Search(string searchKey)
        {
            var searchKeyParameter = searchKey != null ?
                new ObjectParameter("searchKey", searchKey) :
                new ObjectParameter("searchKey", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<USP_Search_Result>("USP_Search", searchKeyParameter);
        }
    }
}
