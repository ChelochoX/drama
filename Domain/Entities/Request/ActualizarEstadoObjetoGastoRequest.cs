namespace Domain.Entities.Requestñ
{
    public class ActualizarEstadoObjetoGastoRequest
    {
        public int CodigoSolicitud { get; set; }
        public int[] CodigosObjetoGasto { get; set; }
        public int Estado { get; set; }
    }
}
