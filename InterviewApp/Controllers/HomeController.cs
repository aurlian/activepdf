using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using InterviewApp.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace InterviewApp.Controllers
{
    public class HomeController : Controller
    {
        private List<String> _validExtensions = new List<String>() { ".jpg", ".jpeg", ".png", ".tiff", ".tif", ".bmp" };

        public IActionResult Index()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    var name = Path.GetFileName(file.FileName);
                    var extension = Path.GetExtension(file.FileName);
                    if (!_validExtensions.Contains(extension.ToLowerInvariant()))
                    {
                        TempData["Message"] = "Not a valid file extension";
                        return RedirectToAction("Index");
                    }

                    string filePath = Path.Combine("Output", String.Format("{0}_{1}.{2}", Guid.NewGuid(), name,extension));

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    APToolkitNET.Toolkit oTK = new APToolkitNET.Toolkit();
                    // Set the PDF page Height and Width to Letter (72 = 1")
                    oTK.OutputPageHeight = 792.0f;
                    oTK.OutputPageWidth = 612.0f;

                    // Create the new PDF file
                    string pdfFileName = "output.pdf";
                    var intOpenOutputFile = oTK.OpenOutputFile(String.Format("{0}/{1}", "output", pdfFileName));
                    if (intOpenOutputFile != 0)
                    {
                        //log and redirect to custom error page
                        //for demo just throw
                        throw new Exception("error in pdf creation");
                    }

                    oTK.NewPage();

                    oTK.SetFont("Helvetica", 12);
                    oTK.PrintText(50.0f, 20.0f, "Signature: Mher Sarkissian");

                    oTK.PrintImage(filePath, 0.0f, 200.0f, 612.0f, 792.0f, true, 0);

                    
                    oTK.CloseOutputFile();
                    oTK.Dispose();

                }
                else
                {
                    TempData["Message"] = "Invalid file upload";
                    return RedirectToAction("Index");
                }

            }
            catch (Exception e)
            {
                //log and redirect to custom error page
                //for demo just throw
                throw e;
            }

            return View("ViewPDF");
        }

        [HttpPost]
        public async Task<JsonResult> GetPdfData()
        {
            return Json(Convert.ToBase64String(System.IO.File.ReadAllBytes("output/output.pdf")));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
