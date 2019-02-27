using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers
{
    public class ControllerHelper
    {
        public static async Task<string> DeletePhoto(DataContext cntx, IDatingRepository repo, Cloudinary cloudinary, int photoId)
        {
            // prendere tutte le foto non ancora approvate e il nome dell'utente<
            var photo = await cntx.Photos.IgnoreQueryFilters().Where(x => x.Id == photoId).FirstOrDefaultAsync();

            if (photo == null)
                return "Couldn't reject the photo";

            if (photo.PublicId != null)
            {
                var deleteParam = new DeletionParams(photo.PublicId);
                var result = cloudinary.Destroy(deleteParam);

                if (result.Result == "ok" || result.Result == "not found")
                    repo.Delete(photo);
            }
            else
            {
                repo.Delete(photo);
            }

            if (await repo.SaveAll())
                return "";

            return "Couldn't delete the photo";
        }
    }
}