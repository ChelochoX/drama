using Application.Services;
using AutoMapper;
using Domain.Entities.GenerarSolicitudporCircunscripcion;
using Domain.Entities.Request;

namespace Application.Mappings
{
    public class GeneralAutomapping : Profile
    {
        public GeneralAutomapping()
        {
            CreateMap<SolicitudObjetosBienesService, SolicitudObjetoBienesDetalleDTO>().ReverseMap();  

            CreateMap<ModificarSolicitudRequest, ModificarSolicitudV2Request>()
                .ForMember(dest => dest.UsuarioModificacion, opt => opt.Ignore());

        }
    }
}
