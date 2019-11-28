using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using MyFriendsApp.API.DTOs;
using MyFriendsApp.API.Models;

namespace MyFriendsApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDto>()
            .ForMember(dest => dest.PhotoUrl, opt => {
                opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
            })
            .ForMember(dest => dest.MyGroups, opt => {
                opt.MapFrom(src => src.UserGroups.Select(p => p.GroupId.ToString()).ToList());
            })
            .ForMember(dest => dest.Age, opt => {
                opt.ResolveUsing(k => k.DateOfBirth.CalculateAge());
            });

            CreateMap<User, UserForDetailedDto>()
            .ForMember(dest => dest.PhotoUrl, opt => {
                opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
            })
            .ForMember(dest => dest.Age, opt => {
                opt.ResolveUsing(k => k.DateOfBirth.CalculateAge());
            });
            
            CreateMap<Photo, PhotosForDetailedDto>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<Photo, PhotoForReturnDto>();
            CreateMap<PhotoForCreationDto, Photo>();
            CreateMap<UserForRegisterDto, User>();
            CreateMap<MessageForCreationDto, Message>().ReverseMap();
            CreateMap<Message, MessageToReturnDto>()
                .ForMember(m => m.SenderPhotoUrl, opt => opt.MapFrom(p =>
                         p.Sender.Photos.FirstOrDefault(k => k.IsMain).Url))
                .ForMember(m => m.RecipientPhotoUrl, opt => opt.MapFrom(p => 
                         p.Recipient.Photos.FirstOrDefault(k => k.IsMain).Url));
        }
       
        
    }
}