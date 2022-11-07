using AutoMapper;
using Entities.DataTransferObjects;
using Entities.Models;

namespace WebApi
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Company, CompanyDTO>()
                .ForMember(c => c.FullAddress, opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));

            CreateMap<Employee, EmployeeDTO>();

            CreateMap<CompanyForCreationDto, Company>()
                .ForMember(c => c.Employee, opt => opt.MapFrom(x => x.Employees));

            CreateMap<EmployeeForCreationDTO, Employee>();

            CreateMap<CompanyForUpdateDTO, Company>()
                .ForMember(c => c.Employee, opt => opt.MapFrom(x => x.Employees))
                .ReverseMap();

            CreateMap<EmployeeForUpdateDTO, Employee>().ReverseMap();
        }
    }
}
