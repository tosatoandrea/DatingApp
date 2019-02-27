using CloudinaryDotNet;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Helpers
{
    public class CloudinaryHelper
    {
        public static Cloudinary GetCloudinaryInstance(IOptions<CloudinarySettings> cloudinaryConfgig)
        {
            Account acc = new Account(
                cloudinaryConfgig.Value.CloudName,
                cloudinaryConfgig.Value.ApiKey,
                cloudinaryConfgig.Value.ApiSecret
            );

            return new Cloudinary(acc);
        }
    }
}