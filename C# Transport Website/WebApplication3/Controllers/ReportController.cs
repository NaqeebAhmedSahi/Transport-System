using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class ReportController : Controller
    {
        private readonly RoutedSystemContext _context;

        public ReportController(RoutedSystemContext context)
        {
            _context = context;
        }

        public IActionResult GenerateReport()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index", "Home");
            }
            var vehicleInfos = _context.VehicleInfos.ToList();
            return View(vehicleInfos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerateReport(DateTime startDate, DateTime endDate, bool inChecked, bool outChecked, string action)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index", "Home");
            }
            var reportData = FetchReportData(startDate, endDate, inChecked, outChecked);

            if (action == "Download")
            {
                var csv = GenerateCsv(reportData);
                var byteArray = Encoding.UTF8.GetBytes(csv);
                var stream = new MemoryStream(byteArray);
                return File(stream, "application/octet-stream", "Report.csv");
            }

            return View(reportData);
        }

        private List<VehicleInfo> FetchReportData(DateTime startDate, DateTime endDate, bool inChecked, bool outChecked)
        {
           
            var reportData = new List<VehicleInfo>();

            if (inChecked && outChecked)
            {
                reportData = _context.VehicleInfos
                    .Where(v => v.DateTime >= startDate && v.DateTime <= endDate)
                    .ToList();
            }
            else if (inChecked)
            {
                reportData = _context.VehicleInfos
                    .Where(v => v.DateTime >= startDate && v.DateTime <= endDate && v.InOrOut.ToLower() == "in")
                    .ToList();
            }
            else if (outChecked)
            {
                reportData = _context.VehicleInfos
                    .Where(v => v.DateTime >= startDate && v.DateTime <= endDate && v.InOrOut.ToLower() == "out")
                    .ToList();
            }
            else
            {
                reportData = _context.VehicleInfos
                    .Where(v => v.DateTime >= startDate && v.DateTime <= endDate)
                    .ToList();
            }

            return reportData;
        }

        private string GenerateCsv(List<VehicleInfo> reportData)
        {
          
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("VehicleId,NumberPlate,InOrOut,DateTime");

            foreach (var vehicleInfo in reportData)
            {
                csvBuilder.AppendLine($"{vehicleInfo.VehicleId},{vehicleInfo.NumberPlate},{vehicleInfo.InOrOut},{vehicleInfo.DateTime.ToString("o", CultureInfo.InvariantCulture)}");
            }

            return csvBuilder.ToString();
        }
    }
}
