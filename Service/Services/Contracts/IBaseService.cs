using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Services.Contracts
{
    public interface IBaseService<T, TDto>
    {
        Task<IEnumerable<TDto>> GetAll();
        Task<TDto> GetById(int id);
        Task Add(TDto itemDto);
        Task Update(TDto itemDto);
        Task Delete(int id);
    }
}
