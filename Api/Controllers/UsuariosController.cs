using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Domain.Models;
using Infrastructure.Data;
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


namespace Api.Controllers
{
    [Route("[controller]/[action]")]
    public class UsuariosController : Controller
    {
        private AbogadosContext _context;

        public UsuariosController(AbogadosContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions, int id) {
            var usuarios = _context.Usuarios.Select(i => new {
                i.IdUsuario,
                i.NombreUsuario,
                i.PasswordHash,
                i.Rol,
                i.Email,
                i.Estado,
                i.IdConsultorio
            }).Where(x=>x.IdConsultorio==id);

            // If underlying data is a large SQL table, specify PrimaryKey and PaginateViaPrimaryKey.
            // This can make SQL execution plans more efficient.
            // For more detailed information, please refer to this discussion: https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "IdUsuario" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Json(await DataSourceLoader.LoadAsync(usuarios, loadOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values) {
            var model = new Usuario();
            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);

            if(!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));

            var result = _context.Usuarios.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { result.Entity.IdUsuario });
        }

        [HttpPut]
        public async Task<IActionResult> Put(int key, string values) {
            var model = await _context.Usuarios.FirstOrDefaultAsync(item => item.IdUsuario == key);
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
            var model = await _context.Usuarios.FirstOrDefaultAsync(item => item.IdUsuario == key);

            _context.Usuarios.Remove(model);
            await _context.SaveChangesAsync();
        }


        private void PopulateModel(Usuario model, IDictionary values) {
            string ID_USUARIO = nameof(Usuario.IdUsuario);
            string NOMBRE_USUARIO = nameof(Usuario.NombreUsuario);
            string PASSWORD_HASH = nameof(Usuario.PasswordHash);
            string ROL = nameof(Usuario.Rol);
            string EMAIL = nameof(Usuario.Email);
            string ESTADO = nameof(Usuario.Estado);
            string ID_CONSULTORIO = nameof(Usuario.IdConsultorio);

            if(values.Contains(ID_USUARIO)) {
                model.IdUsuario = Convert.ToInt32(values[ID_USUARIO]);
            }

            if(values.Contains(NOMBRE_USUARIO)) {
                model.NombreUsuario = Convert.ToString(values[NOMBRE_USUARIO]);
            }

            if(values.Contains(PASSWORD_HASH)) {
                model.PasswordHash = Convert.ToString(values[PASSWORD_HASH]);
            }

            if(values.Contains(ROL)) {
                model.Rol = Convert.ToString(values[ROL]);
            }

            if(values.Contains(EMAIL)) {
                model.Email = Convert.ToString(values[EMAIL]);
            }

            if(values.Contains(ESTADO)) {
                model.Estado = values[ESTADO] != null ? Convert.ToBoolean(values[ESTADO]) : (bool?)null;
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