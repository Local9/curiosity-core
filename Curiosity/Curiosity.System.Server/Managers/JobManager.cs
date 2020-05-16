using Curiosity.System.Library.Events;
using Curiosity.System.Library.Models;
using Curiosity.System.Server.Events;
using Curiosity.System.Server.MySQL;
using System.Collections.Generic;

namespace Curiosity.System.Server.Managers
{
    public class JobManager : Manager<JobManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("job:employees:fetch", new EventCallback(metadata =>
            {
                var job = metadata.Find<string>(0);
                var employees = new List<Employee>();

                using (var context = new StorageContext())
                {
                    foreach (var character in context.Characters)
                    {
                        if ((string)character.Metadata.Employment != job) continue;

                        employees.Add(new Employee
                        {
                            CharacterId = character.CharacterId,
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