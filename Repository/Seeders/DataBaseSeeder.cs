using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Repository.Seeders.SeedersServices.SeedersContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Seeders
{
    public class DataBaseSeeder
    {
        private readonly IEnumerable<ISeeder> _seeders;

        public DataBaseSeeder(IEnumerable<ISeeder> seeders)
        {
            _seeders = seeders;
        }

        public async Task SeedAsync()
        {
            foreach (var seeder in _seeders)
            {
                await seeder.SeedAsync();
            }
        }
    }
}
