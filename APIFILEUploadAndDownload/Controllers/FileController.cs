using APIFILEUploadAndDownload.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace APIFILEUploadAndDownload.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        protected readonly IConfiguration _config;
        private readonly IWebHostEnvironment _webHostEnviroment;
        static string constr = @"Data Source=.;Initial Catalog=FileUploadandDownload;Integrated Security=True";
        public FileController(IConfiguration config, IWebHostEnvironment webHostEnvironment)
        {
            _config = config;
            ConnectionString = _config.GetConnectionString("DefaultConnection");
            ProviderName = "System.Data.SqlClient";
            _webHostEnviroment = webHostEnvironment;
        }
        public string ConnectionString { get; set; }
        public string ProviderName { get; set; }
        [HttpGet]
        [Route("AllFiles")]
        public IActionResult Index()
        {
            return Ok(PopulateFiles());
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> UploadAsync(List<IFormFile> files)
        {

            long size = files.Sum(f => f.Length);

            var roothPath = Path.Combine(_webHostEnviroment.ContentRootPath, "Resources", "Documents");

            if (!Directory.Exists(roothPath))
                Directory.CreateDirectory(roothPath);

            foreach(var file in files)
            {
                var filepath = Path.Combine(roothPath, file.FileName);

                using(var stream = new FileStream(filepath,FileMode.Create))
                {
                    var document = new Documents
                    {
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        FileSize = file.Length
                    };

                    await file.CopyToAsync(stream);

                    using (SqlConnection con = new SqlConnection(constr))
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandText = "INSERT INTO DocumentTbl(FileName, ContentType, Data) VALUES (@Name, @ContentType, @Data)";
                            cmd.Parameters.AddWithValue("@Name", document.FileName);
                            cmd.Parameters.AddWithValue("@ContentType", document.ContentType);
                            cmd.Parameters.AddWithValue("@Data", document.FileSize);
                            cmd.Connection = con;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }
            }


            return Ok(new { count = files.Count, size });
        }
        [HttpPost]
        [Route("download/{Id}")]
        public IActionResult DownloadFile(int Id)
        {
            var model = new Documents();
            var provider = new FileExtensionContentTypeProvider();
            model = PopulateFiles().Find(x => x.Id == Convert.ToInt32(Id));

            if (model == null)
                return NotFound();

            var file = Path.Combine(_webHostEnviroment.ContentRootPath, "Resources", "Documents",model.FileName);

            string contentType;
            if (!provider.TryGetContentType(file, out contentType))
            {
                contentType = "application/octet-stream";
            }

            byte[] fileBytes;
            if (System.IO.File.Exists(file))
            {
                fileBytes = System.IO.File.ReadAllBytes(file);
            }
            else
            {
                return NotFound();
            }


            return File(fileBytes, contentType, model.FileName);

        }

        private static List<Documents> PopulateFiles()
        {
            List<Documents> files = new List<Documents>();
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "SELECT * FROM DocumentTbl";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            files.Add(new Documents
                            {
                                Id = Convert.ToInt32(sdr["DocumentID"].ToString()),
                                FileName = sdr["FileName"].ToString(),
                                ContentType = sdr["ContentType"].ToString(),
                                FileSize =  Convert.ToInt64(sdr["Data"].ToString())
                            });
                        }
                    }
                    con.Close();
                }
            }

            return files;
            }
      


        }
}
