using AutoMapper;
using Dapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Service.Logging;
using Service.Services.Contracts;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Enumerations.Enums;

namespace Service.Services.Implementation
{
    public class ObjetoService : BaseService<Objeto, ObjetoDTO>, IObjetoService
    {
        private new readonly DataBaseContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        private readonly IPropuestaIntercambioService _habitacionService;
        private readonly Logger _logger;

        public ObjetoService(DataBaseContext context, IPropuestaIntercambioService habitacionService, IMapper mapper, Logger logger, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager) : base(context, mapper, logger)
        {
            _context = context;
            _userManager = userManager;
            _habitacionService = habitacionService;
            _logger = logger;

        }

        public async Task<EndpointResponse<List<ObjetoDTO>>> GetByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new EndpointResponse<List<ObjetoDTO>>
                {
                    Message = "El nombre es requerido",
                    Success = false,
                    Data = new List<ObjetoDTO>()
                };
            }

            var result = await _context.Objetos
                .Where(x => x.Nombre.StartsWith(name) && x.EsBorrado == false)
                .ToListAsync();

            if (!result.Any())
            {
                return new EndpointResponse<List<ObjetoDTO>>
                {
                    Message = "No se encontraron objetos con ese nombre",
                    Success = false,
                    Data = new List<ObjetoDTO>()
                };
            }

            var objetoDTOs = result.Select(objeto => new ObjetoDTO
            {
                Id = objeto.Id,
                Nombre = objeto.Nombre,
                Descripcion = objeto.Descripcion,
                EsBorrado = objeto.EsBorrado,
                FechaPublicacion = objeto.FechaPublicacion,
                RutaImagen = objeto.RutaImagen
            }).ToList();

            return new EndpointResponse<List<ObjetoDTO>>
            {
                Message = "Objetos obtenidos con éxito",
                Success = true,
                Data = objetoDTOs
            };
        }

        public async Task<EndpointResponse<List<ObjetoDTO>>> GetAllByIdCategoria(int idCategoria)
        {
            if (idCategoria <= 0)
            {
                return new EndpointResponse<List<ObjetoDTO>>
                {
                    Message = "El id es requerido",
                    Success = false,
                    Data = new List<ObjetoDTO>()
                };
            }

            var result = await _context.Objetos
                .Where(x => x.IdCategoria == idCategoria && x.EsBorrado == false)
                .ToListAsync();

            if (!result.Any())
            {
                return new EndpointResponse<List<ObjetoDTO>>
                {
                    Message = "No se encontraron objetos con esa categoría",
                    Success = false,
                    Data = new List<ObjetoDTO>()
                };
            }

            var objetoDTOs = result.Select(objeto => new ObjetoDTO
            {
                Id = objeto.Id,
                Nombre = objeto.Nombre,
                Descripcion = objeto.Descripcion,
                FechaPublicacion = objeto.FechaPublicacion,
                RutaImagen = objeto.RutaImagen,
                EsBorrado = objeto.EsBorrado
            }).ToList();

            return new EndpointResponse<List<ObjetoDTO>>
            {
                Message = "Objetos obtenidos con éxito",
                Success = true,
                Data = objetoDTOs
            };
        }
        public async Task<string> GuardarObjetoImagen(int idCategoria, IFormFile objetoImagen, string ruta)
        {
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            string extension = Path.GetExtension(objetoImagen.FileName).ToLower();
            if (!validExtensions.Contains(extension))
            {
                throw new InvalidOperationException("El archivo no es una imagen válida. Solo se permiten JPG, JPEG, PNG y GIF.");
            }

            string fecha = DateTime.Now.ToString("dd-MM-yyyy_HHmmss");
            string nombreArchivo = $"Objeto_{idCategoria}_{fecha}{extension}";

            string rutaCarpeta = Path.Combine(ruta, "Uploads", "Objetos", idCategoria.ToString());

            if (!Directory.Exists(rutaCarpeta))
            {
                Directory.CreateDirectory(rutaCarpeta);
            }

            string rutaArchivo = Path.Combine(rutaCarpeta, nombreArchivo);
            using (var fileStream = new FileStream(rutaArchivo, FileMode.Create))
            {
                await objetoImagen.CopyToAsync(fileStream);
            }

            string rutaRelativa = Path.Combine("Uploads", "Objetos", idCategoria.ToString(), nombreArchivo);
            return rutaRelativa;
        }

        public async Task<bool> EliminarObjetoImagen(int idObjeto, string ruta)
        {
            var objeto = await _dbSet.Where(c => c.Id == idObjeto).FirstOrDefaultAsync();

            string rutaArchivo = Path.Combine(ruta, objeto.RutaImagen);

            if (File.Exists(rutaArchivo))
            {
                File.Delete(rutaArchivo);
                return true;
            }
            return false;
        }

        override public async Task Update(ObjetoDTO itemDto)
        {
            try
            {
                var item = _mapper.Map<Objeto>(itemDto);

                var trackedEntity = _context.Set<Objeto>().Local.FirstOrDefault(e => e.Id == item.Id);
                if (trackedEntity != null)
                {
                    _context.Entry(trackedEntity).State = EntityState.Detached;
                }

                _dbSet.Update(item);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogAsync("Error", "Error al actualizar el elemento", ex.ToString());
                throw new Exception("Error al actualizar el elemento", ex);
            }
        }

        override public async Task Delete(int id)
        {
            try
            {
                var item = await _dbSet.FindAsync(id);
                if (item != null)
                {
                    var esBorradoProperty = item.GetType().GetProperty("EsBorrado");
                    if (esBorradoProperty != null)
                    {
                        esBorradoProperty.SetValue(item, true);
                        _dbSet.Update(item);
                        await _context.SaveChangesAsync();
                    }

                }
            }
            catch (Exception ex)
            {
                await _logger.LogAsync("Error", $"Error al eliminar el elemento con id {id}", ex.ToString());
                throw new Exception($"Error al eliminar el elemento con id {id}", ex);
            }
        }

    }
}
