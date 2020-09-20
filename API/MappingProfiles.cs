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

            CreateMap<TaskC, TaskDtoToReturn>()
                .ForMember(t => t.Tags, o => o.MapFrom(s => s.TaskTags.Select(t => t.Tag).ToList()));

            CreateMap<SubTask, SubTaskDtoToReturn>();

            CreateMap<Tag, TagDtoToReturn>();

            CreateMap<TagDtoForCreate, Tag>();

            CreateMap<TagDtoToReturn, Tag>()
                .ForMember(t => t.Id, o => o.Ignore()); ;

            CreateMap<TaskDtoToUpdate, TaskC>()
                .ForMember(t => t.TaskTags, o => o.Ignore())
                .ForMember(t => t.SubTasks, o => o.Ignore());

            CreateMap<SubTaskDtoToReturn, SubTask>()
                .ForMember(s => s.Id, o => o.Ignore());

            CreateMap<SubTaskDtoToReturn, SubTaskDtoForCreate>();
        }
    }
}
