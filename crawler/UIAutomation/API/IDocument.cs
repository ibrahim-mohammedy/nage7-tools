using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAutomation.API
{
    public interface IDocument
    {
        string Id { get; set; }
    }

    public class Document : IDocument
    {
        public string Id { get; set; }
    }
}