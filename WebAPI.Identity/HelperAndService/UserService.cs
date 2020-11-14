using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Repository;
using WebAPI.Dominio;
using System.Drawing;

namespace WebAPI.Identity.Helper
{
    public class UserService
    {
        private string folderPath { get; set; } = @"\wwwroot\tempImport";
        private string targetDirectory { get; set; }

        private readonly IConfiguration _config;
        private readonly Context _context;
        public UserService(IConfiguration config, Context context)
        {
            _config = config;
            _context = context;
        }

        private string BaseDirAPP()
        {
            return _config.GetSection("BaseDirApp").Value;
        }

        public async void SaveImage(IFormFile file, User user )
        {
            if(file == null)
            {
                throw new ArgumentNullException("a imagem não foi enviada");
            }

            CreateDirectory();

            var fileName = file.FileName;
            var savePath = Path.Combine(targetDirectory, fileName);

            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            System.IO.File.Move(savePath, Path.Combine(targetDirectory, user.UserName));

        }
        private void CreateDirectory()
        {
            if (!Directory.Exists($"{this.BaseDirAPP()}{folderPath}"))
                Directory.CreateDirectory($"{this.BaseDirAPP()}{folderPath}");

            targetDirectory = $"{this.BaseDirAPP()}{folderPath}";
        }

        public bool CompareImages(IFormFile file, User user)
        {
            CreateDirectory();

            var OrigIm = new Bitmap(Path.Combine(targetDirectory, user.UserName));
            var InpuIm = new Bitmap(file.OpenReadStream());

            string OrigIm_ref;
            string InpuIm_ref;
            int contador1 = 0;
            int contador2 = 0;
            bool flag = true; 

            if (OrigIm.Width == InpuIm.Width && OrigIm.Height == InpuIm.Height)
            {
                for (int i = 0; i < OrigIm.Width; i++)
                {
                    for (int j = 0; j < OrigIm.Height; j++)
                    {
                        OrigIm_ref = OrigIm.GetPixel(i, j).ToString();
                        InpuIm_ref = InpuIm.GetPixel(i, j).ToString();
                        if (OrigIm_ref != InpuIm_ref)
                        {
                            contador2++;
                            flag = false;
                            break;
                        }
                        contador1++;
                    }
                }
                return flag;
            }
            else
            {
                return false;
            }
        }

        
    }
}
