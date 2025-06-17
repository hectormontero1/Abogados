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
using Infrastructure.Data;

namespace Api.Controllers
{
    [Route("[controller]/[action]")]
    public class AuditoriasController : Controller
    {
        private AbogadosContext _context;
         
        public AuditoriasController(AbogadosContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions, int id) {
            var auditorias = _context.Auditorias.Select(i => new {
                i.IdAuditoria,
                i.Usuario,
                i.Fecha,
                i.Accion,
                i.TablaAfectada,
                i.ClaveRegistro,
                i.DetalleCambio,
                i.IdConsultorio
            }).Where(x => x.IdConsultorio == id);

            // If underlying data is a large SQL table, specify PrimaryKey and PaginateViaPrimaryKey.
            // This can make SQL execution plans more efficient.
            // For more detailed information, please refer to this discussion: https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "IdAuditoria" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Json(await DataSourceLoader.LoadAsync(auditorias, loadOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values) {
            var model = new Auditoria();
            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);

            if(!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));

            var result = _context.Auditorias.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { result.Entity.IdAuditoria });
        }

        [HttpPut]
        public async Task<IActionResult> Put(int key, string values) {
            var model = await _context.Auditorias.FirstOrDefaultAsync(item => item.IdAuditoria == key);
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
            var model = await _context.Auditorias.FirstOrDefaultAsync(item => item.IdAuditoria == key);

            _context.Auditorias.Remove(model);
            await _context.SaveChangesAsync();
        }


        private void PopulateModel(Auditoria model, IDictionary values) {
            string ID_AUDITORIA = nameof(Auditoria.IdAuditoria);
            string USUARIO = nameof(Auditoria.Usuario);
            string FECHA = nameof(Auditoria.Fecha);
            string ACCION = nameof(Auditoria.Accion);
            string TABLA_AFECTADA = nameof(Auditoria.TablaAfectada);
            string CLAVE_REGISTRO = nameof(Auditoria.ClaveRegistro);
            string DETALLE_CAMBIO = nameof(Auditoria.DetalleCambio);
            string ID_CONSULTORIO = nameof(Auditoria.IdConsultorio);

            if(values.Contains(ID_AUDITORIA)) {
                model.IdAuditoria = Convert.ToInt32(values[ID_AUDITORIA]);
            }

            if(values.Contains(USUARIO)) {
                model.Usuario = Convert.ToString(values[USUARIO]);
            }

            if(values.Contains(FECHA)) {
                model.Fecha = values[FECHA] != null ? Convert.ToDateTime(values[FECHA]) : (DateTime?)null;
            }

            if(values.Contains(ACCION)) {
                model.Accion = Convert.ToString(values[ACCION]);
            }

            if(values.Contains(TABLA_AFECTADA)) {
                model.TablaAfectada = Convert.ToString(values[TABLA_AFECTADA]);
            }

            if(values.Contains(CLAVE_REGISTRO)) {
                model.ClaveRegistro = Convert.ToString(values[CLAVE_REGISTRO]);
            }

            if(values.Contains(DETALLE_CAMBIO)) {
                model.DetalleCambio = Convert.ToString(values[DETALLE_CAMBIO]);
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
    }
}