using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _repo.GetUsers();

            var usersToReturnDto = _mapper.Map<IEnumerable<UserForListDto>>(users);

            return Ok(usersToReturnDto);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repo.GetUser(id);

            var userToReturnDto = _mapper.Map<UserForDetailsDto>(user);

            return Ok(userToReturnDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            // l'id viene dal parametro nell'indizizzo
            // in ogni caso, anche se venisse dal body occorre eseguire il seguente test che attesta
            // che l'id utilizzato corrisponde a quello del token di autorizzazione perchè
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromDb = await _repo.GetUser(id);

            _mapper.Map(userForUpdateDto, userFromDb);

            if (await _repo.SaveAll())
                return NoContent(); 

            throw new Exception($"Updating user {id} failed on save");
        }


    }
}