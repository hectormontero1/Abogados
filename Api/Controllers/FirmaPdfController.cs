

using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.Drawing;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Security;
using Syncfusion.Pdf.Barcode;
using Syncfusion.Pdf.Interactive;
using Domain.Models;



[Route("[controller]/[action]")]

public class FirmaPdfController : Controller
{
    [HttpPost]
    public IActionResult FirmarPdf([FromBody] FirmaPdfRequest request)
    {
        try
        {
            byte[] pdfActual = request.PdfBytes;
            using var cert = new X509Certificate2(request.CertificadoP12, request.Password);
            using var ms = new MemoryStream(pdfActual);
            var document = new PdfLoadedDocument(ms);
            var nombreFirmante = cert.GetNameInfo(X509NameType.SimpleName, false);
            var organizacion = cert.GetNameInfo(X509NameType.DnsName, false);
            var numeroSerie = cert.SerialNumber;
            var fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            if (DateTime.Now < cert.NotBefore || DateTime.Now > cert.NotAfter)
                return BadRequest(new { message = "El certificado está vencido o aún no es válido." });
            //if (!cert.Verify())
            //    return BadRequest(new { message = "El certificado no es válido o no tiene una cadena de confianza válida." });
            foreach (var firma in request.Firmas)
            {
                firma.TextoQr = $"Firmado digitalmente por: {nombreFirmante}\nSerie: {numeroSerie}\nFecha: {fecha}";
                var pagina = document.Pages[firma.Pagina - 1];            
                PdfCertificate pdfCert = new PdfCertificate(cert);
                var signature = new PdfSignature(document, pagina, pdfCert, $"Firma_{firma.Pagina}_{firma.PosX}")
                    {
                        Bounds = new RectangleF(firma.PosX, firma.PosY, firma.Ancho, firma.Alto),
                        ContactInfo = "Contacto",
                        LocationInfo = "Ecuador",
                        Reason = "Firmado digitalmente"
                    };
                //PdfGraphics graphics = pagina.Graphics;
                //PdfGraphics graphics = signature.Appearance.Normal.Graphics;
                    var qr = new PdfQRBarcode
                    {
                        Text = firma.TextoQr,
                        XDimension =  0.5f,
                        ErrorCorrectionLevel = PdfErrorCorrectionLevel.High
                    };

                PdfFont fuente = new PdfStandardFont(PdfFontFamily.Helvetica, 3, PdfFontStyle.Bold);
                PdfSolidBrush brush = new PdfSolidBrush(Color.Black);
                string[] lista = nombreFirmante.Split(' ');
                float textoX = firma.PosX + 40; // margen derecho del QR
                float textoY = firma.PosY + 10; // alineado verticalmente
                pagina.Graphics.DrawString(lista[0], fuente, brush, new PointF(textoX, textoY));
                for (int i = 1; i < lista.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                     
                    }
                    else
                    {
                    textoY =textoY +10;
                    pagina.Graphics.DrawString(lista[i] + " " + lista[i + 1], fuente, brush, new PointF(textoX, textoY));
                    }
                   
                    //Console.WriteLine($"Nombre {i}: {lista[i]}");
                }

             



                // Generar QR code
                qr.Draw(pagina.Graphics, new PointF(firma.PosX, firma.PosY));
             
                
                //var campoFirma = new PdfSignatureField(pagina, $"firma_{firma.Pagina}_{firma.PosX}")
                //    {
                //        Signature = signature,
                //        Bounds = signature.Bounds
                //    };

                //    document.Form.Fields.Add(campoFirma);

                    // Dibujar el QR como marca visual
                    //qr.Draw(pagina.Graphics, signature.Bounds.Location);


               
            }
            using var outStream = new MemoryStream();
            document.Save(outStream);
            document.Close(true);

            pdfActual = outStream.ToArray();
            return File(pdfActual, "application/pdf", "firmado.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.InnerException});
        }

    }
}

public class FirmaPdfRequest

{
    public byte[] PdfBytes { get; set; }
    public byte[] CertificadoP12 { get; set; }
    public string Password { get; set; }
    public List<FirmaQr> Firmas { get; set; }
    public List<int> Paginas { get; set; }
 
}




public class FirmaQr
{
    public int Pagina { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float Ancho { get; set; }
    public float Alto { get; set; }
    public string TextoQr { get; set; }
}




