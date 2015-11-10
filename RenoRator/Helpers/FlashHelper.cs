using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace System.Web.Mvc
{
    public enum FlashEnum
    {
        Success = 1,
        Info = 2,
        Warning = 3,
        Error = 4
    }
    public static class FlashHelper
    {
        public static void Flash(this Controller controller, string message,
            FlashEnum type = FlashEnum.Success)
        {
            controller.TempData[string.Format("flash-{0}",
                type.ToString().ToLower())] = message;
        }
    }
}