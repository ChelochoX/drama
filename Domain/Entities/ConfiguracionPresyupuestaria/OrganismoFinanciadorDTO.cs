using FluentValidation;

namespace Domain.Entities.ConfiguracionPresyupuestaria;

public class OrganismoFinanciadorDTO
{
    public int? CodigoOrganismoFinanciador { get; set; }
    public string? NumeroOrganismoFinanciador { get; set; }
    public string? DescripcionOrganismoFinanciador { get; set; }

}