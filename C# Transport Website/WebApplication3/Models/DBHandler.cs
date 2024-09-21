using System.Collections.Generic;
using System.Linq;

namespace WebApplication3.Models
{
    public static class DbHandler
    {
        // Method to get all Users
        public static List<User> GetAllUsers()
        {
            List<User> users;
            using (var db = new RoutedSystemContext())
            {
                users = db.Users.ToList();
            }
            return users;
        }

        // Method to get all Drivers
        public static List<Driver> GetAllDrivers()
        {
            List<Driver> drivers;
            using (var db = new RoutedSystemContext())
            {
                drivers = db.Drivers.ToList();
            }
            return drivers;
        }

        // Method to get all VehicleInfos
        public static List<VehicleInfo> GetAllVehicleInfos()
        {
            List<VehicleInfo> vehicleInfos;
            using (var db = new RoutedSystemContext())
            {
                vehicleInfos = db.VehicleInfos.ToList();
            }
            return vehicleInfos;
        }

        // Additional CRUD operations can be added here as needed
    }
}
