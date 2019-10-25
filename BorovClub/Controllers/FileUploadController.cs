using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BorovClub.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BorovClub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _enviroment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _context;
        public FileUploadController(IWebHostEnvironment environment, IHttpContextAccessor context, UserManager<ApplicationUser> userManager)
        {
            _enviroment = environment;
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("uploadAvatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            Console.WriteLine("Upload Working!!!!");
            if (file != null)
            {
                var user = await _userManager.GetUserAsync(_context.HttpContext.User);
                Console.WriteLine("USER:" + user.FirstName);
                string path = "/avatars/" + file.FileName;

                using (var fileStream = new FileStream(_enviroment.WebRootPath + path, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                user.AvatarPath = path;
                await _userManager.UpdateAsync(user);
                return Ok(path);
            }
            return BadRequest("Something went wrong");
        }
    }
}