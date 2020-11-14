using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace WebAPI.Dominio
{
    public class Registro
    {
        public Registro(string descricao, User user, UserNivelEnum userNivel)
        {
            this.descricao = descricao;
            this.User = user;
            this.data = DateTime.Now;
            this.userNivel = userNivel;
        }

        public Registro()
        {
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string descricao { get; set; }
        public User User { get; set; }
        public DateTime data { get; set; }
        public UserNivelEnum userNivel { get; set; }

    }

    public enum UserNivelEnum 
    {
        USER,
        DIRETOR,
        MINISTRO
    }
}
