using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;

namespace DatingApp.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container. DI here
        public void ConfigureServices(IServiceCollection services)
        {
            //
            //sql server db
            services.AddDbContext<DataContext>(x => x.UseSqlServer(this.Configuration.GetConnectionString("DefaultConnection")));
            //
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(opt => {
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                })
                ;
            //CORS policy
            services.AddCors();
            //AutoMapper
            services.AddAutoMapper();
            //Seed Services
            services.AddTransient<Seed>();
            //AuthRepository services, AddScoped means it will create a new instance per http request
            services.AddScoped<IAuthRepository, AuthRepository>(); //First type param is interface and second tells Asp.Net Core services which object to create
            //AdtingRepository services, to provide methods for CRUD
            services.AddScoped<IDatingRepository, DatingRepository>();
            //AuthenticationServices
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                   {
                       ValidateIssuerSigningKey = true,
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
                            .GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                       ValidateIssuer = false,
                       ValidateAudience = false
                       
                   };

               });
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Seed seeder)
        {
            //app.UseXXX = add middleware to pipeline
            /**
             * in a pipeline, a HttpRequest is received by one middleware and processed 
             * and then 
             *  Either return (generating httpResponse) directly 
             *  Or pass/delegate the HttpRequest to next middleware in the pipeline, that's why app.UseXXX has to setup with certain order. 
             *  it is pipe line order/HttpRequest processed order
             *  
             *  each app.UseXXX() method is extension method, all return same obj: app type of IApplicationBuilder
             *  this building process utilize the builder design pattern to construct a big and complicated obj 
             *  by keep returning itself
            */
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //Global Exception Handler
                app.UseExceptionHandler(
                    //app.UseExceptionHandler add pipeline, input Action<IApplicationBuilder> configureMethod, which is a delegate
                    //output IApplicationBuilder
                    //
                    builder =>
                    {
                        /*
                         * RequestDelegate handler = null;
                        handler = (async context => { await context.Response.WriteAsync(); });
                        builder.Run(handler);
                        */
                        //
                        //builder.Run() add a middleware to pipeline, now we add handler to pipeline
                        //handler is type of RequestDelegate, 
                        //Request Dategate is method signature/pattern, with input HttpContext, output Task. Task = async void. Task<T> = async T
                        //handler = async-ly write msg and status code to httpContext
                        //httpContext is what is being passed inside the pipeline

                        //Code commented same as the code below, app.Run appears means pipeline terminates                       
                        builder.Run(
                            async context =>
                            {
                                Console.WriteLine("app.Run() called, pipeline should terminate");
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                var error = context.Features.Get<IExceptionHandlerFeature>();

                                if (error != null)
                                {
                                    context.Response.AddApplicationError(error.Error.Message);
                                    await context.Response.WriteAsync(error.Error.Message);
                                }
                            }

                            );
                        //builder.Use(async (context, nextMiddleware) =>
                        //           {
                        //               Console.WriteLine("using app.Use instead of app.Run inside global exception handling");
                        //               context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        //               var error = context.Features.Get<IExceptionHandlerFeature>();

                        //               if (error != null)
                        //               {
                        //                   context.Response.AddApplicationError(error.Error.Message);
                        //                   //await context.Response.WriteAsync(error.Error.Message);
                        //               }
                        //               await nextMiddleware.Invoke();
                        //           });
                    });
                //app.UseHsts();
            }

            //Start : Code from start to end not called, another type of middleware not proper
            RequestDelegate handler = new RequestDelegate(async (context) => { Console.WriteLine("Another type of middleware used in app.Use"); });
            Func<RequestDelegate, RequestDelegate> middlewareAnotherType = new Func<RequestDelegate, RequestDelegate>(
                (requestDelegate) =>
                {
                    Console.WriteLine("Inside another type of app use request Delegate");
                    return requestDelegate;
                });
            app.Use(middlewareAnotherType);
            //End

            app.Use(async (context, next) =>
            {
                // Do work that doesn't write to the Response.
                await next.Invoke();
                // Do logging or other work that doesn't write to the Response.
            });
            //test app.Use

            //HttpContext hc = null;
            //async Task requestDelegateMethod(HttpContext hc_local) { return; }
            //RequestDelegate requestDelegate = new RequestDelegate(async (HttpContext hc) => { return; });
            //or RequestDelegate requestDelegate = new RequestDelegate(requestDelegateMethod);
            //HttpContext middleWare1Param1;
            //Func<Task> middleWare1Param2 = new Func<Task>(async () => { return; });
            //Task middleWare1Return = requestDelegateMethod(hc);
            //middleWare1 = new Func<HttpContext, Func<Task>, Task>( async (httpContext, aFunc) => 
            //{
            //    Console.WriteLine("Before next middleware. Request = " + httpContext.Request.ToString() + " Response = " + httpContext.Response.ToString());
            //    await aFunc();
            //    //httpContext.Response.AddApplicationError("TerminatePipeLine here");
            //    //return aFunc();
            //    Console.WriteLine("After next middleware Request = " + httpContext.Request.ToString() + " Response = " + httpContext.Response.ToString());

            //});
            Func<HttpContext, Func<Task>, Task> middleWare1 = new Func<HttpContext, Func<Task>, Task>(middleware1Method); //in HttpContext, Func<Task>, out Task
            Func<HttpContext, Func<Task>, Task> middleWare2 = new Func<HttpContext, Func<Task>, Task>(middleware2Method);
            //middleWare2 = new Func<HttpContext, Func<Task>, Task>(async (httpContext, aFunc) =>
            //{
            //    //Console.WriteLine("Before next middleware. Request = " + httpContext.Request.ToString() + " Response = " + httpContext.Response.ToString());
            //    //await aFunc.Invoke();
            //    httpContext.Response.AddApplicationError("TerminatePipeLine here");
            //    //return aFunc();
            //    //Console.WriteLine("After next middleware Request = " + httpContext.Request.ToString() + " Response = " + httpContext.Response.ToString());

            //});
            app.Use(middleWare1); //app.Use input a middleware, output new app itself
            //app.Run, handler means, asp.net core give input httpContext, handler/RequestDelegate process this httpContext, return httpContext back to previous middleware
            //app.Run(new RequestDelegate(async (context) => {
            //    Console.WriteLine("x app.Run before write response");
            //    //context.Response.AddApplicationError("App.Run inside requestDelegate");
            //    Console.WriteLine("x app.Run after write response");
            //}));
            app.Use(middleWare2);
            app.Use(async (context, nextMiddleware) =>
           {
               Console.WriteLine("3 Outter Before Next.Invoke()");
               //Start: this code below does not run. app.Use nesting not OK
               //app.Use(async (context4, nextMiddleware4) => 
               //{
               //    Console.WriteLine("4 Inner Before Next.Invoke()");
               //    await nextMiddleware4();
               //    Console.WriteLine("4 Inner After Next.Invoke()");
               //});
               //End
               await nextMiddleware.Invoke();
               Console.WriteLine("3 Outter After Next.Invoke()");
           });
            //test app.Run
            //app.UseHttpsRedirection();
            Action<Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicyBuilder> configCorsAction = new Action<Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicyBuilder>(
                x => 
                {
                    x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
                );
            //seeder.SeedUsers();
            app.UseCors(configCorsAction);

            app.UseAuthentication();

            app.UseMvc();
        }
        //private async Task fakeUse(Func<HttpContext,Func<Task>,Task> fakeMiddleware)
        //{
        //    HttpContext hc = null;
        //    Func<Task> nextFakeMiddleware = null;// = new Func<Task>(fakeMiddleware2Method);
        //    await fakeMiddleware.Invoke(hc,nextFakeMiddleware);
        //}
        //private async Task fakeMiddleware1Method(HttpContext httpContext,Func<Task> nextFakeMiddleware)
        //{
        //    await nextFakeMiddleware();
        //}
        //private async Task fakeMiddleware2Method(HttpContext httpContext, Func<Task> nextFakeMiddleware)
        //{
        //    await nextFakeMiddleware();
        //}
        private async Task middleware1Method(HttpContext httpContext,Func<Task> nextMiddlewareFuncDelegate)
        {
            Console.WriteLine("1 Before next middleware. Request = " + httpContext.Request.ToString() + " Response = " + httpContext.Response.ToString());
            await nextMiddlewareFuncDelegate.Invoke();
            //httpContext.Response.AddApplicationError("TerminatePipeLine here");
            //return aFunc();
            Console.WriteLine("1 After next middleware Request = " + httpContext.Request.ToString() + " Response = " + httpContext.Response.ToString());
        }
        private async Task middleware2Method(HttpContext httpContext, Func<Task> nextMiddlewareFuncDelegate)
        {
            Console.WriteLine("2 Before next middleware. Request = " + httpContext.Request.ToString() + " Response = " + httpContext.Response.ToString());
            await nextMiddlewareFuncDelegate();
            //httpContext.Response.AddApplicationError("TerminatePipeLine here");
            //return aFunc();
            Console.WriteLine("2 After next middleware Request = " + httpContext.Request.ToString() + " Response = " + httpContext.Response.ToString());
        }
    }
}
