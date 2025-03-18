using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Contracts;
using Domain.ViewModels.EditPersona;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Service.Services.Implementation;
using Repository.Context;
using Domain.ViewModels.Response;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonaController : BaseController<Persona, PersonaDTO, IPersonaService>
    {
        private readonly ILogService _logService;
        private readonly IWebHostEnvironment _env;
        private readonly DataBaseContext _context;
        private readonly IMapper _mapper;


        // Constructor unificado que inyecta IWebHostEnvironment
        public PersonaController(IPersonaService service, IMapper mapper, ILogService logService, IWebHostEnvironment env, DataBaseContext context)
            : base(service, mapper, logService)
        {
            _logService = logService;
            _env = env;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet("GetAllPersonasIntercambiadores/")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<PersonaDTO>>> GetAllPersonasIntercambiadores()
        {
            try
            {
                var itemsDto = await _service.GetAllPersonasIntercambiadores();
                return Ok(itemsDto);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetPersonaByIdEf)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("GetPersonaByIdUsuario/{idUsuario}")]
        public async Task<ActionResult<PersonaDTO>> GetPersonaByIdUsuario(string idUsuario)
        {
            try
            {
                var result = await _service.GetPersonaByIdUsuario(idUsuario);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetPersonaByIdUsuario)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }

        [HttpGet("GetPersonaByIdEf/{id}")]
        public async Task<ActionResult<PersonaDTO>> GetPersonaByIdEf(int id)
        {
            try
            {
                var result = await _service.GetPersonaByIdEf(id);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetPersonaByIdEf)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }

        [HttpGet("GetPersonaByIdDapper/{id}")]
        public async Task<ActionResult<PersonaDTO>> GetPersonaByIdDapper(int id)
        {
            try
            {
                var result = await _service.GetPersonaByIdDapper(id);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(GetPersonaByIdDapper)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;
            }
        }

        // Endpoint para actualizar el perfil (incluyendo el cambio de foto)
        [HttpPut("UpdatePerfil/{id}")]
        public async Task<IActionResult> UpdatePerfil(int id, [FromForm] EditPersonaVM editPersona)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new EndpointResponse<Persona> { Message = "Error, la información es incorrecta", Success = false, Data = null });
                }

                var persona = await _context.Personas.FindAsync(id);
                if (persona == null)
                {
                    return Ok(new EndpointResponse<Persona> { Message = "Persona no encontrada.", Success = false, Data = null });

                }

                if (editPersona.RutaFotoPerfil != null)
                {
                    // Validar la extensión de la imagen
                    var extension = Path.GetExtension(editPersona.RutaFotoPerfil.FileName).ToLower();
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".svg", ".webp" };
                    if (!allowedExtensions.ToList().Contains(extension))
                    {
                        return Ok(new EndpointResponse<Persona> { Message = "El formato de la imagen no es válido.", Success = false, Data = null });
                    }

                    // Generar un nombre único para el archivo
                    string fecha = DateTime.Now.ToString("dd-MM-yyyy_HHmmss");
                    string nombreArchivo = $"FotoPerfil_{persona.Id}_{fecha}{extension}";

                    var uploadsFolder = Path.Combine(_env.ContentRootPath, "Uploads", "FotoPerfil", persona.Id.ToString());
                    if (Directory.Exists(uploadsFolder))
                    {
                        var archivos = Directory.GetFiles(uploadsFolder);
                        foreach (var archivo in archivos)
                        {
                            if (!archivo.EndsWith("FotoPerfilDefecto.png"))
                            {
                                System.IO.File.Delete(archivo);
                            }
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, nombreArchivo);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await editPersona.RutaFotoPerfil.CopyToAsync(stream);
                    }
                    // Guardar la ruta en la base de datos
                    string rutaProcesada = Path.Combine("Uploads", "FotoPerfil", persona.Id.ToString(), nombreArchivo);
                    
                    persona.RutaFotoPerfil = rutaProcesada;
                }

                persona.Nombre = editPersona.Nombre;

                if (editPersona.Biografia != null)
                {
                    persona.Biografia = editPersona.Biografia;
                }


                _context.Personas.Update(persona);
                await _context.SaveChangesAsync();
                return Ok(new EndpointResponse<Persona> { Message = "Perfil editado correctamente", Success = true, Data = persona });

            }
            catch (Exception ex)
            {
                await _logService.AddAsync(new LogDTO
                {
                    Nivel = "Error",
                    Mensaje = $"Error en el método {nameof(UpdatePerfil)}: {ex.Message}",
                    Excepcion = ex.ToString()
                });
                throw;

            }
        }
    }
}
