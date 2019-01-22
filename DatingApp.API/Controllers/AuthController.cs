using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    // [ApiController] consente di evitare di mettere l'attributo [FromBody], inoltre esegue la validazione in automatico 
    // del modelState e ritorna un badrequest con gli errori del modelState in automatico
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        IConfiguration _config;
        private readonly IMapper _mapper;
        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            this._mapper = mapper;
            this._repo = repo;
            this._config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(/*[FromBody]*/ UserForRegisterDto userForRegisterDto)
        {
            // se non si utilizza l'attributo [ApiController] occorre verificare se il modelState è valido
            // e gestire la risposta nel caso non lo sia
            // usando invece l'attributo [ApiController] questo viene gestito in automatico da AspNet Core
            // if (!ModelState.IsValid)
            //     return BadRequest(ModelState);

            // validate request
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();
            if (await _repo.UserExists(userForRegisterDto.Username))
                return BadRequest("Username already exists");

            var userToCreate = new User
            {
                Username = userForRegisterDto.Username
            };

            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            return StatusCode(201);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {

            //throw new Exception("Il server dice no!");

            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (userFromRepo == null)
                return Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var user = _mapper.Map<UserForListDto>(userFromRepo);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                user
            });

        }



    }
}