using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Anteproyecto
{
    public class UsuariosPoderJudicial
    {
        public int CodigoUsuario { get; set; }
        public int? CodigoTipoUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public string? ApellidoUsuario { get; set; }
        public string? PasswordUsuario { get; set; }
        public string? UsuarioBaseDatos { get; set; }
        public string? PasswordBaseDatos { get; set; }
        public string? Username { get; set; }
        public char? CodigoDespacho { get; set; }
        public int? CodigoUbicacionPresupuestal { get; set; }
        public string? Apodo { get; set; }
        public int? CodigoUbicacionPatrimonio { get; set; }
        public bool? Activo { get; set; }
        public int? CodigoUbicacionAnterior { get; set; }
        public int? CodigoCircunscripcion { get; set; }
        public string? CedulaIdentidad { get; set; }
    }
}
