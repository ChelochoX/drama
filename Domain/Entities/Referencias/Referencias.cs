
namespace Domain.Entities.Referencias
{
    public class Referencias
    {
        public int CodigoReferencia { get; set; }
        public string DominioReferencia { get; set; }
        public int CodigoTipoReferencia { get; set; }
        public string DescripcionReferencia { get; set; }
        public int Estado { get; set; }
        public DateTime? FechaInserto { get; set; }
        public int? UsuarioInserto { get; set; }
        public int? UsuarioModificacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public DateTime? ValorFecha { get; set; }
        public string? ValorAlfanumerico { get; set; }
        public string? DescripcionLarga { get; set; }
        public decimal? ValorDecimal { get; set; }
    }
}
