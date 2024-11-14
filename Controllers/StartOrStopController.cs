using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StartOrStopController : ControllerBase
    {

        private readonly ILogger<StartOrStopController> _logger;

        public StartOrStopController(ILogger<StartOrStopController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("Start")]
        public JsonResult Start()
        {
            return Success(StartWebsite("Operator_Management_Platform_api").Item2);
        }

        [HttpGet]
        [Route("Stop")]
        public JsonResult Stop()
        {
            return Success(StopWebsite("Operator_Management_Platform_api").Item2);
        }

        [HttpPost]
        [Route("UpdateApi")]
        public IActionResult UpdateApi(IFormFile[] files)
        {
            if (files == null || files.Length == 0)
            {
                return BadRequest("No files received from the upload.");
            }
            var stopResult = StopWebsite("Operator_Management_Platform_api");
            if (!stopResult.Item1)
                return Success(stopResult.Item2);
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var filePath = Path.Combine("E:\\CXB_Operator_Management_Platform\\API\\bin\\Debug\\net6.0", file.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                }
            }
            var startResult = StartWebsite("Operator_Management_Platform_api");
            if (!startResult.Item1)
                return Success(startResult.Item2);
            return Success("OK");
        }

        /// <summary>
        /// 返回成功结果
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected JsonResult Success(string msg, object? data = null)
        {
            if (string.IsNullOrWhiteSpace(msg))
                msg = "操作成功";
            if (data == null)
                return new JsonResult(new { Code = 0, Msg = msg });
            else
                return new JsonResult(new { code = 0, Msg = msg, Data = data });
        }

        private (bool, string) StartWebsite(string siteName)
        {
            try
            {
                using (ServerManager serverManager = new ServerManager())
                {
                    Site site = serverManager.Sites[siteName];
                    if (site != null)
                    {
                        site.Start();
                        string appPoolName = site.Applications["/"].ApplicationPoolName;
                        ApplicationPool appPool = serverManager.ApplicationPools[appPoolName];
                        if (appPool != null)
                        {
                            appPool.Start();
                        }
                        serverManager.CommitChanges();
                        return (true, $"Start {siteName} Successfully");
                    }
                    else
                    {
                        return (false, $"Not Found {siteName}");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        private (bool, string) StopWebsite(string siteName)
        {
            try
            {
                using (ServerManager serverManager = new ServerManager())
                {
                    Site site = serverManager.Sites[siteName];
                    if (site != null)
                    {
                        site.Stop();
                        string appPoolName = site.Applications["/"].ApplicationPoolName;
                        ApplicationPool appPool = serverManager.ApplicationPools[appPoolName];
                        if (appPool != null)
                        {
                            appPool.Stop();
                        }
                        serverManager.CommitChanges();
                        return (true, $"Stop {siteName} Successfully");
                    }
                    else
                    {
                        return (false, $"Not Found {siteName}");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
