using FluentValidation;

namespace Domain.Entities.ConsolidarAnteproyectoModificarObjetoGasto;

public class BienesparaConsolidadoDTO
{
    public int CodigoCatalogo { get; set; }
    public string DescripcionCatalogo { get; set; }
    public long ValorUnitario { get; set; }
    public int CodigoUnidadMedida { get; set; }
    public string UnidadMedida { get; set; }
}

