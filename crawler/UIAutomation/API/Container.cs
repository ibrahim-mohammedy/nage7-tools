using System;
using System.Collections.Generic;
using System.Linq;

namespace UIAutomation.API
{
    public class IDeleteIfExists
    {
        public virtual void DeleteIfExists(Func<Document, bool> func)
        {
            throw new NotImplementedException();
        }
    }

    public class Container<T> : IDeleteIfExists where T : IDocument
    {
        protected RESTAPI Api { get; }
        protected string Objects { get; }

        public Container(RESTAPI api, string objects)
        {
            Objects = objects;
            Api = api;
        }

        public virtual List<T> Enumerate()
        {
            return Api.Enumerate<T>(Objects);
        }

        public void Delete(string id)
        {
            Api.Delete(Objects, id);
        }

        public void Delete(IDocument document)
        {
            Api.Delete(Objects, document.Id);
        }

        public override void DeleteIfExists(Func<Document, bool> func)
        {
            IDocument found = Api.Enumerate<Document>(Objects).FirstOrDefault(func);
            if (found == null) return;

            Delete(found);
        }

        public void DeleteIfExists(Func<T, bool> func)
        {
            IDocument found = Find(func);
            if (found == null) return;

            Delete(found);
        }

        public T Find(Func<T, bool> func)
        {
            return Enumerate().FirstOrDefault(func);
        }

        public ApiResult<T> Create(object o)
        {
            return Api.Put<T>(Objects, o);
        }

        public ApiResult<T> Update(IDocument document)
        {
            return Api.Post<T>(Objects, document);
        }

        public ApiResult<T> Update(string id, object o)
        {
            return Api.Post<T>(Objects, id, o);
        }
    }
}