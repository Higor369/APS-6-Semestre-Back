﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Identity.Dto
{
    public class UserDto
    {
        public string UserName { get; set; }


        [DataType(DataType.Password)]
        public string Password { get; set; }
        public WebAPI.Dominio.UserNivelEnum Member { get; set; }
    }
}
