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
    public class ContactosClientesController : Controller
    {
        private AbogadosContext _context;

        public ContactosClientesController(AbogadosContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions) {
            var contactosclientes = _context.ContactosClientes.Select(i => new {
                i.IdContacto,
                i.IdCliente,
                i.Nombre,
                i.Cargo,
                i.Email,
                i.Telefono
            });

            // If underlying data is a large SQL table, specify PrimaryKey and PaginateViaPrimaryKey.
            // This can make SQL execution plans more efficient.
            // For more detailed information, please refer to this discussion: https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "IdContacto" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Json(await DataSourceLoader.LoadAsync(contactosclientes, loadOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values) {
            var model = new ContactosCliente();
            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);

            if(!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));

            var result = _context.ContactosClientes.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { result.Entity.IdContacto });
        }

        [HttpPut]
        public async Task<IActionResult> Put(int key, string values) {
            var model = await _context.ContactosClientes.FirstOrDefaultAsync(item => item.IdContacto == key);
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
            var model = await _context.ContactosClientes.FirstOrDefaultAsync(item => item.IdContacto == key);

            _context.ContactosClientes.Remove(model);
            await _context.SaveChangesAsync();
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

        private void PopulateModel(ContactosCliente model, IDictionary values) {
            string ID_CONTACTO = nameof(ContactosCliente.IdContacto);
            string ID_CLIENTE = nameof(ContactosCliente.IdCliente);
            string NOMBRE = nameof(ContactosCliente.Nombre);
            string CARGO = nameof(ContactosCliente.Cargo);
            string EMAIL = nameof(ContactosCliente.Email);
            string TELEFONO = nameof(ContactosCliente.Telefono);

            if(values.Contains(ID_CONTACTO)) {
                model.IdContacto = Convert.ToInt32(values[ID_CONTACTO]);
            }

            if(values.Contains(ID_CLIENTE)) {
                model.IdCliente = values[ID_CLIENTE] != null ? Convert.ToInt32(values[ID_CLIENTE]) : (int?)null;
            }

            if(values.Contains(NOMBRE)) {
                model.Nombre = Convert.ToString(values[NOMBRE]);
            }

            if(values.Contains(CARGO)) {
                model.Cargo = Convert.ToString(values[CARGO]);
            }

            if(values.Contains(EMAIL)) {
                model.Email = Convert.ToString(values[EMAIL]);
            }

            if(values.Contains(TELEFONO)) {
                model.Telefono = Convert.ToString(values[TELEFONO]);
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
        public async Task<IActionResult> CargarCSV([FromBody] List<ContactosCliente> clientes)
        {
            if (clientes == null || !clientes.Any())
            {
                return BadRequest("No se recibieron datos.");
            }

            // Agregar todos de una sola vez
            await _context.ContactosClientes.AddRangeAsync(clientes);

            // Guardar una sola vez
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Datos procesados correctamente", cantidad = clientes.Count });
        }
    }
}