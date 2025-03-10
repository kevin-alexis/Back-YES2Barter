using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Contracts;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : BaseController<Categoria, CategoriaDTO, ICategoriaService>
    {
        public CategoriaController(ICategoriaService service, IMapper mapper) : base(service, mapper)
        {
            
        }

    }
}
