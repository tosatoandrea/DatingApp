using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DatingApp.API.Dtos;
using Microsoft.AspNetCore.Identity;
using DatingApp.API.Model;
using AutoMapper;
using System.Collections.Generic;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using DatingApp.API.Helpers;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _cntx;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private Cloudinary _cloudinary;
        private readonly IDatingRepository _repo;

        public AdminController(DataContext cntx, UserManager<User> userManager, IMapper mapper,
            IOptions<CloudinarySettings> cloudinaryConfgig, IDatingRepository repo)
        {
            _repo = repo;
            _mapper = mapper;
            _cntx = cntx;
            _userManager = userManager;
            _cloudinary = CloudinaryHelper.GetCloudinaryInstance(cloudinaryConfgig);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("usersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var userList = await (from u in _cntx.Users
                                  orderby u.UserName
                                  select new
                                  {
                                      Id = u.Id,
                                      UserName = u.UserName,
                                      Roles = u.UserRoles.Select(r => r.Role.Name)
                                  }).ToListAsync();

            return Ok(userList);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("editRoles/{userName}")]
        public async Task<IActionResult> EditRoles(string userName, RoleEditDto roleEditDto)
        {
            var user = await _userManager.FindByNameAsync(userName);

            var userRoles = await _userManager.GetRolesAsync(user);

            var selectedRoles = roleEditDto.RoleNames ?? new string[] { };

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!result.Succeeded)
                return BadRequest("Failed to add to roles");

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded)
                return BadRequest("Failed to remove to roles");

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePthotoRole")]
        [HttpGet("photosForModeration")]
        public async Task<IActionResult> GetPhotosForModeration()
        {
            // prendere tutte le foto non ancora approvate e il nome dell'utente<
            var photosToApprove = await _cntx.Photos.IgnoreQueryFilters().Where(x => x.IsApproved == false).Include(x => x.User).ToListAsync();

            var photosToApproveReturn = _mapper.Map<List<PhotoForReturnDto>>(photosToApprove);

            return Ok(photosToApproveReturn);
        }

        [Authorize(Policy = "ModeratePthotoRole")]
        [HttpPut("photosForModeration/{id}")]
        public async Task<IActionResult> ApprovePhoto(int id)
        {
            // prendere tutte le foto non ancora approvate e il nome dell'utente<
            var photoToApprove = await _cntx.Photos.IgnoreQueryFilters().Where(x => x.Id == id).FirstOrDefaultAsync();

            if (photoToApprove != null)
            {
                photoToApprove.IsApproved = true;
                if (_cntx.SaveChanges() > 0)
                    return NoContent();
                else
                    return BadRequest("Error on approve photo");
            }
            else
            {
                return BadRequest("Photo to approve not found");
            }
        }

        [Authorize(Policy = "ModeratePthotoRole")]
        [HttpDelete("photosForModeration/{id}")]
        public async Task<IActionResult> RejectPhoto(int id)
        {
            string message = await ControllerHelper.DeletePhoto(_cntx, _repo, _cloudinary, id);

            if (message == "")
                return Ok();
            else
                return BadRequest(message);
        }


    }
}