using API.DTOs;
using AutoMapper;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<TaskDtoForCreate, TaskC>();
            
            CreateMap<SubTaskDtoForCreate, SubTask>();

            CreateMap<TaskC, TaskDtoToReturn>();

            CreateMap<SubTask, SubTaskDtoToReturn>();

            CreateMap<Tag, TagDtoToReturn>();

            CreateMap<TagDtoForCreate, Tag>();
        }
    }
}
