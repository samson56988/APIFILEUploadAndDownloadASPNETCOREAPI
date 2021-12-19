using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UI.Models;

namespace UI.Controllers
{
    public class FileController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnviroment;

        public FileController(IWebHostEnvironment webHostEnvironment)
        {

            _webHostEnviroment = webHostEnvironment;
        }
        public async System.Threading.Tasks.Task<IActionResult> FileList()
        {
            List<Documents> documents = new List<Documents>();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync("https://localhost:44348/api/File/AllFiles"))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    documents = JsonConvert.DeserializeObject<List<Documents>>(apiResponse);

                }
            }
            return View(documents);
        }


        public IActionResult CreateFile()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateFileAsync(IFormFile files, byte[] bytes)
        {

            var roothPath = Path.Combine(_webHostEnviroment.ContentRootPath, "Resources", "Documents");

            if (!Directory.Exists(roothPath))
                Directory.CreateDirectory(roothPath);
            var filepath = Path.Combine(roothPath, files.FileName);
            using (var stream = new FileStream(filepath, FileMode.Create))
            {
                var document = new Documents
                {
                    FileName = files.FileName,
                    ContentType = files.ContentType,
                    FileSize = files.Length
                };

                using (var client = new HttpClient())
                {

                    
                    using (var content = new MultipartFormDataContent())
                    {
                        bytes = new byte[files.OpenReadStream().Length + 1];
                        files.OpenReadStream().Read(bytes, 0, bytes.Length);
                        var fileContent = new ByteArrayContent(bytes);
                        fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = files.FileName };
                        content.Add(fileContent);
                        var requestUri = "https://localhost:44348/api/File/upload";
                        var result = client.PostAsync(requestUri, content).Result;
                        if (result.StatusCode == System.Net.HttpStatusCode.Created)
                        {

                            string apiResponse = await result.Content.ReadAsStringAsync();
                            document = JsonConvert.DeserializeObject<Documents>(apiResponse);


                        }

                        return RedirectToAction("FileList");
                        //bytes = new byte[files.OpenReadStream().Length + 1];
                        //var document = new Documents();
                        //files.OpenReadStream().Read(bytes, 0, bytes.Length);


                        // //var roothPath = Path.Combine(_webHostEnviroment.ContentRootPath, "Resources", "Documents");         
                        // using (var httpClient = new HttpClient())
                        // {
                        //     //var multipartFormDataContent = new MultipartFormDataContent();
                        //     MultipartFormDataContent content = new MultipartFormDataContent();
                        //     var fileContent = new ByteArrayContent(bytes);
                        //     content.Add(fileContent, "file", files.FileName);                    
                        //     using (var response = await httpClient.PostAsync("https://localhost:44348/api/File/upload", content))
                        //  {

                        //  string apiResponse = await response.Content.ReadAsStringAsync();
                        // document = JsonConvert.DeserializeObject<Documents>(apiResponse);
                        // document.FileName = files.FileName;
                        // document.FileSize = files.Length;
                        // document.ContentType = files.ContentType;
                        //  }

                        // }

                        // return RedirectToAction("FileList");

                    }
                }
            }
                
        }
    }
}

