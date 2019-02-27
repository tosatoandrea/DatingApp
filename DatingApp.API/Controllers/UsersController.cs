using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))] // ad ogni chiamata di una delle action del controller viene eseguita l'action filter LogUserActivity
    [Route("api/[controller]")]
    // [Authorize] da configurazione in startup c'è la sezione che di default imposta Authorize su tutti i controllers
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
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromDb = await _repo.GetUser(currentUserId);

            userParams.UserId = currentUserId;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromDb.Gender == "female" ? "male" : "female";
            }

            var users = await _repo.GetUsers(userParams);

            var usersToReturnDto = _mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(usersToReturnDto);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repo.GetUser(id);

            var userToReturnDto = _mapper.Map<UserForDetailsDto>(user);

            return Ok(userToReturnDto);
        }

        [HttpGet("{id}/alsoToApprovePhotos", Name = "GetUserAllPhotos")]
        public async Task<IActionResult> GetUserAllPhotos(int id)
        {
            var user = await _repo.GetUser(id, true);

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

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            // l'id viene dal parametro nell'indizizzo
            // in ogni caso, anche se venisse dal body occorre eseguire il seguente test che attesta
            // che l'id utilizzato corrisponde a quello del token di autorizzazione perchè
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            // verifica che il like esiste già
            var like = await _repo.GetLike(id, recipientId);

            if (like != null)
                return BadRequest("You already like this user");

            // verifica che esista il destinatario del like
            if (await _repo.GetUser(recipientId) == null)
                return NotFound();

            // crea l'entity like, la aggiunge al db e salva
            like = new Like 
            {
                LikerId = id,
                LikeeId = recipientId
            };
            _repo.Add<Like>(like);
            
            if (await _repo.SaveAll())
                return Ok();
            
            return BadRequest("Fail to like user");
        }


    }
}