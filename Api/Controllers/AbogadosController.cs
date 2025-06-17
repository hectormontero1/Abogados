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
    public class AbogadosController : Controller
    {
        private AbogadosContext _context;

        public AbogadosController(AbogadosContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions, int id) {
            var abogados = _context.Abogados.Select(i => new {
                i.IdAbogado,
                i.Nombre,
                i.Apellido,
                i.Cedula,
                i.Email,
                i.Telefono,
                i.Especialidad,
                i.Estado,
                i.IdConsultorio
            }).Where(x => x.IdConsultorio == id) ;

            // If underlying data is a large SQL table, specify PrimaryKey and PaginateViaPrimaryKey.
            // This can make SQL execution plans more efficient.
            // For more detailed information, please refer to this discussion: https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "IdAbogado" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Json(await DataSourceLoader.LoadAsync(abogados, loadOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values) {
            var model = new Abogado();
            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);

            if(!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));

            var result = _context.Abogados.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { result.Entity.IdAbogado });
        }

        [HttpPut]
        public async Task<IActionResult> Put(int key, string values) {
            var model = await _context.Abogados.FirstOrDefaultAsync(item => item.IdAbogado == key);
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
            var model = await _context.Abogados.FirstOrDefaultAsync(item => item.IdAbogado == key);

            _context.Abogados.Remove(model);
            await _context.SaveChangesAsync();
        }


        private void PopulateModel(Abogado model, IDictionary values) {
            string ID_ABOGADO = nameof(Abogado.IdAbogado);
            string NOMBRE = nameof(Abogado.Nombre);
            string APELLIDO = nameof(Abogado.Apellido);
            string CEDULA = nameof(Abogado.Cedula);
            string EMAIL = nameof(Abogado.Email);
            string TELEFONO = nameof(Abogado.Telefono);
            string ESPECIALIDAD = nameof(Abogado.Especialidad);
            string ESTADO = nameof(Abogado.Estado);
            string ID_CONSULTORIO = nameof(Abogado.IdConsultorio);

            if(values.Contains(ID_ABOGADO)) {
                model.IdAbogado = Convert.ToInt32(values[ID_ABOGADO]);
            }

            if(values.Contains(NOMBRE)) {
                model.Nombre = Convert.ToString(values[NOMBRE]);
            }

            if(values.Contains(APELLIDO)) {
                model.Apellido = Convert.ToString(values[APELLIDO]);
            }

            if(values.Contains(CEDULA)) {
                model.Cedula = Convert.ToString(values[CEDULA]);
            }

            if(values.Contains(EMAIL)) {
                model.Email = Convert.ToString(values[EMAIL]);
            }

            if(values.Contains(TELEFONO)) {
                model.Telefono = Convert.ToString(values[TELEFONO]);
            }

            if(values.Contains(ESPECIALIDAD)) {
                model.Especialidad = Convert.ToString(values[ESPECIALIDAD]);
            }

            if(values.Contains(ESTADO)) {
                model.Estado = Convert.ToBoolean(values[ESTADO]);
            }

            if(values.Contains(ID_CONSULTORIO)) {
                model.IdConsultorio = values[ID_CONSULTORIO] != null ? Convert.ToInt32(values[ID_CONSULTORIO]) : (int?)null;
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


        //[HttpPost("CargarCSV")]
        //public async Task<IActionResult> CargarCSV([FromBody] List<Abogado> abogados)
        //{
        //    if (abogados == null || !abogados.Any())
        //    {
        //        return BadRequest("No se recibieron datos.");
        //    }

        //    // Agregar todos de una sola vez
        //    await _context.Abogados.AddRangeAsync(abogados);

        //    // Guardar una sola vez
        //    await _context.SaveChangesAsync();

        //    return Ok(new { mensaje = "Datos procesados correctamente", cantidad = abogados.Count });
        //}


        [HttpPost("CargarCSV")]
        public async Task<IActionResult> CargarCSV()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            Console.WriteLine("JSON recibido:");
            Console.WriteLine(body); // ← aquí lo verás en la consola del backend

            return Ok(new { mensaje = "Revisado" });
        }
    }
}