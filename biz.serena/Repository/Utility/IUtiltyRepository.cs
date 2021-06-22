using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace biz.serena.Repository.Utility
{
    public interface IUtiltyRepository
    {
        string CompressString(string text);
        string DecompressString(string compressedText);
        string UploadImageBase64(string image, string ruta, string extension);
        Stream DeleteFile(string ruta);
        bool IsBase64(string base64String);
    }
}
