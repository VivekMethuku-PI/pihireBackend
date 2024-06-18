using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.BAL.Common.Handler
{
    public class PageEvent 
    {
        
        private readonly IHostingEnvironment _environment;

        
        public PageEvent(IHostingEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }
      //  iTextSharp.text.Image image;

       
        //public override void OnOpenDocument(PdfWriter writer, Document document)
        //{
        //    string url_path = _environment.ContentRootPath + "\\EmailTemplates\\Images\\paraminfo.png";
        //    image = iTextSharp.text.Image.GetInstance(url_path);
        //    image.SetAbsolutePosition(385, 755);
        //}
        //public override void OnEndPage(PdfWriter writer, Document document)
        //{
        //    writer.DirectContent.AddImage(image);
        //}
    }
}
