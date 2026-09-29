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

        public async Task<Tag?> GetTagByNameAsync(string name)
        {
            return await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await _context.Tags.ToListAsync();
        }

        /// <summary>
        /// Находит тег или добавляет новый в контекст без сохранения:
        /// он запишется тем же SaveChanges, что и игра, которая на него ссылается.
        /// </summary>
        public async Task<Tag> GetOrCreateTagAsync(string name)
        {
            var tag = _context.Tags.Local.FirstOrDefault(t => t.Name == name)
                ?? await GetTagByNameAsync(name);
            if (tag == null)
            {
                tag = new Tag { Name = name };
                _context.Tags.Add(tag);
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