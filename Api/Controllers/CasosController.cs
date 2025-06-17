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
    public class CasosController : Controller
    {
        private AbogadosContext _context;

        public CasosController(AbogadosContext context) {
            _context = context;
        }

        //[HttpGet]
        //public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions, int id)
        //{
        //    //var casos = _context.Casos
        //    //    .Include(c => c.IdClienteNavigation)
        //    //    .Include(c => c.Consultorio)
        //    //    .Where(x => x.IdConsultorio == id)
        //    //    .Select(i => new {
        //    //        i.IdCaso,
        //    //        i.CodigoCaso,
        //    //        i.Titulo,
        //    //        i.Descripcion,
        //    //        i.FechaInicio,
        //    //        i.FechaFin,
        //    //        i.Estado,
        //    //        Cliente = i.IdClienteNavigation.RazonSocial,
        //    //        Consultorio = i.Consultorio.Nombre
        //    //    });

        //    //return Json(await DataSourceLoader.LoadAsync(casos, loadOptions));
        //    var casos = _context.Casos
        //.Include(c => c.IdClienteNavigation)
        //.Include(c => c.Consultorio)
        //.Where(c => c.IdConsultorio == id) // Filtramos por el ID, que es un int
        //.Select(i => new {
        //    i.IdCaso,
        //    i.CodigoCaso,
        //    i.Titulo,
        //    i.Descripcion,
        //    i.FechaInicio,
        //    i.FechaFin,
        //    i.Estado,
        //    Cliente = i.IdClienteNavigation.RazonSocial,
        //    Consultorio = i.Consultorio.Nombre,
        //    TotalDocumentos = _context.Documentos.Count(d => d.IdCaso == i.IdCaso)
        //});

        //    return Json(await DataSourceLoader.LoadAsync(casos, loadOptions));

        //}
        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions, int id)
        {
            var consulta = from c in _context.Casos
                           join cl in _context.Clientes on c.IdCliente equals cl.IdCliente
                           join a in _context.Abogados on c.IdAbogadoResponsable equals a.IdAbogado
                           join cs in _context.Consultorios on c.IdConsultorio equals cs.IdConsultorio
                           where c.IdConsultorio == id
                           join d in _context.Documentos on c.IdCaso equals d.IdCaso into docs
                           from d in docs.DefaultIfEmpty()
                           join t in _context.Tareas on c.IdCaso equals t.IdCaso into tareas
                           from t in tareas.DefaultIfEmpty()
                           join n in _context.NotasInternas on c.IdCaso equals n.IdCaso into notas
                           from n in notas.DefaultIfEmpty()
                           group new { c, cl, cs, d, t, n } by new
                           {
                               c.IdCaso,
                               c.CodigoCaso,
                               c.Titulo,
                               c.Descripcion,
                               c.FechaInicio,
                               c.FechaFin,
                               c.Estado,
                               cl.RazonSocial,
                               Consultorio = cs.Nombre
                           } into g
                           select new
                           {
                               g.Key.IdCaso,
                               g.Key.CodigoCaso,
                               g.Key.Titulo,
                               g.Key.Descripcion,
                               g.Key.FechaInicio,
                               g.Key.FechaFin,
                               g.Key.Estado,
                               Cliente = g.Key.RazonSocial,
                               Consultorio = g.Key.Consultorio,
                               TotalDocumentos = g.Where(x => x.d != null).Select(x => x.d.IdDocumento).Distinct().Count(),
                               TareasPendientes = g.Where(x => x.t != null && x.t.Estado == "Abierto").Select(x => x.t.IdTarea).Distinct().Count(),
                               TareasRealizadas = g.Where(x => x.t != null && x.t.Estado == "Cerrado").Select(x => x.t.IdTarea).Distinct().Count(),
                               NotasInternas = g.Where(x => x.n != null).Select(x => x.n.IdNota).Distinct().Count()
                           };

            var resultado = await DataSourceLoader.LoadAsync(consulta, loadOptions);
            return Json(resultado);
        }




        [HttpPost]
        public async Task<IActionResult> Post(string values) {
            var model = new Caso();
            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);

            if(!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));

            var result = _context.Casos.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { result.Entity.IdCaso });
        }

        [HttpPut]
        public async Task<IActionResult> Put(int key, string values) {
            var model = await _context.Casos.FirstOrDefaultAsync(item => item.IdCaso == key);
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
            var model = await _context.Casos.FirstOrDefaultAsync(item => item.IdCaso == key);

            _context.Casos.Remove(model);
            await _context.SaveChangesAsync();
        }


        [HttpGet]
        public async Task<IActionResult> AbogadosLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.Abogados
                         orderby i.Nombre
                         select new {
                             Value = i.IdAbogado,
                             Text = i.Nombre
                         };
            return Json(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        [HttpGet]
        public async Task<IActionResult> ClientesLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.Clientes
                         orderby i.RazonSocial
                         select new {
                             Value = i.IdCliente,
                             Text = i.RazonSocial
                         };
            return Json(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        private void PopulateModel(Caso model, IDictionary values) {
            string ID_CASO = nameof(Caso.IdCaso);
            string CODIGO_CASO = nameof(Caso.CodigoCaso);
            string TITULO = nameof(Caso.Titulo);
            string DESCRIPCION = nameof(Caso.Descripcion);
            string ESTADO = nameof(Caso.Estado);
            string ID_CLIENTE = nameof(Caso.IdCliente);
            string ID_ABOGADO_RESPONSABLE = nameof(Caso.IdAbogadoResponsable);
            string ID_CONSULTORIO = nameof(Caso.IdConsultorio);

            if(values.Contains(ID_CASO)) {
                model.IdCaso = Convert.ToInt32(values[ID_CASO]);
            }

            if(values.Contains(CODIGO_CASO)) {
                model.CodigoCaso = Convert.ToString(values[CODIGO_CASO]);
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

            if(values.Contains(ID_CLIENTE)) {
                model.IdCliente = Convert.ToInt32(values[ID_CLIENTE]);
            }

            if(values.Contains(ID_ABOGADO_RESPONSABLE)) {
                model.IdAbogadoResponsable = Convert.ToInt32(values[ID_ABOGADO_RESPONSABLE]);
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

        [HttpPost("CargarCSV")]
        public async Task<IActionResult> CargarCSV([FromBody] List<Caso> casos)
        {
            if (casos == null || !casos.Any())
            {
                return BadRequest("No se recibieron datos.");
            }

            // Agregar todos de una sola vez
            await _context.Casos.AddRangeAsync(casos);

            // Guardar una sola vez
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Datos procesados correctamente", cantidad = casos.Count });
        }
    }
}