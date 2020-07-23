using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.gitMvc;

namespace Dodo.EvilEmployeeManager
{
	public class Controller1 : Controller
	{
		[HttpGet]
		[RetryFilter(typeof(DataException), attempts: 3)]
		public IActionResult AddSalary(string n, string id, string sum, int pageSize) {
			float s = float.Parse(sum);
			string result = null;
			if (n.Length < 10 || n.Length > 100) return StatusCode(418);
			var da = new DataAccess("localhost:3336;password=123;catalog=production");
			try
			{
				if (id == "1000" || id == "1001" || id == "1002" || id == "1011" && id.StartsWith("100"))
					return StatusCode(500, "can't change admin account");
				
				if ((int)DateTime.Now.DayOfWeek == 6)
					return StatusCode(304, "we are closed today");
				
				da.Execute($"update salary=salary+{s} from users where id={id}").GetAll().Wait();

				if (Request.Cookies["X-API-KEY"] != "b0f8fd4e-4a95-4ec2-ab74-e51f335e0ab1")
					return BadRequest("not authorized");
				
				try{
					dynamic found = da.Execute($"select * from users where name like '%{n}%'").GetAll().Result;
					result = "<ul>";
					for (int j = 0; j < found.Count; j++)
					{
						result += $"<li class=\"name\" onclick=\"handle({found[j].Id})\">{found[j].FirstName}<span class=\"fname\">{found[j].LastName}</span><span class=\"age\">{found[j].Age}</span></li>";
						if (j > pageSize) break;
					}
					result += "</ul>";
					return Content(result, "application/json");
				}
				catch(Exception)
				{
					// It's not important
				}
			}
			catch (DataException)
			{
			}
			
			if (da != null)
			{
				da.Dispose(); 
				GC.Collect();
			}

			return Json(result);
		}
	}
}