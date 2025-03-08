using AutoMapper;
using Domain.DTOs;
using Domain.Entities;
using Repository.Context;
using Service.Logging;
using Service.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class MensajeService : BaseService<Mensaje, MensajeDTO>, IMensajeService
    {
        private new readonly DataBaseContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        private readonly IPropuestaIntercambioService _habitacionService;
        private readonly Logger _logger;

        public MensajeService(DataBaseContext context, IMapper mapper, Logger logger, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager) : base(context, mapper, logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;

        }
    }
}
