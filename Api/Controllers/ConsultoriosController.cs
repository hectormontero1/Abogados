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
    public class ConsultoriosController : Controller
    {
        private AbogadosContext _context;

        public ConsultoriosController(AbogadosContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions) {
            var consultorios = _context.Consultorios.Select(i => new {
                i.IdConsultorio,
                i.Nombre,
                i.Direccion,
                i.Telefono,
                i.Estado
            });

            // If underlying data is a large SQL table, specify PrimaryKey and PaginateViaPrimaryKey.
            // This can make SQL execution plans more efficient.
            // For more detailed information, please refer to this discussion: https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "IdConsultorio" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Json(await DataSourceLoader.LoadAsync(consultorios, loadOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values) {
            var model = new Consultorio();
            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);

            if(!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));

            var result = _context.Consultorios.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { result.Entity.IdConsultorio });
        }

        [HttpPut]
        public async Task<IActionResult> Put(int key, string values) {
            var model = await _context.Consultorios.FirstOrDefaultAsync(item => item.IdConsultorio == key);
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
            var model = await _context.Consultorios.FirstOrDefaultAsync(item => item.IdConsultorio == key);

            _context.Consultorios.Remove(model);
            await _context.SaveChangesAsync();
        }


        private void PopulateModel(Consultorio model, IDictionary values) {
            string ID_CONSULTORIO = nameof(Consultorio.IdConsultorio);
            string NOMBRE = nameof(Consultorio.Nombre);
            string DIRECCION = nameof(Consultorio.Direccion);
            string TELEFONO = nameof(Consultorio.Telefono);
            string ESTADO = nameof(Consultorio.Estado);

            if(values.Contains(ID_CONSULTORIO)) {
                model.IdConsultorio = Convert.ToInt32(values[ID_CONSULTORIO]);
            }

            if(values.Contains(NOMBRE)) {
                model.Nombre = Convert.ToString(values[NOMBRE]);
            }

            if(values.Contains(DIRECCION)) {
                model.Direccion = Convert.ToString(values[DIRECCION]);
            }

            if(values.Contains(TELEFONO)) {
                model.Telefono = Convert.ToString(values[TELEFONO]);
            }

            if(values.Contains(ESTADO)) {
                model.Estado = values[ESTADO] != null ? Convert.ToBoolean(values[ESTADO]) : (bool?)null;
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

        [HttpPost("CargarCSV")]
        public async Task<IActionResult> CargarCSV([FromBody] List<Consultorio> consultorios)
        {
            if (consultorios == null || !consultorios.Any())
            {
                return BadRequest("No se recibieron datos.");
            }

            // Agregar todos de una sola vez
            await _context.Consultorios.AddRangeAsync(consultorios);

            // Guardar una sola vez
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Datos procesados correctamente", cantidad = consultorios.Count });
        }

    }
}