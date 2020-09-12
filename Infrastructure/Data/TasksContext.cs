using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Data
{
    public class TasksContext : DbContext
    {
        public DbSet<TaskC> Tasks { get; set; }
        public DbSet<SubTask> SubTasks { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //************
            //Tasks configuration
            //************
            modelBuilder.Entity<TaskC>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<TaskC>().Property(t => t.UserId)
                .IsRequired();

            modelBuilder.Entity<TaskC>().Property(t => t.Name)
                .IsRequired().HasMaxLength(100);

            modelBuilder.Entity<TaskC>().Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<TaskC>().Property(t => t.StartDate)
                .IsRequired();

            modelBuilder.Entity<TaskC>().Property(t => t.DueDate)
                .IsRequired();

            modelBuilder.Entity<TaskC>().Property(t => t.Priority)
                .IsRequired();

            modelBuilder.Entity<TaskC>().Property(t => t.State)
                .IsRequired()
                .HasDefaultValue(0);

            modelBuilder.Entity<TaskC>().Property(t => t.Visible)
                .IsRequired()
                .HasDefaultValue(true);

            modelBuilder.Entity<TaskC>().Property(t => t.EllapsedTime)
                .IsRequired()
                .HasDefaultValue(0);

            //************
            //SubTasks configuration
            //************
            modelBuilder.Entity<SubTask>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<SubTask>().Property(s => s.TaskId)
                .IsRequired();

            modelBuilder.Entity<SubTask>()
                .HasOne(s => s.Task)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(s => s.TaskId);

            modelBuilder.Entity<SubTask>().Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<SubTask>().Property(s => s.Priority)
                .IsRequired();

            modelBuilder.Entity<SubTask>().Property(s => s.State)
                .IsRequired()
                .HasDefaultValue(0);

            //************
            //Tags configuration
            //************
            modelBuilder.Entity<Tag>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<TaskC>()
                .Property(t => t.Name)
                .IsRequired().HasMaxLength(100);

            modelBuilder.Entity<TaskC>()
                .Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(200);

            //************
            //TaskTags configuration
            //************
            modelBuilder.Entity<TaskTag>()
                .HasOne(TsTg => TsTg.Tag)
                .WithMany(Ts => Ts.TaskTags)
                .HasForeignKey(TsTg => TsTg.TagId);

            modelBuilder.Entity<TaskTag>()
                .HasOne(TsTg => TsTg.Task)
                .WithMany(Tg => Tg.TaskTags)
                .HasForeignKey(TsTg => TsTg.TaskId);
        }

    }
}
