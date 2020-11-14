using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Dominio;

namespace WebAPI.Identity.Dto
{
    public class RegistroCreateDTO
    {
        public string userName { get; set; }
        public string descricao { get; set; }
        public UserNivelEnum userNivel { get; set; }
    }

    public class RegistroListDTO
    {
        public string userName { get; set; }
    }

}
