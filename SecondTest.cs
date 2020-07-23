using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.gitMvc;

namespace Dodo.EvilEmployeeManager
{
	public class Controller1 : Controller
	{
		private readonly string authKey = "b0f8fd4e-4a95-4ec2-ab74-e51f335e0ab1";
		private readonly string connString = "localhost:3336;password=123;catalog=production";
		private readonly List<string> adminCodes = new List<string>()
		{
			"1000",
			"1001",
			"1002",
			"1011"
		};

		[HttpGet]
		[RetryFilter(typeof(DataException), attempts: 3)]
		public IActionResult AddSalary(string name, string id, string sum, int pageSize) {
			float s = 0F;
			if (!float.TryParse(sum, out s)) return StatusCode(400, "sum should be float");

			if (name.Length < 10 || name.Length > 100) return StatusCode(418);

			string result = null;

			try
			{
				if (Request.Cookies["X-API-KEY"] != authKey)
					return BadRequest("not authorized");

				if (isAdminAccount(id))
					return StatusCode(500, "can't change admin account");

				if (!isWorkDay())
					return StatusCode(304, "we are closed today");


				try
				{
					using (var da = new DataAccess(connString))
					{

						da.Execute($"update salary=salary+{s} from users where id={id}").GetAll().Wait();

						dynamic users = await da.Execute($"select * from users where name like '%{name}%'").GetAll();
						
						result = "<ul>";

						for (int j = 0; j < users.Count; j++)
						{
							if (j > pageSize) break;
							result += $"<li class=\"name\" onclick=\"handle({users[j].Id})\">{users[j].FirstName}<span class=\"fname\">{users[j].LastName}</span><span class=\"age\">{users[j].Age}</span></li>";
						}
						result += "</ul>";
						return Content(result, "application/json");
					}

				}
				catch (Exception)
				{
					// It's not important
				}
			}
			catch (DataException)
			{
			}


			return Json(result);
		}


		#region --------------- internal methods
        	private bool isAdminAccount(string Id) => adminCodes.Contains(Id) && Id.StartsWith("100");
			private bool isWorkDay() => DateTime.Now.DayOfWeek != DayOfWeek.Saturday;

		#endregion

	}
}