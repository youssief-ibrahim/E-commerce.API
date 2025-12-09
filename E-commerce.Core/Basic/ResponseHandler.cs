using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
namespace E_commerce.Core.Basic
{
    public class ResponseHandler
    {
        private readonly IStringLocalizer<ResponseHandler> localizer;
        public ResponseHandler(IStringLocalizer<ResponseHandler> localizer)
        {
            this.localizer= localizer;
        }
        public Response<T> Deleted<T>(T eentity, object Meeta = null)
        {
            return new Response<T>()
            {
                Data = eentity,
                StatusCode = System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = localizer["delete"],
                Meta = Meeta
            };
        }
        public Response<T> Success<T>(T entity, object Meta = null)
        {
            return new Response<T>()
            {
                Data = entity,
                StatusCode = System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = localizer["success"],
                Meta = Meta
            };
        }
        //public Response<List<T>> Success<T>(PagedList<T> pagedData)
        //{
        //    return new Response<List<T>>()
        //    {
        //        Data = pagedData, // <-- الـ PagedList هي نفسها List
        //        StatusCode = System.Net.HttpStatusCode.OK,
        //        Succeeded = true,
        //        Message = localizer["success"],
        //        Meta = pagedData.MetaData // <-- هذا هو المفتاح!
        //    };
        //}

        public Response<T> Unauthorized<T>()
        {
            return new Response<T>()
            {
                StatusCode = System.Net.HttpStatusCode.Unauthorized,
                Succeeded = true,
                Message = localizer["unAuthorized"]
            };
        }
        public Response<T> BadRequestt<T>(string Message = null)
        {
            return new Response<T>()
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Succeeded = false,
                Message = Message == null ? localizer["badRequest"] : Message
            };
        }

        public Response<T> NotFound<T>(string message = null)
        {
            return new Response<T>()
            {
                StatusCode = System.Net.HttpStatusCode.NotFound,
                Succeeded = false,
                Message = message == null ? localizer["notFound"] : message
            };
        }

        public Response<T> Created<T>(T entity, object Meta = null)
        {
            return new Response<T>()
            {
                Data = entity,
                StatusCode = System.Net.HttpStatusCode.Created,
                Succeeded = true,
                Message = localizer["create"],
                Meta = Meta
            };
        }
        public Response<T> Updated<T>(T entityy, object Metaa = null)
        {
            return new Response<T>()
            {
                Data = entityy,
                StatusCode = System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = localizer["update"],
                Meta = Metaa
            };
        }
    }
}
