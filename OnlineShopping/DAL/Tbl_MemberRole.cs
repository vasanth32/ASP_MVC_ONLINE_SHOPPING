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
    using System.Collections.Generic;
    
    public partial class Tbl_MemberRole
    {
        public int MemberRoleId { get; set; }
        public Nullable<int> MemberId { get; set; }
        public Nullable<int> RoleId { get; set; }
    
        public virtual Tbl_Roles Tbl_Roles { get; set; }
    }
}
