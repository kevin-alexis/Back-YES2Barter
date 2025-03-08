using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Contracts;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MensajeController : BaseController<Mensaje, MensajeDTO, IMensajeService>
    {
        private readonly IMensajeService _service;
        public MensajeController(IMensajeService service, IMapper mapper) : base(service, mapper)
        {
            _service = service;
        }
    }

}
