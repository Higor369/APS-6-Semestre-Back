using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Dominio;
using WebAPI.Identity.Dto;
using WebAPI.Repository;

namespace WebAPI.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrosController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly Context _context;
        private readonly RoleManager<Role> _roleManager;

        public RegistrosController(UserManager<User> userManager, Context context, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRegistro(RegistroCreateDTO dto)
        {

            var user = await _userManager.FindByNameAsync(dto.userName);

            if (user == null) throw new HttpRequestException("usuario invalido");

            var registro = new Registro(dto.descricao, user, dto.userNivel);

            _context.RecordContext.Add(registro);
            _context.SaveChanges();


            return Ok(registro);
        }


        [HttpGet("{userName}")]
        public async Task<IActionResult> ListRegistros(string userName)
        {

            var user = await _userManager.FindByNameAsync(userName);

            if (user == null) throw new HttpRequestException("usuario invalido");

            string value;
            try
            {
                value = user.UserRoles.FirstOrDefault().Role.Name;
            }catch(Exception e)
            {
                value = user.Member.ToString();
            }

            var result = new List<Registro>();

            switch(value)
            {
                case "USER":
                    result = _context.RecordContext.Where(x => x.userNivel <= UserNivelEnum.USER).ToList();
                    break;
                case "DIRETOR":
                    result = _context.RecordContext.Where(x => x.userNivel <= UserNivelEnum.DIRETOR).ToList();
                    break;
                case "MINISTRO":
                    result = _context.RecordContext.ToList();
                    break;
                default:
                    throw new Exception("não localizado");
                    break;
            }
            
            return Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> InitializeDatabase()
        {
            _context.Database.EnsureCreated();

            if (!_context.Roles.Any())
            {
                await _roleManager.CreateAsync(new Role { Name = "MINISTRO" });
                await _roleManager.CreateAsync(new Role { Name = "DIRETOR" });
                await _roleManager.CreateAsync(new Role { Name = "USER" });
            }
            if (!_context.Users.Any())
            {
                var users = new List<User> {
                     new User
                    {
                        UserName = "MINISTRO"
                    },
                     new User
                    {
                        UserName = "DIRETOR"
                    },
                    new User
                    {
                        UserName = "USER"
                    },
                };

                foreach (User u in users)
                {
                    await _userManager.CreateAsync(u, password: "12345678");
                    await _userManager.AddToRoleAsync(u, u.UserName);
                }
            }
            if (!_context.RecordContext.Any())
            {
                var diretor = await _userManager.FindByNameAsync("DIRETOR");
                var ministro = await _userManager.FindByNameAsync("MINISTRO");
                var user = await _userManager.FindByNameAsync("USER");

                var listR = new List<Registro>
                {
                    new Registro("registro ambiental de numero 1",user,UserNivelEnum.USER),
                    new Registro("registro ambiental de numero 2",diretor,UserNivelEnum.USER),
                    new Registro("registro ambiental de numero 3",ministro,UserNivelEnum.USER),
                    new Registro("registro ambiental de numero 4",user,UserNivelEnum.DIRETOR),
                    new Registro("registro ambiental de numero 5",diretor,UserNivelEnum.DIRETOR),
                    new Registro("registro ambiental de numero 6",ministro,UserNivelEnum.DIRETOR),
                    new Registro("registro ambiental de numero 7",user,UserNivelEnum.MINISTRO),
                    new Registro("registro ambiental de numero 8",diretor,UserNivelEnum.MINISTRO),
                    new Registro("registro ambiental de numero 9",ministro,UserNivelEnum.MINISTRO),

                };

                foreach (var r in listR)
                {
                    _context.RecordContext.Add(r);
                }
            }
            _context.SaveChanges();

            return Ok();
        }
    }
}
