using System;
using System.Linq;
using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.API.Model;
using DatingApp.API.Helpers;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {   
            CreateMap<User, UserForListDto>()
                .ForMember(dest =>  dest.PhotoUrl, opt => {
                    opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
                })
                .ForMember(dest => dest.Age, opt => {
                    opt.MapFrom((scr, dest) => scr.DateOfBirth.GetAge());
                });
            CreateMap<User, UserForDetailsDto>()
                .ForMember(dest =>  dest.PhotoUrl, opt => {
                    opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
                })
                .ForMember(dest => dest.Age, opt => {
                    opt.MapFrom((scr, dest) => scr.DateOfBirth.GetAge());
                });
            CreateMap<Photo, PhotoForDetailsDto>();
            
        }

    }
}