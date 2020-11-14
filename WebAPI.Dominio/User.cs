using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Dominio
{
    public class User : IdentityUser<int>
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string OrgId { get; set; }
        public UserNivelEnum Member { get; set; }
        public List<UserRole> UserRoles { get; set; }

        public List<Registro> Registros { get; set; }
    }
}
