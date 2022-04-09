using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIAutomation.API;

namespace UIAutomation.Tests.UFC
{
    class Prepaid
    {
        static public Credentials AccountManager { get; } = new Credentials(true, "ufc-qa-pp", "Lootmo@Zipper", "qa pp manager", "Account Manager", true, true, "+972(3)5480342");
        static public Credentials RegularUser { get; } = new Credentials(false, "ufc-qa-pp.reg", "Testing", "qa pp regular", "Regular User", false, false, "");
        static public Credentials PCIUser { get; } = new Credentials(false, "ufc-qa-pp-pci", "Lootmo@Zipper1", "qa pp pci", "Regular User", true, true, "+1 (855) 929-7766");
    }

    class Credit
    {
        static public Credentials AccountManager { get; } = new Credentials(true, "ufc-qa-cr", "Lootmo@Zipper", "qa cr manager", "Account Manager", true, true, "+34-94-107-0002");
        static public Credentials RegularUser { get; } = new Credentials(false, "ufc-qa-cr.reg", "Testing", "qa cr regular", "Regular User", false, false, "");
        static public Credentials PCIUser { get; } = new Credentials(false, "ufc-qa-cr-pci", "Lootmo@Zipper1", "qa cr pci", "Regular User", true, true, "+3905881870002");
    }

    public class Test : UIAutomation.Tests.Test
    {
    }
}