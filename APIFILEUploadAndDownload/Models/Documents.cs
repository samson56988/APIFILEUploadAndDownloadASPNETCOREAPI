using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIFILEUploadAndDownload.Models
{
    public class Documents
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long? FileSize { get; set; }
    }
}
