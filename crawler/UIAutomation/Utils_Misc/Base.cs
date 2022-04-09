using System.Diagnostics;
using UIAutomation.Utils_Misc;

namespace UIAutomation.Pages
{
    public class Base
    {
        protected static log4net.ILog log => log4net.LogManager.GetLogger(new StackTrace().GetFrame(1).GetMethod().DeclaringType);

        protected static void LogStart()
        {
            var child = new StackTrace().GetFrame(1).GetMethod();
            log4net.LogManager.GetLogger(child.DeclaringType).Info($"{child.Name} started");
        }

        protected static void LogEnd()
        {
            var child = new StackTrace().GetFrame(1).GetMethod();
            log4net.LogManager.GetLogger(child.DeclaringType).Info($"{child.Name} completed");
        }
    }
}