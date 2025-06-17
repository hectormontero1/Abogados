using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;

namespace Api.Controllers
{
    [Route("[controller]/[action]")]
    public class DocumentosController : Controller
    {
        private AbogadosContext _context;

        public DocumentosController(AbogadosContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions, int id) {
            var documentos = _context.Documentos.Select(i => new {
                i.IdDocumento,
                i.NombreArchivo,
                i.TipoDocumento,
                i.RutaArchivo,
                i.FechaSubida,
                i.IdCaso
            }).Where(x => x.IdCaso == id); ;

            // If underlying data is a large SQL table, specify PrimaryKey and PaginateViaPrimaryKey.
            // This can make SQL execution plans more efficient.
            // For more detailed information, please refer to this discussion: https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "IdDocumento" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Json(await DataSourceLoader.LoadAsync(documentos, loadOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values) {
            var model = new Documento();
            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);

            if(!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));

            var result = _context.Documentos.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { result.Entity.IdDocumento });
        }

        [HttpPut]
        public async Task<IActionResult> Put(int key, string values) {
            var model = await _context.Documentos.FirstOrDefaultAsync(item => item.IdDocumento == key);
            if(model == null)
                return StatusCode(409, "Object not found");

            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);

            if(!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task Delete(int key) {
            var model = await _context.Documentos.FirstOrDefaultAsync(item => item.IdDocumento == key);

            _context.Documentos.Remove(model);
            await _context.SaveChangesAsync();
        }


        [HttpGet]
        public async Task<IActionResult> CasosLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.Casos
                         orderby i.CodigoCaso
                         select new {
                             Value = i.IdCaso,
                             Text = i.CodigoCaso
                         };
            return Json(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        private void PopulateModel(Documento model, IDictionary values) {
            string ID_DOCUMENTO = nameof(Documento.IdDocumento);
            string NOMBRE_ARCHIVO = nameof(Documento.NombreArchivo);
            string TIPO_DOCUMENTO = nameof(Documento.TipoDocumento);
            string RUTA_ARCHIVO = nameof(Documento.RutaArchivo);
            string FECHA_SUBIDA = nameof(Documento.FechaSubida);
            string ID_CASO = nameof(Documento.IdCaso);

            if(values.Contains(ID_DOCUMENTO)) {
                model.IdDocumento = Convert.ToInt32(values[ID_DOCUMENTO]);
            }

            if(values.Contains(NOMBRE_ARCHIVO)) {
                model.NombreArchivo = Convert.ToString(values[NOMBRE_ARCHIVO]);
            }

            if(values.Contains(TIPO_DOCUMENTO)) {
                model.TipoDocumento = Convert.ToString(values[TIPO_DOCUMENTO]);
            }

            if(values.Contains(RUTA_ARCHIVO)) {
                model.RutaArchivo = Convert.ToString(values[RUTA_ARCHIVO]);
            }

            if(values.Contains(FECHA_SUBIDA)) {
                model.FechaSubida = values[FECHA_SUBIDA] != null ? Convert.ToDateTime(values[FECHA_SUBIDA]) : (DateTime?)null;
            }

            if(values.Contains(ID_CASO)) {
                model.IdCaso = Convert.ToInt32(values[ID_CASO]);
            }
        }

        private string GetFullErrorMessage(ModelStateDictionary modelState) {
            var messages = new List<string>();

            foreach(var entry in modelState) {
                foreach(var error in entry.Value.Errors)
                    messages.Add(error.ErrorMessage);
            }

            return String.Join(" ", messages);
        }
    }
}