// ============================================================
// DbInitializer - Seeds the database with initial data
// Creates the default EO account and environmental permits.
// ============================================================

using Group5_iPERMITAPP.Models;

namespace Group5_iPERMITAPP.Data
{
    /// <summary>
    /// Seeds the Group5_iPERMITDB database with initial data:
    /// - One hard-coded EO account (password: "password")
    /// - Pre-defined environmental permit types
    /// </summary>
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Seed EO account (hard-coded as per requirements)
            if (!context.EOs.Any())
            {
                var eo = new EO
                {
                    ID = "EO001",
                    Name = "Environmental Officer",
                    Password = BCrypt.Net.BCrypt.HashPassword("password")
                };
                context.EOs.Add(eo);
            }

            // Seed Environmental Permits with fees
            if (!context.EnvironmentalPermits.Any())
            {
                var permits = new EnvironmentalPermit[]
                {
                    new EnvironmentalPermit
                    {
                        PermitID = "EP001",
                        PermitName = "Air Emissions Permit",
                        PermitFee = 500.00,
                        Description = "Permit for activities that release pollutants into the air, such as paint spray operations."
                    },
                    new EnvironmentalPermit
                    {
                        PermitID = "EP002",
                        PermitName = "Waste Management Permit",
                        PermitFee = 750.00,
                        Description = "Permit for storage, treatment, and disposal of industrial waste materials."
                    },
                    new EnvironmentalPermit
                    {
                        PermitID = "EP003",
                        PermitName = "Water Discharge Permit",
                        PermitFee = 600.00,
                        Description = "Permit for discharge of water effluents from industrial processes."
                    },
                    new EnvironmentalPermit
                    {
                        PermitID = "EP004",
                        PermitName = "Noise Control Permit",
                        PermitFee = 350.00,
                        Description = "Permit for activities generating noise above regulated thresholds."
                    },
                    new EnvironmentalPermit
                    {
                        PermitID = "EP005",
                        PermitName = "Heating System Permit",
                        PermitFee = 450.00,
                        Description = "Permit for installation and operation of industrial heating systems."
                    },
                    new EnvironmentalPermit
                    {
                        PermitID = "EP006",
                        PermitName = "Hazardous Materials Permit",
                        PermitFee = 900.00,
                        Description = "Permit for handling, storage, and transportation of hazardous materials."
                    }
                };

                context.EnvironmentalPermits.AddRange(permits);
            }

            context.SaveChanges();
        }
    }
}
