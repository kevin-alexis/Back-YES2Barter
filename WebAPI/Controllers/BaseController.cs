using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController<T, TDto, TService> : ControllerBase
        where T : class
        where TDto : class
        where TService : IBaseService<T, TDto>
    {
        protected readonly TService _service;
        protected readonly IMapper _mapper;

        public BaseController(TService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public virtual async Task<ActionResult<IEnumerable<TDto>>> GetAll()
        {
            try
            {
                var itemsDto = await _service.GetAll();
                return Ok(itemsDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TDto>> GetById(int id)
        {
            try
            {
                var itemDto = await _service.GetById(id);
                if (itemDto == null)
                {
                    return NotFound();
                }
                return Ok(itemDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost]
        public virtual async Task<ActionResult> Add([FromBody] TDto itemDto)
        {
            try
            {
                await _service.Add(itemDto);
                return CreatedAtAction(nameof(GetById), new { id = itemDto.GetHashCode() }, itemDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public virtual async Task<ActionResult> Update(int id, [FromBody] TDto itemDto)
        {
            try
            {
                var itemIdProperty = itemDto.GetType().GetProperty("Id");
                if (itemIdProperty == null)
                {
                    return BadRequest("El DTO no tiene una propiedad 'Id'.");
                }

                itemIdProperty.SetValue(itemDto, id);

                await _service.Update(itemDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public virtual async Task<ActionResult> Delete(int id)
        {
            try
            {
                var itemDto = await _service.GetById(id);
                if (itemDto == null)
                {
                    return NotFound();
                }

                await _service.Delete(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
