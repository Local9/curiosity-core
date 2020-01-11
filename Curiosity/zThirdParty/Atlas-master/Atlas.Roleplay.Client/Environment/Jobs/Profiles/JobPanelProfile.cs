using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Atlas.Roleplay.Client.Events;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Atlas.Roleplay.Client.Environment.Jobs.Profiles
{
    public class JobPanelProfile : JobProfile
    {
        public override JobProfile[] Dependencies { get; set; } = {new JobBusinessProfile()};
        public Position Position { get; set; }

        public override async void Begin(Job job)
        {
            await Session.Loading();
            
            Business business;

            while ((business = job.GetProfile<JobBusinessProfile>().Business) == null)
            {
                await BaseScript.Delay(100);
            }

            var character = Cache.Character;
            var roles = job.Roles.Select(role => role.Key).ToArray();

            Array.Sort(roles, (x, y) => y.CompareTo(x));

            var marker = new Marker(Position)
            {
                Message = "Tryck ~INPUT_CONTEXT~ för att komma åt jobbpanelen",
                Color = Color.Transparent,
                Condition = self =>
                    character.Metadata.Employment == job.Attachment &&
                    character.Metadata.EmploymentRole == roles.First()
            };

            marker.Callback += async () =>
            {
                var employees = await EventSystem.GetModule()
                    .Request<List<Employee>>("job:employees:fetch", (int) Job.Attachment);

                foreach (var employee in employees)
                {
                    try
                    {
                        employee.Role = new[] {employee.Role[0], job.Roles[int.Parse(employee.Role[0].ToString())]};
                    }
                    catch (KeyNotFoundException)
                    {
                        // Employee has unknown grade
                    }
                }

                var _roles = new List<object[]>();

                foreach (var role in job.Roles)
                {
                    _roles.Add(new object[] {role.Key, role.Value});
                }

                API.SendNuiMessage(new JsonBuilder()
                    .Add("Operation", "OPEN_PANEL")
                    .Add("Job", new Dictionary<string, object>
                    {
                        ["Label"] = job.Label,
                        ["Roles"] = _roles,
                        ["Employees"] = employees,
                        ["Attachment"] = job.Attachment.ToString()
                    })
                    .Add("Profile", new List<string>())
                    .Add("Statistics", new Dictionary<string, object>
                    {
                        ["Money"] = business.Balance,
                        ["RegisteredDays"] =
                            new TimeSpan(business.Registered - DateTime.Now.Ticks).Days,
                        ["BestEmployee"] = "."
                    })
                    .Build());

                API.SetNuiFocus(true,
                    true);
            };

            marker.Show();
        }
    }
}