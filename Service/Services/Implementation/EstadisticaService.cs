using AutoMapper;
using Dapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.ViewModels.GetEstadisticas;
using Domain.ViewModels.Response;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
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
    public class EstadisticaService : IEstadisticaService
    {
        private readonly DataBaseContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        public EstadisticaService(DataBaseContext context, IMapper mapper, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager) 
        {
            _context = context;
            _userManager = userManager;
        }

    }
}
