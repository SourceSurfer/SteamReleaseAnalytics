using SteamReleaseAnalytics.Core.Models;

namespace SteamReleaseAnalytics.Infrastructure.Repositories
{
    public interface ITagRepository
    {
        Task<Tag?> GetTagByNameAsync(string name);
        Task<List<Tag>> GetAllTagsAsync();
        Task<Tag> GetOrCreateTagAsync(string name);
        Task AddTagAsync(Tag tag);
    }
}