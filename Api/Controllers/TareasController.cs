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
    public class TareasController : Controller
    {
        private AbogadosContext _context;

        public TareasController(AbogadosContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            var tareas = from t in _context.Tareas
                         join c in _context.Casos on t.IdCaso equals c.IdCaso
                         join u in _context.Usuarios on t.IdUsuarioAsignado equals u.IdUsuario
                         select new
                         {
                             t.IdTarea,
                             t.Titulo,
                             t.Descripcion,
                             t.FechaAsignacion,
                             t.FechaLimite,
                             t.Estado,
                             t.IdCaso,
                             t.IdUsuarioAsignado,
                             c.CodigoCaso,
                             TituloCaso = c.Titulo,
                             u.NombreUsuario,
                             u.IdUsuario
                         };

            return Json(await DataSourceLoader.LoadAsync(tareas, loadOptions));
        }


        [HttpPost]
        public async Task<IActionResult> Post(string values) {
            var model = new Tarea();
            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);

            if(!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));

            var result = _context.Tareas.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { result.Entity.IdTarea });
        }

        [HttpPut]
        public async Task<IActionResult> Put(int key, string values) {
            var model = await _context.Tareas.FirstOrDefaultAsync(item => item.IdTarea == key);
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
            var model = await _context.Tareas.FirstOrDefaultAsync(item => item.IdTarea == key);

            _context.Tareas.Remove(model);
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

        [HttpGet]
        public async Task<IActionResult> UsuariosLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.Usuarios
                         orderby i.NombreUsuario
                         select new {
                             Value = i.IdUsuario,
                             Text = i.NombreUsuario
                         };
            return Json(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        private void PopulateModel(Tarea model, IDictionary values) {
            string ID_TAREA = nameof(Tarea.IdTarea);
            string TITULO = nameof(Tarea.Titulo);
            string DESCRIPCION = nameof(Tarea.Descripcion);
            string ESTADO = nameof(Tarea.Estado);
            string ID_CASO = nameof(Tarea.IdCaso);
            string ID_USUARIO_ASIGNADO = nameof(Tarea.IdUsuarioAsignado);

            if(values.Contains(ID_TAREA)) {
                model.IdTarea = Convert.ToInt32(values[ID_TAREA]);
            }

            if(values.Contains(TITULO)) {
                model.Titulo = Convert.ToString(values[TITULO]);
            }

            if(values.Contains(DESCRIPCION)) {
                model.Descripcion = Convert.ToString(values[DESCRIPCION]);
            }

            if(values.Contains(ESTADO)) {
                model.Estado = Convert.ToString(values[ESTADO]);
            }

            if(values.Contains(ID_CASO)) {
                model.IdCaso = values[ID_CASO] != null ? Convert.ToInt32(values[ID_CASO]) : (int?)null;
            }

            if(values.Contains(ID_USUARIO_ASIGNADO)) {
                model.IdUsuarioAsignado = values[ID_USUARIO_ASIGNADO] != null ? Convert.ToInt32(values[ID_USUARIO_ASIGNADO]) : (int?)null;
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