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

            List<TaskC> tasks = await context.Tasks.Include("TaskTags.Tag").Include(t => t.SubTasks).Where(t => t.UserId == userId).ToListAsync();
            if (tasks == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to get tasks" });

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


            task.TaskTags = new List<TaskTag>();
            if (taskDto.TagIds != null)
            {
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

        [HttpPut("tag")]
        public async Task<ActionResult<TagDtoToReturn>> UpdateTag(TagDtoToReturn tagDto)
        {
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userid")?.Value;

            if (string.IsNullOrEmpty(userId)) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid token" });

            var tag = await context.Tags.FindAsync(tagDto.Id);
            if (tag == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to find tag" });
            if (tag.UserId != userId) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Unathorized tag id" });

            mapper.Map<TagDtoToReturn, Tag>(tagDto, tag);

            if (await context.SaveChangesAsync() == 0) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to update tag" });

            return mapper.Map<TagDtoToReturn>(tag);
        }

        [HttpPut("task")]
        public async Task<ActionResult<TaskDtoToReturn>> UpdateTask(TaskDtoToUpdate taskDto)
        {
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userid")?.Value;

            if (string.IsNullOrEmpty(userId)) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid token" });

            var task = await context.Tasks.Include("TaskTags.Tag").Include(t => t.SubTasks).SingleOrDefaultAsync(t => t.Id == taskDto.Id);
            if (task == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to find task" });
            if (task.UserId != userId) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Unathorized task id" });

            //update single value properties
            mapper.Map<TaskDtoToUpdate, TaskC>(taskDto, task);

            //remove the deleted subtasks
            foreach (var subtask in task.SubTasks)
            {
                if (taskDto.SubTasks.Find(s => s.Id == subtask.Id) == null)
                {
                    context.SubTasks.Remove(subtask);
                }
            }


            foreach (var subtaskDto in taskDto.SubTasks)
            {
                var subtask = task.SubTasks.Find(s => s.Id == subtaskDto.Id); //compare subtasks from DTO with subtasks from DB

                if (subtaskDto.Id <= 0) //subtasks from DTO with id 0 or less are treated as new subtasks
                {
                    var newSubTask = mapper.Map<SubTask>(mapper.Map<SubTaskDtoForCreate>(subtaskDto));
                    task.SubTasks.Add(newSubTask);
                }
                else if (subtask != null) //if the substask id is a match, update the properties
                {
                    mapper.Map<SubTaskDtoToReturn, SubTask>(subtaskDto, subtask);
                }
                else // if the id from the DTO subtask does not match any task from the DB, send error
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid subtask id" });
                }
            }

            //remove the deleted task-tag relationships
            var tasktagsToDelete = new List<TaskTag>();
            foreach (var taskTag in task.TaskTags)
            {
                if (taskDto.Tags.Find(t => t.Id == taskTag.TagId) == null)
                {
                    tasktagsToDelete.Add(taskTag);
                }
            }
            foreach (var taskTag in tasktagsToDelete)
            {
                task.TaskTags.Remove(taskTag);
            }

            foreach (var tagDto in taskDto.Tags)
            {
                var subtask = task.TaskTags.Find(s => s.TagId == tagDto.Id); //compare subtasks from DTO with subtasks from DB

                if (subtask == null) //if the substask id is a match, update the properties
                {
                    var tag = await context.Tags.FindAsync(tagDto.Id);
                    if (tag == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to find tag" });
                    if (tag.UserId != userId) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Unathorized tag id" });

                    var taskTag = new TaskTag()
                    {
                        Tag = tag,
                        Task = task
                    };
                    task.TaskTags.Add(taskTag);
                }


            }


            if (await context.SaveChangesAsync() == 0) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to update task" });

            return mapper.Map<TaskDtoToReturn>(task);
        }

        [HttpDelete("task/{id}")]
        public async Task<ActionResult> DeleteTask(int id)
        {
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userid")?.Value;

            if (string.IsNullOrEmpty(userId)) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid token" });

            var task = await context.Tasks.FindAsync(id);
            if (task == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to find task" });
            if (task.UserId != userId) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Unathorized task id" });

            context.Tasks.Remove(task);

            if (await context.SaveChangesAsync() == 0) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to update tag" });

            return Ok();
        }

        [HttpDelete("tag/{id}")]
        public async Task<ActionResult> DeleteTag(int id)
        {
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userid")?.Value;

            if (string.IsNullOrEmpty(userId)) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid token" });

            var tag = await context.Tags.FindAsync(id);
            if (tag == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to find tag" });
            if (tag.UserId != userId) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Unathorized tag id" });

            context.Tags.Remove(tag);

            if (await context.SaveChangesAsync() == 0) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to update tag" });

            return Ok();
        }
    }
}
