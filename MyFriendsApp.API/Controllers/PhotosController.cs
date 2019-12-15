using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MyFriendsApp.API.Data;
using MyFriendsApp.API.DTOs;
using MyFriendsApp.API.Helpers;
using MyFriendsApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;

namespace MyFriendsApp.API.Controllers
{
    //[Authorize] Authorization implemented globally in startup
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private readonly IGroupRepository _groupRepo;
        private readonly UserManager<User> _userManager;

        private Cloudinary _cloudinary;
        public PhotosController(IDatingRepository repo, IGroupRepository groupRepo, IMapper mapper, UserManager<User> userManager,
            IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _userManager = userManager;
            _groupRepo = groupRepo;
            _cloudinaryConfig = cloudinaryConfig;
            _mapper = mapper;
            _repo = repo;

            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);
            return Ok(photo);
        }



        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId,
                        [FromForm]PhotoForCreationDto photoForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var userFromRepo = await _repo.GetUser(userId);

            var file = photoForCreationDto.File;
            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500)
                                                .Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }

            }
            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreationDto);

            if (!userFromRepo.Photos.Any(u => u.IsMain))
                photo.IsMain = true;


            userFromRepo.Photos.Add(photo);



            if (await _repo.SaveAll())
            {
                var photoToRetun = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToRetun);
            }

            return BadRequest("Could not add the photo.");
        }

        //testing purpose only
        // [HttpGet("getGroupFromUser/{id}/{groupName}")]
        // public async Task<User> GetGroupFromUser(int id, string groupName)
        // {
        //     var user =  await _repo.GetUserWithGroup(id, groupName);
        //     return user;

        // }

        // [HttpPost("addPhotoForGroup")]
        // public async Task<IActionResult> AddPhotoForGroup(int userId, string groupName,
        //                 [FromForm]PhotoForCreationDto photoForCreationDto)
        // {
        //     if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        //         return Unauthorized();
        //     var userFromRepo = await _repo.GetUserWithGroup(userId, "KelFamily");

        //     var file = photoForCreationDto.File;
        //     var uploadResult = new ImageUploadResult();

        //     if (file.Length > 0)
        //     {
        //         using (var stream = file.OpenReadStream())
        //         {
        //             var uploadParams = new ImageUploadParams()
        //             {
        //                 File = new FileDescription(file.Name, stream),
        //                 Transformation = new Transformation().Width(500).Height(500)
        //                                         .Crop("fill").Gravity("face")
        //             };

        //             uploadResult = _cloudinary.Upload(uploadParams);
        //         }

        //     }
        //     photoForCreationDto.Url = uploadResult.Uri.ToString();
        //     photoForCreationDto.PublicId = uploadResult.PublicId;

        //     var photo = _mapper.Map<Photo>(photoForCreationDto);

        //     int grpId = _groupRepo.GetGroupId("KelFamily");  // repeats twice and need to get in group name
        //     photo.GroupId = grpId;

        //     //if (!userFromRepo.Photos.Any(u => u.IsMain))
        //     photo.IsMain = true;

        //     //userFromRepo.Photos.Add(photo);
        //     var usergroup = userFromRepo.UserGroups.FirstOrDefault(k => k.GroupId == grpId);
        //     var group = usergroup.Group;
        //     group.ImageUrl = photo.Url;
        //     group.GroupPhotos.Add(photo);

        //     var succeed = await _userManager.UpdateAsync(userFromRepo);

        //     if (succeed.Succeeded)
        //     {
        //             var photoToRetun = _mapper.Map<PhotoForReturnDto>(photo);
        //             return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToRetun);
        //     }

        //     return BadRequest("Could not add the photo.");
        // }


        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var user = await _repo.GetUser(userId);

            if (!user.Photos.Any(kp => kp.Id == id))
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);

            if (photoFromRepo.IsMain)
                return BadRequest("This is already the main photo.");

            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);

            currentMainPhoto.IsMain = false;
            photoFromRepo.IsMain = true;

            if (await _repo.SaveAll())
                return NoContent();

            return BadRequest("Could not set photo to main.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var user = await _repo.GetUser(userId);

            if (!user.Photos.Any(kp => kp.Id == id))
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);

            if (photoFromRepo.IsMain)
                return BadRequest("You cannot delete your main photo.");

            if (photoFromRepo.PublicId != null)
            {
                var deletionParams = new DeletionParams(photoFromRepo.PublicId);
                var result = _cloudinary.Destroy(deletionParams);

                if (result.Result == "ok")
                {
                    _repo.Delete(photoFromRepo);
                }
            }

            if (photoFromRepo.PublicId == null)
            {
                _repo.Delete(photoFromRepo);
            }

            if (await _repo.SaveAll())
                return Ok();

            return BadRequest("Failed to delete the photo.");
        }
    }
}