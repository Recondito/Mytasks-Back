using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using AutoMapper;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly TasksContext context;
        private readonly IMapper mapper;

        public ApplicationController(TasksContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet("tasks")]
        public async Task<ActionResult<List<TaskDtoToReturn>>> GetTasks()
        {
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userid")?.Value;

            if (string.IsNullOrEmpty(userId)) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid token" });

            List<TaskC> tasks = await context.Tasks.Where(t => t.UserId == userId).ToListAsync();
            if(tasks == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to get tasks" });

            return mapper.Map<List<TaskDtoToReturn>>(tasks);
        }

        [HttpGet("tags")]
        public async Task<ActionResult<List<TagDtoToReturn>>> GetTags()
        {
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userid")?.Value;

            if (string.IsNullOrEmpty(userId)) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid token" });

            List<Tag> tags = await context.Tags.Where(t => t.UserId == userId).ToListAsync();
            if (tags == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to get tasks" });

            return mapper.Map<List<TagDtoToReturn>>(tags);
        }

        [HttpPost("task")]
        public async Task<ActionResult<TaskDtoToReturn>> AddTask(TaskDtoForCreate taskDto)
        {
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userid")?.Value;

            if (string.IsNullOrEmpty(userId)) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid token" });


            var task = mapper.Map<TaskC>(taskDto);
            await context.Tasks.AddAsync(task);
            task.UserId = userId;

            
            var taskTags = new List<TaskTag>();

            foreach (var tagId in taskDto.TagIds)
            {
                var tag = await context.Tags.FindAsync(tagId);
                if (tag == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to find tag" });
                if (tag.UserId != userId) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Unauthorized tag id" });

                var taskTag = new TaskTag()
                {
                    Tag = tag,
                    Task = task
                };

                task.TaskTags.Add(taskTag);
            }
            if (await context.SaveChangesAsync() == 0) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to create task" });

            return mapper.Map<TaskDtoToReturn>(task);
        }

        [HttpPost("tag")]
        public async Task<ActionResult<TagDtoToReturn>> AddTag(TagDtoForCreate tagDto)
        {
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userid")?.Value;

            if (string.IsNullOrEmpty(userId)) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid token" });


            var tag = mapper.Map<Tag>(tagDto);
            await context.Tags.AddAsync(tag);
            tag.UserId = userId;
            
            if (await context.SaveChangesAsync() == 0) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to create tag" });

            return mapper.Map<TagDtoToReturn>(tag);
        }

    }
}
