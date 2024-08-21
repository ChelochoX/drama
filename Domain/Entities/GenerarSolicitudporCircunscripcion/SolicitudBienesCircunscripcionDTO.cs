namespace Domain.Entities.GenerarSolicitudporCircunscripcion
{

    public class SolicitudBienesCircunscripcionDTO
    {
        public int CodigoSolicitud { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int Ejercicio { get; set; }
        public int CodigoMateria { get; set; }
        public int CodigoCentroResponsabilidad { get; set; }
        public int CodigoCircunscripcion { get; set; }
        public string UsuarioInserto { get; set; }
        public string descripcion { get; set; }
        public string DescripcionMateria { get; set; }
        public string NombreCircunscripcion { get; set; }
        public string DescrípcionCentroResponsabilidad { get; set; }
        public string ApellidoUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public int NumeroSolicitud { get; set; }
        public int Estado { get; set; }
        public int Poi { get; set; }
    }

}
