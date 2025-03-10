using AutoMapper;
using Azure;
using Dapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.Account;
using Domain.ViewModels.Response;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Service.Logging;
using Service.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Enumerations.Enums;

namespace Service.Services.Implementation
{
    public class CategoriaService : BaseService<Categoria, CategoriaDTO>, ICategoriaService
    {
        private new readonly DataBaseContext _context;
        public CategoriaService(DataBaseContext context, IMapper mapper, ILogService logService) : base(context, mapper, logService)
        {
            _context = context;   
        }

    }
}
