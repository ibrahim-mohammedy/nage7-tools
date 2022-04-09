using System;
using System.Collections.Generic;
using System.Linq;

namespace UIAutomation.API
{
    public class ContainerEx<T> : Container<T> where T : IDocument
    {
        public ContainerEx(RESTAPI api, string objects) : base(api, objects)
        {
        }

        public override List<T> Enumerate()
        {
            return Api.EnumerateEx<T>(Objects);
        }
    }
}