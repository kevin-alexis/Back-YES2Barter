using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Repository.Context;
using Service.Services.Contracts;
using Service.Services.Implementation;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadisticaController : ControllerBase
    {
        private readonly IEstadisticaService _service;
        public EstadisticaController(IEstadisticaService service)
        {
            _service = service;
        }

    }
}
