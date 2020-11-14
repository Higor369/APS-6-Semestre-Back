using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Dominio;
using WebAPI.Identity.Dto;
using WebAPI.Identity.Helper;
using WebAPI.Repository;

namespace WebAPI.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IMapper _mapper;
        private readonly Context _context;

        public UserController(IConfiguration config, UserManager<User> userManager,
                              SignInManager<User> signInManager, IMapper mapper, RoleManager<Role> roleManager, UserService userService, Context context)
        {
            _config = config;
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _roleManager = roleManager;
            _userService = userService;
            _context = context;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {

            var userName = Request.Form["userName"][0];
            var password = Request.Form["password"][0];
            var file = Request.Form.Files[0];

            if (file == null || password == null || userName == null)
            {
                throw new ArgumentException("Faltam dados para prosseguir com a requisição");
            }

            try
            {
                var user = await _userManager.FindByNameAsync(userName);

                
                if(user == null)
                {
                    throw new Exception("Usuário não localizado");
                }

                if(!_userService.CompareImages(Request.Form.Files.FirstOrDefault(), user)) {
                    throw new Exception("Biografia não autorizada");
                }
                

                var result = await _signInManager
                    .CheckPasswordSignInAsync(user, password, false);

                if(result.Succeeded)
                {
                    var appUser = await _userManager.Users
                            .FirstOrDefaultAsync(u => u.NormalizedUserName == user.UserName.ToUpper());

                    var userToReturn = _mapper.Map<UserDto>(appUser);

                    return Ok(new
                    {
                        token = GenerateJWToken(appUser).Result,
                        user = userToReturn
                    });
                }

                return Unauthorized();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"ERROR {ex.Message}");
            }
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register()
        {

            var userName = Request.Form["userName"][0];
            var password = Request.Form["password"][0];
            var member = Request.Form["member"][0].ToUpper();

            var file = Request.Form.Files[0];

            if(file==null || password ==null || userName==null|| member==null)
            {
                throw new ArgumentException("Faltam dados para prosseguir com a requisição");
            }

            try
            {
                var user = await _userManager.FindByNameAsync(userName);

                if (user == null)
                {
                    user = new User
                    {
                        UserName = userName,
                        Member = (UserNivelEnum)Enum.Parse(typeof(UserNivelEnum), member)

                };

                    var result = await _userManager.CreateAsync(user, password);

                    await _roleManager.CreateAsync(new Role { Name = member });

                    var roleResult = await _userManager.AddToRoleAsync(user, member);

                    if (result.Succeeded && roleResult.Succeeded)
                    {
                        var appUser = await _userManager.Users
                            .FirstOrDefaultAsync(u => u.NormalizedUserName == user.UserName.ToUpper());

                        var token = GenerateJWToken(appUser).Result;

                       _userService.SaveImage(file, user); //salva imagem com ID do usuario
                        _context.SaveChanges();
                        //var confirmationEmail = Url.Action("ConfirmEmailAddress", "Home",
                        //    new { token = token, email = user.Email }, Request.Scheme);

                        //System.IO.File.WriteAllText("confirmationEmail.txt", confirmationEmail);
                        return Ok("Registrado com sucesso");
                    }
                }

                throw new Exception("Usuário já cadastrado");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"ERROR {ex.Message}");
            }
        }
        private async Task<string> GenerateJWToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                _config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescription);

            return tokenHandler.WriteToken(token);
        }


        [HttpGet("test")]
        [AllowAnonymous]
        public async Task<IActionResult> Test()
        {
            var userName = Request.Form["userName"][0];
            var file = Request.Form.Files.FirstOrDefault();

            var user = await _userManager.FindByNameAsync(userName);

            _userService.CompareImages(file, user);

            return Ok();

        }
        [HttpGet("teste2")]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            var userName = Request.Form["userName"][0];
            var file = Request.Form.Files.FirstOrDefault();

            var user = await _userManager.FindByNameAsync(userName);

            _userService.SaveImage(file, user);

            return Ok();

        }


    }
}
