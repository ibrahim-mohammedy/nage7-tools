using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAutomation.API
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }

        public List<string> Errors { get; set; }

        public T Object { get; set; }

        public static ApiResult<T> Failed(List<string> errors, T o)
        {
            return new ApiResult<T> { Success = false, Errors = errors, Object = o };
        }

        public static ApiResult<T> Succeeded(T o)
        {
            return new ApiResult<T> { Success = true, Errors = new List<string>(), Object = o };
        }
    }
}