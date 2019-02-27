using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    // [Authorize] da configurazione in startup c'è la sezione che di default imposta Authorize su tutti i controllers
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfgig;
        private Cloudinary _cloudinary;
        private readonly DataContext _cntx;

        public PhotoController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfgig, DataContext cntx)
        {
            _cntx = cntx;
            _cloudinaryConfgig = cloudinaryConfgig;
            _mapper = mapper;
            _repo = repo;

            _cloudinary = CloudinaryHelper.GetCloudinaryInstance(cloudinaryConfgig);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);

            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

            return Ok(photo);
        }


        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotoForCreationDto photoForCreationDto)
        {
            // l'id viene dal parametro nell'indizizzo
            // in ogni caso, anche se venisse dal body occorre eseguire il seguente test che attesta
            // che l'id utilizzato corrisponde a quello del token di autorizzazione
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromDb = await _repo.GetUser(userId);

            var file = photoForCreationDto.File;

            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                };
            }

            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreationDto);

            if (userFromDb.Photos.Any(u => u.IsMain) == false)
                photo.IsMain = true;

            photo.UserId = userId;

            userFromDb.Photos.Add(photo);

            if (await _repo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
            }

            return BadRequest("Couldn't add the photo");
        }

        [HttpPost("{id}/setmain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromDb = await _repo.GetUser(userId);
            if (userFromDb == null)
                return BadRequest("Couldn't update the photo");

            Photo photo = userFromDb.Photos.Where(x => x.Id == id).FirstOrDefault();

            if (photo == null)
                return BadRequest("Couldn't update the photo");

            if (photo.IsMain)
                return BadRequest("This is already the main photo");

            // prendo la foto principale corrente e metto il flag IsMain a false
            Photo currentMainPhoto = userFromDb.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMainPhoto != null)
                currentMainPhoto.IsMain = false;

            // aggiorno la foto come principale
            photo.IsMain = true;

            if (await _repo.SaveAll())
            {
                return NoContent();
            }

            return BadRequest("Couldn't set the main the photo");
        }

        [HttpDelete("{photoId}")]
        public async Task<IActionResult> DeletePhoto(int userId, int photoId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromDb = await _repo.GetUser(userId, true);
            if (userFromDb == null)
                return BadRequest("Couldn't update the photo");

            Photo photo = userFromDb.Photos.Where(x => x.Id == photoId).FirstOrDefault();

            if (photo == null)
                return BadRequest("Couldn't update the photo");

            if (photo.IsMain)
                return BadRequest("This is the main photo, it can't be deleted");

            string message = await ControllerHelper.DeletePhoto(_cntx, _repo, _cloudinary, photoId);

            if (message == "")
                return Ok();
            else
                return BadRequest(message);

            // if (photo.PublicId != null)
            // {
            //     var deleteParam = new DeletionParams(photo.PublicId);
            //     var result = _cloudinary.Destroy(deleteParam);

            //     if (result.Result == "ok" || result.Result == "not found")
            //         _repo.Delete(photo);
            // }
            // else
            // {
            //     _repo.Delete(photo);
            // }

            // if (await _repo.SaveAll())
            //     return Ok();

            // return BadRequest("Couldn't delete the photo");
        }

    }
}