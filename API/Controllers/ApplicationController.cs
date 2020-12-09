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
    //M
    public class ApplicationController : ControllerBase
    {
        //N1
        //M1
        private readonly TasksContext context;
        private readonly IMapper mapper;
        
        //M2
        public ApplicationController(TasksContext context, IMapper mapper)
        {
        //N2
            //M21
            this.context = context;
            this.mapper = mapper;
        }
       
        //M3
        [HttpGet("tasks")]
        public async Task<ActionResult<List<TaskDtoToReturn>>> GetTasks()
        {
            //M31
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userid")?.Value;
            
            //M32 M321
            if (string.IsNullOrEmpty(userId)) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid token" });
            
            //N6
            //M33
            List<TaskC> tasks = await context.Tasks.Include("TaskTags.Tag").Include(t => t.SubTasks).Where(t => t.UserId == userId).ToListAsync();
            //N7 N8
            //M34 M341
            if (tasks == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to get tasks" });
            //N9
            //M35
            return mapper.Map<List<TaskDtoToReturn>>(tasks);
        }

        //M4
        [HttpGet("tags")]
        public async Task<ActionResult<List<TagDtoToReturn>>> GetTags()
        {
            //M41
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userid")?.Value;

            //M42 M421
            if (string.IsNullOrEmpty(userId)) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid token" });
            
            //N13
            //M43
            List<Tag> tags = await context.Tags.Where(t => t.UserId == userId).ToListAsync();
            //N14 N15
            //M44 M441
            if (tags == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to get tasks" });
            //N16
            //M45
            return mapper.Map<List<TagDtoToReturn>>(tags);
        }
        
        //M5
        [HttpPost("task")]
        public async Task<ActionResult<TaskDtoToReturn>> AddTask(TaskDtoForCreate taskDto)
        {
            //M51
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userid")?.Value;
            //M52 M521
            if (string.IsNullOrEmpty(userId)) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid token" });

            //M53
            var task = mapper.Map<TaskC>(taskDto);
            await context.Tasks.AddAsync(task);
            task.UserId = userId;
            task.TaskTags = new List<TaskTag>();
            
            //M54
            if (taskDto.TagIds != null)
            {
                //M541
                foreach (var tagId in taskDto.TagIds)
                {
                    //M5411
                    var tag = await context.Tags.FindAsync(tagId);
                    //M5412 M54121
                    if (tag == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to find tag" });
                    //M5413 M54131
                    if (tag.UserId != userId) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Unauthorized tag id" });
                    
                    //M5414
                    var taskTag = new TaskTag()
                    {
                        //M54141
                        Tag = tag,
                        Task = task
                    };
                    //
                    task.TaskTags.Add(taskTag);
                }
            }
            
            //M55 M551
            if (await context.SaveChangesAsync() == 0) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to create task" });
            
            //M56
            return mapper.Map<TaskDtoToReturn>(task);
        }
        
        //M6
        [HttpPost("tag")]
        public async Task<ActionResult<TagDtoToReturn>> AddTag(TagDtoForCreate tagDto)
        {
            //M61
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userid")?.Value;
            
            //M62 M621
            if (string.IsNullOrEmpty(userId)) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid token" });

            //M63
            var tag = mapper.Map<Tag>(tagDto);
            await context.Tags.AddAsync(tag);
            tag.UserId = userId;

            //M64 M641
            if (await context.SaveChangesAsync() == 0) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to create tag" });
            
            //M65
            return mapper.Map<TagDtoToReturn>(tag);
        }
        
        //M7
        [HttpPut("tag")]
        public async Task<ActionResult<TagDtoToReturn>> UpdateTag(TagDtoToReturn tagDto)
        {
            //M71
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userid")?.Value;

            //M72 M721
            if (string.IsNullOrEmpty(userId)) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid token" });
            
            //M73
            var tag = await context.Tags.FindAsync(tagDto.Id);
            //M74 M741
            if (tag == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to find tag" });
            //M75 M751
            if (tag.UserId != userId) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Unathorized tag id" });
            
            //M76
            mapper.Map<TagDtoToReturn, Tag>(tagDto, tag);
            
            //M77 M771
            if (await context.SaveChangesAsync() == 0) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to update tag" });
            //M78
            return mapper.Map<TagDtoToReturn>(tag);
        }
        
        //M8
        [HttpPut("task")]
        public async Task<ActionResult<TaskDtoToReturn>> UpdateTask(TaskDtoToUpdate taskDto)
        {
            //M81
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userid")?.Value;
            //M82 M821
            if (string.IsNullOrEmpty(userId)) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid token" });
            
            //M83
            var task = await context.Tasks.Include("TaskTags.Tag").Include(t => t.SubTasks).SingleOrDefaultAsync(t => t.Id == taskDto.Id);
            //M84 M841
            if (task == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to find task" });
            //M85 M851
            if (task.UserId != userId) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Unathorized task id" });
            
            //M86
            //update single value properties
            mapper.Map<TaskDtoToUpdate, TaskC>(taskDto, task);

            //remove the deleted subtasks
            //M87
            foreach (var subtask in task.SubTasks)
            {
                //M871
                if (taskDto.SubTasks.Find(s => s.Id == subtask.Id) == null)
                {
                    //M8711
                    context.SubTasks.Remove(subtask);
                }
            }

            //M88
            foreach (var subtaskDto in taskDto.SubTasks)
            {
                //M881
                var subtask = task.SubTasks.Find(s => s.Id == subtaskDto.Id); //compare subtasks from DTO with subtasks from DB
                
                //M882
                if (subtaskDto.Id <= 0) //subtasks from DTO with id 0 or less are treated as new subtasks
                {
                    //M8821
                    var newSubTask = mapper.Map<SubTask>(mapper.Map<SubTaskDtoForCreate>(subtaskDto));
                    task.SubTasks.Add(newSubTask);
                }
                //M883
                else if (subtask != null) //if the substask id is a match, update the properties
                {
                    //M8831
                    mapper.Map<SubTaskDtoToReturn, SubTask>(subtaskDto, subtask);
                }
                //M884
                else // if the id from the DTO subtask does not match any task from the DB, send error
                {
                    //M8841
                    return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid subtask id" });
                }
            }

            //remove the deleted task-tag relationships
            //M89
            var tasktagsToDelete = new List<TaskTag>();
            //M810
            foreach (var taskTag in task.TaskTags)
            {
                //M8101
                if (taskDto.Tags.Find(t => t.Id == taskTag.TagId) == null)
                {
                    //M81011
                    tasktagsToDelete.Add(taskTag);
                }
            }
            //M820
            foreach (var taskTag in tasktagsToDelete)
            {
                //M8201
                task.TaskTags.Remove(taskTag);
            }
            //M830
            foreach (var tagDto in taskDto.Tags)
            {   
                //M8301
                var subtask = task.TaskTags.Find(s => s.TagId == tagDto.Id); //compare subtasks from DTO with subtasks from DB
                
                //M8302
                if (subtask == null) //if the substask id is a match, update the properties
                {
                    //M83021
                    var tag = await context.Tags.FindAsync(tagDto.Id);
                    //M83022 M830221
                    if (tag == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to find tag" });
                    //M83023 M830231
                    if (tag.UserId != userId) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Unathorized tag id" });
                    //M83024
                    var taskTag = new TaskTag()
                    {
                        //M830241
                        Tag = tag,
                        Task = task
                    };
                    //M83025
                    task.TaskTags.Add(taskTag);
                }


            }

            //M840
            if (await context.SaveChangesAsync() == 0) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to update task" });
            //M850
            return mapper.Map<TaskDtoToReturn>(task);
        }
        //M9
        [HttpDelete("task/{id}")]
        public async Task<ActionResult> DeleteTask(int id)
        {
            //M91
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userid")?.Value;
            //M92 M921
            if (string.IsNullOrEmpty(userId)) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid token" });
            //M93
            var task = await context.Tasks.FindAsync(id);
            //M94 M941
            if (task == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to find task" });
            //M95 M951
            if (task.UserId != userId) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Unathorized task id" });
            //M96
            context.Tasks.Remove(task);
            //M97 M971
            if (await context.SaveChangesAsync() == 0) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to update tag" });
            //M98
            return Ok();
        }
        //M10
        [HttpDelete("tag/{id}")]
        public async Task<ActionResult> DeleteTag(int id)
        {
            //M101
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userid")?.Value;
            
            //M102 M1021
            if (string.IsNullOrEmpty(userId)) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid token" });
            //M103
            var tag = await context.Tags.FindAsync(id);
            //N104 N1041
            if (tag == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to find tag" });
            //N105 1051
            if (tag.UserId != userId) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Unathorized tag id" });
            //M106
            context.Tags.Remove(tag);
            //M107 M1071
            if (await context.SaveChangesAsync() == 0) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to update tag" });
            //M108
            return Ok();
        }
    }
}
