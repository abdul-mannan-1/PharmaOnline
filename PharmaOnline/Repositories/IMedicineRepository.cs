using PharmaOnline.Models;

namespace PharmaOnline.Repositories;

public interface IMedicineRepository
{
    Task<IEnumerable<Medicine>> GetAllAsync();
    Task<Medicine?> GetByIdAsync(int id);
    Task<IEnumerable<Medicine>> SearchAsync(string query);
    Task<IEnumerable<Medicine>> GetByCategoryAsync(string category);
    Task<int> AddAsync(Medicine medicine);
    Task UpdateAsync(Medicine medicine);
    Task DeleteAsync(int id);
}
