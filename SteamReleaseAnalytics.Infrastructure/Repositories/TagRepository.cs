using Microsoft.EntityFrameworkCore;

using SteamReleaseAnalytics.Core.Models;
using SteamReleaseAnalytics.Infrastructure.Data;

namespace SteamReleaseAnalytics.Infrastructure.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly SteamDbContext _context;

        public TagRepository(SteamDbContext context)
        {
            _context = context;
        }

        public async Task<Tag> GetTagByNameAsync(string name)
        {
            return await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await _context.Tags.ToListAsync();
        }

        public async Task<Tag> GetOrCreateTagAsync(string name)
        {
            var tag = await GetTagByNameAsync(name);
            if (tag == null)
            {
                tag = new Tag { Name = name };
                await AddTagAsync(tag);
            }
            return tag;
        }

        public async Task AddTagAsync(Tag tag)
        {
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
        }
    }
}