using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Circunscripcion
    {
        public int CodigoCircunscripcion { get; set; }
        public string? NombreCircunscripcion { get; set; }
        public string? NumeroCircunscripcion { get; set; }
        public int? CodigoSubprograma { get; set; }
        public string? Servidor { get; set; }
        public string? BaseDatos { get; set; }
        public int? CodigoCircunscripcionJurisdiccional { get; set; }
    }
}
