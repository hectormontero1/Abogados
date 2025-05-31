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
    public class ClientesController : Controller
    {
        private AbogadosContext _context;

        public ClientesController(AbogadosContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions,int id) {
            var clientes = _context.Clientes.Select(i => new {
                i.IdCliente,
                i.RazonSocial,
                i.RucCedula,
                i.RepresentanteLegal,
                i.Correo,
                i.Telefono,
                i.Direccion,
                i.IdConsultorio,
                i.TipoCliente
            }).Where(x=>x.IdConsultorio==id);

            // If underlying data is a large SQL table, specify PrimaryKey and PaginateViaPrimaryKey.
            // This can make SQL execution plans more efficient.
            // For more detailed information, please refer to this discussion: https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "IdCliente" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Json(await DataSourceLoader.LoadAsync(clientes, loadOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values) {
            var model = new Cliente();
            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);

            if(!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));

            var result = _context.Clientes.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { result.Entity.IdCliente });
        }

        [HttpPut]
        public async Task<IActionResult> Put(int key, string values) {
            var model = await _context.Clientes.FirstOrDefaultAsync(item => item.IdCliente == key);
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
            var model = await _context.Clientes.FirstOrDefaultAsync(item => item.IdCliente == key);

            _context.Clientes.Remove(model);
            await _context.SaveChangesAsync();
        }


        private void PopulateModel(Cliente model, IDictionary values) {
            string ID_CLIENTE = nameof(Cliente.IdCliente);
            string RAZON_SOCIAL = nameof(Cliente.RazonSocial);
            string RUC_CEDULA = nameof(Cliente.RucCedula);
            string REPRESENTANTE_LEGAL = nameof(Cliente.RepresentanteLegal);
            string CORREO = nameof(Cliente.Correo);
            string TELEFONO = nameof(Cliente.Telefono);
            string DIRECCION = nameof(Cliente.Direccion);
            string TIPO_CLIENTE = nameof(Cliente.TipoCliente);

            if(values.Contains(ID_CLIENTE)) {
                model.IdCliente = Convert.ToInt32(values[ID_CLIENTE]);
            }

            if(values.Contains(RAZON_SOCIAL)) {
                model.RazonSocial = Convert.ToString(values[RAZON_SOCIAL]);
            }

            if(values.Contains(RUC_CEDULA)) {
                model.RucCedula = Convert.ToString(values[RUC_CEDULA]);
            }

            if(values.Contains(REPRESENTANTE_LEGAL)) {
                model.RepresentanteLegal = Convert.ToString(values[REPRESENTANTE_LEGAL]);
            }

            if(values.Contains(CORREO)) {
                model.Correo = Convert.ToString(values[CORREO]);
            }

            if(values.Contains(TELEFONO)) {
                model.Telefono = Convert.ToString(values[TELEFONO]);
            }

            if(values.Contains(DIRECCION)) {
                model.Direccion = Convert.ToString(values[DIRECCION]);
            }

            if(values.Contains(TIPO_CLIENTE)) {
                model.TipoCliente = Convert.ToString(values[TIPO_CLIENTE]);
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