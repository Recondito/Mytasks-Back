using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Data
{
    public class TasksContext : DbContext
    {
        public TasksContext(DbContextOptions<TasksContext> options) : base(options)
        {
        }

        public DbSet<TaskC> Tasks { get; set; }
        public DbSet<SubTask> SubTasks { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //************
            //Tasks configuration
            //************
            builder.Entity<TaskC>()
                .HasKey(t => t.Id);

            builder.Entity<TaskC>().Property(t => t.UserId)
                .IsRequired();

            builder.Entity<TaskC>().Property(t => t.Name)
                .IsRequired().HasMaxLength(100);

            builder.Entity<TaskC>().Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(200);

            builder.Entity<TaskC>().Property(t => t.StartDate)
                .IsRequired();

            builder.Entity<TaskC>().Property(t => t.DueDate)
                .IsRequired();

            builder.Entity<TaskC>().Property(t => t.Priority)
                .IsRequired();

            builder.Entity<TaskC>().Property(t => t.State)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Entity<TaskC>().Property(t => t.Visible)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Entity<TaskC>().Property(t => t.ElapsedTime)
                .IsRequired()
                .HasDefaultValue(0);

            //************
            //SubTasks configuration
            //************
            builder.Entity<SubTask>()
                .HasKey(s => s.Id);

            builder.Entity<SubTask>().Property(s => s.TaskId)
                .IsRequired();

            builder.Entity<SubTask>()
                .HasOne(s => s.Task)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(s => s.TaskId);

            builder.Entity<SubTask>().Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Entity<SubTask>().Property(s => s.Priority)
                .IsRequired();

            builder.Entity<SubTask>().Property(s => s.State)
                .IsRequired()
                .HasDefaultValue(0);

            //************
            //Tags configuration
            //************
            builder.Entity<Tag>()
                .HasKey(t => t.Id);

            builder.Entity<Tag>()
                .Property(t => t.Name)
                .IsRequired().HasMaxLength(100);

            builder.Entity<Tag>()
                .Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(200);

            //************
            //TaskTags configuration
            //************
            builder.Entity<TaskTag>()
                .HasOne(TsTg => TsTg.Tag)
                .WithMany(Ts => Ts.TaskTags)
                .HasForeignKey(TsTg => TsTg.TagId);

            builder.Entity<TaskTag>()
                .HasOne(TsTg => TsTg.Task)
                .WithMany(Tg => Tg.TaskTags)
                .HasForeignKey(TsTg => TsTg.TaskId);
        }

    }
}
