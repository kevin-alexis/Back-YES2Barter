using Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Contracts
{
    public interface ILogService
    {
        Task Add(LogDTO logDTO);
        Task<IEnumerable<LogDTO>> GetAll();
        Task<LogDTO> GetById(int id);
    }
}
