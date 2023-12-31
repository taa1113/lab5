﻿using WebApplication1.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Middleware
{
    public class DbInitializerMiddleware
    {
        private readonly RequestDelegate _next;
        public DbInitializerMiddleware(RequestDelegate next)
        {
            // инициализация базы данных 
            _next = next;
        }
        public Task Invoke(HttpContext context)
        {

                DbUserInitializer.Initialize(context).Wait();
                context.Session.SetString("starting", "Yes");


            // Вызов следующего делегата / компонента middleware в конвейере
            return _next.Invoke(context);
        }
    }

}
