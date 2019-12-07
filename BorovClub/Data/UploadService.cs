using BlazorInputFile;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using BorovClub.Models;

namespace BorovClub.Data
{
    public class UploadService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _context;
        private readonly IWebHostEnvironment _enviroment;


        public UploadService(ApplicationDbContext dbContext, IHttpContextAccessor context,
            IWebHostEnvironment enviroment)
        {
            _dbContext = dbContext;
            _context = context;
            _enviroment = enviroment;
        }

        public async Task<IdentityResult> UploadAvatar(IFileListEntry[] files)
        {
            var file = files.FirstOrDefault();

            if (file != null)
            {
                var id = _context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var user = _dbContext.Users.Find(id);
                string path = "/avatars/" + file.Name;
                using (var fileStream = new FileStream(_enviroment.WebRootPath + path, FileMode.Create))
                {
                    await file.Data.CopyToAsync(fileStream);
                }
                Console.WriteLine("NEW AVATAR ON" + path);
                user.AvatarPath = path;
                var saved = false;
                while (!saved)
                {
                    try
                    {
                        // Attempt to save changes to the database
                        _dbContext.SaveChanges();
                        saved = true;
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        foreach (var entry in ex.Entries)
                        {
                            if (entry.Entity is ApplicationUser)
                            {
                                var proposedValues = entry.CurrentValues;
                                var databaseValues = entry.GetDatabaseValues();

                                foreach (var property in proposedValues.Properties)
                                {
                                    var proposedValue = proposedValues[property];
                                    var databaseValue = databaseValues[property];

                                    // TODO: decide which value should be written to database
                                    // proposedValues[property] = <value to be saved>;
                                }

                                // Refresh original values to bypass next concurrency check
                                entry.OriginalValues.SetValues(databaseValues);
                            }
                            else
                            {
                                throw new NotSupportedException(
                                    "Don't know how to handle concurrency conflicts for "
                                    + entry.Metadata.Name);
                            }
                        }
                    }
                }
            }
            else
            {
                return IdentityResult.Failed();
            }

            return IdentityResult.Success;
        }
    }
}
