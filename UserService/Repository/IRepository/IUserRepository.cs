using UserService.Models;
using UserService.Models.Dtos;

namespace UserService.Repository.IRepository
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetAsync(string id);
        Task<bool> AddAsync(User user);
        Task<bool> UpdateAsync(string id, DataUserDto user);
        Task<bool> DeleteAsync(string id);
    }
}
