using Atlas.Roleplay.Library.Events;
using Atlas.Roleplay.Library.Models;
using Atlas.Roleplay.Server.Events;
using Atlas.Roleplay.Server.MySQL;
using System.Collections.Generic;

namespace Atlas.Roleplay.Server.Managers
{
    public class JobManager : Manager<JobManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("job:employees:fetch", new EventCallback(metadata =>
            {
                var job = metadata.Find<int>(0);
                var employees = new List<Employee>();

                using (var context = new StorageContext())
                {
                    foreach (var character in context.Characters)
                    {
                        if ((int)character.Metadata.Employment != job) continue;

                        employees.Add(new Employee
                        {
                            Seed = character.Seed,
                            Name = character.Fullname,
                            Role = new object[] { character.Metadata.EmploymentRole },
                            Salary = 0,
                            MonthlyRevenue = 0,
                            TotalRevenue = 0
                        });
                    }
                }

                return employees;
            }));
        }
    }
}