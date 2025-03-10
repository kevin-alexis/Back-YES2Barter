using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Contracts
{
    public interface ICategoriaService : IBaseService<Categoria, CategoriaDTO>
    {
    }
}
