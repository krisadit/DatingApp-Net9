using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository(DataContext context) : IPhotoRepository
    {
        public async Task<Photo?> GetPhotoById(int id)
        {
            return await context.Photos
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<PhotoApprovalDTO>> GetUnapprovedPhotos()
        {
            return await context.Photos
                .IgnoreQueryFilters()
                .Where(photo => !photo.IsApproved)
                .Select(p => new PhotoApprovalDTO()
                {
                    Id = p.Id,
                    IsApproved = p.IsApproved,
                    Url = p.Url,
                    Username = p.AppUser.UserName
                }).ToListAsync();
        }

        public void RemovePhoto(Photo photo)
        {
            context.Photos.Remove(photo);
        }
    }
}
