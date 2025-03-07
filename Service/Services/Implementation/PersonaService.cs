using Dapper;
using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Service.Services.Contracts;

namespace Service.Services.Implementation
{
    public class PersonaService : BaseService<Persona, PersonaDTO>, IPersonaService
    {
        public PersonaService(DataBaseContext context, IMapper mapper) : base(context, mapper)
        {
        }

        // Obtener Persona por Id usando Entity Framework
        public async Task<PersonaDTO> GetPersonaByIdEf(int id)
        {
            try
            {
                var item = await _dbSet.Where(e => e.Id == id && !e.EsBorrado).FirstOrDefaultAsync();
                if (item != null)
                {
                    return _mapper.Map<PersonaDTO>(item);
                }
                return default(PersonaDTO);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el elemento con id {id}", ex);
            }
        }

        // Obtener Persona por Id usando Dapper
        public async Task<PersonaDTO> GetPersonaByIdDapper(int id)
        {
            try
            {
                using (var connection = _context.Database.GetDbConnection())
                {
                    var query = "SELECT * FROM Tbl_Personas WHERE Id = @Id AND EsBorrado = 0";
                    var item = await connection.QueryFirstOrDefaultAsync<Persona>(query, new { Id = id });
                    if (item != null)
                    {
                        return _mapper.Map<PersonaDTO>(item);
                    }
                    return default(PersonaDTO);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el elemento con id {id}", ex);
            }
        }
    }
}
